using System.Web.Http;

namespace LinkComposer.AspNet.Sample.Controllers
{
	[Route("[controller]")]
	public class HomeController : ApiController
	{
		[HttpGet]
		public IHttpActionResult Get()
		{
			return Json(new
			{
				Navigation = new
				{
					Rel = "self",
					Href = ""
				}
			});
		}

		public class PaginationModel 
        {
            public string Offset { get; set; }
			public string Limit { get; set; }
        }

		[HttpPost]
		public IHttpActionResult Post([FromUri] PaginationModel model)
        {
			return Json(model);
        }
	}
}
