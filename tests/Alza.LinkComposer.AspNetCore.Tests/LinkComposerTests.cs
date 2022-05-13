using Alza.LinkComposer.Configuration;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Alza.LinkComposer.AspNetCore.Tests;

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
                        Host = "localhost",
                        Scheme = "http",
                    }
                }
            }
        });

        var templateBinder = new Mock<TemplateBinder>(Mock.Of<TemplateBinder>(), null, null, null, null);

        var tbFactory = new Mock<TemplateBinderFactory>();
        tbFactory.Setup(x => x.Create(It.IsAny<RoutePattern>())).Returns(templateBinder.Object);

        var linkComposer = new LinkComposer(options, tbFactory.Object);

        var link = linkComposer.Link<HomePageControllerLink>(l => l.GetModel("1", new HomePageControllerLink.TestQueryModel
        {
            
        }));

        Assert.NotNull(link);
    }
}
