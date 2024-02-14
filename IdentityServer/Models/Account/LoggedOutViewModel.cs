namespace IdentityServerHost.Quickstart.UI
{
    public class LoggedOutViewModel
    {
        /// <summary>
        /// POST �α׾ƿ� ������ URI
        /// </summary>
        public string PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Ŭ���̾�Ʈ��
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// �α׾ƿ� IFRAME URL
        /// </summary>
        public string SignOutIframeUrl { get; set; }

        /// <summary>
        /// �α׾ƿ� �� �ڵ� ������ ����
        /// </summary>
        public bool AutomaticRedirectAfterSignOut { get; set; }

        /// <summary>
        /// �α׾ƿ� ID
        /// </summary>
        public string LogoutId { get; set; }

        /// <summary>
        /// �ܺ� �α׾ƿ� Ʈ���� ����
        /// </summary>
        public bool TriggerExternalSignout => ExternalAuthenticationScheme != null;

        /// <summary>
        /// �ܺ� ���� ��ȹ
        /// </summary>
        public string ExternalAuthenticationScheme { get; set; }
    }
}