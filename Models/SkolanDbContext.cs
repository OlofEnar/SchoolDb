using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SchoolDB.Models;

public partial class SkolanDbContext : DbContext
{
    public SkolanDbContext()
    {
    }

    public SkolanDbContext(DbContextOptions<SkolanDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<StudContactDetail> StudContactDetails { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentAddress> StudentAddresses { get; set; }

    public virtual DbSet<StudentClass> StudentClasses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=OLOFMALEKIN7101\\SQLEXPRESS;Database=SkolanDB;Trusted_Connection=True;Encrypt=false");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(e => e.CourseCode)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.CourseName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FkempId).HasColumnName("FKEmpId");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmpId);

            entity.Property(e => e.EmployeeSsn)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("EmployeeSSN");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FkdepartmentId).HasColumnName("FKDepartmentId");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TeachingField)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Fkdepartment).WithMany(p => p.Employees)
                .HasForeignKey(d => d.FkdepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_Departments");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.Property(e => e.FkcourseId).HasColumnName("FKCourseId");
            entity.Property(e => e.FkstudentId).HasColumnName("FKStudentId");

            entity.HasOne(d => d.Fkcourse).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.FkcourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_Courses");

            entity.HasOne(d => d.Fkstudent).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.FkstudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_Students");
        });

        modelBuilder.Entity<StudContactDetail>(entity =>
        {
            entity.HasKey(e => e.StudContactId);

            entity.Property(e => e.FkstudentId).HasColumnName("FKStudentId");
            entity.Property(e => e.StudContactFirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StudContactLastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StudContactNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StudContactRole)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FkstudContactId).HasColumnName("FKStudContactId");
            entity.Property(e => e.FkstudentAddressId).HasColumnName("FKStudentAddressId");
            entity.Property(e => e.FkstudentClassId).HasColumnName("FKStudentClassId");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StudentSsn)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("StudentSSN");

            entity.HasOne(d => d.FkstudContact).WithMany(p => p.Students)
                .HasForeignKey(d => d.FkstudContactId)
                .HasConstraintName("FK_Students_Students");

            entity.HasOne(d => d.FkstudentAddress).WithMany(p => p.Students)
                .HasForeignKey(d => d.FkstudentAddressId)
                .HasConstraintName("FK_Students_StudentAddress");

            entity.HasOne(d => d.FkstudentClass).WithMany(p => p.Students)
                .HasForeignKey(d => d.FkstudentClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_StudentClass");
        });

        modelBuilder.Entity<StudentAddress>(entity =>
        {
            entity.HasKey(e => e.StudentAdressId);

            entity.ToTable("StudentAddress");

            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FkstudentId).HasColumnName("FKStudentId");
            entity.Property(e => e.PostCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Street1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Street 1");
            entity.Property(e => e.Street2)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Street 2");
        });

        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.ToTable("StudentClass");

            entity.Property(e => e.FkclassPrincipal).HasColumnName("FKClassPrincipal");
            entity.Property(e => e.StudentClassCode)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.StudentClassName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StudentClassSubName)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
