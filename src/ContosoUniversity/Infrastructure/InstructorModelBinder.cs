namespace ContosoUniversity.Infrastructure
{
    using System.Threading.Tasks;
    using DAL;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Binds a route/query/form value holding an Instructor id to the tracked Instructor entity.
    /// </summary>
    public class InstructorModelBinder : IModelBinder
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

            var instructor = await dbContext.Instructors.FindAsync(id);

            bindingContext.Result = ModelBindingResult.Success(instructor);
        }
    }
}
