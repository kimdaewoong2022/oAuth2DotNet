using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Quickstart.UI
{
    public class LoginInputModel
    {
        /// <summary>
        /// 사용자명
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// 패스워드
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// 로그인 기억 여부
        /// </summary>
        public bool RememberLogin { get; set; }

        /// <summary>
        /// 반환 URL
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}