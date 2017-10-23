ResxToCs
========

A .NET Core CLI tool that converts the `*.resx` files to the `*.Designer.cs` files.

## Installation
This library can be installed through NuGet - [http://nuget.org/packages/dotnet-resx2cs](http://nuget.org/packages/dotnet-resx2cs).

The CLI tool must be installed into `.csproj` file:

```xml
<ItemGroup>
	<DotNetCliToolReference Include="dotnet-resx2cs" Version="1.0.0-alpha1" />
</ItemGroup>

<Target Name="PrecompileScript" BeforeTargets="BeforeBuild" Condition=" '$(IsCrossTargetingBuild)' != 'true' ">
	<Exec Command="dotnet resx2cs" />
</Target>
```

## Release History
See the [changelog](CHANGELOG.md).

## License
[Apache License Version 2.0](LICENSE.txt)