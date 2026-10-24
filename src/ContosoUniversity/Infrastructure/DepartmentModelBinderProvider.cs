namespace ContosoUniversity.Infrastructure
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Models;

    public class DepartmentModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            return typeof(Department).IsAssignableFrom(context.Metadata.ModelType)
                ? new DepartmentModelBinder()
                : null;
        }
    }
}
