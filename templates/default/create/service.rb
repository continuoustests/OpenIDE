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
	puts "Creates a new C# windows service project"
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
		<ProjectGuid>{FE73B0F1-8331-4F95-AC4E-3D114D93B97D}</ProjectGuid>
		<OutputType>WinExe</OutputType>
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
		<DebugType>pdbonly</DebugType>
		<Optimize>true</Optimize>
		<OutputPath>bin\\Release</OutputPath>
		<ErrorReport>prompt</ErrorReport>
		<WarningLevel>4</WarningLevel>
		<ConsolePause>false</ConsolePause>
	</PropertyGroup>
	<PropertyGroup>
    	<StartupObject>#{name}.Program</StartupObject>
	</PropertyGroup>
	<ItemGroup>
		<Reference Include=\"System\" />
		<Reference Include=\"System.ServiceProcess\" />
		<Reference Include=\"System.Configuration.Install\" />
	</ItemGroup>
	<ItemGroup>
	    <Compile Include=\"#{name}Service.cs\">
    	  <SubType>Component</SubType>
	    </Compile>
		<Compile Include=\"Program.cs\" />
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

def getService(name)
	content = "using System.ServiceProcess;
namespace #{name}
{
	public class #{name}Service : ServiceBase
	{
		public #{name}Service()
		{
			ServiceName = \"#{name}\";
		}

		protected override void OnStart(string[] args)
        {
		}

		protected override void OnStop()
        {
		}
	}
}
"
	content
end

def getProgram(name)
	content = "using System;
using System.ServiceProcess;

namespace #{name}
{
	static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
			{ 
				new #{name}Service()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
"
	content
end

def write(filepath, contents)
	File.open(filepath, "w") { |file|
		file.puts contents 
	}
end

def file_name(path, name)
	File.join(File.dirname(path), name)
end

filepath = ARGV[0]
project_name = File.basename(filepath, File.extname(filepath))
write(filepath, getProjectConent(project_name))

write(file_name(filepath, "AssemblyInfo.cs"), getAssemblyConent(project_name))
write(file_name(filepath, project_name + "Service.cs"), getService(project_name))
write(file_name(filepath, "Program.cs"), getProgram(project_name))

