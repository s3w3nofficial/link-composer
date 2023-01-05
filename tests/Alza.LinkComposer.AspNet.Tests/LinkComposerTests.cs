using Alza.LinkComposer.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Alza.LinkComposer.AspNet.Tests
{
    public class LinkComposerBaseUriFactory : ILinkComposerBaseUriFactory
    {
        public Uri GetBaseUri(string projectName)
        {
            return new Uri("https://localhost:7009");
        }
    }

    [TestClass]
    public class LinkComposerTests
    {
        [TestMethod]
        public void Test()
        {
            var linkComposerBaseUriFactory = new LinkComposerBaseUriFactory();
            var linkComposer = new LinkComposer(linkComposerBaseUriFactory);

            var link = linkComposer.Link<HomePageControllerLink>(l => l.GetModel("1", new HomePageControllerLink.TestQueryModel
            {

            }));

            Assert.IsNotNull(link);
        }
    }
}
