# FastBuilder 
FastBuilder is a net 5 library that allows you to speed up build process for faster iterations 
FastBuilder is based on idea of omitting full build process that includes neget packages check and references check.
FastBuilder analises output of a common "dotnet build" command and retrieves full dependency graph and all roslyn command arguments.
After the first dotnet build FastBuilder watches for source and dependencies changes.
If only source files were changed or added, FastBuilder changes arguments and calls only final roslyn command omitting all the preparation steps.

No dependencies allow it to integrate in any project with minimal impact, but FastBuilder relies on assumption that dotnet cli tools are installed and working correctly.

Rough measurements for hello world type of program where sources are changed by introducing new types

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