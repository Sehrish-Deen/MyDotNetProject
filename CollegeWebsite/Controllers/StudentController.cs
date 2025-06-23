using CollegeWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeWebsite.Controllers
{
    public class StudentController : Controller
    {
        private readonly ItmcollegeContext context;

        // Constructor to inject database context
        public StudentController(ItmcollegeContext context)
        {
            this.context = context;
        }

        // Default action - can be used as landing page or redirect as needed
        public IActionResult Index()
        {
            return View();
        }

        // GET: SignUp page
        public IActionResult SignUp()
        {
            // If student already logged in, redirect to dashboard
            if (HttpContext.Session.GetString("StudentSession") != null)
            {
                return RedirectToAction("StudentDashboard");
            }

            return View();
        }

        // POST: Handle student registration
        [HttpPost]
        public async Task<IActionResult> SignUp(Student stdtbl)
        {
            // Validate the form inputs
            if (!ModelState.IsValid)
            {
                return View(stdtbl);
            }

            // Check if email already exists (case insensitive)
            var existingStd = await context.Students
                .FirstOrDefaultAsync(a => a.Email.Trim().ToLower() == stdtbl.Email.Trim().ToLower());

            if (existingStd != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(stdtbl);
            }

            // Add new student record and save to database
            await context.Students.AddAsync(stdtbl);
            await context.SaveChangesAsync();

            // Show success message and redirect back to signup (or redirect elsewhere if needed)
            TempData["Success"] = "Registered Successfully.";
            return RedirectToAction("SignUp", "Student");
        }

        // GET: Login page
        public IActionResult SignIn()
        {
            // If already logged in, redirect to dashboard
            if (HttpContext.Session.GetString("StudentSession") != null)
            {
                return RedirectToAction("StudentDashboard");
            }
            return View();
        }

        // POST: Handle login submission
        [HttpPost]
        public IActionResult SignIn(Student stdtbl)
        {
            // Check database for matching email and password
            var stdData = context.Students
                .FirstOrDefault(x => x.Email == stdtbl.Email && x.Password == stdtbl.Password);

            if (stdData != null)
            {
                // Save session variables on successful login
                HttpContext.Session.SetString("StudentSession", stdData.StudentName);
                HttpContext.Session.SetInt32("StudentId", stdData.StudentId);

                return RedirectToAction("StudentDashboard");
            }
            else
            {
                // Login failed - show error message
                TempData["Error"] = "Invalid email or password.";
                return View();
            }
        }

        // GET: Student dashboard - shows after login
        public async Task<IActionResult> StudentDashboard()
        {
            // Check if session exists, otherwise redirect to login
            if (HttpContext.Session.GetString("StudentSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("StudentSession");
                ViewBag.StudentId = HttpContext.Session.GetInt32("StudentId");
            }
            else
            {
                return RedirectToAction("SignIn");
            }

            return View();
        }

        // Logout the student and clear session
        public IActionResult Logout()
        {
            if (HttpContext.Session.GetString("StudentSession") != null)
            {
                HttpContext.Session.Remove("StudentSession");
                HttpContext.Session.Remove("StudentId");
            }
            return RedirectToAction("SignIn", "Student");
        }

        // GET: Change password page
        public IActionResult ChangePassword()
        {
            // Pass session info to view
            ViewBag.MySession = HttpContext.Session.GetString("StudentSession");
            ViewBag.StudentId = HttpContext.Session.GetInt32("StudentId");

            return View();
        }

        // POST: Change password action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            // Pass session info to view
            ViewBag.MySession = HttpContext.Session.GetString("StudentSession");
            ViewBag.StudentId = HttpContext.Session.GetInt32("StudentId");

            // Validate model
            if (!ModelState.IsValid)
                return View(model);

            // Get logged-in student ID from session
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                // No valid session - redirect to login
                return RedirectToAction("SignIn", "Student");
            }

            // Fetch student from database
            var student = context.Students.FirstOrDefault(a => a.StudentId == studentId.Value);
            if (student == null)
            {
                ModelState.AddModelError("", "Student not found.");
                return View(model);
            }

            // Verify current password matches
            if (student.Password?.Trim() != model.CurrentPassword?.Trim())
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            // Update password
            student.Password = model.NewPassword?.Trim();
            context.SaveChanges();

            ViewBag.Message = "Password changed successfully!";
            ModelState.Clear();

            return View();
        }
    }
}
