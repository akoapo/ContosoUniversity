namespace ContosoUniversity.Infrastructure
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;

    /// <summary>
    /// Turns an <c>Expression&lt;Action&lt;TController&gt;&gt;</c> into an application URL.
    /// <para>
    /// The MVC5 implementation used the MVC-Futures <c>CachedExpressionCompiler</c>,
    /// <c>RouteTable.Routes.GetVirtualPathForArea</c> and <c>ActionLinkAreaAttribute</c>, and then
    /// rewrote the "/Ui" segment (every feature controller is called <c>UiController</c>) into the
    /// feature-folder name. Here the rewrite is done up front: the controller name fed to the
    /// router is the feature-folder name taken from the controller's namespace, which is exactly
    /// what <see cref="FeatureControllerNameConvention"/> registered with MVC. Generated URLs are
    /// therefore unchanged (/Student/Edit/5, /Department/Index, ...).
    /// </para>
    /// </summary>
    public static class LinkBuilder
    {
        public static string BuildUrlFromExpression<TController>(IUrlHelper urlHelper,
            Expression<Action<TController>> action) where TController : Controller
        {
            if (urlHelper == null)
            {
                throw new ArgumentNullException(nameof(urlHelper));
            }

            var routeValues = GetRouteValuesFromExpression(action, out var actionName, out var controllerName);

            return urlHelper.Action(actionName, controllerName, routeValues);
        }

        public static string BuildUrlFromExpression<TController>(LinkGenerator linkGenerator,
            Microsoft.AspNetCore.Http.HttpContext httpContext,
            Expression<Action<TController>> action) where TController : Controller
        {
            if (linkGenerator == null)
            {
                throw new ArgumentNullException(nameof(linkGenerator));
            }

            var routeValues = GetRouteValuesFromExpression(action, out var actionName, out var controllerName);

            routeValues["action"] = actionName;
            routeValues["controller"] = controllerName;

            return linkGenerator.GetPathByRouteValues(httpContext, routeName: null, values: routeValues);
        }

        /// <summary>
        /// The feature-folder name: <c>ContosoUniversity.Features.Student.UiController</c> =&gt; "Student".
        /// Falls back to the conventional "FooController" =&gt; "Foo" trim for any controller that does
        /// not live in a feature folder.
        /// </summary>
        public static string GetControllerName<TController>()
        {
            var controllerType = typeof(TController);
            const string featuresPrefix = "ContosoUniversity.Features.";
            var typeNamespace = controllerType.Namespace ?? string.Empty;

            if (typeNamespace.StartsWith(featuresPrefix, StringComparison.Ordinal))
            {
                var featureName = typeNamespace.Substring(featuresPrefix.Length).Split('.')[0];

                if (!string.IsNullOrEmpty(featureName))
                {
                    return featureName;
                }
            }

            var name = controllerType.Name;

            return name.EndsWith("Controller", StringComparison.Ordinal)
                ? name.Substring(0, name.Length - "Controller".Length)
                : name;
        }

        private static RouteValueDictionary GetRouteValuesFromExpression<TController>(
            Expression<Action<TController>> action, out string actionName, out string controllerName)
            where TController : Controller
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!(action.Body is MethodCallExpression call))
            {
                throw new ArgumentException(
                    "The action expression must be a single call to a controller action method.", nameof(action));
            }

            controllerName = GetControllerName<TController>();
            actionName = GetTargetActionName(call.Method);

            var routeValues = new RouteValueDictionary();

            // An [Area] on the controller must take part in URL generation, as the MVC5
            // ActionLinkAreaAttribute/GetVirtualPathForArea pair did.
            var areaAttribute = typeof(TController)
                .GetCustomAttributes(typeof(AreaAttribute), inherit: true)
                .FirstOrDefault() as AreaAttribute;

            if (areaAttribute != null)
            {
                routeValues["area"] = areaAttribute.RouteValue;
            }

            AddParameterValuesFromExpressionToDictionary(routeValues, call);

            return routeValues;
        }

        private static string GetTargetActionName(MethodInfo methodInfo)
        {
            // [ActionName] is the modern replacement for the MVC5 AsyncController "...Async"
            // suffix-stripping: it is what MVC itself uses to name the action.
            var actionNameAttribute = methodInfo
                .GetCustomAttributes(typeof(ActionNameAttribute), inherit: true)
                .FirstOrDefault() as ActionNameAttribute;

            if (actionNameAttribute != null && !string.IsNullOrEmpty(actionNameAttribute.Name))
            {
                return actionNameAttribute.Name;
            }

            return methodInfo.Name;
        }

        private static void AddParameterValuesFromExpressionToDictionary(RouteValueDictionary routeValues,
            MethodCallExpression call)
        {
            var parameters = call.Method.GetParameters();

            for (var i = 0; i < parameters.Length; i++)
            {
                var argument = call.Arguments[i];
                var value = Evaluate(argument);

                if (argument.Type.IsClass && argument.Type != typeof(string))
                {
                    // Complex argument: flatten its public properties into route values,
                    // exactly like the old `new RouteValueDictionary(value)` expansion.
                    foreach (var pair in new RouteValueDictionary(value))
                    {
                        routeValues[pair.Key] = pair.Value;
                    }
                }
                else
                {
                    routeValues[parameters[i].Name] = value;
                }
            }
        }

        private static object Evaluate(Expression expression)
        {
            if (expression is ConstantExpression constant)
            {
                return constant.Value;
            }

            // Replaces MVC-Futures CachedExpressionCompiler.Evaluate.
            if (expression is MemberExpression member && member.Expression is ConstantExpression target)
            {
                if (member.Member is FieldInfo field)
                {
                    return field.GetValue(target.Value);
                }

                if (member.Member is PropertyInfo property)
                {
                    return property.GetValue(target.Value);
                }
            }

            var lambda = Expression.Lambda<Func<object>>(
                Expression.Convert(expression, typeof(object)));

            return lambda.Compile()();
        }
    }
}
