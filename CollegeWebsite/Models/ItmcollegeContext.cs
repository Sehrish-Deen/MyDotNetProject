using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CollegeWebsite.Models;

public partial class ItmcollegeContext : DbContext
{
    public ItmcollegeContext()
    {
    }

    public ItmcollegeContext(DbContextOptions<ItmcollegeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<AdmissionApplication> AdmissionApplications { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Faculty> Faculties { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.ToTable("Admin");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsFixedLength()
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsFixedLength()
                .HasColumnName("password");
        });

        modelBuilder.Entity<AdmissionApplication>(entity =>
        {
            entity.HasKey(e => e.AdmissionId).HasName("PK__Admissio__C97EEFA2C8961151");

            entity.HasIndex(e => e.UniqueAdmissionNumber, "UQ__Admissio__32AFCB4EC2F24E65").IsUnique();

            entity.Property(e => e.AdmissionId).HasColumnName("AdmissionID");
            entity.Property(e => e.AdmissionFor).HasMaxLength(100);
            entity.Property(e => e.AdmissionStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Waiting");
            entity.Property(e => e.ApplicationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.BoardOrUniversity).HasMaxLength(255);
            entity.Property(e => e.ClassObtained).HasMaxLength(100);
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.EnrollmentNumber).HasMaxLength(100);
            entity.Property(e => e.ExamCenter).HasMaxLength(255);
            entity.Property(e => e.FatherName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.MarksOutOf).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MarksSecured).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PermanentAddress).HasMaxLength(255);
            entity.Property(e => e.ResidentialAddress).HasMaxLength(255);
            entity.Property(e => e.Stream).HasMaxLength(100);
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.StudentName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UniqueAdmissionNumber).HasMaxLength(50);

            entity.HasOne(d => d.Department).WithMany(p => p.AdmissionApplications)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Admission__Depar__7E37BEF6");

            entity.HasOne(d => d.Student).WithMany(p => p.AdmissionApplications)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Admission__Stude__7D439ABD");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D718718E5CD60");

            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CourseName).HasMaxLength(100);
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.ImagePath).HasMaxLength(255);

            entity.HasOne(d => d.Department).WithMany(p => p.Courses)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Courses__Departm__3B75D760");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BCD2EE5C4EE");

            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.ImagePath).HasMaxLength(255);
        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.FacultyId).HasName("PK__Faculty__306F636E647EE1D1");

            entity.ToTable("Faculty");

            entity.Property(e => e.FacultyId).HasColumnName("FacultyID");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FacultyName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Photo).HasMaxLength(255);
            entity.Property(e => e.Qualification).HasMaxLength(150);

            entity.HasOne(d => d.Department).WithMany(p => p.Faculties)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__Faculty__Departm__5CD6CB2B");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDF672548D4A");

            entity.ToTable("Feedback");

            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Subject).HasMaxLength(200);
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B9982A3D908");

            entity.HasIndex(e => e.Email, "UQ__Students__A9D1053494D95F3E").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(200);
            entity.Property(e => e.StudentName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
