using System;
using IdentityServer4.Models;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Extensions
{
    /// <summary>
    /// 컨트롤러 확장
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 네이티브 클라이언트 여부 구하기
        /// </summary>
        /// <returns>네이티브 클라이언트 여부</returns>
        /// <remarks>리디렉션 URI가 네이티브 클라이언트용인지 확인한다.</remarks>
        public static bool IsNativeClient(this AuthorizationRequest context)
        {
            return !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
               && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);
        }

        /// <summary>
        /// 로딩 페이지 처리하기
        /// </summary>
        /// <param name="controller">컨트롤러</param>
        /// <param name="viewName">뷰 명칭</param>
        /// <param name="redirectURI">재전송 URI</param>
        /// <returns>액션 결과</returns>
        public static IActionResult LoadingPage(this Controller controller, string viewName, string redirectUri)
        {
            controller.HttpContext.Response.StatusCode = 200;
            controller.HttpContext.Response.Headers["Location"] = "";

            return controller.View(viewName, new RedirectViewModel { RedirectUrl = redirectUri });
        }
    }
}
