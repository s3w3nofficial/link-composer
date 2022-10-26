using Alza.LinkComposer.Configuration;
using Alza.LinkComposer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;

namespace Alza.LinkComposer.AspNet
{
    public class LinkComposer : ILinkComposer
    {
        private readonly IOptions<LinkComposerSettings> _options;
        public LinkComposer(IOptions<LinkComposerSettings> options)
        {
            this._options = options;
        }

        public Uri Link<T>(Expression<Action<T>> method)
            where T : LinkComposerController 
        {
            var invocatitonInfo = Helpers.GetInvocation(method);
            return GenerateLink<T>(invocatitonInfo);
        }

        public Uri Link<T>(Expression<Func<T, Task>> method)
            where T : LinkComposerController
        {
            var invocatitonInfo = Helpers.GetInvocation(method);
            return GenerateLink<T>(invocatitonInfo);
        }

        public Uri Link<T>(Expression<Action<T>> method, object additionalQueryParams) where T : LinkComposerController
        {
            var invocatitonInfo = Helpers.GetInvocation(method);
            return GenerateLink<T>(invocatitonInfo);
        }

        public Uri Link<T>(Expression<Func<T, Task>> method, object additionalQueryParams) where T : LinkComposerController
        {
            var invocatitonInfo = Helpers.GetInvocation(method);
            return GenerateLink<T>(invocatitonInfo);
        }

        private Uri GenerateLink<T>(Invocation invocatitonInfo)
        {
            var queryString =
                $"?{HttpUtility.UrlDecode(string.Join("&", invocatitonInfo.ParameterValues.Select(kvp => $"{kvp.Key}={kvp.Value}")))}";

            var config = this._options.Value.Routes[invocatitonInfo.ProjectName];

            invocatitonInfo.MethodTemplate ??= "";
            var routeTemplate = TemplateParser.Parse(invocatitonInfo.MethodTemplate);
            var pool = new DefaultObjectPoolProvider().Create(new UriBuilderContextPooledObjectPolicy());
            var binder = new TemplateBinder(UrlEncoder.Default, pool, routeTemplate, null);
            var path = binder.BindValues(new RouteValueDictionary(invocatitonInfo.RouteParameterValues));

            var controllerRouteTemplate = TemplateParser.Parse(invocatitonInfo.ControllerTemplate);
            var controllerBinder = new TemplateBinder(UrlEncoder.Default, pool, controllerRouteTemplate, null);
            var pathBase = controllerBinder.BindValues(new RouteValueDictionary(invocatitonInfo.ControllerRouteParameterValues));

            var url = UriHelper.BuildAbsolute(config.Scheme,
                new HostString(config.Host),
                new PathString(pathBase),
                new PathString(path),
                new QueryString(invocatitonInfo.ParameterValues.Count > 0 ? queryString : ""));

            return new Uri(url);
        }
    }
}
