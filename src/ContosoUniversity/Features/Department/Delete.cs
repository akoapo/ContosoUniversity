namespace ContosoUniversity.Features.Department
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using DAL;
    using DelegateDecompiler.EntityFrameworkCore;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class Delete
    {
        public class Query : IRequest<Command>
        {
            public int Id { get; set; }
        }

        public class Command : IRequest
        {
            public string Name { get; set; }

            public decimal Budget { get; set; }

            public DateTime StartDate { get; set; }

            public int DepartmentID { get; set; }

            [Display(Name = "Administrator")]
            public string AdministratorFullName { get; set; }

            public byte[] RowVersion { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            private readonly SchoolContext _db;
            private readonly IMapper _mapper;

            public QueryHandler(SchoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Command> Handle(Query message, CancellationToken cancellationToken)
            {
                var department = await _mapper
                    .ProjectTo<Command>(_db.Departments.Where(d => d.DepartmentID == message.Id))
                    .DecompileAsync()
                    .SingleOrDefaultAsync(cancellationToken);

                return department;
            }
        }

        public class CommandHandler : IRequestHandler<Command>
        {
            private readonly SchoolContext _db;

            public CommandHandler(SchoolContext db)
            {
                _db = db;
            }

            public async Task Handle(Command message, CancellationToken cancellationToken)
            {
                var department = await _db.Departments.FindAsync(new object[] { message.DepartmentID }, cancellationToken);

                _db.Departments.Remove(department);
            }
        }
    }
}
