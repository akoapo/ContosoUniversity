namespace ContosoUniversity.Infrastructure
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Models;

    public class InstructorModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            return typeof(Instructor).IsAssignableFrom(context.Metadata.ModelType)
                ? new InstructorModelBinder()
                : null;
        }
    }
}
