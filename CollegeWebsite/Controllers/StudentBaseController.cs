using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

public class StudentBaseController : Controller
{
    // This method is called before any action method in controllers that inherit from this base controller
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Retrieve the session values for the logged-in student
        var sessionName = context.HttpContext.Session.GetString("StudentSession");
        var sessionId = context.HttpContext.Session.GetInt32("StudentId");

        // Check if the session values are missing or null (user is not logged in)
        if (string.IsNullOrEmpty(sessionName) || sessionId == null)
        {
            // If no valid session, redirect user to the SignIn action of the Student controller
            context.Result = new RedirectToActionResult("SignIn", "Student", null);
        }
        else
        {
            // If session exists, store session data in ViewBag for use in views (e.g., to show student name/id)
            ViewBag.MySession = sessionName;
            ViewBag.StudentId = sessionId;
        }

        // Call the base method so any other processing can happen
        base.OnActionExecuting(context);
    }
}
