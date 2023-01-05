using Alza.LinkComposer.Interfaces;

namespace Alza.LinkComposer.AspNetCore.Sample.Services
{
    public class LinkComposerBaseUriFactory : ILinkComposerBaseUriFactory
    {
        public Uri GetBaseUri(Uri url)
        {
            if (url is null)
                throw new ArgumentNullException(nameof(url));

            return new Uri(url.AbsoluteUri.Replace(".cz", ".com"));
        }
    }
}
