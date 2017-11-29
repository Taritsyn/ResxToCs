ResxToCs
========

A set of .NET tools that converts a Resx code into C# code.

## ResxToCs.Core
A .NET library that converts a Resx code into C# code.

### Installation
This library can be installed through NuGet - [http://nuget.org/packages/ResxToCs.Core](http://nuget.org/packages/ResxToCs.Core).

### Usage
*Coming soon…*

## dotnet-resx2cs
A .NET Core CLI tool that converts the `.resx` files to the `.Designer.cs` files.

### Installation
This tool can be installed through NuGet - [http://nuget.org/packages/dotnet-resx2cs](http://nuget.org/packages/dotnet-resx2cs). Installation of this package should be done by adding the following code into `.csproj` file:

```xml
<ItemGroup>
	<DotNetCliToolReference Include="dotnet-resx2cs" Version="…" />
</ItemGroup>
```

### Usage
In simplest case, you just need to add the following code into `.csproj` file:

```xml
<Target Name="СonvertResxToCs" BeforeTargets="BeforeCompile">
	<Exec Command="dotnet resx2cs" />
</Target>
```

If your `.resx` files are outside the project, then you can specify a another directory by using the following command:

```
dotnet resx2cs my-resource-directory
```

## ResxToCs.MSBuild
A MSBuild task that converts the `.resx` files to the `.Designer.cs` files.

### Installation
This tool can be installed through NuGet - [http://nuget.org/packages/ResxToCs.MSBuild](http://nuget.org/packages/ResxToCs.MSBuild).

### Usage
In simplest case, you do not need to do anything. But if your `.resx` files are outside the project, then you need to first disable the default target:

```xml
<PropertyGroup>
	… 
	<DisableDefaultResxToCsConversionTarget>true</DisableDefaultResxToCsConversionTarget>
	… 
</PropertyGroup>
```

Then add a new target and explicitly specify the value of `InputDirectory` property:

```xml
<Target Name="СonvertResxToCs" BeforeTargets="BeforeCompile">
	<ResxToCsTask InputDirectory="my-resource-directory" />
</Target>
```

## Release History
See the [changelog](CHANGELOG.md).

## License
[Apache License Version 2.0](LICENSE.txt)