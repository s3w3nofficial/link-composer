namespace LinkComposer.AspNetCore.Sample.Controllers
{
    public class TestQueryModel
    {
        public int? Limit { get; set; }
        public int Offset { get; set; }

        public IEnumerable<int> Tests { get; set; }

        public SubTestQueryModel SubTest { get; set; }
    }
}
