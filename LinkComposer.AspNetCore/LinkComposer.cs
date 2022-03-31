using LinkComposer.Configuration;
using LinkComposer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Web;

namespace LinkComposer.AspNetCore
{
    public class LinkComposer : ILinkComposer
    {
        private readonly IOptions<LinkComposerSettings> _options;
        private readonly TemplateBinderFactory _templateBinderFactory;
        public LinkComposer(IOptions<LinkComposerSettings> options, TemplateBinderFactory templateBinderFactory)
        {
            this._options = options;
            this._templateBinderFactory = templateBinderFactory;
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
            var routePattern = RoutePatternFactory.Parse(invocatitonInfo.MethodTemplate);
            var binder = this._templateBinderFactory.Create(routePattern);
            var path = binder.BindValues(new RouteValueDictionary(invocatitonInfo.RouteParameterValues));

            var controllerRoutePattern = RoutePatternFactory.Parse(invocatitonInfo.ControllerTemplate);
            var controllerBinder = this._templateBinderFactory.Create(controllerRoutePattern);
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
