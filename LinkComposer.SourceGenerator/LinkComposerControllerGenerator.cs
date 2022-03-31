using Microsoft.CodeAnalysis;

namespace LinkComposer.SourceGenerator
{

    [Generator]
    public class LinkComposerControllerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {

        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxTrees = context.Compilation.SyntaxTrees;

            foreach (var syntaxTree in syntaxTrees)
            {
                var model = context.Compilation.GetSemanticModel(syntaxTree);

                var root = syntaxTree.GetRoot();

                new ControllerSyntaxWalker(context, model).Visit(root);
            }
        }
    }
}
