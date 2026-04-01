//+#nuget Global.Sys;
//+#embed ILMerge.zip;
using Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Global.EasyObject;
using static Global.Sys;

//Echo(new { args });
if (args.Length != 1) {
    Log("Wrong number of arguments for dnmerge.exe");
    Environment.Exit(1);
}
string cwd = GetCwd();
string instRoot = Path.Combine(
     Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
     ".ProgramData",
     "dnmerge.exe");
var extractRoot = Installer.InstallResourceZip(Assembly.GetExecutingAssembly(), instRoot, "dnmerge:ILMerge.zip");
string primaryAssembly = args[0];
string primaryBaseName = Path.GetFileName(primaryAssembly);
var finfo = new FileInfo(primaryAssembly);
var dinfo = finfo.Directory!;
#if false
var files = Sys.ExpandWildcardList($"{dinfo.FullName}/*.exe", $"{dinfo.FullName}/*.dll");
#else
SetCwd(dinfo.FullName);
var files = Sys.ExpandWildcardList("*.exe", "*.dll");
#endif
string? primary = null;
List<string> secondary = [];
foreach (var file in files) {
    string baseName = Path.GetFileName(file);
    if (baseName == primaryBaseName) {
        primary = baseName;
    } else {
        secondary.Add(baseName);
    }
}
//Log(primary, "primary");
//Log(secondary, "secondary");
List<string> cmd = [];
cmd.Add($"-out:{cwd}/{primaryBaseName}");
cmd.Add("-internalize");
cmd.Add("-wildcards");
cmd.Add("-allowDup");
cmd.Add(primary!);
cmd.AddRange(secondary);
//Log(cmd, "cmd");
int exitCode = RunCommand(Path.Combine(extractRoot, "ILMerge.exe"), cmd!.ToArray());
Environment.Exit(exitCode);
