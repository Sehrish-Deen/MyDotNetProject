using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace CollegeWebsite.Controllers
{
    // BaseController inherits from Controller
    // This controller is intended to be a base for other controllers
    // It handles session validation globally before any action executes
    public class BaseController : Controller
    {
        // Override OnActionExecuting to run code before any action method runs
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Retrieve the session value stored under the key "AdminSession"
            var sessionValue = context.HttpContext.Session.GetString("AdminSession");

            // If session is null, user is not authenticated or session expired
            if (sessionValue == null)
            {
                // Redirect the user to the SignIn action of Dashboard controller
                context.Result = new RedirectToActionResult("SignIn", "Dashboard", null);
            }
            else
            {
                // If session exists, store the session value in ViewData globally
                // So it can be accessed in any view rendered by controllers that inherit BaseController
                ViewData["MySession"] = sessionValue;
            }

            // Call the base method to ensure normal execution continues
            base.OnActionExecuting(context);
        }
    }
}
