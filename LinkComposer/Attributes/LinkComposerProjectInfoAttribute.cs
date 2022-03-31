namespace LinkComposer.Attributes
{
    public class LinkComposerProjectInfoAttribute : Attribute
    {
        public string Name { get; set; }

        public LinkComposerProjectInfoAttribute(string name)
        {
            this.Name = name;
        }
    }
}
