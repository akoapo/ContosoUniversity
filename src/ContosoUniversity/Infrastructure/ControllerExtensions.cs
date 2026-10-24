namespace ContosoUniversity.Infrastructure
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    /// <summary>
    /// The POST actions answer AJAX form submits, so instead of a 302 they return a small JSON
    /// document of the shape <c>{ "redirect": "/Student/Index" }</c>. The layout's submit handler
    /// reads that property and navigates the browser, so neither the payload shape nor the
    /// "application/json" content type may change.
    /// </summary>
    public static class ControllerExtensions
    {
        public static IActionResult RedirectToActionJson<TController>(this Controller controller,
            Expression<Action<TController>> action)
            where TController : Controller
        {
            return controller.JsonNet(new
            {
                redirect = controller.Url.Action(action)
            });
        }

        public static IActionResult RedirectToActionJson<TController>(this TController controller,
            Expression<Action<TController>> action)
            where TController : Controller
        {
            return controller.JsonNet(new
            {
                redirect = controller.Url.Action(action)
            });
        }

        public static IActionResult RedirectToActionJson<TController>(this TController controller, string action)
            where TController : Controller
        {
            return controller.JsonNet(new
            {
                redirect = controller.Url.Action(action)
            });
        }

        public static ContentResult JsonNet(this Controller controller, object model)
        {
            var serialized = JsonConvert.SerializeObject(model, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return new ContentResult
            {
                Content = serialized,
                ContentType = "application/json"
            };
        }
    }
}
