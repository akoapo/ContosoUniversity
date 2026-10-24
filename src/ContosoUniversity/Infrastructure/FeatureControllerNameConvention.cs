namespace ContosoUniversity.Infrastructure
{
    using System;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;

    /// <summary>
    /// Replaces the MVC5 <c>ControllerFactory</c> (a <c>DefaultControllerFactory</c> whose
    /// <c>GetControllerType</c> mapped the route controller name to
    /// <c>ContosoUniversity.Features.{name}.UiController</c>).
    /// Here the mapping is applied in the opposite direction at startup: every feature-folder
    /// <c>UiController</c> is given the controller name of its feature folder, so the default
    /// <c>{controller}/{action}/{id?}</c> route keeps serving /Home, /Student, /Course,
    /// /Department and /Instructor.
    /// </summary>
    public class FeatureControllerNameConvention : IControllerModelConvention
    {
        private const string FeaturesNamespacePrefix = "ContosoUniversity.Features.";

        public void Apply(ControllerModel controller)
        {
            var typeNamespace = controller.ControllerType.Namespace;

            if (string.IsNullOrEmpty(typeNamespace))
            {
                return;
            }

            if (!typeNamespace.StartsWith(FeaturesNamespacePrefix, StringComparison.Ordinal))
            {
                return;
            }

            var featureName = typeNamespace
                .Substring(FeaturesNamespacePrefix.Length)
                .Split('.')[0];

            if (string.IsNullOrEmpty(featureName))
            {
                return;
            }

            controller.ControllerName = featureName;
        }
    }
}
