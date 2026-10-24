namespace ContosoUniversity.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq.Expressions;
    using System.Text.Encodings.Web;
    using HtmlTags;
    using HtmlTags.Conventions;
    using HtmlTags.Conventions.Elements;
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.DependencyInjection;

    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Emits the RequireJS bootstrap for the per-view module <c>~/Features/{controller}/{view}.js</c>,
        /// but only when that file actually exists. The MVC5 version used
        /// <c>VirtualPathUtility.ToAbsolute</c> + <c>HttpContext.Server.MapPath</c>; those are replaced
        /// by <see cref="IUrlHelper.Content"/> and the content-root file provider of
        /// <see cref="IWebHostEnvironment"/> (the Features folder lives under the content root,
        /// not under wwwroot).
        /// </summary>
        public static IHtmlContent RequireJs(this IHtmlHelper helper)
        {
            var services = helper.ViewContext.HttpContext.RequestServices;
            var environment = services.GetRequiredService<IWebHostEnvironment>();
            var urlHelper = services.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(helper.ViewContext);

            var controllerName = helper.ViewContext.RouteData.Values["controller"]?.ToString();
            var viewName = Path.GetFileNameWithoutExtension(
                helper.ViewContext.ExecutingFilePath ?? helper.ViewContext.View?.Path ?? string.Empty);

            if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(viewName))
            {
                return HtmlString.Empty;
            }

            // Asset note: the require.js library moved with the rest of the static content from
            // ~/Content/js/lib to wwwroot/js/lib.
            var requirePath = urlHelper.Content("~/js/lib/require-2.1.18.js");

            var virtualPath = string.Format("~/Features/{0}/{1}.js", controllerName, viewName);
            var relativePath = string.Format("Features/{0}/{1}.js", controllerName, viewName);

            if (!environment.ContentRootFileProvider.GetFileInfo(relativePath).Exists)
            {
                return HtmlString.Empty;
            }

            var scriptTag = string.Format("<script data-main='{0}' src='{1}'></script>",
                urlHelper.Content(virtualPath), requirePath);

            return new HtmlString(scriptTag);
        }

        public static HtmlTag Input<T>(this IHtmlHelper<T> helper, Expression<Func<T, object>> expression)
            where T : class
        {
            var generator = GetGenerator(helper, helper.ViewData.Model);
            return generator.InputFor(expression);
        }

        public static HtmlTag Label<T>(this IHtmlHelper<T> helper, Expression<Func<T, object>> expression)
            where T : class
        {
            var generator = GetGenerator(helper, helper.ViewData.Model);
            return generator.LabelFor(expression);
        }

        public static HtmlTag Display<T>(this IHtmlHelper<T> helper, Expression<Func<T, object>> expression)
            where T : class
        {
            var generator = GetGenerator(helper, helper.ViewData.Model);
            return generator.DisplayFor(expression);
        }

        public static HtmlTag DisplayLabel<T>(this IHtmlHelper<T> helper, Expression<Func<T, object>> expression)
            where T : class
        {
            var generator = GetGenerator(helper, helper.ViewData.Model);
            return generator.TagFor(expression, "DisplayLabels");
        }

        /// <summary>
        /// Header-row variant: the view's model is a list, but the label belongs to the item type.
        /// </summary>
        public static HtmlTag DisplayLabel<T>(this IHtmlHelper<IList<T>> helper, Expression<Func<T, object>> expression)
            where T : class
        {
            var generator = GetGenerator(helper, default(T));
            return generator.TagFor(expression, "DisplayLabels");
        }

        /// <summary>
        /// <c>DisplayNameFor</c> for a list-typed view model: resolves the display name of a
        /// property of the item type. MVC5 did this by re-creating an
        /// <c>HtmlHelper&lt;IEnumerable&lt;TModel&gt;&gt;</c>; in ASP.NET Core the metadata provider
        /// answers the same question directly.
        /// </summary>
        public static IHtmlContent DisplayNameFor<TModel, TValue>(this IHtmlHelper<IList<TModel>> html,
            Expression<Func<TModel, TValue>> expression)
        {
            var metadataProvider = html.ViewContext.HttpContext.RequestServices
                .GetRequiredService<IModelMetadataProvider>();

            var propertyName = GetPropertyName(expression);

            var displayName = propertyName;

            if (propertyName != null)
            {
                var metadata = metadataProvider.GetMetadataForProperties(typeof(TModel));

                foreach (var property in metadata)
                {
                    if (property.PropertyName == propertyName)
                    {
                        displayName = property.GetDisplayName();
                        break;
                    }
                }
            }

            return new HtmlString(HtmlEncoder.Default.Encode(displayName ?? string.Empty));
        }

        public static HtmlTag InputBlock<T>(this IHtmlHelper<T> helper,
            Expression<Func<T, object>> expression,
            Action<HtmlTag> inputModifier = null) where T : class
        {
            inputModifier = inputModifier ?? (_ => { });

            var divTag = new HtmlTag("div");
            divTag.AddClass("col-md-10");

            var inputTag = helper.Input(expression);
            inputModifier(inputTag);

            divTag.Append(inputTag);

            return divTag;
        }

        public static HtmlTag FormBlock<T>(this IHtmlHelper<T> helper,
            Expression<Func<T, object>> expression,
            Action<HtmlTag> labelModifier = null,
            Action<HtmlTag> inputBlockModifier = null,
            Action<HtmlTag> inputModifier = null
            ) where T : class
        {
            labelModifier = labelModifier ?? (_ => { });
            inputBlockModifier = inputBlockModifier ?? (_ => { });

            var divTag = new HtmlTag("div");
            divTag.AddClass("form-group");

            var labelTag = helper.Label(expression);
            labelModifier(labelTag);

            var inputBlockTag = helper.InputBlock(
                expression,
                inputModifier);
            inputBlockModifier(inputBlockTag);

            divTag.Append(labelTag);
            divTag.Append(inputBlockTag);

            return divTag;
        }

        public static HtmlTag ValidationDiv(this IHtmlHelper helper)
        {
            return new HtmlTag("div")
                .Id("validationSummary")
                .AddClass("alert")
                .AddClass("alert-danger")
                .AddClass("hidden");
        }

        public static HtmlTag Link<TController>(this IHtmlHelper helper, Expression<Action<TController>> action)
            where TController : Controller
        {
            var linkText = ((MethodCallExpression)action.Body).Method.Name;

            return helper.Link(action, linkText);
        }

        public static HtmlTag Link<TController>(this IHtmlHelper helper, Expression<Action<TController>> action,
            string linkText) where TController : Controller
        {
            var urlHelper = helper.ViewContext.HttpContext.RequestServices
                .GetRequiredService<IUrlHelperFactory>()
                .GetUrlHelper(helper.ViewContext);

            var url = LinkBuilder.BuildUrlFromExpression(urlHelper, action);

            return new HtmlTag("a", t =>
            {
                t.Text(linkText);
                t.Attr("href", url);
            });
        }

        /// <summary>
        /// The element generator for the current request. The convention library is a singleton
        /// resolved from the request services, and the request services double as the service
        /// locator handed to HtmlTags — that is how <c>ElementRequest.Get&lt;SchoolContext&gt;()</c>
        /// in the entity select builders gets the request-scoped DbContext.
        /// </summary>
        private static IElementGenerator<T> GetGenerator<T>(IHtmlHelper helper, T model) where T : class
        {
            var services = helper.ViewContext.HttpContext.RequestServices;
            var library = services.GetRequiredService<HtmlConventionLibrary>();

            return ElementGenerator<T>.For(library, type => services.GetService(type), model);
        }

        private static string GetPropertyName(LambdaExpression expression)
        {
            var body = expression.Body;

            if (body is UnaryExpression unary)
            {
                body = unary.Operand;
            }

            return (body as MemberExpression)?.Member.Name;
        }
    }
}
