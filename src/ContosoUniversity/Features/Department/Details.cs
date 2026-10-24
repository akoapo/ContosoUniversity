namespace ContosoUniversity.Features.Department
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DAL;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class Details
    {
        public class Query : IRequest<Model>
        {
            public int Id { get; set; }
        }

        public class Model
        {
            public string Name { get; set; }

            public decimal Budget { get; set; }

            public DateTime StartDate { get; set; }

            public int DepartmentID { get; set; }

            [Display(Name = "Administrator")]
            public string AdministratorFullName { get; set; }

        }

        public class QueryHandler : IRequestHandler<Query, Model>
        {
            private readonly SchoolContext _context;

            public QueryHandler(SchoolContext context)
            {
                _context = context;
            }

            public async Task<Model> Handle(Query message, CancellationToken cancellationToken)
            {
                // Same projection as the original EF6 Database.SqlQuery<Model> call; the column list is
                // explicit (instead of d.*) because EF Core maps SqlQueryRaw results by property name.
                string query = @"
SELECT d.DepartmentID, d.Name, d.Budget, d.StartDate, p.LastName + ', ' + p.FirstName AS [AdministratorFullName]
FROM Department d
LEFT OUTER JOIN Person p ON d.InstructorID = p.ID
WHERE d.DepartmentID = {0}";
                Model department = await _context.Database
                    .SqlQueryRaw<Model>(query, message.Id)
                    .SingleOrDefaultAsync(cancellationToken);

                return department;
            }
        }
    }
}
