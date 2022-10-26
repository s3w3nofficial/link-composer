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

        public ControllerSyntaxWalker(GeneratorExecutionContext context, SemanticModel semanticModel)
        {
            _context = context;
            _semanticModel = semanticModel;
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

            _controllerLinkBuilder = new ControllerLinkBuilder(className, _context.Compilation.AssemblyName, controllerRouteAttributeValue, _semanticModel);

            base.VisitClassDeclaration(node);

            var controllerLink = _controllerLinkBuilder.Build();

            var usings = ComponentFactory.CreateUsings();
            var members = ComponentFactory.CreateMembers(controllerLink);
            var ns = ComponentFactory.CreateNamespace(usings, members);

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

            var actionName = node.Identifier.ToString();

            // exclude parameters
            var actionParameters = node.ParameterList.Parameters
                .Where(ap => IsValidParameter(ap))
                .Where(ap => ap.Type?.ToString() != nameof(CancellationToken));

            _controllerLinkBuilder.AddMethod(route, actionName, actionParameters);

            base.VisitMethodDeclaration(node);
        }

        private bool IsValidParameter(ParameterSyntax ap)
        {
            if (ap.Type is null)
                return false;

            if (ap.AttributeLists
                .Any(a => a.ToString().Contains(Constants.FromBodyAttribute) || a.ToString().Contains(Constants.FromServicesAttribute)))
                return false;

            return true;
        }
    }
}
