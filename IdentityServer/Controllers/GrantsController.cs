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
    /// ���� �ο� ��Ʈ�ѷ�
    /// </summary>
    /// <remarks>�� ��Ʈ�ѷ��� ����ϸ� ����ڰ� Ŭ���̾�Ʈ�� �ο��� ������ ����� �� �ִ�.</remarks>
    [SecurityHeaders]
    [Authorize]
    public class GrantsController : Controller
    {
        /// <summary>
        /// �ſ� ���� ��ȭ�� ����
        /// </summary>
        private readonly IIdentityServerInteractionService _interaction;

        /// <summary>
        /// Ŭ���̾�Ʈ �����
        /// </summary>
        private readonly IClientStore _clients;

        /// <summary>
        /// ���ҽ� �����
        /// </summary>
        private readonly IResourceStore _resources;

        /// <summary>
        /// �̺�Ʈ ����
        /// </summary>
        private readonly IEventService _events;

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="identityServerInteractionService">�ſ� ���� ��ȭ�� ����</param>
        /// <param name="clientStore">Ŭ���̾�Ʈ �����</param>
        /// <param name="resourceStore">���ҽ� �����</param>
        /// <param name="eventService">�̺�Ʈ ����</param>
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
        /// �ε��� ������ ó���ϱ�
        /// </summary>
        /// <returns>�׼� ��� �½�ũ</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View("Index", await BuildViewModelAsync());
        }

        /// <summary>
        /// ��� ������ ó���ϱ�
        /// </summary>
        /// <param name="clientID">Ŭ���̾�Ʈ ID</param>
        /// <returns>�׼� ��� �½�ũ</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(string clientId)
        {
            await _interaction.RevokeUserConsentAsync(clientId);
            await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));

            return RedirectToAction("Index");
        }

        /// <summary>
        /// �� �� ����� (�񵿱�)
        /// </summary>
        /// <returns>���� �ο� �� �� �½�ũ</returns>
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