using Alza.LinkComposer.Interfaces;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Moq;
using System;
using Xunit;

namespace Alza.LinkComposer.AspNetCore.Tests;

public class LinkComposerBaseUriFactory : ILinkComposerBaseUriFactory
{
    public Uri GetBaseUri(string projectName)
    {
        return new Uri("https://localhost:7009");
    }
}

public class LinkComposerTests
{
    [Fact]
    public void Tets()
    {
        var templateBinder = new Mock<TemplateBinder>(Mock.Of<TemplateBinder>(), null, null, null, null);

        var tbFactory = new Mock<TemplateBinderFactory>();
        tbFactory.Setup(x => x.Create(It.IsAny<RoutePattern>())).Returns(templateBinder.Object);

        var linkComposerBaseUriFactory = new LinkComposerBaseUriFactory();
        var linkComposer = new LinkComposer(tbFactory.Object, linkComposerBaseUriFactory);

        var link = linkComposer.Link<HomePageControllerLink>(l => l.GetModel("1", new HomePageControllerLink.TestQueryModel
        {
            
        }));

        Assert.NotNull(link);
    }
}
