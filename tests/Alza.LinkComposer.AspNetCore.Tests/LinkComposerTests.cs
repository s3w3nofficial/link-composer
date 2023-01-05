using Alza.LinkComposer.Interfaces;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Moq;
using System;
using Xunit;

namespace Alza.LinkComposer.AspNetCore.Tests;

public class LinkComposerBaseUriProvider : ILinkComposerBaseUriProvider
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

        var linkComposerBaseUriProvider = new LinkComposerBaseUriProvider();
        var linkComposer = new LinkComposer(tbFactory.Object, linkComposerBaseUriProvider);

        var link = linkComposer.Link<HomePageControllerLink>(l => l.GetModel("1", new HomePageControllerLink.TestQueryModel
        {
            
        }));

        Assert.NotNull(link);
    }
}
