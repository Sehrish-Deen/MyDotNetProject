using CollegeWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace CollegeWebsite.Controllers
{
    // CourseController inherits from BaseController, so it has session checking
    public class CourseController : BaseController
    {
        private readonly ItmcollegeContext context;       // Database context for entity operations
        private readonly IWebHostEnvironment env;          // For accessing web root path to save images

        // Constructor to inject dependencies
        public CourseController(ItmcollegeContext context, IWebHostEnvironment env)
        {
            this.context = context;
            this.env = env;
        }

        // GET: Course/Index
        public IActionResult Index()
        {
            return View();
        }



        // GET: Course/AddCourse - Returns the form for adding a new course
        public IActionResult AddCourse()
        {
            var viewModel = new CourseViewModel
            {
                Departments = GetDepartments()   // Populate dropdown for departments
            };
            return View(viewModel);
        }

        // POST: Course/AddCourse - Handles course creation form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(CourseViewModel model)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                model.Departments = GetDepartments();
                return View(model);
            }

            // Duplicate check - same course name under the same department is not allowed
            bool courseExists = context.Courses.Any(c =>
                c.CourseName.Trim().ToLower() == model.CourseName.Trim().ToLower() &&
                c.DepartmentId == model.DepartmentId);

            if (courseExists)
            {
                TempData["Ext_error"] = "This course already exists in the selected department.";
                model.Departments = GetDepartments();
                return View(model);
            }

            // Check if an image file is uploaded
            if (model.ImageFile == null)
            {
                TempData["Ext_error"] = "Please select an image.";
                model.Departments = GetDepartments();
                return View(model);
            }

            string imageName = "";
            var ext = Path.GetExtension(model.ImageFile.FileName).ToLower();
            var size = model.ImageFile.Length;

            // Validate image extension and size (less than or equal to 1 MB)
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
            {
                if (size <= 1_000_000)
                {
                    // Save image to wwwroot/images folder with a unique name
                    string folder = Path.Combine(env.WebRootPath, "images");
                    imageName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(folder, imageName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }

                    // Create new course entity and save to database
                    var course = new Course
                    {
                        CourseName = model.CourseName,
                        CourseDescription = model.CourseDescription,
                        DepartmentId = model.DepartmentId,
                        ImagePath = imageName
                    };

                    context.Courses.Add(course);
                    await context.SaveChangesAsync();

                    TempData["insert_success"] = "Course added successfully.";
                    return RedirectToAction("ShowCourses");
                }
                else
                {
                    TempData["Size_error"] = "Image must be less than 1 MB.";
                }
            }
            else
            {
                TempData["Ext_error"] = "Only PNG, JPG, JPEG images are allowed.";
            }

            model.Departments = GetDepartments();
            return View(model);
        }

        // GET: Course/ShowCourses - Displays all courses with their departments
        // GET: Course/ShowCourses - Displays all courses with optional search
        public IActionResult ShowCourses(string search)
        {
            var courses = context.Courses.Include(c => c.Department).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                courses = courses.Where(c =>
                    EF.Functions.Like(c.CourseName, $"%{search}%") ||
                    EF.Functions.Like(c.Department.DepartmentName, $"%{search}%"));
            }

            return View(courses.ToList());
        }






        // GET: Course/Details/5 - Shows details of a specific course
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await context.Courses.Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            return View(course);
        }

        // GET: Course/Edit/5 - Shows the edit form with existing data
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var viewModel = new CourseViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseDescription = course.CourseDescription,
                DepartmentId = course.DepartmentId,
                Departments = GetDepartments()  // Populate department dropdown
            };

            ViewBag.ExistingImage = course.ImagePath;  // Pass existing image to view
            return View(viewModel);
        }

        // POST: Course/Edit - Handles the update form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = GetDepartments();
                return View(model);
            }

            var course = await context.Courses.FindAsync(model.CourseId);
            if (course == null) return NotFound();

            // Duplicate check excluding the current course being edited
            bool courseExists = context.Courses.Any(c =>
                c.CourseId != model.CourseId &&
                c.CourseName.Trim().ToLower() == model.CourseName.Trim().ToLower() &&
                c.DepartmentId == model.DepartmentId);

            if (courseExists)
            {
                TempData["Ext_error"] = "Another course with the same name exists in the selected department.";
                model.Departments = GetDepartments();
                ViewBag.ExistingImage = course.ImagePath;
                return View(model);
            }

            // Update course properties
            course.CourseName = model.CourseName;
            course.CourseDescription = model.CourseDescription;
            course.DepartmentId = model.DepartmentId;

            // If new image uploaded, validate and save
            if (model.ImageFile != null)
            {
                var ext = Path.GetExtension(model.ImageFile.FileName).ToLower();
                var size = model.ImageFile.Length;

                if (!(ext == ".png" || ext == ".jpg" || ext == ".jpeg"))
                {
                    TempData["Ext_error"] = "Only PNG, JPG, JPEG images are allowed.";
                    model.Departments = GetDepartments();
                    ViewBag.ExistingImage = course.ImagePath;
                    return View(model);
                }

                if (size > 1_000_000)
                {
                    TempData["Size_error"] = "Image must be less than 1 MB.";
                    model.Departments = GetDepartments();
                    ViewBag.ExistingImage = course.ImagePath;
                    return View(model);
                }

                // Save new image
                string uploadsFolder = Path.Combine(env.WebRootPath, "images");
                string imageName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, imageName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                // Update image path in database
                course.ImagePath = imageName;
            }

            await context.SaveChangesAsync();

            TempData["update_success"] = "Course updated successfully.";
            return RedirectToAction("ShowCourses");
        }

        // GET: Course/Delete/5 - Shows delete confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await context.Courses.Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            return View(course);
        }

        // POST: Course/Delete/5 - Handles the actual deletion after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await context.Courses.FindAsync(id);
            if (course != null)
            {
                // Delete associated image file if exists
                var imagePath = Path.Combine(env.WebRootPath, "images", course.ImagePath ?? "");
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                context.Courses.Remove(course);
                await context.SaveChangesAsync();

                TempData["delete_success"] = "Course deleted successfully.";
            }

            return RedirectToAction("ShowCourses");
        }

        // Helper method to get list of departments for dropdown
        private List<SelectListItem> GetDepartments()
        {
            return context.Departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.DepartmentName
            }).ToList();
        }

   
    }
}
