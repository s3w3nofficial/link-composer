using Alza.LinkComposer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;

namespace Alza.LinkComposer.AspNet
{
    public class LinkComposer : ILinkComposer
    {
        private readonly ILinkComposerBaseUriProvider _linkComposerBaseUriProvider;
        public LinkComposer(ILinkComposerBaseUriProvider linkComposerBaseUriProvider)
        {
            this._linkComposerBaseUriProvider = linkComposerBaseUriProvider ?? throw new ArgumentNullException(nameof(linkComposerBaseUriProvider));
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

            var json = JsonConvert.SerializeObject(additionalQueryParams);
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (parameters is null)
                throw new JsonSerializationException("could not serialize parameters");

            foreach (var parameter in parameters)
                invocatitonInfo.ParameterValues.Add(parameter.Key, parameter.Value);

            return GenerateLink<T>(invocatitonInfo);
        }

        public Uri Link<T>(Expression<Func<T, Task>> method, object additionalQueryParams) where T : LinkComposerController
        {
            var invocatitonInfo = Helpers.GetInvocation(method);

            var json = JsonConvert.SerializeObject(additionalQueryParams);
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (parameters is null)
                throw new JsonSerializationException("could not serialize parameters");

            foreach (var parameter in parameters)
                invocatitonInfo.ParameterValues.Add(parameter.Key, parameter.Value);

            return GenerateLink<T>(invocatitonInfo);
        }

        private Uri GenerateLink<T>(Invocation invocatitonInfo)
        {
            var queryString =
                $"?{HttpUtility.UrlDecode(string.Join("&", invocatitonInfo.ParameterValues.Select(kvp => $"{kvp.Key}={kvp.Value}")))}";

            var baseUri = this._linkComposerBaseUriProvider.GetBaseUri(invocatitonInfo.ProjectName);

            if (invocatitonInfo.MethodTemplate is null)
                invocatitonInfo.MethodTemplate = "";

            var routeTemplate = TemplateParser.Parse(invocatitonInfo.MethodTemplate);
            var pool = new DefaultObjectPoolProvider().Create(new UriBuilderContextPooledObjectPolicy());
            var binder = new TemplateBinder(UrlEncoder.Default, pool, routeTemplate, null);
            var path = binder.BindValues(new RouteValueDictionary(invocatitonInfo.RouteParameterValues));

            var controllerRouteTemplate = TemplateParser.Parse(invocatitonInfo.ControllerTemplate);
            var controllerBinder = new TemplateBinder(UrlEncoder.Default, pool, controllerRouteTemplate, null);
            var pathBase = controllerBinder.BindValues(new RouteValueDictionary(invocatitonInfo.ControllerRouteParameterValues));
            var hostBase = baseUri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Scheme, UriFormat.UriEscaped).TrimEnd('/');

            var url = UriHelper.BuildAbsolute(baseUri.Scheme,
                new HostString(hostBase),
                new PathString(pathBase),
                new PathString(path),
                new QueryString(invocatitonInfo.ParameterValues.Count > 0 ? queryString : ""));

            return new Uri(url);
        }
    }
}
