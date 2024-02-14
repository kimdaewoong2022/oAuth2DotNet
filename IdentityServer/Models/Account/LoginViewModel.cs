using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServerHost.Quickstart.UI
{
    public class LoginViewModel : LoginInputModel
    {
        /// <summary>
        /// �α��� ��� ��� ����
        /// </summary>
        public bool AllowRememberLogin { get; set; } = true;

        /// <summary>
        /// ���� �α��� ���� ����
        /// </summary>
        public bool EnableLocalLogin { get; set; } = true;

        /// <summary>
        /// �ܺ� ������ ���� ������
        /// </summary>
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();

        /// <summary>
        /// ǥ�� �ܺ� ������ ���� ������
        /// </summary>
        public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName));

        /// <summary>
        /// �ܺ� �α��θ� ����
        /// </summary>
        public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;

        /// <summary>
        /// �ܺ� �α��� ��ȹ
        /// </summary>
        public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;
    }
}