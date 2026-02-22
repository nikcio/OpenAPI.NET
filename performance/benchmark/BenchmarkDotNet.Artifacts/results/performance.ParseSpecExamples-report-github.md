```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7840/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 7800X3D 4.20GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.311
  [Host]   : .NET 8.0.24 (8.0.24, 8.0.2426.7010), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.24 (8.0.24, 8.0.2426.7010), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method               | Mean          | Error         | StdDev       | Gen0       | Gen1      | Gen2      | Allocated    |
|--------------------- |--------------:|--------------:|-------------:|-----------:|----------:|----------:|-------------:|
| PetstoreMinimal      |      34.03 μs |      19.44 μs |     1.066 μs |     1.2207 |         - |         - |      69.3 KB |
| ComprehensiveApi     |     197.11 μs |     166.53 μs |     9.128 μs |     7.8125 |    2.9297 |         - |    384.75 KB |
| UmbracoManagementApi |     307.11 μs |     150.66 μs |     8.258 μs |    11.7188 |    5.8594 |         - |    588.31 KB |
| GitHub               | 717,372.93 μs | 170,283.24 μs | 9,333.799 μs | 20000.0000 | 6000.0000 | 1000.0000 | 984780.34 KB |
