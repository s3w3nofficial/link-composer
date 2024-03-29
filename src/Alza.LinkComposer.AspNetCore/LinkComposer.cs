﻿using Alza.LinkComposer.Interfaces;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Web;

namespace Alza.LinkComposer.AspNetCore
{
    public class LinkComposer : ILinkComposer
    {
        private readonly TemplateBinderFactory _templateBinderFactory;
        private readonly ILinkComposerBaseUriProvider _linkComposerBaseUriProvider;
        public LinkComposer(TemplateBinderFactory templateBinderFactory, ILinkComposerBaseUriProvider linkComposerBaseUriProvider)
        {
            this._templateBinderFactory = templateBinderFactory ?? throw new ArgumentNullException(nameof(templateBinderFactory));
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

            invocatitonInfo.MethodTemplate ??= "";
            var routePattern = RoutePatternFactory.Parse(invocatitonInfo.MethodTemplate);
            var binder = this._templateBinderFactory.Create(routePattern);
            var path = binder.BindValues(new RouteValueDictionary(invocatitonInfo.RouteParameterValues));

            var controllerRoutePattern = RoutePatternFactory.Parse(invocatitonInfo.ControllerTemplate);
            var controllerBinder = this._templateBinderFactory.Create(controllerRoutePattern);
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
