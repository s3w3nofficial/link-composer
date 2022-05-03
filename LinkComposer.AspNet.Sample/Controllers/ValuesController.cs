using System.Web.Http;

namespace LinkComposer.AspNet.Sample.Controllers
{
	[Route("[controller]")]
	public class ValuesController : ApiController
	{
		// GET api/values
		[HttpGet]
		public IHttpActionResult Get()
		{
			return Json(new string[] { "value1", "value2" });
		}

		// GET api/values/5
		[HttpGet]
		public string Get(int id)
		{
			return "value";
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/values/5
		[HttpPut]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete]
		public void Delete(int id)
		{
		}
	}
}
