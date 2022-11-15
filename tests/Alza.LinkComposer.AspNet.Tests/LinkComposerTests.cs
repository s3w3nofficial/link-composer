using Alza.LinkComposer.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Alza.LinkComposer.AspNet.Tests
{
    [TestClass]
    public class LinkComposerTests
    {
        [TestMethod]
        public void Test()
        {
            var options = Options.Create(new LinkComposerSettings
            {
                Routes = new Dictionary<string, LinkComposerRouteSettings>
            {
                {
                    "Alza.LinkComposer.AspNetCore.Sample",
                    new LinkComposerRouteSettings
                    {
                        Host = "localhost",
                        Scheme = "http",
                    }
                }
            }
            });

            var linkComposer = new LinkComposer(options);

            var link = linkComposer.Link<HomePageControllerLink>(l => l.GetModel("1", new HomePageControllerLink.TestQueryModel
            {

            }));

            Assert.IsNotNull(link);
        }
    }
}
