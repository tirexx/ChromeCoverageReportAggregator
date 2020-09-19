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

## How to use
Iterate every page of your website and all resolutions you support, move the mouse on elements with hover animation. Trigger all the javascript events. Save coverage reports using chrome into some folder.

Than run
```
Hosts\Aggregator.Hosts.Console\bin\Debug\netcoreapp3.1\Aggregator.Hosts.Console.exe aggregate -i "C:\temp\coverage_reports" -o "D:\temp\aggregated_coverage_reports"
```
Check the output folder for resources report. Identify files that has most uncoverd volume.
```
|   %   | Not covered|    Covered   |   Length   | Url
|  0.73 |   470.6 KB |       3.5 KB |   474.0 KB | https://...
```
Check the output folder for the files, containing combined coverage. For css file, manually compare them with the original files to identify missing media queries, hover styles, @fonts etc. excluded by chrome coverage.

#### Changelog
* 2020.8.16
	* initial
* 2020.8.21
	* bug fixes
* 2020.8.29
	* coverage report added
	* excluded empty files report added
