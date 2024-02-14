using System;

namespace IdentityServerHost.Quickstart.UI
{
    public class AccountOptions
    {
        /// <summary>
        /// 지역 로그인 허용 여부
        /// </summary>
        public static bool AllowLocalLogin = true;

        /// <summary>
        /// 로그인 기억 허용 여부
        /// </summary>
        public static bool AllowRememberLogin = true;

        /// <summary>
        /// 로그인 기억 기간
        /// </summary>
        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

        /// <summary>
        /// 로그아웃 프롬프트 표시 여부
        /// </summary>
        public static bool ShowLogoutPrompt = true;
     
        /// <summary>
        /// 로그아웃시 자동으로 리다이렉트를 할지 말지
        /// </summary>
        public static bool AutomaticRedirectAfterSignOut = false;

        /// <summary>
        /// 무효 자격 증명 에러 메시지
        /// </summary>
        public static string InvalidCredentialsErrorMessage = "Invalid username or password";
    }
}
