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
    /// �� ���� ��Ʈ�ѷ��� ���� �� �ܺ� ������ ���� �Ϲ����� �α���/�α׾ƿ�/���κ����� ��ũ�÷θ� �����մϴ�.
    /// �α��� ���񽺴� ����� ������ ����ҿ��� ��ȣ �ۿ��� ĸ��ȭ�մϴ�. �� ������ ����Ҵ� �޸� ������ ������ ���δ��ǿ� ����� �� �����ϴ�!
    /// ��ȣ �ۿ� ���񽺴� UI�� ��ȿ�� �˻� �� ���ؽ�Ʈ �˻��� ���� IdentityServer�� ����ϴ� ����� �����մϴ�.
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        #region Field        

        /// <summary>
        /// �׽�Ʈ ����� �����
        /// </summary>
        private readonly TestUserStore _users;

        /// <summary>
        /// �ſ� ���� ��ȭ�� ����
        /// </summary>
        private readonly IIdentityServerInteractionService _interaction;

        /// <summary>
        /// Ŭ���̾�Ʈ �����
        /// </summary>
        private readonly IClientStore _clientStore;

        /// <summary>
        /// ���� ��ȹ ������
        /// </summary>
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        /// <summary>
        /// �̺�Ʈ �����
        /// </summary>
        private readonly IEventService _events;

        #endregion

        #region ������ - AccountController(identityServerInteractionService, clientStore, authenticationSchemeProvider, eventService, testUserStore)

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="identityServerInteractionService">�ſ� ���� ��ȭ�� ����</param>
        /// <param name="clientStore">Ŭ���̾�Ʈ �����</param>
        /// <param name="authenticationSchemeProvider">���� ��ȹ ������</param>
        /// <param name="eventService">�̺�Ʈ ����</param>
        /// <param name="testUserStore">�׽�Ʈ ����� �����</param>
        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            TestUserStore users = null)
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
            //TestUserStore�� DI�� ������ ���� ����� �÷����� ����մϴ�.
            //���⿡ ����� ���� ID ���� ���̺귯��(��: ASP.NET ID)�� �����ϴ� ��
            _users = users ?? new TestUserStore(TestUsers.Users);

            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
        }

        #endregion 

        /// <summary>
        /// �α��� ������ ó���ϱ�
        /// </summary>
        /// <param name="returnURL">��ȯ URL</param>
        /// <returns>�׼� ��� �½�ũ</returns>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // �α��� �������� ǥ���� ������ �� �� �ֵ��� ���� �ۼ��Ѵ�.
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // �α��� �ɼ��� �ϳ� ���̸� �ܺ� �������̴�.
                return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// �α��� ������ ó���ϱ�
        /// </summary>
        /// <param name="model">�α��� �Է� ��</param>
        /// <param name="button">��ư Ÿ��</param>
        /// <returns>�׼� ��� �½�ũ</returns>
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
                    // ����ڰ� ����ϸ� ����� IdentityServer�� �ٽ� �����ϴ�.
                    // ���Ǹ� �ź��߽��ϴ�(�� Ŭ���̾�Ʈ�� ���Ǹ� �䱸���� �ʴ� ��쿡��).
                    // �׼��� �ź� OIDC ���� ������ Ŭ���̾�Ʈ�� �ٽ� �����ϴ�.
                    await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (context.IsNativeClient())
                    {
                        // Ŭ���̾�Ʈ�� ����Ƽ���̹Ƿ� ������ ��ȯ�ϴ� ����� �̷��� ������
                        // ���� ����ڿ��� �� ���� UX�� �����ϱ� ���� ���̴�.
                        return this.LoadingPage("Redirect", model.ReturnUrl);
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // �ùٸ� ���ؽ�Ʈ�� ���� ������ Ȩ �������� ���ư���.
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                // �޸� �� ����ҿ� ���� ����� �̸�/��й�ȣ�� �����Ѵ�. -> DB���� �����ϴ� ������� ���� �ʼ�!! 
                // ValidateCredentials : �ڰ� ���� ����
                if (_users.ValidateCredentials(model.Username, model.Password))
                {
                    var user = _users.FindByUsername(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username, clientId: context?.Client.ClientId));

                    // ����ڰ� "����ϱ�"�� ������ ��쿡�� ���⿡�� ����� ���Ḧ �����Ѵ�.
                    // �׷��� ������ ��Ű �̵��� ������ ���ῡ �����Ѵ�.
                    AuthenticationProperties props = null;
                    if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                        };
                    };

                    // ������Ʈ ID �� ����� �̸����� ���� ��Ű�� �߱��Ѵ�.
                    var isuser = new IdentityServerUser(user.SubjectId)
                    {
                        DisplayName = user.Username
                    };

                    await HttpContext.SignInAsync(isuser, props);

                    if (context != null)
                    {
                        if (context.IsNativeClient())
                        {
                            // Ŭ���̾�Ʈ�� �⺻�̹Ƿ� ������ ��ȯ�ϴ� ����� �̷��� ������
                            // ���� ����ڿ��� �� ���� UX�� �����ϱ� ���� ���̴�.
                            return this.LoadingPage("Redirect", model.ReturnUrl);
                        }

                        // GetAuthorizationContextAsync�� null�� �ƴ� ���� ��ȯ�߱� ������ model.ReturnUrl�� �ŷ��� �� �ִ�.
                        return Redirect(model.ReturnUrl);
                    }

                    // ���� �������� ��û�Ѵ�.
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
                        // ����ڰ� �Ǽ� ��ũ�� Ŭ������ �� �ִ� - ��ϵǾ�� �Ѵ�.
                        throw new Exception("invalid return URL");
                    }
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client.ClientId));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // ������ �߻��ߴ�. ������ �ִ� ����� ǥ���Ѵ�.
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }


        /// <summary>
        /// �α׾ƿ� ������ ó���ϱ�
        /// </summary>
        /// <param name="logoutID">�α׾ƿ� ID</param>
        /// <returns>�׼� ��� �½�ũ</returns>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // �α� �ƿ� �������� ǥ���� ������ �� �� �ֵ��� �α׾ƿ� �� ���� �����.
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // �α� �ƿ� ��û�� IdentityServer���� ����� ������ ���
                // ������Ʈ�� ǥ���� �ʿ䰡 ������ ����ڸ� ���� �α׾ƿ� �� �� �ִ�.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// �α׾ƿ� ������ ó���ϱ�
        /// </summary>
        /// <param name="model">�α׾ƿ� �Է� ��</param>
        /// <returns>�׼� ��� �½�ũ</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // �α׾ƿ��� �������� ǥ���� ������ �� �� �ֵ��� �α׾ƿ� �Ϸ�� �� ���� �����.
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // ���� ���� ��Ű�� �����Ѵ�.
                await HttpContext.SignOutAsync();

                // �α׾ƿ� �̺�Ʈ�� �߻���Ų��.
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // ����Ʈ�� ID �����ڿ��� �α׾ƿ��� Ʈ�����ؾ� �ϴ��� Ȯ���Ѵ�.
            if (vm.TriggerExternalSignout)
            {
                // ����ڰ� �α׾ƿ��� �� ����Ʈ�� �����ڰ� �ٽ� ���� ���𷺼ǵǵ��� ��ȯ URL�� �ۼ��Ѵ�.
                // �̸� ���� �̱� ���� �ƿ� ó���� �Ϸ��� �� �ִ�.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // �̷��� �ϸ� �α׾ƿ��� ���� �ܺ� �����ڷ��� ���𷺼��� Ʈ���ŵȴ�.
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        /// <summary>
        /// �׼��� �źν� ������ ó���ϱ�
        /// </summary>
        /// <returns>�׼� ���</returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/

        /// <summary>
        /// �α��� �� �� ����� (�񵿱�)
        /// </summary>
        /// <param name="returnURL">��ȯ URL</param>
        /// <returns>�α��� �� �� �½�ũ</returns>
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // �̴� UI�� �ܶ���Ű�� �ϳ��� �ܺ� IdP�� Ʈ�����ϱ� ���� ���̴�.
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
        /// �α��� �� �� ����� (�񵿱�)
        /// </summary>
        /// <param name="model">�α��� �Է� ��</param>
        /// <returns>�α��� �� �� �½�ũ</returns>
        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        /// <summary>
        /// �α׾ƿ� �� �� ����� (�񵿱�)
        /// </summary>
        /// <param name="logoutID">�α׾ƿ� ID</param>
        /// <returns>�α׾ƿ� �� �� �½�ũ</returns>
        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // ����ڰ� �������� ���� ��� �α׾ƿ��� �������� ǥ���Ѵ�.
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // �ڵ����� �α׾ƿ��ص� �����ϴ�.
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // �α׾ƿ� ������Ʈ�� ǥ���Ѵ�.
            // �̰��� ����ڰ� �ٸ� �Ǽ� �� �������� ���� �ڵ����� �α׾ƿ��Ǵ� ������ �����Ѵ�.
            return vm;
        }

        /// <summary>
        /// �α׾ƿ� �Ϸ�� �� �� ����� (�񵿱�)
        /// </summary>
        /// <param name="logoutID">�α׾ƿ� ID</param>
        /// <returns>�α׾ƿ� �Ϸ�� �� �� �½�ũ</returns>
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
                            // ���� �α׾ƿ� ���ؽ�Ʈ�� ���� ��� ���� ������ �Ѵ�.
                            // �̷��� �ϸ� �α׾ƿ��ϱ� ���� ���� �α����� ����ڷκ��� �ʿ��� ������ ĸó�ϰ�
                            // �α� �ƿ��� ���� �ܺ� IdP�� �������Ѵ�.
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
