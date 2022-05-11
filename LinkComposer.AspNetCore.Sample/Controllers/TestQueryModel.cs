using System.Data;

namespace LinkComposer.AspNetCore.Sample.Controllers
{
    public class TestQueryModel
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
