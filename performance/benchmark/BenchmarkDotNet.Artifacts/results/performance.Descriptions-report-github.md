```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7840/25H2/2025Update/HudsonValley2)
AMD Ryzen 7 7800X3D 4.20GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.311
  [Host]   : .NET 8.0.24 (8.0.24, 8.0.2426.7010), X64 RyuJIT x86-64-v4
  ShortRun : .NET 8.0.24 (8.0.24, 8.0.2426.7010), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method       | Mean          | Error           | StdDev        | Gen0       | Gen1      | Gen2      | Allocated    |
|------------- |--------------:|----------------:|--------------:|-----------:|----------:|----------:|-------------:|
| PetStoreYaml |     221.54 μs |       199.72 μs |     10.947 μs |     3.9063 |         - |         - |    232.09 KB |
| PetStoreJson |      95.89 μs |        65.62 μs |      3.597 μs |     3.4180 |    0.4883 |         - |    171.02 KB |
| GHESYaml     | 346,680.73 μs |   161,357.87 μs |  8,844.569 μs |  4000.0000 | 3000.0000 | 1000.0000 |  182320.4 KB |
| GHESJson     | 218,480.07 μs | 1,193,323.64 μs | 65,410.095 μs |  1000.0000 |         - |         - | 110658.27 KB |
| GHES3_1Yaml  | 672,202.90 μs |   198,308.77 μs | 10,869.973 μs | 20000.0000 | 6000.0000 | 1000.0000 | 975757.91 KB |
| GHES3_1Json  | 497,114.63 μs |   263,760.44 μs | 14,457.600 μs | 20000.0000 | 8000.0000 | 2000.0000 | 901972.15 KB |
