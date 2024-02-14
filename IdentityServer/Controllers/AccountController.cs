using IdentityModel;
using IdentityServer.Attributes;
using IdentityServer.Extensions;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Controllers
{
    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// 이 샘플 컨트롤러는 로컬 및 외부 계정에 대한 일반적인 로그인/로그아웃/프로비저닝 워크플로를 구현합니다.
    /// 로그인 서비스는 사용자 데이터 저장소와의 상호 작용을 캡슐화합니다. 이 데이터 저장소는 메모리 내에만 있으며 프로덕션에 사용할 수 없습니다!
    /// 상호 작용 서비스는 UI가 유효성 검사 및 컨텍스트 검색을 위해 IdentityServer와 통신하는 방법을 제공합니다.
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        #region Field        

        /// <summary>
        /// 테스트 사용자 저장소
        /// </summary>
        private readonly TestUserStore _users;

        /// <summary>
        /// 신원 서버 대화형 서비스
        /// </summary>
        private readonly IIdentityServerInteractionService _interaction;

        /// <summary>
        /// 클라이언트 저장소
        /// </summary>
        private readonly IClientStore _clientStore;

        /// <summary>
        /// 인증 계획 공급자
        /// </summary>
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        /// <summary>
        /// 이벤트 저장소
        /// </summary>
        private readonly IEventService _events;

        #endregion

        #region 생성자 - AccountController(identityServerInteractionService, clientStore, authenticationSchemeProvider, eventService, testUserStore)

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="identityServerInteractionService">신원 서버 대화형 서비스</param>
        /// <param name="clientStore">클라이언트 저장소</param>
        /// <param name="authenticationSchemeProvider">인증 계획 공급자</param>
        /// <param name="eventService">이벤트 서비스</param>
        /// <param name="testUserStore">테스트 사용자 저장소</param>
        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            TestUserStore users = null)
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
            //TestUserStore가 DI에 없으면 전역 사용자 컬렉션을 사용합니다.
            //여기에 사용자 정의 ID 관리 라이브러리(예: ASP.NET ID)를 연결하는 곳
            _users = users ?? new TestUserStore(TestUsers.Users);

            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
        }

        #endregion 

        /// <summary>
        /// 로그인 페이지 처리하기
        /// </summary>
        /// <param name="returnURL">반환 URL</param>
        /// <returns>액션 결과 태스크</returns>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // 로그인 페이지에 표시할 내용을 알 수 있도록 모델을 작성한다.
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // 로그인 옵션은 하나 뿐이며 외부 공급자이다.
                return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// 로그인 페이지 처리하기
        /// </summary>
        /// <param name="model">로그인 입력 모델</param>
        /// <param name="button">버튼 타입</param>
        /// <returns>액션 결과 태스크</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    // 사용자가 취소하면 결과를 IdentityServer로 다시 보냅니다.
                    // 동의를 거부했습니다(이 클라이언트가 동의를 요구하지 않는 경우에도).
                    // 액세스 거부 OIDC 오류 응답을 클라이언트에 다시 보냅니다.
                    await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (context.IsNativeClient())
                    {
                        // 클라이언트가 네이티브이므로 응답을 반환하는 방법의 이러한 변경은
                        // 최종 사용자에게 더 나은 UX를 제공하기 위한 것이다.
                        return this.LoadingPage("Redirect", model.ReturnUrl);
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // 올바른 컨텍스트가 없기 때문에 홈 페이지로 돌아간다.
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                // 메모리 내 저장소에 대해 사용자 이름/비밀번호를 검증한다. -> DB에서 검증하는 방식으로 수정 필수!! 
                // ValidateCredentials : 자격 증명 검증
                if (_users.ValidateCredentials(model.Username, model.Password))
                {
                    var user = _users.FindByUsername(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username, clientId: context?.Client.ClientId));

                    // 사용자가 "기억하기"를 선택한 경우에만 여기에서 명시적 만료를 설정한다.
                    // 그렇지 않으면 쿠키 미들웨어에 구성된 만료에 의존한다.
                    AuthenticationProperties props = null;
                    if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                        };
                    };

                    // 서브젝트 ID 및 사용자 이름으로 인증 쿠키를 발급한다.
                    var isuser = new IdentityServerUser(user.SubjectId)
                    {
                        DisplayName = user.Username
                    };

                    await HttpContext.SignInAsync(isuser, props);

                    if (context != null)
                    {
                        if (context.IsNativeClient())
                        {
                            // 클라이언트는 기본이므로 응답을 반환하는 방법의 이러한 변경은
                            // 최종 사용자에게 더 나은 UX를 제공하기 위한 것이다.
                            return this.LoadingPage("Redirect", model.ReturnUrl);
                        }

                        // GetAuthorizationContextAsync가 null이 아닌 값을 반환했기 때문에 model.ReturnUrl을 신뢰할 수 있다.
                        return Redirect(model.ReturnUrl);
                    }

                    // 로컬 페이지를 요청한다.
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect("~/");
                    }
                    else
                    {
                        // 사용자가 악성 링크를 클릭했을 수 있다 - 기록되어야 한다.
                        throw new Exception("invalid return URL");
                    }
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client.ClientId));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // 문제가 발생했다. 오류가 있는 양식을 표시한다.
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }


        /// <summary>
        /// 로그아웃 페이지 처리하기
        /// </summary>
        /// <param name="logoutID">로그아웃 ID</param>
        /// <returns>액션 결과 태스크</returns>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // 로그 아웃 페이지가 표시할 내용을 알 수 있도록 로그아웃 뷰 모델을 만든다.
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // 로그 아웃 요청이 IdentityServer에서 제대로 인증된 경우
                // 프롬프트를 표시할 필요가 없으며 사용자를 직접 로그아웃 할 수 있다.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// 로그아웃 페이지 처리하기
        /// </summary>
        /// <param name="model">로그아웃 입력 모델</param>
        /// <returns>액션 결과 태스크</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // 로그아웃한 페이지가 표시할 내용을 알 수 있도록 로그아웃 완료시 뷰 모델을 만든다.
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // 로컬 인증 쿠키를 삭제한다.
                await HttpContext.SignOutAsync();

                // 로그아웃 이벤트를 발생시킨다.
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // 업스트림 ID 공급자에서 로그아웃을 트리거해야 하는지 확인한다.
            if (vm.TriggerExternalSignout)
            {
                // 사용자가 로그아웃한 후 업스트림 공급자가 다시 당사로 리디렉션되도록 반환 URL을 작성한다.
                // 이를 통해 싱글 사인 아웃 처리를 완료할 수 있다.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // 이렇게 하면 로그아웃을 위해 외부 공급자로의 리디렉션이 트리거된다.
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        /// <summary>
        /// 액세스 거부시 페이지 처리하기
        /// </summary>
        /// <returns>액션 결과</returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/

        /// <summary>
        /// 로그인 뷰 모델 만들기 (비동기)
        /// </summary>
        /// <param name="returnURL">반환 URL</param>
        /// <returns>로그인 뷰 모델 태스크</returns>
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // 이는 UI를 단락시키고 하나의 외부 IdP만 트리거하기 위한 것이다.
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        /// <summary>
        /// 로그인 뷰 모델 만들기 (비동기)
        /// </summary>
        /// <param name="model">로그인 입력 모델</param>
        /// <returns>로그인 뷰 모델 태스크</returns>
        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        /// <summary>
        /// 로그아웃 뷰 모델 만들기 (비동기)
        /// </summary>
        /// <param name="logoutID">로그아웃 ID</param>
        /// <returns>로그아웃 뷰 모델 태스크</returns>
        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // 사용자가 인증되지 않은 경우 로그아웃된 페이지만 표시한다.
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // 자동으로 로그아웃해도 안전하다.
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // 로그아웃 프롬프트를 표시한다.
            // 이것은 사용자가 다른 악성 웹 페이지에 의해 자동으로 로그아웃되는 공격을 방지한다.
            return vm;
        }

        /// <summary>
        /// 로그아웃 완료시 뷰 모델 만들기 (비동기)
        /// </summary>
        /// <param name="logoutID">로그아웃 ID</param>
        /// <returns>로그아웃 완료시 뷰 모델 태스크</returns>
        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // 현재 로그아웃 컨텍스트가 없는 경우 새로 만들어야 한다.
                            // 이렇게 하면 로그아웃하기 전에 현재 로그인한 사용자로부터 필요한 정보를 캡처하고
                            // 로그 아웃을 위해 외부 IdP로 재전송한다.
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
    }
}
