using Microsoft.AspNetCore.Mvc;
using CollegeWebsite.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

public class AdmissionController : StudentBaseController
{
    private readonly ItmcollegeContext _context;

    public AdmissionController(ItmcollegeContext context)
    {
        _context = context;
    }

    // GET: Admission/Apply
    // Show the admission application form to logged-in students
    [HttpGet]
    public async Task<IActionResult> Apply()
    {
        // Get the logged-in student's ID from session
        int? studentId = HttpContext.Session.GetInt32("StudentId");
        if (studentId == null)
            return RedirectToAction("SignIn", "Student");  // Redirect if not logged in

        // Fetch student details from DB
        var student = await _context.Students.FindAsync(studentId.Value);

        // Prepare the ViewModel with student's info
        var model = new AdmissionFormViewModel
        {
            StudentId = studentId.Value,
            StudentName = student?.StudentName ?? "Unknown"
        };

        // Load department list for dropdown selection in the view
        ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName");

        return View(model);  // Display the form
    }

    // POST: Admission/Apply
    // Handle form submission for admission application
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(AdmissionFormViewModel model)
    {
        // Get logged-in student ID
        int? studentId = HttpContext.Session.GetInt32("StudentId");
        if (studentId == null)
            return RedirectToAction("SignIn", "Student");  // Redirect if not logged in

        // Check if the student already submitted an application to avoid duplicates
        bool alreadyApplied = await _context.AdmissionApplications
            .AnyAsync(a => a.StudentId == studentId.Value);

        if (alreadyApplied)
        {
            // Add model error and reload form with error message
            ModelState.AddModelError(string.Empty, "You have already submitted the admission form. Duplicate submission is not allowed.");
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName");
            return View(model);
        }

        // If model validation failed, reload form with validation errors
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName");
            return View(model);
        }

        // Create a new admission application entity from the form data
        var admission = new AdmissionApplication
        {
            StudentId = studentId.Value,
            StudentName = model.StudentName,
            FatherName = model.FatherName,
            DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth),
            Gender = model.Gender,
            ResidentialAddress = model.ResidentialAddress,
            PermanentAddress = model.PermanentAddress,
            DepartmentId = model.DepartmentId ?? 0,
            AdmissionFor = model.AdmissionFor,
            BoardOrUniversity = model.BoardOrUniversity,
            EnrollmentNumber = model.EnrollmentNumber,
            ExamCenter = model.ExamCenter,
            Stream = model.Stream,
            MarksSecured = model.MarksSecured,
            MarksOutOf = model.MarksOutOf,
            ClassObtained = model.ClassObtained,
            SportsDetails = model.SportsDetails,
            ApplicationDate = DateTime.Now,
            AdmissionStatus = "Pending",  // Default status when submitted
            UniqueAdmissionNumber = GenerateUniqueAdmissionNumber()  // Generate unique ID for admission
        };

        // Add the new application to the database and save changes
        _context.AdmissionApplications.Add(admission);
        await _context.SaveChangesAsync();

        // Show success message with unique admission number
        TempData["Success"] = "Form submitted successfully. Your Admission ID is: " + admission.UniqueAdmissionNumber + ". You can use this ID to check your admission status.";


        return RedirectToAction("Apply");  // Redirect back to the form page (could be changed to a success page)
    }

    // Helper method to generate a unique admission number
    private string GenerateUniqueAdmissionNumber()
    {
        // Generates a random unique string with prefix "ADM-"
        return "ADM-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }

    // GET: Admission/CheckStatus
    // Show form to enter unique admission number to check status
    [HttpGet]
    public IActionResult CheckStatus()
    {
        return View();
    }

    // POST: Admission/CheckStatus
    // Search and display admission status by unique admission number
    [HttpPost]
    public async Task<IActionResult> CheckStatus(string uniqueAdmissionNumber)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(uniqueAdmissionNumber))
        {
            ViewBag.Message = "Please enter a valid admission number.";
            return View();
        }

        // Find the admission application by unique admission number and include department info
        var admission = await _context.AdmissionApplications
            .Include(a => a.Department)
            .FirstOrDefaultAsync(a => a.UniqueAdmissionNumber == uniqueAdmissionNumber.Trim());

        if (admission == null)
        {
            ViewBag.Message = "No admission found with this admission number.";
            return View();
        }

        // Show admission status in a different view
        return View("StatusResult", admission);
    }

    // Generate a PDF version of the empty admission form for download or printing
    public IActionResult EmptyPdfForm()
    {
        var model = new AdmissionFormViewModel();
        return new ViewAsPdf("Apply", model)
        {
            FileName = "AdmissionForm.pdf",
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
        };
    }

    // Provide a static PDF admission form for download from the server
    public IActionResult DownloadAdmissionForm()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/forms/Student_Admission_Form.pdf");
        var contentType = "application/pdf";
        var fileName = "Student_Admission_Form.pdf";

        // Check if file exists
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        // Read file bytes and return as a downloadable file
        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, contentType, fileName);
    }
}
