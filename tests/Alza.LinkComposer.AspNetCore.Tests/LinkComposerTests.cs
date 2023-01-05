using Alza.LinkComposer.Configuration;
using Alza.LinkComposer.Interfaces;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Alza.LinkComposer.AspNetCore.Tests;

public class LinkComposerBaseUriFactory : ILinkComposerBaseUriFactory
{
    public Uri GetBaseUri(Uri url)
    {
        return new Uri(url.AbsoluteUri.Replace(".cz", ".sk"));
    }
}

public class LinkComposerTests
{
    [Fact]
    public void Tets()
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

        var templateBinder = new Mock<TemplateBinder>(Mock.Of<TemplateBinder>(), null, null, null, null);

        var tbFactory = new Mock<TemplateBinderFactory>();
        tbFactory.Setup(x => x.Create(It.IsAny<RoutePattern>())).Returns(templateBinder.Object);

        var linkComposerBaseUriFactory = new LinkComposerBaseUriFactory();
        var linkComposer = new LinkComposer(options, tbFactory.Object, linkComposerBaseUriFactory);

        var link = linkComposer.Link<HomePageControllerLink>(l => l.GetModel("1", new HomePageControllerLink.TestQueryModel
        {
            
        }));

        Assert.NotNull(link);
    }
}
