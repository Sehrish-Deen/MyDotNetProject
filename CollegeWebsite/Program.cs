using CollegeWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace CollegeWebsite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create a WebApplicationBuilder instance to configure services and the app
            var builder = WebApplication.CreateBuilder(args);

            // Add MVC controllers with views service for handling web requests and rendering views
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllersWithViews(); // Duplicate call; can be removed

            // Build a temporary service provider to access configuration settings
            var provider = builder.Services.BuildServiceProvider();
            var config = provider.GetRequiredService<IConfiguration>();

            // Register the database context service and configure it to use SQL Server
            // The connection string named "dbcs" is read from appsettings.json or environment
            builder.Services.AddDbContext<ItmcollegeContext>(item =>
                item.UseSqlServer(config.GetConnectionString("dbcs")));

            // Enable session state to store user data across multiple requests
            builder.Services.AddSession();

            // Build the app instance after all services have been configured
            var app = builder.Build();

            // Setup Rotativa for PDF generation, specifying the path to the web root and Rotativa files
            Rotativa.AspNetCore.RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

            // Configure middleware in the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                // Use custom error handler page in production environment
                app.UseExceptionHandler("/Home/Error");

                // Use HTTP Strict Transport Security (HSTS) for security
                app.UseHsts();
            }

            // Enable routing middleware to map incoming requests to endpoints/controllers
            app.UseRouting();

            // Enable session middleware (must be before authentication/authorization)
            app.UseSession();

            // Redirect HTTP requests to HTTPS for security
            app.UseHttpsRedirection();

            // Serve static files (css, js, images) from wwwroot folder
            app.UseStaticFiles();

            // Enable authorization middleware (authorization policies can be added if needed)
            app.UseAuthorization();

            // Configure the default route pattern for MVC controllers
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Run the application and start listening for HTTP requests
            app.Run();
        }
    }
}
