using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // example method with policy applied, see app settings
        [Authorize(Policy = "Viewer")]
        public ActionResult AuthorisedRequest()
        {
            return Ok("Authorised for Viewer policy");
        }
    }
}
