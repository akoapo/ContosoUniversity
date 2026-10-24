namespace ContosoUniversity.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ContosoUniversity.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// EF Core replacement for the EF6 migrations seeder (<c>Migrations/Configuration.Seed</c>),
    /// which was the seed that actually ran in the original application. (The EF6
    /// <c>SchoolInitializer</c> / <c>DropCreateDatabaseIfModelChanges</c> path was dead code —
    /// both <c>Database.SetInitializer</c> and the <c>&lt;databaseInitializer&gt;</c> config
    /// element were commented out — so its data is deliberately NOT used here.)
    ///
    /// EF6 <c>AddOrUpdate(p =&gt; p.Key, ...)</c> has no EF Core equivalent; the per-table
    /// "insert only when empty" guards below are the EF Core substitute and make the routine
    /// safe to run repeatedly.
    ///
    /// Insertion order (students, then instructors, then departments, courses, office
    /// assignments, course instructors, enrollments) is preserved from the original because
    /// Student and Instructor share the TPH <c>Person</c> table and therefore share an
    /// identity sequence.
    ///
    /// Call from Program.cs:
    ///     using (var scope = app.Services.CreateScope())
    ///         await DbSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService&lt;SchoolContext&gt;());
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(SchoolContext context, CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await context.Database.MigrateAsync(cancellationToken);

            var students = await SeedStudentsAsync(context, cancellationToken);
            var instructors = await SeedInstructorsAsync(context, cancellationToken);
            var departments = await SeedDepartmentsAsync(context, instructors, cancellationToken);
            var courses = await SeedCoursesAsync(context, departments, cancellationToken);
            await SeedOfficeAssignmentsAsync(context, instructors, cancellationToken);
            await SeedCourseInstructorsAsync(context, courses, instructors, cancellationToken);
            await SeedEnrollmentsAsync(context, students, courses, cancellationToken);
        }

        private static async Task<List<Student>> SeedStudentsAsync(SchoolContext context,
            CancellationToken cancellationToken)
        {
            if (await context.Students.AnyAsync(cancellationToken))
            {
                return await context.Students.OrderBy(s => s.ID).ToListAsync(cancellationToken);
            }

            var students = new List<Student>
            {
                new Student { FirstMidName = "Carson",   LastName = "Alexander",
                    EnrollmentDate = DateTime.Parse("2010-09-01") },
                new Student { FirstMidName = "Meredith", LastName = "Alonso",
                    EnrollmentDate = DateTime.Parse("2012-09-01") },
                new Student { FirstMidName = "Arturo",   LastName = "Anand",
                    EnrollmentDate = DateTime.Parse("2013-09-01") },
                new Student { FirstMidName = "Gytis",    LastName = "Barzdukas",
                    EnrollmentDate = DateTime.Parse("2012-09-01") },
                new Student { FirstMidName = "Yan",      LastName = "Li",
                    EnrollmentDate = DateTime.Parse("2012-09-01") },
                new Student { FirstMidName = "Peggy",    LastName = "Justice",
                    EnrollmentDate = DateTime.Parse("2011-09-01") },
                new Student { FirstMidName = "Laura",    LastName = "Norman",
                    EnrollmentDate = DateTime.Parse("2013-09-01") },
                new Student { FirstMidName = "Nino",     LastName = "Olivetto",
                    EnrollmentDate = DateTime.Parse("2005-09-01") }
            };

            students.ForEach(s => context.Students.Add(s));
            await context.SaveChangesAsync(cancellationToken);

            return students;
        }

        private static async Task<List<Instructor>> SeedInstructorsAsync(SchoolContext context,
            CancellationToken cancellationToken)
        {
            if (await context.Instructors.AnyAsync(cancellationToken))
            {
                return await context.Instructors.OrderBy(i => i.ID).ToListAsync(cancellationToken);
            }

            var instructors = new List<Instructor>
            {
                new Instructor { FirstMidName = "Kim",     LastName = "Abercrombie",
                    HireDate = DateTime.Parse("1995-03-11") },
                new Instructor { FirstMidName = "Fadi",    LastName = "Fakhouri",
                    HireDate = DateTime.Parse("2002-07-06") },
                new Instructor { FirstMidName = "Roger",   LastName = "Harui",
                    HireDate = DateTime.Parse("1998-07-01") },
                new Instructor { FirstMidName = "Candace", LastName = "Kapoor",
                    HireDate = DateTime.Parse("2001-01-15") },
                new Instructor { FirstMidName = "Roger",   LastName = "Zheng",
                    HireDate = DateTime.Parse("2004-02-12") }
            };

            instructors.ForEach(s => context.Instructors.Add(s));
            await context.SaveChangesAsync(cancellationToken);

            return instructors;
        }

        private static async Task<List<Department>> SeedDepartmentsAsync(SchoolContext context,
            List<Instructor> instructors, CancellationToken cancellationToken)
        {
            if (await context.Departments.AnyAsync(cancellationToken))
            {
                return await context.Departments.OrderBy(d => d.DepartmentID).ToListAsync(cancellationToken);
            }

            var departments = new List<Department>
            {
                new Department { Name = "English",     Budget = 350000,
                    StartDate = DateTime.Parse("2007-09-01"),
                    InstructorID = instructors.Single(i => i.LastName == "Abercrombie").ID },
                new Department { Name = "Mathematics", Budget = 100000,
                    StartDate = DateTime.Parse("2007-09-01"),
                    InstructorID = instructors.Single(i => i.LastName == "Fakhouri").ID },
                new Department { Name = "Engineering", Budget = 350000,
                    StartDate = DateTime.Parse("2007-09-01"),
                    InstructorID = instructors.Single(i => i.LastName == "Harui").ID },
                new Department { Name = "Economics",   Budget = 100000,
                    StartDate = DateTime.Parse("2007-09-01"),
                    InstructorID = instructors.Single(i => i.LastName == "Kapoor").ID }
            };

            departments.ForEach(s => context.Departments.Add(s));
            await context.SaveChangesAsync(cancellationToken);

            return departments;
        }

        private static async Task<List<Course>> SeedCoursesAsync(SchoolContext context,
            List<Department> departments, CancellationToken cancellationToken)
        {
            if (await context.Courses.AnyAsync(cancellationToken))
            {
                return await context.Courses.OrderBy(c => c.CourseID).ToListAsync(cancellationToken);
            }

            // CourseID is mapped ValueGeneratedNever, so the explicit course numbers below are
            // inserted as-is (no IDENTITY_INSERT needed).
            var courses = new List<Course>
            {
                new Course { CourseID = 1050, Title = "Chemistry",      Credits = 3,
                    DepartmentID = departments.Single(s => s.Name == "Engineering").DepartmentID },
                new Course { CourseID = 4022, Title = "Microeconomics", Credits = 3,
                    DepartmentID = departments.Single(s => s.Name == "Economics").DepartmentID },
                new Course { CourseID = 4041, Title = "Macroeconomics", Credits = 3,
                    DepartmentID = departments.Single(s => s.Name == "Economics").DepartmentID },
                new Course { CourseID = 1045, Title = "Calculus",       Credits = 4,
                    DepartmentID = departments.Single(s => s.Name == "Mathematics").DepartmentID },
                new Course { CourseID = 3141, Title = "Trigonometry",   Credits = 4,
                    DepartmentID = departments.Single(s => s.Name == "Mathematics").DepartmentID },
                new Course { CourseID = 2021, Title = "Composition",    Credits = 3,
                    DepartmentID = departments.Single(s => s.Name == "English").DepartmentID },
                new Course { CourseID = 2042, Title = "Literature",     Credits = 4,
                    DepartmentID = departments.Single(s => s.Name == "English").DepartmentID }
            };

            courses.ForEach(s => context.Courses.Add(s));
            await context.SaveChangesAsync(cancellationToken);

            return courses;
        }

        private static async Task SeedOfficeAssignmentsAsync(SchoolContext context,
            List<Instructor> instructors, CancellationToken cancellationToken)
        {
            if (await context.OfficeAssignments.AnyAsync(cancellationToken))
            {
                return;
            }

            var officeAssignments = new List<OfficeAssignment>
            {
                new OfficeAssignment {
                    InstructorID = instructors.Single(i => i.LastName == "Fakhouri").ID,
                    Location = "Smith 17" },
                new OfficeAssignment {
                    InstructorID = instructors.Single(i => i.LastName == "Harui").ID,
                    Location = "Gowan 27" },
                new OfficeAssignment {
                    InstructorID = instructors.Single(i => i.LastName == "Kapoor").ID,
                    Location = "Thompson 304" }
            };

            officeAssignments.ForEach(s => context.OfficeAssignments.Add(s));
            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedCourseInstructorsAsync(SchoolContext context, List<Course> courses,
            List<Instructor> instructors, CancellationToken cancellationToken)
        {
            if (await context.CourseInstructors.AnyAsync(cancellationToken))
            {
                return;
            }

            // The original Configuration.Seed's AddOrUpdateInstructor(course, instructor) pairs.
            var assignments = new[]
            {
                new { Course = "Chemistry",      Instructor = "Kapoor" },
                new { Course = "Chemistry",      Instructor = "Harui" },
                new { Course = "Microeconomics", Instructor = "Zheng" },
                new { Course = "Macroeconomics", Instructor = "Zheng" },
                new { Course = "Calculus",       Instructor = "Fakhouri" },
                new { Course = "Trigonometry",   Instructor = "Harui" },
                new { Course = "Composition",    Instructor = "Abercrombie" },
                new { Course = "Literature",     Instructor = "Abercrombie" }
            };

            foreach (var assignment in assignments)
            {
                context.CourseInstructors.Add(new CourseInstructor
                {
                    CourseID = courses.Single(c => c.Title == assignment.Course).CourseID,
                    InstructorID = instructors.Single(i => i.LastName == assignment.Instructor).ID
                });
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedEnrollmentsAsync(SchoolContext context, List<Student> students,
            List<Course> courses, CancellationToken cancellationToken)
        {
            var enrollments = new List<Enrollment>
            {
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Alexander").ID,
                    CourseID = courses.Single(c => c.Title == "Chemistry").CourseID,
                    Grade = Grade.A
                },
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Alexander").ID,
                    CourseID = courses.Single(c => c.Title == "Microeconomics").CourseID,
                    Grade = Grade.C
                },
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Alexander").ID,
                    CourseID = courses.Single(c => c.Title == "Macroeconomics").CourseID,
                    Grade = Grade.B
                },
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Alonso").ID,
                    CourseID = courses.Single(c => c.Title == "Calculus").CourseID,
                    Grade = Grade.B
                },
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Alonso").ID,
                    CourseID = courses.Single(c => c.Title == "Trigonometry").CourseID,
                    Grade = Grade.B
                },
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Alonso").ID,
                    CourseID = courses.Single(c => c.Title == "Composition").CourseID,
                    Grade = Grade.B
                },
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Anand").ID,
                    CourseID = courses.Single(c => c.Title == "Chemistry").CourseID
                },
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Anand").ID,
                    CourseID = courses.Single(c => c.Title == "Microeconomics").CourseID,
                    Grade = Grade.B
                },
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Barzdukas").ID,
                    CourseID = courses.Single(c => c.Title == "Chemistry").CourseID,
                    Grade = Grade.B
                },
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Li").ID,
                    CourseID = courses.Single(c => c.Title == "Composition").CourseID,
                    Grade = Grade.B
                },
                new Enrollment {
                    StudentID = students.Single(s => s.LastName == "Justice").ID,
                    CourseID = courses.Single(c => c.Title == "Literature").CourseID,
                    Grade = Grade.B
                }
            };

            // The original seeder checked each enrollment individually rather than the whole
            // table, so that behavior is preserved here.
            foreach (var enrollment in enrollments)
            {
                var exists = await context.Enrollments.AnyAsync(
                    e => e.StudentID == enrollment.StudentID && e.CourseID == enrollment.CourseID,
                    cancellationToken);

                if (!exists)
                {
                    context.Enrollments.Add(enrollment);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
