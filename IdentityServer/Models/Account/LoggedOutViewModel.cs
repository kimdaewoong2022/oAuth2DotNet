namespace IdentityServerHost.Quickstart.UI
{
    public class LoggedOutViewModel
    {
        /// <summary>
        /// POST 로그아웃 재전송 URI
        /// </summary>
        public string PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// 클라이언트명
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// 로그아웃 IFRAME URL
        /// </summary>
        public string SignOutIframeUrl { get; set; }

        /// <summary>
        /// 로그아웃 후 자동 재전송 여부
        /// </summary>
        public bool AutomaticRedirectAfterSignOut { get; set; }

        /// <summary>
        /// 로그아웃 ID
        /// </summary>
        public string LogoutId { get; set; }

        /// <summary>
        /// 외부 로그아웃 트리거 여부
        /// </summary>
        public bool TriggerExternalSignout => ExternalAuthenticationScheme != null;

        /// <summary>
        /// 외부 인증 계획
        /// </summary>
        public string ExternalAuthenticationScheme { get; set; }
    }
}