using System;

namespace Alza.LinkComposer.Attributes
{
    public class LinkComposerRouteAttribute : Attribute
    {
        public string Template { get; set; } = null;

        public string ControllerTemplate { get; set; } = null;

        public int? Version { get; set; } = null;

        public LinkComposerRouteAttribute()
        {

        }

        public LinkComposerRouteAttribute(string template)
        {
            this.Template = template;
        }

        public LinkComposerRouteAttribute(string template, string controllerTenplate)
        {
            this.Template = template;
            this.ControllerTemplate = controllerTenplate;
        }

        public LinkComposerRouteAttribute(string template, string controllerTemplate, int version)
        {
            this.Template = template;
            this.ControllerTemplate = controllerTemplate;
            this.Version = version;
        }
    }
}
