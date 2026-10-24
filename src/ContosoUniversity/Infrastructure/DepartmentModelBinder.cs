namespace ContosoUniversity.Infrastructure
{
    using System.Threading.Tasks;
    using DAL;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Binds a route/query/form value holding a Department id to the tracked Department entity.
    /// </summary>
    public class DepartmentModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            var attemptedValue = valueProviderResult.FirstValue;

            if (string.IsNullOrWhiteSpace(attemptedValue) || !int.TryParse(attemptedValue, out var id))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return;
            }

            // SchoolContext is scoped: it must come from the request's service provider.
            var dbContext = bindingContext.HttpContext.RequestServices.GetRequiredService<SchoolContext>();

            var department = await dbContext.Departments.FindAsync(id);

            bindingContext.Result = ModelBindingResult.Success(department);
        }
    }
}
