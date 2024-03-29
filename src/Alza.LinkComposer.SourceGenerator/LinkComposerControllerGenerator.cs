﻿using Microsoft.CodeAnalysis;
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
            /*
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            */
#endif

            Debug.WriteLine("Execute code generator");

            string projectName = null;

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ProjectName", out var projectNameValue))
                projectName = projectNameValue;

            var syntaxTrees = context.Compilation.SyntaxTrees;

            foreach (var syntaxTree in syntaxTrees)
            {
                var model = context.Compilation.GetSemanticModel(syntaxTree);

                var root = syntaxTree.GetRoot();

                new ControllerSyntaxWalker((string name, string source) =>
                {
                    context.AddSource(name, source);
                }, model, projectName).Visit(root);
            }
        }
    }
}
