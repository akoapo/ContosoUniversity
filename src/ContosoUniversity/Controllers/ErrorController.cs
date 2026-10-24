namespace ContosoUniversity.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Target of app.UseExceptionHandler("/Error"); renders Features/Shared/Error.cshtml
    /// (resolved by FeatureViewLocationExpander's "~/Features/Shared/{0}.cshtml" format).
    /// Replaces the MVC5 global HandleErrorAttribute registered in FilterConfig.
    /// This is a conventional top-level controller rather than a feature-folder
    /// <c>UiController</c>: the error endpoint belongs to no business feature, and the
    /// literal <c>ErrorController</c> name keeps the "/Error" wire statically resolvable
    /// instead of depending on FeatureControllerNameConvention's runtime renaming.
    /// </summary>
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View("Error");
        }
    }
}
