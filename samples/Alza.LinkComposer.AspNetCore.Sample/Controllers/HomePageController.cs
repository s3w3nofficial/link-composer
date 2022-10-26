using Alza.LinkComposer.Interfaces;
using Alza.LinkComposer.Links;
using Microsoft.AspNetCore.Mvc;

namespace Alza.LinkComposer.AspNetCore.Sample.Controllers
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
                Navigation = _linkComposer.Link<HomePageControllerLink>(x => x.Navigation("ahoj")),
            };
        }

        [HttpGet("get/{id}", Name = nameof(Get))]
        public object Get(int? id)
        {
            return new
            {
                Get = _linkComposer.Link<HomePageControllerLink>(x => x.Get(id))
            };
        }

        [HttpGet("getArray/{ids}", Name = nameof(GetArray))]
        public object GetArray(string[] ids)
        {
            return new
            {
                Get = _linkComposer.Link<HomePageControllerLink>(x => x.GetArray(ids))
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

        [HttpGet("getModel2/{id}", Name = nameof(GetModel2))]
        public object GetModel2(string id, [FromQuery] TestQueryModel testQueryModel)
        {
            return new
            {
                GetModel = _linkComposer.Link<HomePageControllerLink>(x => x.GetModel2(id, new HomePageControllerLink.TestQueryModel
                {
                    Limit = 10,
                    Offset = 20
                }))
            };
        }

        [HttpGet("getModel3/{id}", Name = nameof(GetModel3))]
        public object GetModel3(string id, [FromQuery] TestQueryModel[] testQueryModel)
        {
            return new
            {
                GetModel = _linkComposer.Link<HomePageControllerLink>(x => x.GetModel3(id, new[] {
                    new HomePageControllerLink.TestQueryModel
                    {
                        Limit = 10,
                        Offset = 20
                    }
                }))
            };
        }

        [HttpGet("getModel4/{id}", Name = nameof(GetModel4))]
        public object GetModel4(string id, [FromQuery] string[] names)
        {
            return new
            {
                GetModel = _linkComposer.Link<HomePageControllerLink>(x => x.GetModel4(id, names))
            };
        }

        [HttpPost("postBody", Name = nameof(PostBody))]
        public object PostBody(TestQueryModel testQueryModel)
        {
            return new
            {
                PostBody = _linkComposer.Link<HomePageControllerLink>(x => x.PostBody(new HomePageControllerLink.TestQueryModel
                {

                }))
            };
        }

        [HttpPost("postPath/{path}", Name = nameof(PostPath))]
        public object PostPath([FromQuery] Uri path)
        {
            return new
            {
                PostBody = _linkComposer.Link<HomePageControllerLink>(x => x.PostPath(path))
            };
        }

        [HttpPost("sameParams/{name}", Name = nameof(SameParams))]
        public object SameParams(string name)
        {
            return new
            {
                PostBody = _linkComposer.Link<HomePageControllerLink>(x => x.SameParams(name))
            };
        }

        [HttpPost("sameParams/{token}", Name = nameof(SameParams))]
        public object SameParams(string token, [FromBody] TestQueryModel text)
        {
            return new
            {
                PostBody = _linkComposer.Link<HomePageControllerLink>(x => x.SameParams2(token))
            };
        }

        [HttpPost("routeParams", Name = nameof(RouteParams))]
        public object RouteParams(TestQueryModel model)
        {
            return new
            {
                PostBody = _linkComposer.Link<HomePageControllerLink>(x => x.RouteParams(new HomePageControllerLink.TestQueryModel
                {

                }))
            };
        }

        [HttpPost("Enums", Name = nameof(Enums))]
        public object Enums([FromQuery] TestEnumQueryModel? model)
        {
            return new
            {
                PostBody = _linkComposer.Link<HomePageControllerLink>(x => x.Enums(HomePageControllerLink.TestEnumQueryModel.a))
            };
        }
    }
}
