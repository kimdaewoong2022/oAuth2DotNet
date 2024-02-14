// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServerHost.Quickstart.UI;
using IdentityServer.Attributes;

namespace IdentityServer.Controllers
{
    /// <summary>
    /// 권한 부여 컨트롤러
    /// </summary>
    /// <remarks>이 컨트롤러를 사용하면 사용자가 클라이언트에 부여된 권한을 취소할 수 있다.</remarks>
    [SecurityHeaders]
    [Authorize]
    public class GrantsController : Controller
    {
        /// <summary>
        /// 신원 서비스 대화형 서비스
        /// </summary>
        private readonly IIdentityServerInteractionService _interaction;

        /// <summary>
        /// 클라이언트 저장소
        /// </summary>
        private readonly IClientStore _clients;

        /// <summary>
        /// 리소스 저장소
        /// </summary>
        private readonly IResourceStore _resources;

        /// <summary>
        /// 이벤트 서비스
        /// </summary>
        private readonly IEventService _events;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="identityServerInteractionService">신원 서버 대화형 서비스</param>
        /// <param name="clientStore">클라이언트 저장소</param>
        /// <param name="resourceStore">리소스 저장소</param>
        /// <param name="eventService">이벤트 서비스</param>
        public GrantsController(IIdentityServerInteractionService interaction,
            IClientStore clients,
            IResourceStore resources,
            IEventService events)
        {
            _interaction = interaction;
            _clients = clients;
            _resources = resources;
            _events = events;
        }

        /// <summary>
        /// 인덱스 페이지 처리하기
        /// </summary>
        /// <returns>액션 결과 태스크</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View("Index", await BuildViewModelAsync());
        }

        /// <summary>
        /// 취소 페이지 처리하기
        /// </summary>
        /// <param name="clientID">클라이언트 ID</param>
        /// <returns>액션 결과 태스크</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(string clientId)
        {
            await _interaction.RevokeUserConsentAsync(clientId);
            await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));

            return RedirectToAction("Index");
        }

        /// <summary>
        /// 뷰 모델 만들기 (비동기)
        /// </summary>
        /// <returns>권한 부여 뷰 모델 태스크</returns>
        private async Task<GrantsViewModel> BuildViewModelAsync()
        {
            var grants = await _interaction.GetAllUserGrantsAsync();

            var list = new List<GrantViewModel>();
            foreach (var grant in grants)
            {
                var client = await _clients.FindClientByIdAsync(grant.ClientId);
                if (client != null)
                {
                    var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes);

                    var item = new GrantViewModel()
                    {
                        ClientId = client.ClientId,
                        ClientName = client.ClientName ?? client.ClientId,
                        ClientLogoUrl = client.LogoUri,
                        ClientUrl = client.ClientUri,
                        Description = grant.Description,
                        Created = grant.CreationTime,
                        Expires = grant.Expiration,
                        IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                        ApiGrantNames = resources.ApiScopes.Select(x => x.DisplayName ?? x.Name).ToArray()
                    };

                    list.Add(item);
                }
            }

            return new GrantsViewModel
            {
                Grants = list
            };
        }
    }
}