namespace ContosoUniversity.Infrastructure.Mapping
{
    using System.Linq;
    using AutoMapper;
    using DelegateDecompiler;
    using X.PagedList;
    using X.PagedList.Extensions;

    public static class MapperExtensions
    {
        /// <summary>
        /// Projects a queryable onto <typeparamref name="TDestination"/> with AutoMapper, decompiles
        /// any <c>[Computed]</c> members so they translate to SQL, and pages the result.
        /// The static AutoMapper <c>Mapper</c> facade is gone, so the mapper is passed in.
        /// </summary>
        public static IPagedList<TDestination> ProjectToPagedList<TDestination>(this IQueryable queryable,
            IMapper mapper, int pageNumber, int pageSize)
        {
            return mapper.ProjectTo<TDestination>(queryable).Decompile().ToPagedList(pageNumber, pageSize);
        }
    }
}
