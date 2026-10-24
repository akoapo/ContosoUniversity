namespace ContosoUniversity.Features.Department
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using DAL;
    using DelegateDecompiler.EntityFrameworkCore;
    using FluentValidation;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class Edit
    {
        public class Query : IRequest<Command>
        {
            public int Id { get; set; }
        }

        public class Command : IRequest
        {
            public string Name { get; set; }

            public decimal? Budget { get; set; }

            public DateTime? StartDate { get; set; }

            public Instructor Administrator { get; set; }
            public int DepartmentID { get; set; }
            public byte[] RowVersion { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(m => m.Name).NotNull().Length(3, 50);
                RuleFor(m => m.Budget).NotNull();
                RuleFor(m => m.StartDate).NotNull();
                RuleFor(m => m.Administrator).NotNull();
            }
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
            private readonly IMapper _mapper;

            public CommandHandler(SchoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task Handle(Command message, CancellationToken cancellationToken)
            {
                var dept = await _db.Departments.FindAsync(new object[] { message.DepartmentID }, cancellationToken);

                // Maps RowVersion from the posted command onto the tracked entity so that the
                // optimistic-concurrency check still fires on SaveChanges.
                _mapper.Map(message, dept);
            }
        }
    }

}
