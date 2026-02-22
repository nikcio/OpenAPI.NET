```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7840/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 7800X3D 4.20GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.311
  [Host]   : .NET 8.0.24 (8.0.24, 8.0.2426.7010), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.24 (8.0.24, 8.0.2426.7010), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                      | Mean       | Error       | StdDev    | Gen0   | Allocated |
|---------------------------- |-----------:|------------:|----------:|-------:|----------:|
| EmptyApiCallback            |   2.481 ns |   2.4479 ns | 0.1342 ns | 0.0006 |      32 B |
| EmptyApiComponents          |   3.910 ns |   4.3307 ns | 0.2374 ns | 0.0022 |     112 B |
| EmptyApiContact             |   2.981 ns |   4.8406 ns | 0.2653 ns | 0.0010 |      48 B |
| EmptyApiDiscriminator       |   3.296 ns |   6.4020 ns | 0.3509 ns | 0.0010 |      48 B |
| EmptyDocument               | 281.238 ns | 100.5558 ns | 5.5118 ns | 0.0224 |    1144 B |
| EmptyApiEncoding            |   4.607 ns |   6.0391 ns | 0.3310 ns | 0.0016 |      80 B |
| EmptyApiExample             |   3.147 ns |   3.6035 ns | 0.1975 ns | 0.0014 |      72 B |
| EmptyApiExternalDocs        |   3.121 ns |   3.3864 ns | 0.1856 ns | 0.0008 |      40 B |
| EmptyApiHeader              |   3.131 ns |   3.3111 ns | 0.1815 ns | 0.0016 |      80 B |
| EmptyApiInfo                |   3.308 ns |   1.7112 ns | 0.0938 ns | 0.0016 |      80 B |
| EmptyApiLicense             |   2.691 ns |   4.2087 ns | 0.2307 ns | 0.0010 |      48 B |
| EmptyApiLink                |   2.905 ns |   2.2682 ns | 0.1243 ns | 0.0014 |      72 B |
| EmptyApiMediaType           |   2.913 ns |   0.5192 ns | 0.0285 ns | 0.0016 |      80 B |
| EmptyApiOAuthFlow           |   2.953 ns |   3.7799 ns | 0.2072 ns | 0.0013 |      64 B |
| EmptyApiOAuthFlows          |   3.036 ns |   3.0146 ns | 0.1652 ns | 0.0013 |      64 B |
| EmptyApiOperation           |  38.837 ns |  31.5235 ns | 1.7279 ns | 0.0075 |     376 B |
| EmptyApiParameter           |   3.504 ns |   3.0887 ns | 0.1693 ns | 0.0019 |      96 B |
| EmptyApiPathItem            |   2.944 ns |   1.3680 ns | 0.0750 ns | 0.0013 |      64 B |
| EmptyApiPaths               |  48.388 ns |  15.6882 ns | 0.8599 ns | 0.0049 |     248 B |
| EmptyApiRequestBody         |   3.099 ns |   1.8840 ns | 0.1033 ns | 0.0010 |      48 B |
| EmptyApiResponse            |   3.291 ns |   3.0696 ns | 0.1683 ns | 0.0013 |      64 B |
| EmptyApiResponses           |  35.719 ns |  19.9628 ns | 1.0942 ns | 0.0049 |     248 B |
| EmptyApiSchema              |   7.969 ns |   7.7347 ns | 0.4240 ns | 0.0083 |     416 B |
| EmptyApiSecurityRequirement |   6.886 ns |   4.1945 ns | 0.2299 ns | 0.0021 |     104 B |
| EmptyApiSecurityScheme      |   3.797 ns |   4.6495 ns | 0.2549 ns | 0.0021 |     104 B |
| EmptyApiServer              |   2.820 ns |   4.7039 ns | 0.2578 ns | 0.0011 |      56 B |
| EmptyApiServerVariable      |   2.612 ns |   1.2550 ns | 0.0688 ns | 0.0010 |      48 B |
| EmptyApiTag                 |   3.019 ns |   4.3093 ns | 0.2362 ns | 0.0014 |      72 B |
