using CollegeWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeWebsite.Controllers
{
    public class DepartmentController : BaseController
    {
        ItmcollegeContext context;           // Database context for data access
        IWebHostEnvironment Env;             // Environment to access web root path for file uploads

        // Constructor to inject dependencies
        public DepartmentController(ItmcollegeContext context, IWebHostEnvironment env)
        {
            this.context = context;
            Env = env;
        }

        // GET: /Department/Index
        // Displays a list of all departments
        public IActionResult Index(string search)
        {
            if (search != null)
            {

                return View(context.Departments.Where(name => EF.Functions.Like(name.DepartmentName, $"%{search}%")).ToList());
            }
            else
            {
                return View(context.Departments.ToList());
            }
        
        }

        // GET: /Department/AddDepartment
        // Returns the Add Department form view
        public IActionResult AddDepartment()
        {
            return View();
        }

        // POST: /Department/AddDepartment
        // Handles form submission to add a new department
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDepartment(DepartmentViewModel addDept)
        {
            if (!ModelState.IsValid)
            {
                // Return with validation errors if model is invalid
                return View(addDept);
            }

            // Check if a department with the same name already exists (case-insensitive)
            var existingDept = await context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName.Trim().ToLower() == addDept.DepartmentName.Trim().ToLower());

            if (existingDept != null)
            {
                TempData["Ext_error"] = "This department already exists.";
                return View(addDept);
            }

            // Ensure an image file is selected
            if (addDept.ImagePath == null)
            {
                TempData["Ext_error"] = "Please select an image.";
                return View(addDept);
            }

            string filename = "";
            var ext = Path.GetExtension(addDept.ImagePath.FileName).ToLower();
            var size = addDept.ImagePath.Length;

            // Validate image file extension and size (<= 1 MB)
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
            {
                if (size <= 1000000)
                {
                    // Generate unique filename and save file to /wwwroot/images folder
                    string folder = Path.Combine(Env.WebRootPath, "images");
                    filename = Guid.NewGuid().ToString() + "_" + addDept.ImagePath.FileName;
                    string filepath = Path.Combine(folder, filename);

                    using (var stream = new FileStream(filepath, FileMode.Create))
                    {
                        await addDept.ImagePath.CopyToAsync(stream);
                    }

                    // Create new department entity and save to database
                    Department d = new Department()
                    {
                        DepartmentName = addDept.DepartmentName,
                        Description = addDept.Description,
                        ImagePath = filename
                    };

                    await context.Departments.AddAsync(d);
                    await context.SaveChangesAsync();

                    TempData["insert_success"] = "Department Added Successfully";
                    return RedirectToAction("Index");
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

            // Return to view if any validation fails
            return View(addDept);
        }

        // GET: /Department/Details/{id}
        // Displays details of a specific department
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || context.Departments == null)
            {
                return NotFound();
            }

            // Find department by id
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.DepartmentId == id);

            if (dept == null)
            {
                return NotFound();
            }

            return View(dept);
        }

        // GET: /Department/Edit/{id}
        // Returns form to edit department details
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || context.Departments == null)
                return NotFound();

            var dept = await context.Departments.FindAsync(id);
            if (dept == null)
                return NotFound();

            // Map existing data to ViewModel (except ImagePath because it's an IFormFile)
            var viewModel = new DepartmentViewModel
            {
                DepartmentId = dept.DepartmentId,
                DepartmentName = dept.DepartmentName,
                Description = dept.Description
                // ImagePath is IFormFile, so not assigned here
            };

            // Pass existing image filename to ViewBag to display current image in view
            ViewBag.ExistingImage = dept.ImagePath;
            return View(viewModel);
        }

        // POST: /Department/Edit/{id}
        // Handles updating a department record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, keep existing image info for display
                ViewBag.ExistingImage = (await context.Departments.FindAsync(id))?.ImagePath;
                return View(model);
            }

            // Check for duplicate department name excluding current record
            var duplicate = await context.Departments
                .FirstOrDefaultAsync(d =>
                    d.DepartmentId != id &&
                    d.DepartmentName.Trim().ToLower() == model.DepartmentName.Trim().ToLower());

            if (duplicate != null)
            {
                TempData["Ext_error"] = "Another department with the same name already exists.";
                ViewBag.ExistingImage = (await context.Departments.FindAsync(id))?.ImagePath;
                return View(model);
            }

            var dept = await context.Departments.FindAsync(id);
            if (dept == null)
                return NotFound();

            // Update department properties
            dept.DepartmentName = model.DepartmentName;
            dept.Description = model.Description;

            if (model.ImagePath != null)
            {
                // Validate and save new uploaded image file if provided
                var ext = Path.GetExtension(model.ImagePath.FileName).ToLower();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    if (model.ImagePath.Length <= 1000000)
                    {
                        var fileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                        var path = Path.Combine(Env.WebRootPath, "images", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await model.ImagePath.CopyToAsync(stream);
                        }

                        dept.ImagePath = fileName;
                    }
                    else
                    {
                        TempData["Size_error"] = "Image must be less than 1MB";
                        ViewBag.ExistingImage = dept.ImagePath;
                        return View(model);
                    }
                }
                else
                {
                    TempData["Ext_error"] = "Only PNG, JPG, JPEG images allowed.";
                    ViewBag.ExistingImage = dept.ImagePath;
                    return View(model);
                }
            }
            else
            {
                // No new image uploaded - preserve existing image filename
                var existing = await context.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.DepartmentId == id);
                dept.ImagePath = existing?.ImagePath;
            }

            // Save changes to database
            context.Departments.Update(dept);
            await context.SaveChangesAsync();

            TempData["update_success"] = "Department updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: /Department/Delete/{id}
        // Shows confirmation page before deleting a department
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dept = await context.Departments.FirstOrDefaultAsync(x => x.DepartmentId == id);

            if (dept == null)
            {
                return NotFound();
            }

            return View(dept);
        }

        // POST: /Department/DeleteConfirmed/{id}
        // Deletes the department after confirmation, ensuring no linked courses exist
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            // Include related courses for dependency check
            var dept = await context.Departments
                .Include(d => d.Courses)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (dept == null)
            {
                return NotFound();
            }

            // Prevent deletion if department has courses assigned
            if (dept.Courses != null && dept.Courses.Any())
            {
                TempData["delete_error"] = "Cannot delete department with assigned courses.";
                return RedirectToAction("Index");
            }

            // Delete image file from wwwroot/images if exists
            var imagePath = Path.Combine(Env.WebRootPath, "images", dept.ImagePath);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            // Remove department record from database
            context.Departments.Remove(dept);
            await context.SaveChangesAsync();

            TempData["delete_success"] = "Department deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
