ResxToCs
========

A .NET tools that converts a Resx code into C# code.

## Installation
This library can be installed through NuGet - [http://nuget.org/packages/dotnet-resx2cs](http://nuget.org/packages/dotnet-resx2cs).

The CLI tool must be installed into `.csproj` file:

```xml
<ItemGroup>
	<DotNetCliToolReference Include="dotnet-resx2cs" Version="1.0.0-alpha1" />
</ItemGroup>

<Target Name="СonvertResxToCs" BeforeTargets="BeforeBuild">
	<Exec Command="dotnet resx2cs" />
</Target>
```

## Release History
See the [changelog](CHANGELOG.md).

## License
[Apache License Version 2.0](LICENSE.txt)