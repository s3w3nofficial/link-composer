using Alza.LinkComposer.AspNetCore.Sample.Models;
using System.Data;

namespace Alza.LinkComposer.AspNetCore.Sample.Controllers
{
    public class TestQueryModel : BaseRequestModel
    {
        public int? Limit { get; set; }

        public int Offset { get; set; }

        public IEnumerable<int> Tests { get; set; }

        public IEnumerable<SubTestQueryModel>? SubTests { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public TestEnumQueryModel TestEnumQueryModel { get; set; }

        public CommandType CommandType  { get; set; }
    }
}
