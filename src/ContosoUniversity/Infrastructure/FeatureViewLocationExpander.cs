namespace ContosoUniversity.Infrastructure
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc.Razor;

    /// <summary>
    /// Replaces the MVC5 <c>FeatureViewLocationRazorViewEngine</c>. Keeps the feature-folder
    /// view layout: a view/partial/layout named <c>{0}</c> rendered by controller <c>{1}</c>
    /// is looked up in <c>~/Features/{1}/</c> first and then in <c>~/Features/Shared/</c>.
    /// The .vbhtml entries are dropped (Razor for ASP.NET Core is C# only).
    /// </summary>
    public class FeatureViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // No request-specific values participate in the cache key: the formats below are static.
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            yield return "~/Features/{1}/{0}.cshtml";
            yield return "~/Features/Shared/{0}.cshtml";
        }
    }
}
