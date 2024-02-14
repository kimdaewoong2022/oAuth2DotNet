namespace IdentityServerHost.Quickstart.UI
{
    public class LogoutViewModel : LogoutInputModel
    {
        /// <summary>
        /// 로그아웃 프롬프트 표시 여부
        /// </summary>
        public bool ShowLogoutPrompt { get; set; } = true;
    }
}
