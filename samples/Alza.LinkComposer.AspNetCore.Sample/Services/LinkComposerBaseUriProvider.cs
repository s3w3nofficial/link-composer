using Alza.LinkComposer.Interfaces;

namespace Alza.LinkComposer.AspNetCore.Sample.Services
{
    public class LinkComposerBaseUriProvider : ILinkComposerBaseUriProvider
    {
        public Uri GetBaseUri(string projectName)
        {
            if (projectName is null)
                throw new ArgumentNullException(nameof(projectName));

            switch(projectName)
            {
                case "Alza.LinkComposer.Links.Sample":
                    return new Uri("https://localhost:7009");
                default:
                    return new Uri("https://alza.cz");
            }
        }
    }
}
