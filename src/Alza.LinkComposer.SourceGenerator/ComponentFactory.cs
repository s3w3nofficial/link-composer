using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Alza.LinkComposer.SourceGenerator
{
    public static class ComponentFactory
    {
        public static ClassDeclarationSyntax CreateControllerLinkClass(string className, string projectName, int? apiVersion = null)
        {
            var attribute = SyntaxFactory.Attribute(
                SyntaxFactory.IdentifierName(Constants.LinkComposerProjectInfo))
                    .AddArgumentListArguments(SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal($"{Constants.AlzaLinkComposerLinksNamesapce}.{projectName}"))));

            if (apiVersion != null)
                attribute = attribute
                    .AddArgumentListArguments(SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(apiVersion.Value))));

            return SyntaxFactory.ClassDeclaration(className)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword)))
                .AddAttributeLists(SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(attribute)))
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.ParseTypeName(Constants.LinkComposerController)));
        }

        public static AttributeListSyntax CreateLinkComposerRouteAttribute(string route, string controllerRoute, int? apiVersion = null)
        {
            var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(Constants.LinkComposerRoute))
                .AddArgumentListArguments(SyntaxFactory.AttributeArgument(
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(route ?? ""))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(controllerRoute ?? ""))));

            if (apiVersion != null)
                attribute = attribute
                    .AddArgumentListArguments(SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(apiVersion.Value))));

            return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));
        }

        public static string CreateComment(string route, string controllerRoute, int? apiVersion = null)
        {
            var comment = $" {string.Join("/", controllerRoute, route)}";

            apiVersion ??= 1;

            return comment.Replace("v{version:apiVersion}", $"v{apiVersion}");
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

        public static (ClassDeclarationSyntax cls, List<ITypeSymbol> additional) GetClassFromTypeSymbol(ITypeSymbol typeSymbol)
        {
            var additionalClasses = new List<ITypeSymbol>();

            var members = new List<MemberDeclarationSyntax>();

            var properties = typeSymbol
                .GetMembers()
                .Where(a => a.Kind == SymbolKind.Property)
                .Select(a => (IPropertySymbol)a)
                .Where(p =>
                {
                    var attributes = p.GetAttributes();
                    if (attributes.Any(a =>
                    {
                        var name = a.AttributeClass.ToString();

                        if (name.EndsWith("FromBodyAttribute"))
                            return true;

                        if (name.EndsWith("FromServicesAttribute"))
                            return true;

                        if (name.EndsWith("FromFormAttribute"))
                            return true;

                        return false;
                    }))
                        return false;

                    return true;
                })
                .Where(p => IsAutoProperty(p));

            foreach (var property in properties)
            {
                // HACK
                if (property.Type?.Kind is SymbolKind.ArrayType)
                    continue;

                if (property.Type?.Name == "CancellationToken")
                    continue;

                if (property.Type is INamedTypeSymbol namedType
                    && namedType.IsGenericType)
                {
                    var firstTypeArg = namedType.TypeArguments.FirstOrDefault();
                    if (IsTypeSymbolCustomClass(firstTypeArg) || IsTypeSymbolCustomEnum(firstTypeArg))
                        additionalClasses.Add(firstTypeArg);
                }

                if (IsTypeSymbolCustomClass(property.Type) || IsTypeSymbolCustomEnum(property.Type))
                    additionalClasses.Add(property.Type);

                members.Add(GetPropertyFromPropertySymbol(property));
            }

            var pmMembers = SyntaxFactory.List<MemberDeclarationSyntax>()
                .AddRange(members);

            var cls = SyntaxFactory.ClassDeclaration(typeSymbol.Name)
                .WithMembers(pmMembers)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            if (IsTypeSymbolDerived(typeSymbol))
                cls = cls.WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(
                                SyntaxFactory.IdentifierName(typeSymbol.BaseType.Name)))));

            return (cls, additionalClasses);
        }

        public static PropertyDeclarationSyntax GetPropertyFromPropertySymbol(IPropertySymbol propertySymbol)
        {
            var type = CreateTypeFromPropertySymbol(propertySymbol);
            var name = propertySymbol.Name;

            return SyntaxFactory.PropertyDeclaration(type, name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                     SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                     SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
             );
        }

        public static MethodDeclarationSyntax CreateControllerLinkAction(string comment, AttributeListSyntax attributes, string actionName, IEnumerable<ParameterSyntax> parameters)
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier(actionName))
                .WithAttributeLists(
                    SyntaxFactory.SingletonList(
                        attributes
                        .WithOpenBracketToken(
                            SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(
                                    SyntaxFactory.Trivia(
                                        SyntaxFactory.DocumentationCommentTrivia(
                                            SyntaxKind.SingleLineDocumentationCommentTrivia,
                                            SyntaxFactory.List(
                                                new XmlNodeSyntax[]{
                                                    SyntaxFactory.XmlText()
                                                    .WithTextTokens(
                                                        SyntaxFactory.TokenList(
                                                            SyntaxFactory.XmlTextLiteral(
                                                                SyntaxFactory.TriviaList(
                                                                    SyntaxFactory.DocumentationCommentExterior("///")),
                                                                " ",
                                                                " ",
                                                                SyntaxFactory.TriviaList()))),
                                                    SyntaxFactory.XmlExampleElement(
                                                        SyntaxFactory.SingletonList<XmlNodeSyntax>(
                                                            SyntaxFactory.XmlText()
                                                            .WithTextTokens(
                                                                SyntaxFactory.TokenList(
                                                                    new []{
                                                                        SyntaxFactory.XmlTextNewLine(
                                                                            SyntaxFactory.TriviaList(),
                                                                            "\n",
                                                                            "\n",
                                                                            SyntaxFactory.TriviaList()),
                                                                        SyntaxFactory.XmlTextLiteral(
                                                                            SyntaxFactory.TriviaList(
                                                                                SyntaxFactory.DocumentationCommentExterior("    ///")),
                                                                            comment,
                                                                            comment,
                                                                            SyntaxFactory.TriviaList()),
                                                                        SyntaxFactory.XmlTextNewLine(
                                                                            SyntaxFactory.TriviaList(),
                                                                            "\n",
                                                                            "\n",
                                                                            SyntaxFactory.TriviaList()),
                                                                        SyntaxFactory.XmlTextLiteral(
                                                                            SyntaxFactory.TriviaList(
                                                                                SyntaxFactory.DocumentationCommentExterior("    ///")),
                                                                            " ",
                                                                            " ",
                                                                            SyntaxFactory.TriviaList())}))))
                                                    .WithStartTag(
                                                        SyntaxFactory.XmlElementStartTag(
                                                            SyntaxFactory.XmlName(
                                                                SyntaxFactory.Identifier("summary"))))
                                                    .WithEndTag(
                                                        SyntaxFactory.XmlElementEndTag(
                                                            SyntaxFactory.XmlName(
                                                                SyntaxFactory.Identifier("summary")))),
                                                    SyntaxFactory.XmlText()
                                                    .WithTextTokens(
                                                        SyntaxFactory.TokenList(
                                                            SyntaxFactory.XmlTextNewLine(
                                                                SyntaxFactory.TriviaList(),
                                                                "\n",
                                                                "\n",
                                                                SyntaxFactory.TriviaList())))})))),
                                SyntaxKind.OpenBracketToken,
                                SyntaxFactory.TriviaList()))))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SeparatedList(parameters)))
                .WithBody(
                    SyntaxFactory.Block());
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

                var genericType = SyntaxFactory.ParseTypeName(SyntaxFactory.GenericName(SyntaxFactory.Identifier(
                    namedTypeSymbol.OriginalDefinition.Name),
                    SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(types))).ToFullString());

                var type = SyntaxFactory.ParseTypeName($"{genericType}");

                if (namedTypeSymbol.NullableAnnotation is NullableAnnotation.Annotated)
                    return SyntaxFactory.NullableType(type, SyntaxFactory.Token(SyntaxKind.QuestionToken));

                return type;
            }

            string name = namedTypeSymbol.Name;
            if (IsSystemNamespace(namedTypeSymbol))
                name = $"{namedTypeSymbol.ContainingSymbol}.{namedTypeSymbol.Name}";

            return SyntaxFactory.ParseTypeName(name);
        }

        public static ParameterSyntax CreateCleanParameter(ParameterSyntax p)
        {
            return SyntaxFactory.Parameter(SyntaxFactory.List<AttributeListSyntax>(), SyntaxFactory.TokenList(), p.Type, p.Identifier, p.Default);
        }

        public static SyntaxList<MemberDeclarationSyntax> CreateMembers(ClassDeclarationSyntax @class)
        {
            return SyntaxFactory.List<MemberDeclarationSyntax>()
                .Add(@class);
        }

        public static SyntaxList<UsingDirectiveSyntax> CreateUsings()
        {
            return SyntaxFactory.List<UsingDirectiveSyntax>()
                .Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(Constants.SystemNamespace)))
                .Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(Constants.SystemCollectionsGenericNamespace)))
                .Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(Constants.AlzaLinkComposerAttributesNamespace)));
        }

        public static NamespaceDeclarationSyntax CreateNamespace(string projectName, SyntaxList<UsingDirectiveSyntax> usings, SyntaxList<MemberDeclarationSyntax> members)
        {
            var identifier = projectName is null ? Constants.AlzaLinkComposerLinksNamesapce : $"{Constants.AlzaLinkComposerLinksNamesapce}.{projectName}";

            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(identifier),
                SyntaxFactory.List<ExternAliasDirectiveSyntax>(),
                usings, members).NormalizeWhitespace();
        }

        public static string GetNewClassName(string name)
        {
            return name.Replace("Controller", "ControllerLink");
        }

        public static TypeSyntax ExtractIfNullable(TypeSyntax type)
        {
            if (type is NullableTypeSyntax nullableType)
                return nullableType.ElementType;

            return type;
        }

        public static bool IsTypeArray(ITypeSymbol typeSymbol)
        {
            return typeSymbol.Kind == SymbolKind.ArrayType;
        }

        public static bool IsTypeSymbolCustomClass(ITypeSymbol typeSymbol)
        {
            return !typeSymbol.IsValueType
                && typeSymbol.SpecialType != SpecialType.System_String
                && !IsSystemNamespace(typeSymbol);
        }

        public static bool IsTypeSymbolCustomEnum(ITypeSymbol typeSymbol)
        {
            return typeSymbol.TypeKind is TypeKind.Enum
                && !IsSystemNamespace(typeSymbol);
        }
        
        public static bool IsTypeSymbolDerived(ITypeSymbol typeSymbol)
        {
            return typeSymbol.BaseType != null && typeSymbol.BaseType.Name != "Object";
        }

        public static bool IsSystemNamespace(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.ContainingSymbol is null)
                return false;

            if (typeSymbol.ContainingSymbol.ToString().StartsWith(Constants.SystemNamespace))
                return true;

            if (typeSymbol.ContainingSymbol.ToString().StartsWith(Constants.MicrosoftAspNetCoreHttpNamespace))
                return true;

            return false;
        }

        private static bool IsAutoProperty(IPropertySymbol propertySymbol)
        {
            var fields = propertySymbol.ContainingType.GetMembers().OfType<IFieldSymbol>();
            return fields.Any(field => SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, propertySymbol));
        }

        public static bool IsController(string name)
        {
            if (name.EndsWith("Controller"))
                return true;

            var split = name.Split(new string[] { "ControllerV" }, System.StringSplitOptions.None);

            if (int.TryParse(split?.Last(), out int t))
                return true;

            return false;
        }
    }
}
