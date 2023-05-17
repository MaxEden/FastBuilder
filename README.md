# FastBuilder 
FastBuilder is a Net 5 library that allows you to speed up a build process for faster iterations. FastBuilder is based on the idea of omitting the full build process that includes checking nuget packages and references. FastBuilder parses the output of a common "dotnet build" command and retrieves the full dependency graph and all roslyn command arguments. After the initial dotnet build, FastBuilder watches for changes to the source and dependencies. If only source files have been changed or added, FastBuilder changes the arguments and calls only the final roslyn command, omitting all preparation steps.

There are no dependencies, so it can be integrated into any project with minimal impact. However, FastBuilder does assume that the dotnet cli tools are installed and working correctly.

Rough measurements for a hello world type of program where the sources are being changed by the introduction of new types

regular dotnet build
```
Finished dotnet build first in 2088,2467 ms

Finished dotnet build second in 1131,4602 ms
```

fast builder
```
FastBuilder: -> H:\GITS\Mandarin\FastBuilder\TestHelloWorld\bin\Debug\net5.0\TestHelloWorld.dll
Finished First build in 2139,0242 ms

FastBuilder: fast -> H:\GITS\Mandarin\FastBuilder\TestHelloWorld\bin\Debug\net5.0\TestHelloWorld.dll
Finished Second build 0 in 844,705 ms

FastBuilder: fast -> H:\GITS\Mandarin\FastBuilder\TestHelloWorld\bin\Debug\net5.0\TestHelloWorld.dll
Finished Second build 1 in 871,3692 ms

FastBuilder: fast -> H:\GITS\Mandarin\FastBuilder\TestHelloWorld\bin\Debug\net5.0\TestHelloWorld.dll
Finished Second build 2 in 891,5943 ms

FastBuilder: fast -> H:\GITS\Mandarin\FastBuilder\TestHelloWorld\bin\Debug\net5.0\TestHelloWorld.dll
Finished Second build 3 in 862,7999 ms

FastBuilder: fast -> H:\GITS\Mandarin\FastBuilder\TestHelloWorld\bin\Debug\net5.0\TestHelloWorld.dll
Finished Second build 4 in 936,6807 ms
```
