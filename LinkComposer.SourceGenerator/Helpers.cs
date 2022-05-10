using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace LinkComposer.SourceGenerator
{
    public static class Helpers
    {
        public static ClassDeclarationSyntax CreateControllerLinkClass(string className, string assemblyName)
        {
            return SyntaxFactory.ClassDeclaration(className)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .AddAttributeLists(SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.IdentifierName("LinkComposerProjectInfo"))
                                .AddArgumentListArguments(SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(assemblyName)))))))
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.ParseTypeName("LinkComposerController")));
        }

        public static AttributeListSyntax CreateLinkComposerRouteAttribute(string route, string controllerRoute)
        {
            return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("LinkComposerRoute"))
                .AddArgumentListArguments(SyntaxFactory.AttributeArgument(
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(route ?? ""))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(controllerRoute ?? ""))))));
        }

        public static EnumDeclarationSyntax GetEnumFromTypeSymbol(ITypeSymbol typeSymbol)
        {
            var fields = typeSymbol
                .GetMembers()
                .Where(a => a.Kind == SymbolKind.Field)
                .Select(a => (IFieldSymbol)a)
                .Select(f =>
                {
                    return SyntaxFactory.EnumMemberDeclaration(f.Name);
                });

            var members = SyntaxFactory.SeparatedList<EnumMemberDeclarationSyntax>()
                .AddRange(fields);

            var enumDeclaration = SyntaxFactory.EnumDeclaration(typeSymbol.Name)
                .WithMembers(members)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            return enumDeclaration;
        }

        public static (ClassDeclarationSyntax cls, List<ClassDeclarationSyntax> additional) GetClassFromTypeSymbol(ITypeSymbol typeSymbol)
        {
            var additionalClasses = new List<ClassDeclarationSyntax>();

            var members = new List<MemberDeclarationSyntax>();

            var properties = typeSymbol
                .GetMembers()
                .Where(a => a.Kind == SymbolKind.Property)
                .Select(a => (IPropertySymbol)a);

            foreach (var property in properties)
            {
                if (IsTypeSymbolCustomClass(property.Type))
                {
                    var reuslt = GetClassFromTypeSymbol(property.Type);
                    additionalClasses.Add(reuslt.cls);
                    additionalClasses.AddRange(reuslt.additional);
                }

                members.Add(GetPropertyFromPropertySymbol(property));
            }

            var pmMembers = SyntaxFactory.List<MemberDeclarationSyntax>()
                .AddRange(members);

            var cls = SyntaxFactory.ClassDeclaration(typeSymbol.Name)
                .WithMembers(pmMembers)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            return (cls, additionalClasses);
        }

        public static PropertyDeclarationSyntax GetPropertyFromPropertySymbol(IPropertySymbol propertySymbol)
        {
            var type = CreateTypeFromPropertySymbol(propertySymbol);

            return SyntaxFactory.PropertyDeclaration(type, propertySymbol.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                     SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                     SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
             );
        }

        public static bool IsTypeSymbolCustomClass(ITypeSymbol typeSymbol)
        {
            return !typeSymbol.IsValueType
                && typeSymbol.SpecialType != SpecialType.System_String
                && !IsSystemNamespace(typeSymbol);
        }

        public static bool IsSystemNamespace(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.ContainingAssembly.Name.StartsWith("System"))
                return true;

            return false;
        }

        public static MethodDeclarationSyntax CreateControllerLinkAction(AttributeListSyntax attributes, string actionName, IEnumerable<ParameterSyntax> parameters)
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.List(new[] { attributes }),
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword)),
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                null,
                SyntaxFactory.Identifier(actionName),
                null,
                SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)),
                SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
                SyntaxFactory.Block(),
                null);
        }

        public static TypeSyntax CreateTypeFromPropertySymbol(IPropertySymbol propertySymbol)
        {
            var namedType = (INamedTypeSymbol)propertySymbol.Type;
            return CreateTypeFromNamedTypeSymbol(namedType);
        }

        private static TypeSyntax CreateTypeFromNamedTypeSymbol(INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsGenericType)
            {
                if (namedTypeSymbol.NullableAnnotation is NullableAnnotation.Annotated
                    && namedTypeSymbol.OriginalDefinition.Name == "Nullable")
                {
                    var nullableType = SyntaxFactory.ParseTypeName(namedTypeSymbol.TypeArguments.FirstOrDefault().Name);
                    return SyntaxFactory.NullableType(nullableType, SyntaxFactory.Token(SyntaxKind.QuestionToken));
                }

                var types = new List<TypeSyntax>();
                foreach (var arg in namedTypeSymbol.TypeArguments)
                    types.Add(CreateTypeFromNamedTypeSymbol((INamedTypeSymbol)arg));

                var type = SyntaxFactory.ParseTypeName(SyntaxFactory.GenericName(SyntaxFactory.Identifier(
                    namedTypeSymbol.OriginalDefinition.Name),
                    SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(types))).ToFullString());

                if (namedTypeSymbol.NullableAnnotation is NullableAnnotation.Annotated)
                    return SyntaxFactory.NullableType(type, SyntaxFactory.Token(SyntaxKind.QuestionToken));

                return type;
            }

            return SyntaxFactory.ParseTypeName(namedTypeSymbol.Name);
        }
    }
}
