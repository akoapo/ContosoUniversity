namespace ContosoUniversity.Infrastructure
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Newtonsoft.Json;

    /// <summary>
    /// Short-circuits an action when model binding/FluentValidation produced errors.
    /// GET requests get a bare 400; every other verb gets a 400 whose body is the
    /// Newtonsoft-serialized ModelState — the layout's AJAX error handler parses that payload
    /// to highlight the offending fields, so the shape must stay identical.
    /// </summary>
    public class ValidatorActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
            {
                return;
            }

            if (HttpMethods.IsGet(context.HttpContext.Request.Method))
            {
                context.Result = new BadRequestResult();
            }
            else
            {
                var content = JsonConvert.SerializeObject(context.ModelState,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                context.HttpContext.Response.StatusCode = 400;
                context.Result = new ContentResult
                {
                    Content = content,
                    ContentType = "application/json",
                    StatusCode = 400
                };
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
