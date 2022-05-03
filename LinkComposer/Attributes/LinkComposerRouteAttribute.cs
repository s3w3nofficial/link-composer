using System;

namespace LinkComposer.Attributes
{
    public class LinkComposerRouteAttribute : Attribute
    {
        public string Template { get; set; } = null;

        public string ControllerTemplate { get; set; } = null;

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
    }
}
