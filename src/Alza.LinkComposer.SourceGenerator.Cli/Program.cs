using Alza.LinkComposer.SourceGenerator;
using Cocona;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

var builder = CoconaApp.CreateBuilder();
var app = builder.Build();

app.AddCommand("generate", async (string projectPath, string outputDirectoryPath) =>
{
    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), projectPath);
    var otputPath = Path.Combine(Directory.GetCurrentDirectory(), outputDirectoryPath);

    if (!fullPath.EndsWith(".csproj"))
        throw new Exception("Please specify valid .csproj path");

    MSBuildLocator.RegisterDefaults();

    using var workspace = MSBuildWorkspace.Create();
    var project = await workspace.OpenProjectAsync(fullPath);
    var compilation = await project.GetCompilationAsync();

    if (compilation is null)
        throw new Exception();

    string? projectName = null;

    if (project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.ProjectName", out var projectNameValue))
        projectName = projectNameValue;

    var syntaxTrees = compilation.SyntaxTrees;

    var files = new Dictionary<string, string>();

    foreach (var syntaxTree in syntaxTrees)
    {
        var model = compilation.GetSemanticModel(syntaxTree);

        var root = syntaxTree.GetRoot();

        new ControllerSyntaxWalker((string name, string source) =>
        {
            files.Add(name, source);
        }, model, projectName).Visit(root);
    }

    foreach (var key in files.Keys)
        File.WriteAllText(Path.Combine(otputPath, $"{key}.g.cs"), files[key]);
});

app.Run();
