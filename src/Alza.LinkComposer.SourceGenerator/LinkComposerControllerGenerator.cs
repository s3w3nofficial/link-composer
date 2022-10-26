using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Alza.LinkComposer.SourceGenerator
{

    [Generator]
    public class LinkComposerControllerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {

        }

        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

            Debug.WriteLine("Execute code generator");

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
