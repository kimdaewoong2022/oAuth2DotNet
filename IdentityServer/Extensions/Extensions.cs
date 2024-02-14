using System;
using IdentityServer4.Models;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Extensions
{
    /// <summary>
    /// ��Ʈ�ѷ� Ȯ��
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// ����Ƽ�� Ŭ���̾�Ʈ ���� ���ϱ�
        /// </summary>
        /// <returns>����Ƽ�� Ŭ���̾�Ʈ ����</returns>
        /// <remarks>���𷺼� URI�� ����Ƽ�� Ŭ���̾�Ʈ������ Ȯ���Ѵ�.</remarks>
        public static bool IsNativeClient(this AuthorizationRequest context)
        {
            return !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
               && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);
        }

        /// <summary>
        /// �ε� ������ ó���ϱ�
        /// </summary>
        /// <param name="controller">��Ʈ�ѷ�</param>
        /// <param name="viewName">�� ��Ī</param>
        /// <param name="redirectURI">������ URI</param>
        /// <returns>�׼� ���</returns>
        public static IActionResult LoadingPage(this Controller controller, string viewName, string redirectUri)
        {
            controller.HttpContext.Response.StatusCode = 200;
            controller.HttpContext.Response.Headers["Location"] = "";

            return controller.View(viewName, new RedirectViewModel { RedirectUrl = redirectUri });
        }
    }
}
