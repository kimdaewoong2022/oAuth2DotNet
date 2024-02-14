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
        /// ������
        /// </summary>
        /// <param name="identityServerInteractionService">�ſ� ���� ��ȭ�� ����</param>
        /// <param name="clientStore">Ŭ���̾�Ʈ �����</param>
        /// <param name="eventService">�̺�Ʈ ����</param>
        /// <param name="logger">�α� ��ϱ�</param>
        /// <param name="testUserStore">�׽�Ʈ ����� �����</param>
        public ExternalController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events,
            ILogger<ExternalController> logger,
            TestUserStore users = null)
        {
            // TestUserStore�� DI�� ���� ��� �۷ι� ����� �÷��Ǹ� ����Ѵ�.
            // ���⿡ ����� ���� ID ���� ���̺귯��(�� : ASP.NET ID)�� ������ �� �ִ�.
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

            // returnURL�� �˻��Ѵ� - ��ȿ�� OIDC URL�̰ų� ���� �������� ���ư���.
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // ����ڰ� �Ǽ� ��ũ�� Ŭ������ �� �ִ� - ��ϵǾ�� �Ѵ�.
                throw new Exception("invalid return URL");
            }

            // ������ �����ϰ� ��ȯ URL �� ü�踦 �պ��Ѵ�.
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
        /// �ݹ� ������ ó���ϱ�
        /// </summary>
        /// <returns>�׼� ��� �½�ũ</returns>
        /// <remarks>�ܺ� ���� �� ó���Ѵ�.</remarks>
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            // �ӽ� ��Ű���� �ܺ� ID�� �д´�.
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

            // ����� �� �ܺ� ������ ������ ��ȸ�Ѵ�.
            var (user, provider, providerUserId, claims) = FindUserFromExternalProvider(result);
            if (user == null)
            {
                // �� ���ÿ����� ����� ����� ���� ����� ���� ��ũ �÷θ� ������ �� �ִ�.
                // �� ���ÿ����� ���� ����� �������� �ʴ´�.
                // ���ο� �ܺ� ����ڸ� �ڵ� ���κ������ϱ⸸ �ϸ� �ȴ�.
                user = AutoProvisionUser(provider, providerUserId, claims);
            }

            // �̸� ���� ���� Ư�� �������ݿ� ���� �߰� Ŭ���� �Ǵ� �Ӽ��� �����ϰ� ���� ���� ��Ű�� ������ �� �ִ�.
            // �Ϲ������� �ش� �������ݿ��� �α׾ƿ��ϴ� �� �ʿ��� �����͸� �����ϴ� �� ���ȴ�.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);

            // ����ڿ� ���� ���� ��Ű�� �߱��Ѵ�.
            var isuser = new IdentityServerUser(user.SubjectId)
            {
                DisplayName = user.Username,
                IdentityProvider = provider,
                AdditionalClaims = additionalLocalClaims
            };

            await HttpContext.SignInAsync(isuser, localSignInProps);

            // �ܺ� ������ ���Ǵ� �ӽ� ��Ű�� �����Ѵ�.
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // ��ȯ URL�� ���Ѵ�.
            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            // �ܺ� �α����� OIDC ��û ���ؽ�Ʈ�� �ִ��� Ȯ���Ѵ�.
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.SubjectId, user.Username, true, context?.Client.ClientId));

            if (context != null)
            {
                if (context.IsNativeClient())
                {
                    // Ŭ���̾�Ʈ�� �⺻�̹Ƿ� ������ ��ȯ�ϴ� ����� �̷��� ������ ���� ����ڿ��� �� ���� UX�� �����ϱ� ���� ���̴�.
                    return this.LoadingPage("Redirect", returnUrl);
                }
            }

            return Redirect(returnUrl);
        }

        /// <summary>
        /// �ܺ� �����ڿ��� ����� ã��
        /// </summary>
        /// <param name="authenticateResult">���� ���</param>
        /// <returns>(�׽�Ʈ �����, ������, ������ ����� ID, Ŭ���� ���� ������)</returns>
        private (TestUser user, string provider, string providerUserId, IEnumerable<Claim> claims) FindUserFromExternalProvider(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // �����ڰ� �߱��� �ܺ� ������� ���� ID�� Ȯ���Ѵ�.
            // ���� �Ϲ����� Ŭ���� ������ �ܺ� �����ڿ� ���� sub Ŭ���� �� NameIdentifier�̸� �ٸ� Ŭ���� ������ ���� �� �ִ�.
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // ����ڸ� ���κ������� �� �߰� Ŭ�������� �������� �ʵ��� ����� ID Ŭ������ �����Ѵ�.
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // �ܺ� ����ڸ� ã�´�.
            var user = _users.FindByExternalProvider(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        /// <summary>
        /// ����� �ڵ� ���κ������ϱ�
        /// </summary>
        /// <param name="provider">������</param>
        /// <param name="providerUserID">������ ����� ID</param>
        /// <param name="claimEnumerable">Ŭ���� ���� ������</param>
        /// <returns>�׽�Ʈ �����</returns>
        private TestUser AutoProvisionUser(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            var user = _users.AutoProvisionUser(provider, providerUserId, claims.ToList());
            return user;
        }

        /// <summary>
        /// �α��� �ݹ� ó���ϱ�
        /// </summary>
        /// <param name="externalAuthenticateResult">�ܺ� ���� ���</param>
        /// <param name="localClaimList">���� Ŭ���� ����Ʈ</param>
        /// <param name="localLoginAuthenticationProperties">���� �α��� ���� �Ӽ�</param>
        /// <remarks>
        ///  �ܺ� �α����� OIDC ����� ��� �α׾ƿ� �۾��� ���� �����ؾ� �� Ư�� ������ �ִ�.
        /// �̰��� WS-Fed, SAML2p �Ǵ� ��Ÿ �������ݿ� ���� �ٸ���.
        /// </remarks>
        private void ProcessLoginCallback(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // �ܺ� �ý����� ���� ID Ŭ������ ���� ��� ���� ���� �ƿ��� ����� �� �ֵ��� �����Ѵ�.
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // �ܺ� �����ڰ� id_token�� ������ ��� �α� �ƿ��� ���� �����Ѵ�.
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
            }
        }
    }
}