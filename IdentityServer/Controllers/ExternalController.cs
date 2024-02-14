using IdentityModel;
using IdentityServer.Attributes;
using IdentityServer.Extensions;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class ExternalController : Controller
    {
        private readonly TestUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly ILogger<ExternalController> _logger;
        private readonly IEventService _events;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="identityServerInteractionService">신원 서버 대화형 서비스</param>
        /// <param name="clientStore">클라이언트 저장소</param>
        /// <param name="eventService">이벤트 서비스</param>
        /// <param name="logger">로그 기록기</param>
        /// <param name="testUserStore">테스트 사용자 저장소</param>
        public ExternalController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events,
            ILogger<ExternalController> logger,
            TestUserStore users = null)
        {
            // TestUserStore가 DI에 없는 경우 글로벌 사용자 컬렉션만 사용한다.
            // 여기에 사용자 지정 ID 관리 라이브러리(예 : ASP.NET ID)를 연결할 수 있다.
            _users = users ?? new TestUserStore(TestUsers.Users);

            _interaction = interaction;
            _clientStore = clientStore;
            _logger = logger;
            _events = events;
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public IActionResult Challenge(string scheme, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // returnURL을 검사한다 - 유효한 OIDC URL이거나 로컬 페이지로 돌아간다.
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // 사용자가 악성 링크를 클릭했을 수 있다 - 기록되어야 한다.
                throw new Exception("invalid return URL");
            }

            // 도전을 시작하고 반환 URL 및 체계를 왕복한다.
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    { "returnUrl", returnUrl },
                    { "scheme", scheme },
                }
            };

            return Challenge(props, scheme);

        }

        /// <summary>
        /// 콜백 페이지 처리하기
        /// </summary>
        /// <returns>액션 결과 태스크</returns>
        /// <remarks>외부 인증 후 처리한다.</remarks>
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            // 임시 쿠키에서 외부 ID를 읽는다.
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                _logger.LogDebug("External claims: {@claims}", externalClaims);
            }

            // 사용자 및 외부 공급자 정보를 조회한다.
            var (user, provider, providerUserId, claims) = FindUserFromExternalProvider(result);
            if (user == null)
            {
                // 이 샘플에서는 사용자 등록을 위한 사용자 지정 워크 플로를 시작할 수 있다.
                // 이 샘플에서는 수행 방법을 보여주지 않는다.
                // 새로운 외부 사용자를 자동 프로비저닝하기만 하면 된다.
                user = AutoProvisionUser(provider, providerUserId, claims);
            }

            // 이를 통해 사용된 특정 프로토콜에 대한 추가 클레임 또는 속성을 수집하고 로컬 인증 쿠키에 저장할 수 있다.
            // 일반적으로 해당 프로토콜에서 로그아웃하는 데 필요한 데이터를 저장하는 데 사용된다.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);

            // 사용자에 대한 인증 쿠키를 발급한다.
            var isuser = new IdentityServerUser(user.SubjectId)
            {
                DisplayName = user.Username,
                IdentityProvider = provider,
                AdditionalClaims = additionalLocalClaims
            };

            await HttpContext.SignInAsync(isuser, localSignInProps);

            // 외부 인증시 사용되는 임시 쿠키를 삭제한다.
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // 반환 URL을 구한다.
            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            // 외부 로그인이 OIDC 요청 컨텍스트에 있는지 확인한다.
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.SubjectId, user.Username, true, context?.Client.ClientId));

            if (context != null)
            {
                if (context.IsNativeClient())
                {
                    // 클라이언트는 기본이므로 응답을 반환하는 방법의 이러한 변경은 최종 사용자에게 더 나은 UX를 제공하기 위한 것이다.
                    return this.LoadingPage("Redirect", returnUrl);
                }
            }

            return Redirect(returnUrl);
        }

        /// <summary>
        /// 외부 공급자에서 사용자 찾기
        /// </summary>
        /// <param name="authenticateResult">인증 결과</param>
        /// <returns>(테스트 사용자, 공급자, 공급자 사용자 ID, 클레임 열거 가능형)</returns>
        private (TestUser user, string provider, string providerUserId, IEnumerable<Claim> claims) FindUserFromExternalProvider(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // 공급자가 발급한 외부 사용자의 고유 ID를 확인한다.
            // 가장 일반적인 클레임 유형은 외부 공급자에 따라 sub 클레임 및 NameIdentifier이며 다른 클레임 유형이 사용될 수 있다.
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // 사용자를 프로비저닝할 때 추가 클레임으로 포함하지 않도록 사용자 ID 클레임을 제거한다.
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // 외부 사용자를 찾는다.
            var user = _users.FindByExternalProvider(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        /// <summary>
        /// 사용자 자동 프로비저닝하기
        /// </summary>
        /// <param name="provider">공급자</param>
        /// <param name="providerUserID">공급자 사용자 ID</param>
        /// <param name="claimEnumerable">클레임 열거 가능형</param>
        /// <returns>테스트 사용자</returns>
        private TestUser AutoProvisionUser(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            var user = _users.AutoProvisionUser(provider, providerUserId, claims.ToList());
            return user;
        }

        /// <summary>
        /// 로그인 콜백 처리하기
        /// </summary>
        /// <param name="externalAuthenticateResult">외부 인증 결과</param>
        /// <param name="localClaimList">로컬 클레임 리스트</param>
        /// <param name="localLoginAuthenticationProperties">로컬 로그인 인증 속성</param>
        /// <remarks>
        ///  외부 로그인이 OIDC 기반인 경우 로그아웃 작업을 위해 보존해야 할 특정 사항이 있다.
        /// 이것은 WS-Fed, SAML2p 또는 기타 프로토콜에 따라 다르다.
        /// </remarks>
        private void ProcessLoginCallback(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // 외부 시스템이 세션 ID 클레임을 보낸 경우 단일 사인 아웃에 사용할 수 있도록 복사한다.
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // 외부 공급자가 id_token을 발행한 경우 로그 아웃을 위해 보관한다.
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
            }
        }
    }
}