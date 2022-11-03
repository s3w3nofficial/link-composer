using System;

namespace Alza.LinkComposer.Attributes
{
    public class LinkComposerProjectInfoAttribute : Attribute
    {
        public string Name { get; set; }

        public int? Version { get; set; } = null;

        public LinkComposerProjectInfoAttribute(string name)
        {
            this.Name = name;
        }

        public LinkComposerProjectInfoAttribute(string name, int version)
        {
            this.Name = name;
            this.Version = version;
        }
    }
}
