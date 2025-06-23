using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollegeWebsite.Models;
using System.Threading.Tasks;
using System.Linq;
using CollegeWebsite.Controllers;

public class AdminController : BaseController
{
    private readonly ItmcollegeContext _context;

    public AdminController(ItmcollegeContext context)
    {
        _context = context;
    }

    // GET: Admin/AdmissionApplications
    // Fetch all admission applications including their related departments,
    // ordered by application date in descending order, then pass to the view
    public async Task<IActionResult> AdmissionApplications()
    {
        var applications = await _context.AdmissionApplications
            .Include(a => a.Department)  // Include Department navigation property
            .OrderByDescending(a => a.ApplicationDate)  // Sort newest applications first
            .ToListAsync();

        return View(applications);  // Return data to the view
    }

    // GET: Admin/UpdateStatus/5
    // Load a specific admission application by ID to show the update form
    public async Task<IActionResult> UpdateStatus(int? id)
    {
        if (id == null) return NotFound();  // Return 404 if no ID is provided

        var application = await _context.AdmissionApplications.FindAsync(id);

        if (application == null) return NotFound();  // Return 404 if no application found

        return View(application);  // Pass the application to the view for editing
    }

    // POST: Admin/UpdateStatus/5
    // Receive the updated admission status and save changes to the database
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string admissionStatus)
    {
        var application = await _context.AdmissionApplications.FindAsync(id);
        if (application == null) return NotFound();  // Return 404 if application not found

        application.AdmissionStatus = admissionStatus;  // Update status field
        _context.Update(application);  // Mark entity as modified
        await _context.SaveChangesAsync();  // Save changes to DB

        TempData["Message"] = "Admission status updated successfully.";  // Set success message

        return RedirectToAction(nameof(AdmissionApplications));  // Redirect to list view
    }
}
