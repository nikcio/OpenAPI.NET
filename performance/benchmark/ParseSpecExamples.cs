using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace performance;

[MemoryDiagnoser]
[JsonExporter]
[ShortRunJob]
public class ParseSpecExamples
{
    private readonly Dictionary<string, string> _contents = new(StringComparer.OrdinalIgnoreCase);
    private OpenApiReaderSettings _readerSettings;

    private const string SpecExamplesDir = "openapi-spec-examples";

    private static string FindRepoRoot()
    {
        // Walk up from the base directory until we find the solution file
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Microsoft.OpenApi.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not find repository root (Microsoft.OpenApi.sln)");
    }

    private static readonly (string Name, string File, string Format)[] Specs =
    [
        ("PetstoreMinimal", "petstore-minimal.json", OpenApiConstants.Json),
        ("ComprehensiveApi", "comprehensive-api.json", OpenApiConstants.Json),
        ("UmbracoManagementApi", "umbraco-management-api.json", OpenApiConstants.Json),
        ("GitHub", "github.yaml", OpenApiConstants.Yaml),
    ];

    [GlobalSetup]
    public void Setup()
    {
        _readerSettings = new OpenApiReaderSettings();
        _readerSettings.AddYamlReader();

        var specDir = Path.Combine(FindRepoRoot(), SpecExamplesDir);

        foreach (var (name, file, _) in Specs)
        {
            var path = Path.Combine(specDir, file);
            _contents[name] = File.ReadAllText(path);
        }
    }

    [Benchmark]
    public ReadResult PetstoreMinimal()
    {
        return OpenApiDocument.Parse(_contents["PetstoreMinimal"], OpenApiConstants.Json, _readerSettings);
    }

    [Benchmark]
    public ReadResult ComprehensiveApi()
    {
        return OpenApiDocument.Parse(_contents["ComprehensiveApi"], OpenApiConstants.Json, _readerSettings);
    }

    [Benchmark]
    public ReadResult UmbracoManagementApi()
    {
        return OpenApiDocument.Parse(_contents["UmbracoManagementApi"], OpenApiConstants.Json, _readerSettings);
    }

    [Benchmark]
    public ReadResult GitHub()
    {
        return OpenApiDocument.Parse(_contents["GitHub"], OpenApiConstants.Yaml, _readerSettings);
    }
}
