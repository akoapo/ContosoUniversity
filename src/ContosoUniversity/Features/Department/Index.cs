namespace ContosoUniversity.Features.Department
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using DAL;
    using DelegateDecompiler.EntityFrameworkCore;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class Index
    {
        public class Query : IRequest<List<Model>>
        {
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

        public class QueryHandler : IRequestHandler<Query, List<Model>>
        {
            private readonly SchoolContext _context;
            private readonly IMapper _mapper;

            public QueryHandler(SchoolContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<List<Model>> Handle(Query message, CancellationToken cancellationToken)
            {
                return await _mapper.ProjectTo<Model>(_context.Departments)
                    .DecompileAsync()
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
