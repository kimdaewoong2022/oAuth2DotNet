using System.Linq;
using System.Threading.Tasks;
using IdentityServer.Attributes;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    [SecurityHeaders]
    [Authorize]
    public class DiagnosticsController : Controller
    {
        /// <summary>
        /// 인덱스 페이지 처리하기
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var localAddresses = new string[] { 
                "127.0.0.1", 
                "::1", 
                HttpContext.Connection.LocalIpAddress.ToString() 
            };

            if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
            {
                return NotFound();
            }

            var model = new DiagnosticsViewModel(await HttpContext.AuthenticateAsync());
            return View(model);
        }
    }
}