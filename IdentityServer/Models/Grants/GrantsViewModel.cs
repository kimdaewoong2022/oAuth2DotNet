// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;

namespace IdentityServerHost.Quickstart.UI
{
    public class GrantsViewModel
    {
        /// <summary>
        /// 권한 부여 열거 가능형
        /// </summary>
        public IEnumerable<GrantViewModel> Grants { get; set; }
    }

    public class GrantViewModel
    {
        /// <summary>
        /// 클라이언트 ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// 클라이언트명
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// 클라이언트 URL
        /// </summary>
        public string ClientUrl { get; set; }

        /// <summary>
        /// 클라이언트 로고 URL
        /// </summary>
        public string ClientLogoUrl { get; set; }

        /// <summary>
        /// 설명
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 생성 일시
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// 만기 일시
        /// </summary>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// 신원 권한 부여 명칭 열거 가능형
        /// </summary>
        public IEnumerable<string> IdentityGrantNames { get; set; }

        /// <summary>
        /// API 권한 부여 명칭 열거 가능형
        /// </summary>
        public IEnumerable<string> ApiGrantNames { get; set; }
    }
}