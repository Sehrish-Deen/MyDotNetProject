using CollegeWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CollegeWebsite.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ItmcollegeContext context;

        // Constructor: Inject database context
        public DashboardController(ItmcollegeContext context)
        {
            this.context = context;
        }

        // Default landing page (can be homepage or dashboard redirect)
        public IActionResult Index()
        {
            return View();
        }

        // GET: Admin SignIn page
        public IActionResult SignIn()
        {
            // If admin session exists, redirect to admin dashboard directly
            if (HttpContext.Session.GetString("AdminSession") != null)
            {
                return RedirectToAction("AdminDashboard");
            }
            return View();
        }

        // POST: Handle Admin SignIn
        [HttpPost]
        public IActionResult SignIn(Admin admintbl)
        {
            // Check if admin credentials match any record in the database
            var adminData = context.Admins
                .Where(x => x.Email == admintbl.Email && x.Password == admintbl.Password)
                .FirstOrDefault();

            if (adminData != null)
            {
                // Set session variables on successful login
                HttpContext.Session.SetString("AdminSession", adminData.Name);
                HttpContext.Session.SetInt32("AdminId", adminData.Id);
                return RedirectToAction("AdminDashboard");
            }
            else
            {
                // Invalid login attempt
                ViewBag.Message = "Login Failed";
            }
            return View();
        }

        // GET: Admin Dashboard (protected by session check)
        public async Task<IActionResult> AdminDashboard()
        {
            // Redirect to sign-in if session expired or missing
            if (HttpContext.Session.GetString("AdminSession") == null)
            {
                return RedirectToAction("SignIn");
            }

            // Pass logged-in admin name to view via ViewBag
            ViewBag.MySession = HttpContext.Session.GetString("AdminSession");

            // Retrieve first department and course info (sample display)
            var department = await context.Departments.FirstOrDefaultAsync();
            var course = await context.Courses.FirstOrDefaultAsync();

            // Retrieve lists of faculties, students, and feedbacks
            var faculties = await context.Faculties.ToListAsync();
            var students = await context.Students.ToListAsync();
            var feedbacks = await context.Feedbacks.ToListAsync();

            // Create ViewModel with all necessary data for dashboard
            var viewModel = new CDViewModel
            {
                DepartmentName = department?.DepartmentName ?? "No Department",
                DepartmentDescription = department?.Description,
                DepartmentImagePath = department?.ImagePath,

                CourseName = course?.CourseName ?? "No Course",
                CourseDescription = course?.CourseDescription,
                CourseImagePath = course?.ImagePath,

                Faculties = faculties,
                Students = students,
                Feedbacks = feedbacks
            };

            // Pass counts to the ViewBag for quick stats on dashboard
            ViewBag.DepartmentCount = await context.Departments.CountAsync();
            ViewBag.CourseCount = await context.Courses.CountAsync();
            ViewBag.FacultyCount = faculties.Count;
            ViewBag.StudentCount = students.Count;
            ViewBag.FeedbackCount = feedbacks.Count;

            return View(viewModel);
        }

        // Logout action: clears session and redirects to SignIn
        public IActionResult Logout()
        {
            if (HttpContext.Session.GetString("AdminSession") != null)
            {
                HttpContext.Session.Remove("AdminSession");
                HttpContext.Session.Remove("AdminId");
                return RedirectToAction("SignIn");
            }
            return RedirectToAction("SignIn");
        }

        // GET: Admin SignUp page
        public IActionResult SignUp()
        {
            // Redirect logged-in admin to dashboard
            if (HttpContext.Session.GetString("AdminSession") != null)
            {
                return RedirectToAction("AdminDashboard");
            }

            return View();
        }

        // POST: Handle Admin SignUp
        [HttpPost]
        public async Task<IActionResult> SignUp(Admin admintbl)
        {
            if (!ModelState.IsValid)
            {
                // Return view if model validation fails
                return View(admintbl);
            }

            // Check if email is already registered (case-insensitive)
            var existingAdmin = await context.Admins
                .FirstOrDefaultAsync(a => a.Email.Trim().ToLower() == admintbl.Email.Trim().ToLower());

            if (existingAdmin != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(admintbl);
            }

            // Add new admin and save changes
            await context.Admins.AddAsync(admintbl);
            await context.SaveChangesAsync();

            TempData["Success"] = "Registered Successfully.";
            return RedirectToAction("SignUp");
        }

        // GET: View all feedbacks (protected by session)
        public IActionResult ViewFeedbacks()
        {
            if (HttpContext.Session.GetString("AdminSession") == null)
            {
                return RedirectToAction("SignIn");
            }

            ViewBag.MySession = HttpContext.Session.GetString("AdminSession");

            // Retrieve all feedbacks sorted by latest submission date
            var feedbacks = context.Feedbacks
                .OrderByDescending(f => f.SubmittedAt)
                .ToList();

            return View(feedbacks);
        }

        // GET: Confirm feedback delete view
        public IActionResult Delete(int id)
        {
            var feedback = context.Feedbacks.Find(id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View(feedback);
        }

        // POST: Delete feedback confirmation and processing
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var feedback = context.Feedbacks.Find(id);
            if (feedback != null)
            {
                context.Feedbacks.Remove(feedback);
                context.SaveChanges();
                TempData["delete_success"] = "Feedback deleted successfully.";
            }
            return RedirectToAction("ViewFeedbacks");
        }

        // GET: Change password page for logged-in admin
        public IActionResult ChangePassword()
        {
            var sessionValue = HttpContext.Session.GetString("AdminSession");
            if (sessionValue == null)
            {
                return RedirectToAction("SignIn");
            }

            ViewData["MySession"] = sessionValue;
            return View();
        }

        // POST: Handle Change Password form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var sessionValue = HttpContext.Session.GetString("AdminSession");
            if (sessionValue == null)
            {
                return RedirectToAction("SignIn");
            }

            ViewData["MySession"] = sessionValue;

            if (!ModelState.IsValid)
            {
                // Return if validation fails
                return View(model);
            }

            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                // Session expired or invalid, redirect to sign-in
                return RedirectToAction("SignIn");
            }

            var admin = context.Admins.FirstOrDefault(a => a.Id == adminId.Value);
            if (admin == null)
            {
                ModelState.AddModelError("", "Admin not found.");
                return View(model);
            }

            // Validate current password correctness
            if (admin.Password?.Trim() != model.CurrentPassword?.Trim())
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            // Update password if all validations pass
            admin.Password = model.NewPassword?.Trim();
            context.SaveChanges();

            ViewBag.Message = "Password changed successfully!";
            ModelState.Clear(); // Clear form fields
            return View();
        }
    }
}
