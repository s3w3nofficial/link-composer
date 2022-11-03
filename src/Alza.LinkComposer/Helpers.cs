using Alza.LinkComposer.Attributes;
using Microsoft.AspNetCore.Routing.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Alza.LinkComposer
{
    public static class Helpers
    {
        public static Invocation GetInvocation<T>(Expression<Func<T, Task>> action)
           where T : LinkComposerController
        {
            if (!(action.Body is MethodCallExpression callExpression))
                throw new ArgumentException("Action must be a method call", nameof(action));

            return GetInvocationFromMethodCall<T>(callExpression);
        }

        public static Invocation GetInvocation<T>(Expression<Action<T>> action)
            where T : LinkComposerController
        {
            if (!(action.Body is MethodCallExpression callExpression))
                throw new ArgumentException("Action must be a method call", nameof(action));

            return GetInvocationFromMethodCall<T>(callExpression);
        }

        private static Invocation GetInvocationFromMethodCall<T>(MethodCallExpression callExpression)
            where T : LinkComposerController
        {
            var httpMethodAttribute = callExpression.Method.GetCustomAttributes<LinkComposerRouteAttribute>().FirstOrDefault();
            var template = httpMethodAttribute?.Template;
            var controllerTemplate = httpMethodAttribute?.ControllerTemplate;

            var values = callExpression.Arguments.Select(ReduceToConstant).ToList();
            var names = callExpression
                .Method
                .GetParameters()
                .Select(i => i.Name)
                .ToList();

            var parameters = names.Zip(values, (k, v) => new { k, v })
                .ToDictionary(x => x.k, x => x.v);

            Dictionary<string, string> routeParameters = null;
            // filter out route parameters
            if (template != null)
            {
                routeParameters = GetRouteParameters(template, parameters)
                    .ToDictionary(x => x.Key, x => x.Value.ToString());

                foreach (var rp in routeParameters)
                    parameters.Remove(rp.Key);
            }

            Dictionary<string, string> controllerRouteParameters = null;
            if (controllerTemplate != null)
            {
                controllerRouteParameters = GetRouteParameters(controllerTemplate, parameters)
                    .ToDictionary(x => x.Key, x => x.Value.ToString());

                foreach (var rp in controllerRouteParameters)
                    parameters.Remove(rp.Key);

            }

            var queryParameters = new Dictionary<string, object>();

            foreach (var parameter in parameters)
            {
                if (parameter.Value is ValueType || parameter.Value is string)
                    queryParameters.Add(parameter.Key, parameter.Value);
                else
                {
                    var properties = parameter.Value.GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        var propertyValue = GetPropValue(parameter.Value, property.Name);
                        if (propertyValue != null)
                            queryParameters.Add(property.Name.ToLowerInvariant(), propertyValue);
                    }
                }
            }

            var projectAttribute = typeof(T).GetCustomAttribute<LinkComposerProjectInfoAttribute>();

            if (httpMethodAttribute.Version is null && projectAttribute.Version != null)
                controllerRouteParameters.Add("version", projectAttribute.Version.ToString());

            if (httpMethodAttribute.Version != null)
                controllerRouteParameters.Add("version", httpMethodAttribute.Version.ToString());

            return new Invocation
            {
                ProjectName = projectAttribute.Name,
                ParameterValues = queryParameters,
                RouteParameterValues = routeParameters,
                ControllerName = typeof(T).Name.Replace("ControllerLink", ""),
                MethodName = callExpression.Method.Name,
                MethodTemplate = template,
                ControllerTemplate = controllerTemplate,
                ControllerRouteParameterValues = controllerRouteParameters
            };
        }

        private static object ReduceToConstant(Expression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        private static Dictionary<string, object> GetRouteParameters(string routeTemplate, Dictionary<string, object> parameters)
        {
            var template = TemplateParser.Parse(routeTemplate);
            return parameters
                .Where(p => template.Parameters.Any(tp => tp.Name == p.Key))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private static object GetPropValue(object src, string propName)
        {
            var property = src.GetType().GetProperty(propName);

            if (property is null)
                return null;

            var value = property.GetValue(src, null);

            if (value is null)
                return null;

            if (value.GetType().IsEnum)
                return Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));

            return value;
        }
    }
}