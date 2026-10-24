namespace ContosoUniversity.Infrastructure.DataAccess
{
    using System.Threading.Tasks;
    using ContosoUniversity.DAL;
    using Microsoft.AspNetCore.Mvc.Filters;

    /// <summary>
    /// Opens a transaction on the request-scoped SchoolContext before the action runs and
    /// commits (or rolls back, when the action threw) afterwards - the ASP.NET Core equivalent
    /// of the MVC5 ActionFilterAttribute that resolved the context from StructureMap.
    /// Register globally: options.Filters.Add&lt;MvcTransactionFilter&gt;();
    /// </summary>
    public class MvcTransactionFilter : IAsyncActionFilter
    {
        private readonly SchoolContext _context;

        public MvcTransactionFilter(SchoolContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Logger.Instance.Verbose("MvcTransactionFilter::OnActionExecuting");
            _context.BeginTransaction();

            var executedContext = await next();

            // Logger.Instance.Verbose("MvcTransactionFilter::OnActionExecuted");
            _context.CloseTransaction(executedContext.Exception);
        }
    }
}
