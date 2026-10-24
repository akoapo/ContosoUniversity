namespace ContosoUniversity.Infrastructure.Tags
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using HtmlTags;
    using HtmlTags.Conventions;

    public class DefaultAspNetMvcHtmlConventions : HtmlConventionRegistry
    {
        public DefaultAspNetMvcHtmlConventions()
        {
            Editors.Always.AddClass("form-control");

            Editors.IfPropertyIs<DateTime?>().ModifyWith(m => m.CurrentTag
                .AddPattern("9{1,2}/9{1,2}/9999")
                .AddPlaceholder("MM/DD/YYYY")
                .AddClass("datepicker")
                .Value(m.Value<DateTime?>() != null ? m.Value<DateTime>().ToShortDateString() : string.Empty));
            Editors.If(er => er.Accessor.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                .BuildBy(a => new HiddenTag().Value(a.StringValue()));
            Editors.IfPropertyIs<byte[]>()
                .BuildBy(a => new HiddenTag().Value(ToBase64(a.Value<byte[]>())));

            Labels.Always.AddClass("control-label");
            Labels.Always.AddClass("col-md-2");
            Labels.ModifyForAttribute<DisplayAttribute>((t, a) => t.Text(a.Name));
            Editors.BuilderPolicy<InstructorSelectElementBuilder>();
            Editors.BuilderPolicy<DepartmentSelectElementBuilder>();
            DisplayLabels.Always.BuildBy<DefaultDisplayLabelBuilder>();
            DisplayLabels.ModifyForAttribute<DisplayAttribute>((t, a) => t.Text(a.Name));
            Displays.IfPropertyIs<DateTime>()
                .ModifyWith(m => m.CurrentTag.Text(m.Value<DateTime>().ToShortDateString()));
            Displays.IfPropertyIs<DateTime?>()
                .ModifyWith(m => m.CurrentTag.Text(m.Value<DateTime?>() == null
                    ? null
                    : m.Value<DateTime?>().Value.ToShortDateString()));
            Displays.IfPropertyIs<decimal>()
                .ModifyWith(m => m.CurrentTag.Text(m.Value<decimal>().ToString("C")));
        }

        /// <summary>
        /// The custom "DisplayLabels" tag category — the table headers in the Index/Details views
        /// are built from it (see HtmlHelperExtensions.DisplayLabel).
        /// </summary>
        public ElementCategoryExpression DisplayLabels
        {
            get
            {
                return new ElementCategoryExpression(
                    Library.TagLibrary.Category("DisplayLabels").Profile(TagConstants.Default));
            }
        }

        private static string ToBase64(byte[] value)
        {
            return value == null ? string.Empty : Convert.ToBase64String(value);
        }
    }
}
