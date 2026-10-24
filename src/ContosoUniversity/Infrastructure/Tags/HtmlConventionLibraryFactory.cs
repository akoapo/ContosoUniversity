namespace ContosoUniversity.Infrastructure.Tags
{
    using HtmlTags.Conventions;

    /// <summary>
    /// Replaces the StructureMap <c>HtmlTagRegistry</c>. Builds the application's single
    /// <see cref="HtmlConventionLibrary"/> by applying this project's conventions and then the
    /// HtmlTags defaults — same order as the original registry. The resulting library is
    /// immutable in practice and is registered as a singleton in Program.cs.
    /// </summary>
    public static class HtmlConventionLibraryFactory
    {
        public static HtmlConventionLibrary Create()
        {
            var htmlConventionLibrary = new HtmlConventionLibrary();

            new DefaultAspNetMvcHtmlConventions().Apply(htmlConventionLibrary);
            new DefaultHtmlConventions().Apply(htmlConventionLibrary);

            return htmlConventionLibrary;
        }
    }
}
