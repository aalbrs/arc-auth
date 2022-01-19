using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcAuth.Controllers
{
    public class AccountController : Controller
    {
        // user info API method
        public ActionResult Info()
        {
            var response = new Dictionary<string, string>()
            {
                { "name", User.Identity.Name }
            };
            return Ok(response);
        }

        // sends user to Portal authorise endpoint, and on return back to default static site
        [Authorize]
        public ActionResult SignIn()
        {
            return new RedirectResult("/");
        }

        // sign user out
        [AllowAnonymous]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();
            return Ok("User has been signed out");
        }

        // example method with policy applied, see app settings
        [Authorize(Policy = "Viewer")]
        public ActionResult AuthorisedRequest()
        {
            return Ok("Authorised for Viewer policy");
        }
    }
}
