namespace ContosoUniversity.DAL
{
    using System;
    using System.Data;
    using ContosoUniversity.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    public class SchoolContext : DbContext
    {
        private IDbContextTransaction _currentTransaction;

        public SchoolContext(DbContextOptions<SchoolContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<OfficeAssignment> OfficeAssignments { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<CourseInstructor> CourseInstructors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // EF Core does not pluralize table names, but the EF6 model explicitly removed the
            // PluralizingTableNameConvention, so the singular table names are configured here to
            // keep the existing database schema.
            modelBuilder.Entity<Course>(b =>
            {
                b.ToTable("Course");
                b.HasKey(c => c.CourseID);
                b.Property(c => c.CourseID).ValueGeneratedNever();
                b.Property(c => c.Title).HasMaxLength(50);
                b.HasOne(c => c.Department)
                    .WithMany(d => d.Courses)
                    .HasForeignKey(c => c.DepartmentID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Department>(b =>
            {
                b.ToTable("Department");
                b.HasKey(d => d.DepartmentID);
                b.Property(d => d.Name).HasMaxLength(50);
                b.Property(d => d.Budget).HasColumnType("money");
                b.Property(d => d.RowVersion).IsRowVersion();
                b.HasOne(d => d.Administrator)
                    .WithMany()
                    .HasForeignKey(d => d.InstructorID)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                // EF6 Entity<Department>().MapToStoredProcedures() -> explicit EF Core
                // stored-procedure mapping against the DepartmentSP procedures.
                b.InsertUsingStoredProcedure("Department_Insert", sp => sp
                    .HasParameter(d => d.Name)
                    .HasParameter(d => d.Budget)
                    .HasParameter(d => d.StartDate)
                    .HasParameter(d => d.InstructorID)
                    .HasResultColumn(d => d.DepartmentID)
                    .HasResultColumn(d => d.RowVersion));

                b.UpdateUsingStoredProcedure("Department_Update", sp => sp
                    .HasOriginalValueParameter(d => d.DepartmentID, p => p.HasName("DepartmentID"))
                    .HasParameter(d => d.Name)
                    .HasParameter(d => d.Budget)
                    .HasParameter(d => d.StartDate)
                    .HasParameter(d => d.InstructorID)
                    .HasOriginalValueParameter(d => d.RowVersion, p => p.HasName("RowVersion_Original"))
                    .HasResultColumn(d => d.RowVersion));

                b.DeleteUsingStoredProcedure("Department_Delete", sp => sp
                    .HasOriginalValueParameter(d => d.DepartmentID, p => p.HasName("DepartmentID"))
                    .HasOriginalValueParameter(d => d.RowVersion, p => p.HasName("RowVersion_Original"))
                    .HasRowsAffectedReturnValue());
            });

            modelBuilder.Entity<Enrollment>(b =>
            {
                b.ToTable("Enrollment");
                b.HasKey(e => e.EnrollmentID);
                b.HasOne(e => e.Course)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(e => e.CourseID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasOne(e => e.Student)
                    .WithMany(s => s.Enrollments)
                    .HasForeignKey(e => e.StudentID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Table-per-hierarchy: Person is the base table, Student/Instructor are the
            // derived types. The 'Student' discriminator value is relied upon by the raw SQL
            // in Features/Home/About.cs.
            modelBuilder.Entity<Person>(b =>
            {
                b.ToTable("Person");
                b.HasKey(p => p.ID);
                b.HasDiscriminator<string>("Discriminator")
                    .HasValue<Student>("Student")
                    .HasValue<Instructor>("Instructor");
                b.Property("Discriminator").HasMaxLength(128).IsRequired();
            });

            // HireDate / EnrollmentDate live on the shared Person table; EF Core makes
            // derived-type columns nullable in a TPH table automatically, which matches the
            // EF6 Inheritance migration (both columns are nullable in the database).

            modelBuilder.Entity<OfficeAssignment>(b =>
            {
                b.ToTable("OfficeAssignment");
                b.HasKey(o => o.InstructorID);
                b.Property(o => o.InstructorID).ValueGeneratedNever();
                b.Property(o => o.Location).HasMaxLength(50);
                b.HasOne(o => o.Instructor)
                    .WithOne(i => i.OfficeAssignment)
                    .HasForeignKey<OfficeAssignment>(o => o.InstructorID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CourseInstructor>(b =>
            {
                b.ToTable("CourseInstructor");
                b.HasKey(ci => new { ci.CourseID, ci.InstructorID });
                b.HasOne(ci => ci.Course)
                    .WithMany(c => c.CourseInstructors)
                    .HasForeignKey(ci => ci.CourseID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasOne(ci => ci.Instructor)
                    .WithMany(i => i.CourseInstructors)
                    .HasForeignKey(ci => ci.InstructorID)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public void BeginTransaction()
        {
            try
            {
                if (_currentTransaction != null)
                {
                    return;
                }

                _currentTransaction = Database.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            catch (Exception)
            {
                // todo: log transaction exception
                throw;
            }
        }

        public void CloseTransaction()
        {
            CloseTransaction(exception: null);
        }

        public void CloseTransaction(Exception exception)
        {
            try
            {
                if (_currentTransaction != null && exception != null)
                {
                    // todo: log exception
                    _currentTransaction.Rollback();
                    return;
                }

                SaveChanges();

                if (_currentTransaction != null)
                {
                    _currentTransaction.Commit();
                }
            }
            catch (Exception)
            {
                // todo: log exception
                if (_currentTransaction != null && _currentTransaction.GetDbTransaction().Connection != null)
                {
                    _currentTransaction.Rollback();
                }

                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
    }
}
