using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Quickstart.UI
{
    public class LoginInputModel
    {
        /// <summary>
        /// ����ڸ�
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// �н�����
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// �α��� ��� ����
        /// </summary>
        public bool RememberLogin { get; set; }

        /// <summary>
        /// ��ȯ URL
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}