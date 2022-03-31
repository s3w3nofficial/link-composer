using LinkComposer.Links;
using Microsoft.AspNetCore.Mvc;
using LinkComposer.Interfaces;

namespace LinkComposer.AspNetCore.Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomePageController : ControllerBase
    {
        private readonly ILinkComposer _linkComposer;
        public HomePageController(ILinkComposer linkComposer)
        {
            _linkComposer = linkComposer ?? throw new ArgumentNullException(nameof(linkComposer));
        }

        [HttpGet("navigation", Name = nameof(Navigation))]
        public object Navigation(string test = "ahoj")
        {
            return new
            {
                Test = test,
                Navigation = _linkComposer.Link<HomePageControllerLink>(x => x.Navigation()),
            };
        }

        [HttpGet("get/{id}", Name = nameof(Get))]
        public object Get(int id)
        {
            return new
            {
                Get = _linkComposer.Link<HomePageControllerLink>(x => x.Get(id))
            };
        }

        [HttpGet("getByGuid/{id}", Name = nameof(GetByGUID))]
        public object GetByGUID(Guid id)
        {
            return new
            {
                GetByGUID = _linkComposer.Link<HomePageControllerLink>(x => x.GetByGUID(id))
            };
        }

        [HttpGet("getModel/{id}", Name = nameof(GetModel))]
        public object GetModel(string id, [FromQuery] TestQueryModel testQueryModel)
        {
            return new
            {
                GetModel = _linkComposer.Link<HomePageControllerLink>(x => x.GetModel(id, new HomePageControllerLink.TestQueryModel
                {
                    Limit = 10,
                    Offset = 20
                }))
            };
        }

        [HttpPost("postBody", Name = nameof(PostBody))]
        public object PostBody(TestQueryModel testQueryModel)
        {
            return new
            {
                PostBody = _linkComposer.Link<HomePageControllerLink>(x => x.PostBody())
            };
        }
    }
}
