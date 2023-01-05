using Alza.LinkComposer.Configuration;
using Alza.LinkComposer.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Alza.LinkComposer.AspNet.Tests
{
    public class LinkComposerBaseUriFactory : ILinkComposerBaseUriFactory
    {
        public Uri GetBaseUri(Uri url)
        {
            return new Uri(url.AbsoluteUri.Replace(".cz", ".sk"));
        }
    }

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
                            Url = new Uri("http://localhost:5900")
                        }
                    }
                }
            });

            var linkComposerBaseUriFactory = new LinkComposerBaseUriFactory();
            var linkComposer = new LinkComposer(options, linkComposerBaseUriFactory);

            var link = linkComposer.Link<HomePageControllerLink>(l => l.GetModel("1", new HomePageControllerLink.TestQueryModel
            {

            }));

            Assert.IsNotNull(link);
        }
    }
}
