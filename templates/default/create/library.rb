#!/usr/bin/env ruby

if ARGV[0] == 'get_file'
	puts ''
	exit
end

if ARGV[0] == 'get_position'
	puts '0|0'
	exit
end

if ARGV[0] == 'get_definition'
	puts "Creates a new C# library project"
	exit
end

def getProjectConent(name)
	content = "<?xml version=\"1.0\" encoding=\"utf-8\"?>
<Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">
	<PropertyGroup>
		<Configuration Condition=\" '$(Configuration)' == '' \">Debug</Configuration>
		<Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform>
		<ProductVersion>9.0.21022</ProductVersion>
		<SchemaVersion>2.0</SchemaVersion>
		<ProjectGuid>{FB9BD7EA-F1F8-4F9C-8B82-17E703C0C766}</ProjectGuid>
		<OutputType>Library</OutputType>
		<RootNamespace>#{name}</RootNamespace>
		<AssemblyName>#{name}</AssemblyName>
		<TargetFrameworkVersion>v3.5</TargetFrameworkVersion>
	</PropertyGroup>
	<PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">
		<DebugSymbols>true</DebugSymbols>
		<DebugType>full</DebugType>
		<Optimize>false</Optimize>
		<OutputPath>bin\\Debug</OutputPath>
		<DefineConstants>DEBUG</DefineConstants>
		<ErrorReport>prompt</ErrorReport>
		<WarningLevel>4</WarningLevel>
		<ConsolePause>false</ConsolePause>
	</PropertyGroup>
	<PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' \">
		<DebugType>none</DebugType>
		<Optimize>false</Optimize>
		<OutputPath>bin\\Release</OutputPath>
		<ErrorReport>prompt</ErrorReport>
		<WarningLevel>4</WarningLevel>
		<ConsolePause>false</ConsolePause>
	</PropertyGroup>
	<ItemGroup>
		<Reference Include=\"System\" />
	</ItemGroup>
	<ItemGroup>
		<Compile Include=\"AssemblyInfo.cs\" />
	</ItemGroup>
	<Import Project=\"$(MSBuildBinPath)\\Microsoft.CSharp.targets\" />
</Project>"
	content
end

def getAssemblyConent(name)
	content = "using System.Reflection;
using System.Runtime.CompilerServices;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle(\"#{name}\")]
[assembly: AssemblyDescription(\"\")]
[assembly: AssemblyConfiguration(\"\")]
[assembly: AssemblyCompany(\"\")]
[assembly: AssemblyProduct(\"\")]
[assembly: AssemblyCopyright(\"\")]
[assembly: AssemblyTrademark(\"\")]
[assembly: AssemblyCulture(\"\")]

// The assembly version has the format \"{Major}.{Minor}.{Build}.{Revision}\".
// The form \"{Major}.{Minor}.*\" will automatically update the build and revision,
// and \"{Major}.{Minor}.{Build}.*\" will update just the revision.

[assembly: AssemblyVersion(\"1.0.*\")]

// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile(\"\")]
"
	content
end

filepath = ARGV[0]
project_name = File.basename(filepath, File.extname(filepath))

File.open(filepath, "w") { |file|
	file.puts getProjectConent(project_name)
}

assembly = File.join(File.dirname(filepath), "AssemblyInfo.cs")
File.open(assembly, "w") { |file|
	file.puts getAssemblyConent(project_name)
}
