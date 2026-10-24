namespace ContosoUniversity.Features.Course
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using DAL;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class Index
    {
        public class Query : IRequest<Result>
        {
            public Department SelectedDepartment { get; set; }
        }

        public class Result
        {
            public Department SelectedDepartment { get; set; }
            public List<Course> Courses { get; set; }

            public class Course
            {
                public int CourseID { get; set; }
                public string Title { get; set; }
                public int Credits { get; set; }
                public string DepartmentName { get; set; }
            }
        }

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly SchoolContext _db;
            private readonly IMapper _mapper;

            public Handler(SchoolContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result> Handle(Query message, CancellationToken cancellationToken)
            {
                int? departmentID = message.SelectedDepartment == null
                    ? (int?)null
                    : message.SelectedDepartment.DepartmentID;

                var query = _db.Courses
                    .Where(c => !departmentID.HasValue || c.DepartmentID == departmentID)
                    .OrderBy(d => d.CourseID);

                var courses = await _mapper.ProjectTo<Result.Course>(query)
                    .ToListAsync(cancellationToken);

                return new Result
                {
                    Courses = courses,
                    SelectedDepartment = message.SelectedDepartment
                };
            }
        }
    }
}
