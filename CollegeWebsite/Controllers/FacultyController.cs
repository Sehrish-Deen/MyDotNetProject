using CollegeWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace CollegeWebsite.Controllers
{
    public class FacultyController : BaseController
    {
        private readonly ItmcollegeContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        // Constructor to inject DbContext and Hosting Environment (for file storage)
        public FacultyController(ItmcollegeContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Show the Add Faculty form with departments dropdown
        public IActionResult AddFaculty()
        {
            var viewModel = new FacultyViewModel
            {
                Departments = GetDepartments()  // Populate department list for dropdown
            };
            return View(viewModel);
        }

        // POST: Handle Add Faculty form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFaculty(FacultyViewModel model)
        {
            // Validate model state; if invalid, re-display form with departments
            if (!ModelState.IsValid)
            {
                model.Departments = GetDepartments();
                return View(model);
            }

            // Normalize faculty name and email for comparison
            string facultyName = model.FacultyName?.Trim().ToLower() ?? "";
            string email = model.Email?.Trim().ToLower() ?? "";

            // Check if the faculty with same name, email, and department already exists
            bool facultyExists = _context.Faculties.Any(f =>
                f.FacultyName.ToLower() == facultyName &&
                f.Email.ToLower() == email &&
                f.DepartmentId == model.DepartmentId);

            if (facultyExists)
            {
                TempData["Error"] = "This faculty member already exists in the selected department.";
                model.Departments = GetDepartments();
                return View(model);
            }

            // Check if a photo file was uploaded
            if (model.Photo == null || string.IsNullOrEmpty(model.Photo.FileName))
            {
                TempData["Error"] = "Please upload a photo.";
                model.Departments = GetDepartments();
                return View(model);
            }

            string imageName = "";
            var ext = Path.GetExtension(model.Photo.FileName).ToLower();
            var size = model.Photo.Length;

            // Validate allowed image extensions and size <= 1MB
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
            {
                if (size <= 1_000_000)
                {
                    // Create directory for storing images if it doesn't exist
                    string folder = Path.Combine(_hostEnvironment.WebRootPath, "facultyImages");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    // Generate unique file name to avoid conflicts
                    imageName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    string filePath = Path.Combine(folder, imageName);

                    // Save the photo to server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Photo.CopyToAsync(stream);
                    }

                    // Create new Faculty entity and save to database
                    var faculty = new Faculty
                    {
                        FacultyName = model.FacultyName,
                        Qualification = model.Qualification,
                        Designation = model.Designation,
                        Email = model.Email,
                        Phone = model.Phone,
                        DepartmentId = model.DepartmentId,
                        Photo = imageName
                    };

                    _context.Faculties.Add(faculty);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Faculty added successfully.";
                    return RedirectToAction("ShowFaculties");
                }
                else
                {
                    TempData["Error"] = "Photo must be less than 1 MB.";
                }
            }
            else
            {
                TempData["Error"] = "Only PNG, JPG, JPEG images are allowed.";
            }

            // If error occurs, reload departments and return view
            model.Departments = GetDepartments();
            return View(model);
        }

        // GET: Show list of all faculties with their departments
        public async Task<IActionResult> ShowFaculties(string search)
        {
            var faculties = _context.Faculties.Include(f => f.Department).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                faculties = faculties.Where(f =>
                    f.FacultyName.ToLower().Contains(search) ||
                    f.Department.DepartmentName.ToLower().Contains(search));
            }

            return View(await faculties.ToListAsync());
        }


        // Helper method to get all departments from database
        private List<Department> GetDepartments()
        {
            return _context.Departments.ToList();
        }

        // GET: Show the Edit form for a faculty member by id
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null) return NotFound();

            // Populate the view model with existing data
            var viewModel = new FacultyViewModel
            {
                FacultyId = faculty.FacultyId,
                FacultyName = faculty.FacultyName,
                Qualification = faculty.Qualification,
                Designation = faculty.Designation,
                Email = faculty.Email,
                Phone = faculty.Phone,
                DepartmentId = faculty.DepartmentId,
                Departments = GetDepartments()
            };

            ViewBag.ExistingPhoto = faculty.Photo; // Pass existing photo filename to view
            return View(viewModel);
        }

        // POST: Handle Edit form submission to update faculty details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FacultyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = GetDepartments();
                ViewBag.ExistingPhoto = model.Photo;
                return View(model);
            }

            var faculty = await _context.Faculties.FindAsync(model.FacultyId);
            if (faculty == null) return NotFound();

            // Check for duplicate faculty (excluding current faculty)
            string facultyName = model.FacultyName?.Trim().ToLower() ?? "";
            string email = model.Email?.Trim().ToLower() ?? "";

            bool exists = _context.Faculties.Any(f =>
                f.FacultyId != model.FacultyId &&
                f.FacultyName.ToLower() == facultyName &&
                f.Email.ToLower() == email &&
                f.DepartmentId == model.DepartmentId);

            if (exists)
            {
                TempData["Ext_error"] = "Another faculty member with the same name and email exists in this department.";
                model.Departments = GetDepartments();
                ViewBag.ExistingPhoto = faculty.Photo;
                return View(model);
            }

            // Update faculty entity properties
            faculty.FacultyName = model.FacultyName;
            faculty.Qualification = model.Qualification;
            faculty.Designation = model.Designation;
            faculty.Email = model.Email;
            faculty.Phone = model.Phone;
            faculty.DepartmentId = model.DepartmentId;

            // If a new photo is uploaded, validate and save it
            if (model.Photo != null)
            {
                var ext = Path.GetExtension(model.Photo.FileName).ToLower();
                var size = model.Photo.Length;

                if (!(ext == ".png" || ext == ".jpg" || ext == ".jpeg"))
                {
                    TempData["Ext_error"] = "Only PNG, JPG, JPEG images are allowed.";
                    model.Departments = GetDepartments();
                    ViewBag.ExistingPhoto = faculty.Photo;
                    return View(model);
                }

                if (size > 1_000_000)
                {
                    TempData["Size_error"] = "Image must be less than 1 MB.";
                    model.Departments = GetDepartments();
                    ViewBag.ExistingPhoto = faculty.Photo;
                    return View(model);
                }

                string folder = Path.Combine(_hostEnvironment.WebRootPath, "facultyImages");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string imageName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(folder, imageName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(stream);
                }

                faculty.Photo = imageName; // Update photo field
            }
            // Else keep existing photo unchanged

            await _context.SaveChangesAsync();

            TempData["update_success"] = "Faculty updated successfully.";
            return RedirectToAction("ShowFaculties");
        }

        // GET: Show confirmation page before deleting a faculty
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var faculty = await _context.Faculties.Include(f => f.Department)
                                                  .FirstOrDefaultAsync(f => f.FacultyId == id);

            if (faculty == null) return NotFound();

            return View(faculty); // Show confirmation view
        }

        // POST: Confirm deletion of faculty
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty != null)
            {
                // Delete photo file from server if it exists
                var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "facultyImages", faculty.Photo ?? "");
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                // Remove faculty record from database
                _context.Faculties.Remove(faculty);
                await _context.SaveChangesAsync();

                TempData["delete_success"] = "Faculty deleted successfully.";
            }

            return RedirectToAction("ShowFaculties");
        }

        // GET: Show detailed view of a single faculty including department info
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var faculty = await _context.Faculties
                .Include(f => f.Department)     // Include related department data
                .FirstOrDefaultAsync(f => f.FacultyId == id);

            if (faculty == null) return NotFound();

            return View(faculty);
        }
    }
}
