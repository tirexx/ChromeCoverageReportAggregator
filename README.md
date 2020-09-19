# ChromeCoverageReportAggregator
NETCore 3.1 Chrome coverage reports aggregator.

Every single star makes maintainer happy! ⭐

## Build
```
dotnet build ChromeCoverageReportAggregator.sln 
```

## Get command line help
```
Hosts\Aggregator.Hosts.Console\bin\Debug\netcoreapp3.1\Aggregator.Hosts.Console.exe help
Hosts\Aggregator.Hosts.Console\bin\Debug\netcoreapp3.1\Aggregator.Hosts.Console.exe help aggregate
```


## Run
Iterate every page of your website and all resolutions you support, move the mouse on elements with hover animation. Save coverage reports using chrome into some folder.
Than run
```
Hosts\Aggregator.Hosts.Console\bin\Debug\netcoreapp3.1\Aggregator.Hosts.Console.exe aggregate -i "C:\temp\coverage_reports" -o "D:\temp\aggregated_coverage_reports"
```
Check the output folder for the files, containing combined coverage. For css file, manually compare them with the original files to identify missing media queries, hover styles, @fonts etc. excluded by chrome coverage.

#### Changelog
* 2020.8.16
	* initial
