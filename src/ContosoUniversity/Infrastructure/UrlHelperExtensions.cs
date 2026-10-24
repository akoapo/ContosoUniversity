namespace ContosoUniversity.Infrastructure
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Strongly typed <c>Url.Action(c =&gt; c.Edit(...))</c> on <see cref="IUrlHelper"/>.
    /// Named <c>UrlHelperExpressionExtensions</c> so it cannot be confused with the framework's
    /// own <c>Microsoft.AspNetCore.Mvc.UrlHelperExtensions</c>.
    /// </summary>
    public static class UrlHelperExpressionExtensions
    {
        public static string Action<TController>(this IUrlHelper helper, Expression<Action<TController>> action)
            where TController : Controller
        {
            return LinkBuilder.BuildUrlFromExpression(helper, action);
        }
    }
}
