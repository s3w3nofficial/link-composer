using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Alza.LinkComposer.SourceGenerator
{
    public sealed class ControllerLinkBuilder
    {
        private ClassDeclarationSyntax _classDeclarationSyntax;
        private readonly string _routeAttributeValue;
        private readonly SemanticModel _semanticModel;

        private List<MethodDeclarationSyntax> _methods = new();
        private List<EnumDeclarationSyntax> _enums = new();
        private List<ClassDeclarationSyntax> _classes = new();

        public ControllerLinkBuilder(string name, string assemblyName, string routeAttributeValue, SemanticModel semanticModel)
        {
            _classDeclarationSyntax = ComponentFactory.CreateControllerLinkClass(name, assemblyName);
            _routeAttributeValue = routeAttributeValue;
            _semanticModel = semanticModel;
        }

        public void AddMethod(string route, string name, IEnumerable<ParameterSyntax> parameters)
        {
            var routeAttribute = ComponentFactory.CreateLinkComposerRouteAttribute(route, _routeAttributeValue);

            // if parameter is a custom type recreate it
            AddCustomTypes(parameters);

            // this removes all the attributes and other unused info
            var cleanParameters = parameters
                .Select(ap => ComponentFactory.CreateCleanParameter(ap));

            // add number behind method if method with same name and parameters exists
            var newName = GetMethodName(name, cleanParameters);

            // create method
            var method = ComponentFactory.CreateControllerLinkAction(routeAttribute, newName, cleanParameters);

            _methods.Add(method);
        }

        public void AddEnum(EnumDeclarationSyntax enumDeclarationSyntax)
        {
            _enums.Add(enumDeclarationSyntax);
        }

        public ClassDeclarationSyntax Build()
        {
           _classDeclarationSyntax = _classDeclarationSyntax
                .AddMembers(_methods.ToArray())
                .AddMembers(_enums.ToArray())
                .AddMembers(_classes.ToArray());

            return _classDeclarationSyntax;
        }

        #region InternalMethods
        private void AddCustomTypes(IEnumerable<ParameterSyntax> parameters)
        {
            foreach (var parameter in parameters)
            {
                var type = ComponentFactory.ExtractIfNullable(parameter.Type);

                var typeInfo = _semanticModel.GetTypeInfo(type);
                var semanticType = typeInfo.Type;

                // Work Around
                if (semanticType is null)
                    continue;

                if (ComponentFactory.IsTypeArray(semanticType))
                    semanticType = ((IArrayTypeSymbol)semanticType).ElementType;

                if (ComponentFactory.IsTypeSymbolCustomEnum(semanticType))
                {
                    var enumDeclaration = ComponentFactory.GetEnumFromTypeSymbol(semanticType);
                    AddEnum(enumDeclaration);
                }

                if (ComponentFactory.IsTypeSymbolCustomClass(semanticType))
                {
                    var members = new Dictionary<ITypeSymbol, MemberDeclarationSyntax>();
                    var types = new Queue<ITypeSymbol>();
                    types.Enqueue(semanticType);

                    while (types.Count > 0)
                    {
                        var current = types.Dequeue();

                        if (members.ContainsKey(current))
                            continue;

                        if (_classes.Any(c => c.Identifier.Text == current.Name))
                            continue;

                        if (_enums.Any(e => e.Identifier.Text == current.Name))
                            continue;

                        if (ComponentFactory.IsTypeSymbolCustomEnum(current))
                        {
                            var e = ComponentFactory.GetEnumFromTypeSymbol(current);
                            members.Add(current, e);

                            _enums.Add(e);

                            continue;
                        }

                        var res = ComponentFactory.GetClassFromTypeSymbol(current);

                        members.Add(current, res.cls);

                        foreach (var addition in res.additional)
                            if (!types.Any(a => a.Equals(addition)))
                                types.Enqueue(addition);

                        _classes.Add(res.cls);
                    }
                }
            }
        }

        private string GetMethodName(string name, IEnumerable<ParameterSyntax> parameters)
        {
            int methodNumber = 1;
            string newName = name;
            var methodParameterNames = parameters.Select(p => p.Type.ToFullString()).ToArray();
            do
            {
                var existingMethod = _methods.FirstOrDefault(m => m.Identifier.Text == newName);
                if (existingMethod is null)
                    break;

                var existingMethodParameters = existingMethod.ParameterList.Parameters.Select(p => p.Type.ToFullString()).ToArray();
                if (!HasSameParams(methodParameterNames, existingMethodParameters))
                    break;

                newName = $"{name}{methodNumber}";
                methodNumber += 1;
            }
            while (true);

            return newName;
        }

        private static bool HasSameParams(string[] params1, string[] params2)
        {
            if (params1.Length != params2.Length)
                return false;

            for (int i = 0; i < params1.Length; i++)
                if (params1[i] != params2[i])
                    return false;

            return true;
        }

        #endregion
    }
}
