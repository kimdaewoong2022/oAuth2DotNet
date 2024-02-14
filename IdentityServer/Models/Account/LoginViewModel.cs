using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServerHost.Quickstart.UI
{
    public class LoginViewModel : LoginInputModel
    {
        /// <summary>
        /// 로그인 기억 허용 여부
        /// </summary>
        public bool AllowRememberLogin { get; set; } = true;

        /// <summary>
        /// 로컬 로그인 가능 여부
        /// </summary>
        public bool EnableLocalLogin { get; set; } = true;

        /// <summary>
        /// 외부 공급자 열거 가능형
        /// </summary>
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();

        /// <summary>
        /// 표시 외부 공급자 열거 가능형
        /// </summary>
        public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName));

        /// <summary>
        /// 외부 로그인만 여부
        /// </summary>
        public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;

        /// <summary>
        /// 외부 로그인 계획
        /// </summary>
        public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;
    }
}