using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading;

namespace Alza.LinkComposer.SourceGenerator
{
    public class ControllerSyntaxWalker : CSharpSyntaxWalker
    {
        private ControllerLinkBuilder _controllerLinkBuilder;

        private readonly GeneratorExecutionContext _context;
        private readonly SemanticModel _semanticModel;
        private readonly string _projectName;

        public ControllerSyntaxWalker(GeneratorExecutionContext context, SemanticModel semanticModel, string projectName)
        {
            _context = context;
            _semanticModel = semanticModel;
            _projectName = projectName;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (!ComponentFactory.IsController(node.Identifier.Text))
                return;

            var className = ComponentFactory.GetNewClassName(node.Identifier.ToString());

            var controllerRouteAttribute = node.AttributeLists.FirstOrDefault(a => a.ToString().StartsWith("[RoutePrefix("));

            if (controllerRouteAttribute is null)
                controllerRouteAttribute = node.AttributeLists.FirstOrDefault(a => a.ToString().StartsWith("[Route("));

            var controllerRouteAttributeValue = controllerRouteAttribute?.Attributes.FirstOrDefault()?.ArgumentList?.Arguments.FirstOrDefault()?.ToFullString()?.Replace("\"", "");

            // Get ApiVersion attribute value
            int? apiVersion = null;
            var apiVersionAttribute = node.AttributeLists.FirstOrDefault(a => a.ToString().StartsWith("[ApiVersion("));
            if (apiVersionAttribute != null)
            {
                var tmp = apiVersionAttribute?.Attributes.FirstOrDefault().ArgumentList?.Arguments.FirstOrDefault().ToFullString()?.Replace("\"", "");
                if (int.TryParse(tmp, out int version))
                    apiVersion = version;
            }

            _controllerLinkBuilder = new ControllerLinkBuilder(className, _context.Compilation.AssemblyName, controllerRouteAttributeValue, _semanticModel, apiVersion);

            base.VisitClassDeclaration(node);

            var controllerLink = _controllerLinkBuilder.Build();

            var usings = ComponentFactory.CreateUsings();
            var members = ComponentFactory.CreateMembers(controllerLink);
            var ns = ComponentFactory.CreateNamespace(_projectName, usings, members);

            _context.AddSource($"{className}.g.cs", ns.ToFullString());
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var attributes = node.AttributeLists;
            var httpAttribute = attributes.FirstOrDefault(a => a.ToString().StartsWith("[Http"));

            if (httpAttribute == null)
                return;

            var routeAttribute = attributes.FirstOrDefault(a => a.ToString().StartsWith("[Route("));
            var routeAttributeValue = routeAttribute?.Attributes.FirstOrDefault()?.ArgumentList?.Arguments.FirstOrDefault()?.ToFullString()?.Replace("\"", "");

            var httpAttributeValue = httpAttribute.Attributes.FirstOrDefault()?.ArgumentList?.Arguments.FirstOrDefault()?.ToFullString()?.Replace("\"", "");

            // Big hack
            if (httpAttributeValue != null && httpAttributeValue.StartsWith("Name"))
                httpAttributeValue = null;

            var route = httpAttributeValue ?? routeAttributeValue;

            // Get ApiVersion attribute value
            int? apiVersion = null;
            var apiVersionAttribute = attributes.FirstOrDefault(a => a.ToString().StartsWith("[ApiVersion("));
            if (apiVersionAttribute != null)
            {
                var tmp = apiVersionAttribute?.Attributes.FirstOrDefault().ArgumentList?.Arguments.FirstOrDefault().ToFullString()?.Replace("\"", "");
                if (int.TryParse(tmp, out int version))
                    apiVersion = version;
            }

            var actionName = node.Identifier.ToString();

            // exclude parameters
            var actionParameters = node.ParameterList.Parameters
                .Where(ap => IsValidParameter(ap))
                .Where(ap => ap.Type?.ToString() != nameof(CancellationToken));

            _controllerLinkBuilder.AddMethod(route, actionName, actionParameters, apiVersion);

            base.VisitMethodDeclaration(node);
        }

        private bool IsValidParameter(ParameterSyntax ap)
        {
            if (ap.Type is null)
                return false;

            if (ap.AttributeLists
                .Any(a => a.ToString().Contains(Constants.FromBodyAttribute) 
                    || a.ToString().Contains(Constants.FromServicesAttribute) 
                    || a.ToString().Contains(Constants.FromFormAttribute)))
            {
                return false;
            }

            return true;
        }
    }
}
