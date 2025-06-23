using CollegeWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CollegeWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;   // Logger instance for logging information/errors
        private readonly ItmcollegeContext _context;        // Database context for accessing the database

        // Constructor injecting logger and database context via Dependency Injection
        public HomeController(ILogger<HomeController> logger, ItmcollegeContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Home/Index
        // Retrieves list of courses joined with departments, along with faculties and students data,
        // to display on the home page.
        public IActionResult Index()
        {
            var data = (from c in _context.Courses
                        join d in _context.Departments on c.DepartmentId equals d.DepartmentId
                        select new CDViewModel
                        {
                            DepartmentName = d.DepartmentName,
                            DepartmentDescription = d.Description,
                            DepartmentImagePath = d.ImagePath,

                            CourseName = c.CourseName,
                            CourseDescription = c.CourseDescription,
                            CourseImagePath = c.ImagePath,

                            // Get faculties related to the department, including their department info
                            Faculties = _context.Faculties
                                .Include(f => f.Department)
                                .Where(f => f.DepartmentId == d.DepartmentId)
                                .ToList(),

                            // Get all students (could be optimized based on requirements)
                            Students = _context.Students.ToList()
                        }).ToList();

            return View(data); // Pass the data to the view for rendering
        }


        // GET: Home/DepartmentsServices
        // Retrieves courses with their departments and faculties to show on the departments services page.
        public IActionResult DepartmentsServices()
        {
            var data = (from c in _context.Courses
                        join d in _context.Departments on c.DepartmentId equals d.DepartmentId
                        select new CDViewModel
                        {
                            DepartmentName = d.DepartmentName,
                            DepartmentDescription = d.Description,
                            DepartmentImagePath = d.ImagePath,

                            CourseName = c.CourseName,
                            CourseDescription = c.CourseDescription,
                            CourseImagePath = c.ImagePath,

                            // Faculties linked to each department
                            Faculties = _context.Faculties
                                .Include(f => f.Department)
                                .Where(f => f.DepartmentId == d.DepartmentId)
                                .ToList()
                        }).ToList();

            return View(data); // Send the data to the DepartmentsServices view
        }


       
        // Handles AJAX post request to submit user feedback.
        // Validates input and saves feedback to database, returns JSON response.
        [HttpPost]
        public JsonResult SubmitFeedback([Bind(Prefix = "FeedbackInput")] Feedback model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please fill all required fields correctly." });
            }

            model.SubmittedAt = DateTime.Now;
            _context.Feedbacks.Add(model);
            _context.SaveChanges();

            return Json(new { success = true, message = "Your message has been sent successfully!" });
        }


        // GET: Home/Privacy
        // Returns the Privacy policy page
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: Home/Error
        // Handles error page and shows error details like request ID
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
