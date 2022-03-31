using LinkComposer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LinkComposer.AspNetCore.Sample.Controllers
{
    [ApiController]
    [Route("abcd")]
    public class PrefixController : ControllerBase
    {
        private readonly ILinkComposer _linkComposer;
        public PrefixController(ILinkComposer linkComposer)
        {
            _linkComposer = linkComposer ?? throw new ArgumentNullException(nameof(linkComposer));
        }

        [HttpGet]
        [Route("navigation", Name = nameof(PrefixNavigation))]
        public object PrefixNavigation()
        {
            return new
            {
                Navigation = _linkComposer.Link<Links.PrefixControllerLink>(x => x.PrefixNavigation())
            };
        }
    }
}
