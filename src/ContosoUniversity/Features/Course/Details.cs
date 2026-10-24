namespace ContosoUniversity.Features.Course
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using DAL;
    using FluentValidation;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class Details
    {
        public class Query : IRequest<Model>
        {
            public int? Id { get; set; }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(m => m.Id).NotNull();
            }
        }

        public class Model
        {
            public int CourseID { get; set; }
            public string Title { get; set; }
            public int Credits { get; set; }
            public string DepartmentName { get; set; }
        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly SchoolContext _db;
            private readonly IMapper _mapper;

            public Handler(SchoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Model> Handle(Query message, CancellationToken cancellationToken)
            {
                return await _mapper
                    .ProjectTo<Model>(_db.Courses.Where(i => i.CourseID == message.Id))
                    .SingleOrDefaultAsync(cancellationToken);
            }
        }
    }
}
