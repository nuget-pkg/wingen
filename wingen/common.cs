//+#nuget CommandLineParser@2.9.1;
//+#inc   CscsUtil.cs
//+#def   USE_CSCS_UTIL
//+#def   XYZ
//+#ico   app.ico

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using Global;
using Local;
using static Global.EasyObject;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Main;

internal class Options
{
    [Option('d', "dll", HelpText = "Generate DLL project.", Required = false)]
    public bool GenerateDllProject { get; set; }

    [Option('s', "single", HelpText = "Generate file-based app code only.", Required = false)]
    public bool SingleOnly { get; set; }

    [Option('c', "convert", HelpText = "Convert to normal project.", Required = false)]
    public bool ConvertToProject { get; set; }

    // [Value(1, MetaName = "Source File", HelpText = "Please specify one cs source file name.", Required = true)]
    // public string srcFile { get; set; }
    [Value(1, MetaName = "Source File List", HelpText = "Please specify cs source file names.", Required = false)]
    public IEnumerable<string>? SrcFileList { get; set; }
}

public static class Program
{
    private const string RunShTemplate = """
                                         # /usr/bin/env bash.exe
                                         # -*- mode: sh -*-
                                         set -e
                                         script_dir="$(dirname "$0")"
                                         script_dir="$(realpath $script_dir)"
                                         {{GENERATOR}} $script_dir/{{PROGRAM}}.cs
                                         if [ "$1" == "@vs" ]; then
                                           devenv.exe $script_dir/.build/{{PROGRAM}}/{{PROGRAM}}.csproj &
                                         elif [ "$1" == "@rider" ]; then
                                           rider64.exe $script_dir/.build/{{PROGRAM}}/{{PROGRAM}}.csproj &
                                         elif [ "$1" == "@clean" ]; then
                                         {{CLEAN_STEP}}
                                         elif [ "$1" == "@build" ]; then
                                         {{BUILD_STEP}}
                                         elif [ "$1" == "@merge" ]; then
                                         {{MERGE_STEP}}
                                         elif [ "$1" == "@pack" ]; then
                                         {{PACK_STEP}}
                                         elif [ "$1" == "@check" ]; then
                                           cd $script_dir/.build/{{PROGRAM}}
                                           dotnet list package --outdated
                                         elif [ "$1" == "@update" ]; then
                                           cd $script_dir/.build/{{PROGRAM}}
                                           dotnet package update
                                         else
                                           dotnet run --property WarningLevel=0 --project $script_dir/.build/{{PROGRAM}}/{{PROGRAM}}.csproj $*
                                         fi
                                         """;

    private const string CleanShTemplate = Const.CleanShTemplate;
    private const string BuildShTemplate = Const.BuildShTemplate;
    private const string MergeShTemplate = Const.MergeShTemplate;
    private const string PackShTemplate = Const.PackShTemplate;

    private const string Template = """
                                    <Project Sdk="Microsoft.NET.Sdk">
                                      <PropertyGroup>
                                        <OutputType>{{OUTPUT_TYPE}}</OutputType>
                                        <LangVersion>latest</LangVersion>
                                        <TargetFramework>{{NET_VSN}}</TargetFramework>
                                        <AssemblyName>{{ROOT_NAME_SPACE}}</AssemblyName>
                                        <RootNamespace>{{ROOT_NAME_SPACE}}</RootNamespace>
                                        <Nullable>enable</Nullable>
                                        <ImplicitUsings>disable</ImplicitUsings>{{ICO_SPEC}}
                                        <WarningsAsErrors>nullable</WarningsAsErrors>
                                        <NoWarn></NoWarn>
                                        <PlatformTarget>AnyCPU</PlatformTarget>
                                        <Prefer32Bit>false</Prefer32Bit>{{USE_FORM}}
                                        <Version>0.0.0.0</Version>
                                      </PropertyGroup>
                                      <PropertyGroup>
                                        <DebugType>full</DebugType>
                                        <TieredCompilationQuickJit>false</TieredCompilationQuickJit>
                                      </PropertyGroup>
                                      <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Debug|AnyCPU'">
                                        <DebugType>full</DebugType>
                                      </PropertyGroup>
                                      <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|AnyCPU'">
                                        <DebugType>pdbonly</DebugType>
                                      </PropertyGroup>
                                      <PropertyGroup>
                                        <!--<ApplicationIcon>./app.ico</ApplicationIcon>-->
                                      </PropertyGroup>
                                      <ItemGroup Condition="'$(TargetFramework.TrimEnd(`0123456789`))' == 'net'">
                                        <Reference Include="Microsoft.CSharp" />
                                        <Reference Include="System" />
                                        <Reference Include="System.Core" />
                                        <Reference Include="System.Data" />
                                        <Reference Include="System.Data.DataSetExtensions" />
                                        <Reference Include="System.IO.Compression" />
                                        <Reference Include="System.Net.Http" />
                                        <Reference Include="System.Runtime.Remoting" />
                                        <Reference Include="System.Web" />
                                        <Reference Include="System.Xml" />
                                        <Reference Include="System.Xml.Linq" />
                                      </ItemGroup>
                                      <ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
                                        <PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
                                        <PackageReference Include="System.Dynamic.Runtime" Version="4.3.0" />
                                      </ItemGroup>
                                      <ItemGroup>{{PACKAGES}}
                                      </ItemGroup>
                                      <ItemGroup>{{SOURCES}}
                                      </ItemGroup>
                                      <ItemGroup>{{ASSEMBLIES}}
                                      </ItemGroup>
                                      <ItemGroup>{{RESOURCES}}
                                      </ItemGroup>
                                      <ItemGroup>
                                        <Content Include="assets\**">
                                          <Link>assets\%(RecursiveDir)\%(Filename)%(Extension)</Link>
                                          <TargetPath>assets\%(RecursiveDir)\%(Filename)%(Extension)</TargetPath>
                                          <CopyToOutputDirectory>Always</CopyToOutputDirectory>
                                        </Content>
                                        <Content Include="native\**">
                                          <Link>native\%(RecursiveDir)\%(Filename)%(Extension)</Link>
                                          <TargetPath>%(RecursiveDir)\%(Filename)%(Extension)</TargetPath>
                                          <CopyToOutputDirectory>Always</CopyToOutputDirectory>
                                        </Content>
                                      </ItemGroup>
                                    </Project>
                                    """;

    private const string SlnTemplate = """
                                       Microsoft Visual Studio Solution File, Format Version 12.00
                                       # Visual Studio Version 18
                                       VisualStudioVersion = 18.2.11415.280 d18.0
                                       MinimumVisualStudioVersion = 10.0.40219.1
                                       Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "{{PROGRAM}}", "{{PROGRAM}}.csproj", "{3379CD41-E550-CFE0-8396-9E8D5BD3E337}"
                                       EndProject
                                       Global
                                       	GlobalSection(SolutionConfigurationPlatforms) = preSolution
                                       		Debug|Any CPU = Debug|Any CPU
                                       		Release|Any CPU = Release|Any CPU
                                       	EndGlobalSection
                                       	GlobalSection(ProjectConfigurationPlatforms) = postSolution
                                       		{3379CD41-E550-CFE0-8396-9E8D5BD3E337}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                                       		{3379CD41-E550-CFE0-8396-9E8D5BD3E337}.Debug|Any CPU.Build.0 = Debug|Any CPU
                                       		{3379CD41-E550-CFE0-8396-9E8D5BD3E337}.Release|Any CPU.ActiveCfg = Release|Any CPU
                                       		{3379CD41-E550-CFE0-8396-9E8D5BD3E337}.Release|Any CPU.Build.0 = Release|Any CPU
                                       	EndGlobalSection
                                       	GlobalSection(SolutionProperties) = preSolution
                                       		HideSolutionNode = FALSE
                                       	EndGlobalSection
                                       	GlobalSection(ExtensibilityGlobals) = postSolution
                                       		SolutionGuid = {9F003DC0-89C0-4F0B-B30E-9F2F68D22734}
                                       	EndGlobalSection
                                       EndGlobal

                                       """;

    private static readonly string Generator = Const._generator;
    private /*const*/ static readonly string NetVsn = Const.NetVsn;

    public static void RecursiveDelete(DirectoryInfo baseDir)
    {
        if (!baseDir.Exists) return;
        foreach (var dir in baseDir.EnumerateDirectories()) RecursiveDelete(dir);
        baseDir.Delete(true);
    }

    public static void Main(string[] args)
    {
        try
        {
            ShowLineNumbers = false;
            var parseResult = Parser.Default.ParseArguments<Options>(args);
            switch (parseResult.Tag)
            {
                //
                case ParserResultType.Parsed:
                    {
                        var parsed = parseResult as Parsed<Options>;
                        var opt = parsed!.Value;
                        if (!opt.SrcFileList!.Any())
                        {
                            // srcFileList.Count == 0
                            //Console.WriteLine("Searching *.main.cs ...");
                            var cwd = Directory.GetCurrentDirectory();
                            string[] files = Directory.GetFiles(cwd, "*.cs", SearchOption.AllDirectories);
                            foreach (var srcFile in files)
                            {
                                //if (srcFile.Contains("+")) {
                                //    continue;
                                //}
                                if (srcFile.EndsWith(".main.cs"))
                                {
                                    //Console.WriteLine(srcFile);
                                    var parser = new CscsUtil(srcFile);
                                    MainHelper(parser, srcFile, false, opt.SingleOnly, opt.ConvertToProject);
                                }

                                if (srcFile.EndsWith(".dll.cs"))
                                {
                                    //Console.WriteLine(srcFile);
                                    var parser = new CscsUtil(srcFile);
                                    MainHelper(parser, srcFile, true, opt.SingleOnly, opt.ConvertToProject);
                                }
                            }
                        }
                        else
                        {
                            foreach (var srcFile in opt.SrcFileList!)
                            {
                                //if (srcFile.Contains("+")) {
                                //    continue;
                                //}
                                //Console.WriteLine(srcFile);
                                var parser = new CscsUtil(srcFile);
                                MainHelper(parser, srcFile, opt.GenerateDllProject, opt.SingleOnly, opt.ConvertToProject);
                            }
                        }
                    }
                    break;
                //
                case ParserResultType.NotParsed:
                    {
                        //
                        //var notParsed = parseResult as NotParsed<Options>;
                        Console.Error.WriteLine(HelpText.AutoBuild(parseResult));
                        Environment.Exit(1);
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
            Environment.Exit(1);
        }
    }

    private static void ConvertToProject(CscsUtil parser, string projFileName, bool generateDllProject)
    {
        projFileName = Path.GetFullPath(projFileName);
        //string generator = generateDllProject ? _generator + " --dll" : _generator;

        parser.ParseProject(projFileName, generateDllProject);
        parser.DebugDump();

        var outType = parser.OutType;
        var srcList = parser.SrcList;
        var pkgList = parser.PkgList;
        var asmList = parser.AsmList;
        var resList = parser.ResList;
        var defList = parser.DefList;
        //if (!pkgList.Contains("System.Text.Encoding.CodePages")) pkgList.Add("System.Text.Encoding.CodePages");
        var projDir = Path.GetDirectoryName(projFileName);
        var baseName = Path.GetFileNameWithoutExtension(projFileName);
        //var projFile = File.ReadAllText(projFileName);
        Directory.SetCurrentDirectory(projDir!);

        var rootNs = baseName;
        if (baseName.EndsWith(".main"))
        {
            rootNs = Path.GetFileNameWithoutExtension(baseName);
        }
        else if (baseName.EndsWith(".dll"))
        {
            generateDllProject = true;
            //NetVsn = "netstandard2.0";
            rootNs = Path.GetFileNameWithoutExtension(baseName);
        }

        if (Directory.Exists(".p." + baseName + "+")) RecursiveDelete(new DirectoryInfo(".p." + baseName + "+"));
        Directory.CreateDirectory(".p." + baseName + "+");

        var runtimeIdentifier = $"\n    <!--<RuntimeIdentifier>{Const._runtime}</RuntimeIdentifier>-->";

        var pkgSpec = "";
        foreach (var t in pkgList)
        {
            var pkgParsed = t.Split('@');
            var pkgName = pkgParsed[0];
            var isPrivate = false;
            if (pkgName.Contains("*"))
            {
                isPrivate = true;
                pkgName = pkgName.Replace("*", "");
            }

            var pkgVer = pkgParsed.Length >= 2 ? pkgParsed[1] : "*";
            if (isPrivate)
                pkgSpec += "\n" +
                           """     <PackageReference Include="{{NAME}}" Version="{{VERSION}}" PrivateAssets="All" />"""
                               .Replace("{{NAME}}", pkgName).Replace("{{VERSION}}", pkgVer);
            else
                pkgSpec += "\n" + """    <PackageReference Include="{{NAME}}" Version="{{VERSION}}" />"""
                    .Replace("{{NAME}}", pkgName).Replace("{{VERSION}}", pkgVer);
            if (pkgName == "DNNE")
                runtimeIdentifier =
                    $"\n    <RuntimeIdentifier>{Const._runtime}</RuntimeIdentifier>\n    <GenerateRuntimeConfigurationFiles>true</GenerateRuntimeConfigurationFiles>";
        }

        var home = Sys.FindHome(new DirectoryInfo(projDir!));
        Debug(home, "home");
        var asmSpec = "";
        foreach (var t in asmList)
            asmSpec += "\n" +
                       """    <Reference Include="{{BASENAME}}"><HintPath>{{NAME}}</HintPath></Reference>"""
                           .Replace("$(HOME)", home)
                           .Replace("{{BASENAME}}", Path.GetFileNameWithoutExtension(t))
                           .Replace("{{NAME}}", t);

        foreach (var t in srcList)
        {
            var srcFilePath = t.Replace("$(HOME)", home);
            var srcFileName = Path.GetFileName(srcFilePath);
            Debug(new { t, srcFilePath, srcFileName });
            if (srcFilePath.StartsWith($"{projDir}\\"))
            {
                File.Copy(srcFilePath, Path.Combine(projDir!, ".p." + baseName + "+", srcFileName), true);
            }
            else
            {
                var targetPath = t.Replace("$(HOME)\\", "");
                targetPath = Path.Combine(projDir!, ".p." + baseName + "+", targetPath);
                Sys.PrepareForFile(targetPath);
                File.Copy(srcFilePath, targetPath, true);
            }
        }

        var resSpec = "";
        foreach (var t in resList)
        {
            var srcFilePath = t.Replace("$(HOME)", home);
            var srcFileName = Path.GetFileName(srcFilePath);
            File.Copy(srcFilePath, Path.Combine(projDir!, ".p." + baseName + "+", srcFileName), true);
            resSpec += "\n" + """    <EmbeddedResource Include="{{NAME}}" />""".Replace("{{NAME}}", srcFileName);
        }

        var defSpec = "";
        if (defList.Count > 0)
            defSpec += "\n" +
                       """    <DefineConstants>$(DefineConstants);{{DEFINES}}</DefineConstants>""".Replace(
                           "{{DEFINES}}", string.Join(";", defList));

        var content = Template
                //.Replace("{{OUTPUT_TYPE}}", generateDllProject ? "Library" : "Exe")
                .Replace("{{OUTPUT_TYPE}}", generateDllProject ? "Library" : outType)
                .Replace("{{ROOT_NAME_SPACE}}", rootNs)
                .Replace("{{NET_VSN}}", NetVsn)
                .Replace("{{PROGRAM}}", baseName)
                .Replace("{{PACKAGES}}", pkgSpec)
                .Replace("{{ASSEMBLIES}}", asmSpec)
                .Replace("{{SOURCES}}", "")
                .Replace("{{RESOURCES}}", resSpec)
                .Replace("{{RUNTIME_IDENTIFIER}}", runtimeIdentifier)
                .Replace("{{DEFINE_SPEC}}", defSpec)
                .Replace("{{USE_FORM}}", outType == "WinExe" ? "\n<UseWindowsForms>true</UseWindowsForms>" : "")
            ;
        Debug(content, "content");
        Sys.SaveAllText(Path.Combine(projDir!, ".p." + baseName + "+", baseName + ".csproj"), content);
        Sys.SaveAllText(Path.Combine(projDir!, ".p." + baseName + "+", baseName + ".sln"),
            SlnTemplate.Replace("{{PROGRAM}}", baseName)
        );
        Sys.SaveAllText(Path.Combine(projDir!, ".p." + baseName + "+", "run"),
            $"""
             #! /usr/bin/env bash.exe
             set -e
             script_dir="$(dirname "$0")"
             script_dir="$(realpath $script_dir)"
             dotnet run --property WarningLevel=0 --project "$script_dir/{baseName + ".csproj"}" "$@"

             """);
    }

    private static void MainHelper(CscsUtil parser, string projFileName, bool generateDllProject, bool singleOnly,
        bool convertToProject)
    {
        if (convertToProject)
        {
            ConvertToProject(parser, projFileName, generateDllProject);
            return;
        }

        projFileName = Path.GetFullPath(projFileName);
        var generator = generateDllProject ? Generator + " --dll" : Generator;
        parser.ParseProject(projFileName, generateDllProject);
        parser.DebugDump();
        var outType = parser.OutType;
        var srcList = parser.SrcList;
        var pkgList = parser.PkgList;
        var asmList = parser.AsmList;
        var resList = parser.ResList;
        var defList = parser.DefList;
        var icoList = parser.IcoList;
        //if (!pkgList.Contains("System.Text.Encoding.CodePages")) pkgList.Add("System.Text.Encoding.CodePages");
        var projDir = Path.GetDirectoryName(projFileName);
        var home = Sys.FindHome(new DirectoryInfo(projDir!));
        Debug(home, "home");
        var baseName = Path.GetFileNameWithoutExtension(projFileName);
        Directory.SetCurrentDirectory(projDir!);
        var rootNs = baseName;
        if (baseName.EndsWith(".main"))
        {
            rootNs = Path.GetFileNameWithoutExtension(baseName);
        }
        else if (baseName.EndsWith(".dll"))
        {
            generateDllProject = true;
            //NetVsn = "netstandard2.0";
            rootNs = Path.GetFileNameWithoutExtension(baseName);
        }

        if (singleOnly && !generateDllProject)
        {
            var fileBasedCode = $"""
                                 #!/usr/bin/env -S dotnet run --property WarningLevel=0
                                 #:sdk      Microsoft.NET.Sdk
                                 #:property TargetFramework = {NetVsn}
                                 #:property LangVersion = preview
                                 #:property PublishAot = false
                                 #:property ImplicitUsings = false
                                 #:property Nullable = enable
                                 #:property PlatformTarget = AnyCPU
                                 #:property Prefer32Bit = false
                                 #:property AllowUnsafeBlocks = true

                                 """;
            foreach (var t in pkgList)
            {
                var pkgParsed = t.Split('@');
                var pkgName = pkgParsed[0];
                var pkgVer = pkgParsed.Length >= 2 ? pkgParsed[1] : "*";
                fileBasedCode += $"#:package  {pkgName}@{pkgVer}\n";
            }

            foreach (var t in srcList)
            {
                Debug(t);
                var realSrcPath = t.Replace("$(HOME)", home);
                fileBasedCode += $"\n/***** {t} *****/\n";
                fileBasedCode += File.ReadAllText(realSrcPath);
                fileBasedCode = fileBasedCode.Trim() + "\n";
                fileBasedCode = fileBasedCode.Replace("\n", "\n");
            }

            Sys.SaveAllText($".s.{rootNs}", fileBasedCode);
        }

        if (singleOnly) return;
        Directory.CreateDirectory(".build/" + baseName);
        var runtimeIdentifier = $"\n    <!--<RuntimeIdentifier>{Const._runtime}</RuntimeIdentifier>-->";
        var pkgSpec = "";
        foreach (var t in pkgList)
        {
            var pkgParsed = t.Split('@');
            var pkgName = pkgParsed[0];
            var isPrivate = false;
            if (pkgName.Contains("*"))
            {
                isPrivate = true;
                pkgName = pkgName.Replace("*", "");
            }

            var pkgVer = pkgParsed.Length >= 2 ? pkgParsed[1] : "*";
            if (isPrivate)
                pkgSpec += "\n" +
                           """     <PackageReference Include="{{NAME}}" Version="{{VERSION}}" PrivateAssets="All" />"""
                               .Replace("{{NAME}}", pkgName).Replace("{{VERSION}}", pkgVer);
            else
                pkgSpec += "\n" + """    <PackageReference Include="{{NAME}}" Version="{{VERSION}}" />"""
                    .Replace("{{NAME}}", pkgName).Replace("{{VERSION}}", pkgVer);
            if (pkgName == "DNNE")
                runtimeIdentifier =
                    $"\n    <RuntimeIdentifier>{Const._runtime}</RuntimeIdentifier>\n    <GenerateRuntimeConfigurationFiles>true</GenerateRuntimeConfigurationFiles>";
        }

        var asmSpec = "";
        foreach (var t in asmList)
            asmSpec += "\n" +
                       """    <Reference Include="{{BASENAME}}"><HintPath>{{NAME}}</HintPath></Reference>"""
                           .Replace("$(HOME)", home)
                           .Replace("{{BASENAME}}", Path.GetFileNameWithoutExtension(t))
                           .Replace("{{NAME}}", t);
        var srcSpec = "";
        foreach (var t in srcList)
        {
            var srcFileName = Path.GetFileName(t);
            //var srcBaseName = srcFileName.Substring(0, srcFileName.Length - 3);
            srcSpec += "\n" + """    <Compile Include="{{PATH}}" Link="{{NAME}}" />"""
                .Replace("{{NAME}}", srcFileName).Replace("{{PATH}}", t);
        }

        var resSpec = "";
        foreach (var t in resList)
            resSpec += "\n" + """    <EmbeddedResource Include="{{PATH}}" />""".Replace("{{PATH}}", t);
        var defSpec = "";
        if (defList.Count > 0)
            defSpec += "\n" +
                       """    <DefineConstants>$(DefineConstants);{{DEFINES}}</DefineConstants>""".Replace(
                           "{{DEFINES}}", string.Join(";", defList));
        if (icoList.Count > 0) Log(icoList, "icoList");
        var icoSpec = "";
        if (icoList.Count > 0)
            icoSpec += "\n" + """    <ApplicationIcon>{{ICON}}</ApplicationIcon>""".Replace("{{ICON}}", icoList[0]);
        if (icoSpec.Length > 0) Log(icoSpec, "icoSpec");
        var content = Template
                //.Replace("{{OUTPUT_TYPE}}", generateDllProject ? "Library" : "Exe")
                .Replace("{{OUTPUT_TYPE}}", generateDllProject ? "Library" : outType)
                .Replace("{{ROOT_NAME_SPACE}}", rootNs)
                .Replace("{{NET_VSN}}", NetVsn)
                .Replace("{{PROGRAM}}", baseName)
                .Replace("{{PACKAGES}}", pkgSpec)
                .Replace("{{ASSEMBLIES}}", asmSpec)
                .Replace("{{SOURCES}}", srcSpec)
                .Replace("{{RESOURCES}}", resSpec)
                .Replace("{{RUNTIME_IDENTIFIER}}", runtimeIdentifier)
                .Replace("{{DEFINE_SPEC}}", defSpec)
                .Replace("{{USE_FORM}}", outType == "WinExe" ? "\n<UseWindowsForms>true</UseWindowsForms>" : "")
                .Replace("{{ICO_SPEC}}", icoSpec)
            ;
        //Echo(content, "content");
        Sys.SaveAllText(".build/" + baseName + "\\" + baseName + ".csproj", content);
        Sys.SaveAllText(".build/" + baseName + "\\" + baseName + ".sln",
            SlnTemplate.Replace("{{PROGRAM}}", baseName)
        );
        var cleanStep = CleanShTemplate
            .Replace("{{GENERATOR}}", generator)
            .Replace("{{PROGRAM}}", baseName)
            .Replace("{{ROOT_NAME_SPACE}}", rootNs).Replace("{{NET_VSN}}", NetVsn);
        var buildStep = BuildShTemplate
            .Replace("{{GENERATOR}}", generator)
            .Replace("{{PROGRAM}}", baseName)
            .Replace("{{ROOT_NAME_SPACE}}", rootNs).Replace("{{NET_VSN}}", NetVsn);
        var mergeStep = MergeShTemplate
                .Replace("{{GENERATOR}}", generator)
                .Replace("{{PROGRAM}}", baseName)
                .Replace("{{ROOT_NAME_SPACE}}", rootNs)
                .Replace("{{EXT}}", generateDllProject ? "dll" : "exe")
            ;
        var packStep = PackShTemplate
                .Replace("{{GENERATOR}}", generator)
                .Replace("{{PROGRAM}}", baseName)
                .Replace("{{ROOT_NAME_SPACE}}", rootNs)
                .Replace("{{EXT}}", generateDllProject ? "dll" : "exe")
            ;
        var runFilePath = Path.GetFullPath(".r." + rootNs + ".sh");
        Sys.SaveAllText(runFilePath,
            RunShTemplate
                .Replace("{{GENERATOR}}", generator)
                .Replace("{{PROGRAM}}", baseName)
                .Replace("{{CLEAN_STEP}}", cleanStep)
                .Replace("{{BUILD_STEP}}", buildStep)
                .Replace("{{MERGE_STEP}}", mergeStep)
                .Replace("{{PACK_STEP}}", packStep)
                .Replace("{{NET_VSN}}", NetVsn)
        );
        Console.Error.WriteLine(runFilePath);
        //string scriptFilePath = $"do.{rootNs}";
        //string scriptFilePath = $"{rootNs}.do";
        var scriptFilePath = $"{rootNs}.task";
        Sys.SaveAllText(scriptFilePath,
                $$$"""
                       #! /usr/bin/env bash.exe
                       # -*- mode: sh -*-
                       script_dir="$(dirname "$0")"
                       script_dir="$(realpath $script_dir)"
                       str="$1"
                       if [[ "$<>str:0:1</>" == "@" ]]; then
                         if [[ "$str" == "@run" ]]; then
                           shift
                           bash.exe "$script_dir/.r.{{{rootNs}}}.sh" "$@"
                         elif [[ "$str" == "@exe" ]]; then
                           if [ ! -f "$script_dir/{{{rootNs}}}.exe" ]; then
                             {{{generator}}} "$script_dir/{{{baseName}}}.cs"
                             bash.exe "$script_dir/.r.{{{rootNs}}}.sh" "@merge" -f 1>&2
                           fi
                           shift
                           $script_dir/{{{rootNs}}}.exe "$@"
                         elif [[ "$str" == "@bin" ]]; then
                           if [ ! -f "$script_dir/{{{rootNs}}}.exe" ]; then
                             {{{generator}}} "$script_dir/{{{baseName}}}.cs"
                             bash.exe "$script_dir/.r.{{{rootNs}}}.sh" "@pack" -f 1>&2
                           fi
                           shift
                           $script_dir/{{{rootNs}}}.exe "$@"
                         else
                           bash.exe "$script_dir/.r.{{{rootNs}}}.sh" "$@"
                         fi
                       else
                         cscs -nuget:restore "$script_dir/{{{baseName}}}.cs" 1>&2
                         cscs -l:0 "$script_dir/{{{baseName}}}.cs" "$@"
                       fi
                       """
                    .Replace("<>", "{").Replace("</>", "}")
            )
            ;
        var cmdFilePath = $"do.{rootNs}.cmd";
        Sys.SaveAllText(cmdFilePath,
                $$$"""
                       @echo off
                       set script_dir=%~dp0
                       set "script_dir=%script_dir:\=/%"
                       set str=%1
                       if "%str:~0,1%"=="@" (
                         if "%str%"=="@run" (
                           bash.exe -c "%script_dir%/.r.{{{rootNs}}}.sh %2 %3 %4 %5 %6 %7 %8 %9
                         ) else if "%str%"=="@exe" (
                           if not exist "%script_dir%/{{{rootNs}}}.exe" (
                             wingen %script_dir%/{{{baseName}}}.cs
                             bash.exe -c "%script_dir%/.r.{{{rootNs}}}.sh @merge -f"
                           )
                           "%script_dir%/{{{rootNs}}}.exe" %2 %3 %4 %5 %6 %7 %8 %9
                         ) else if "%str%"=="@bin" (
                           if not exist "%script_dir%/{{{rootNs}}}.exe" (
                             wingen %script_dir%/{{{baseName}}}.cs
                             bash.exe -c "%script_dir%/.r.{{{rootNs}}}.sh @pack -f"
                           )
                           "%script_dir%/{{{rootNs}}}.exe" %2 %3 %4 %5 %6 %7 %8 %9
                         ) else (
                           bash.exe -c "%script_dir%/.r.{{{rootNs}}}.sh %*"
                         )
                       ) else (
                         cscs -nuget:restore "%script_dir%/{{{baseName}}}.cs">NUL 2>&1
                         cscs -l:0 "%script_dir%/{{{baseName}}}.cs" %*
                       )
                       """
                    .Replace("<>", "{").Replace("</>", "}")
            )
            ;
    }
}
