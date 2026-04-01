namespace Local
{
    internal static class Const
    {
        public static string _generator = "unigen.exe";
        public const string _runtime = "linux-x64";
        public const string NetVsn = "net10.0";
        public const string CleanShTemplate = CleanShTemplateDotnet;
        public const string BuildShTemplate = BuildShTemplateDotnet;
        public const string MergeShTemplate = MergeShTemplateDotnet;
        public const string PackShTemplate = PackShTemplateDotnet;
        private const string CleanShTemplateDotnet = """
                                               cd "$(dirname "$0")"
                                               cwd=`pwd`
                                               rm -rvf {{ROOT_NAME_SPACE}}.exe {{ROOT_NAME_SPACE}}.dll {{ROOT_NAME_SPACE}}.pdb .build/{{PROGRAM}}/bin .build/{{PROGRAM}}/obj
                                             """;
        private const string BuildShTemplateDotnet = """
                                                   cd "$(dirname "$0")"
                                                   cwd=`pwd`
                                                   {{GENERATOR}} {{PROGRAM}}.cs
                                                   rm -rf {{PROGRAM}}.bin {{PROGRAM}}.resource
                                                   dotnet publish -r linux-x64 -c release -p:SelfContained=false -p:PublishSingleFile=false .build/{{PROGRAM}}/{{PROGRAM}}.csproj
                                                   cd .build/{{PROGRAM}}/bin/release/{{NET_VSN}}/linux-x64/publish
                                                   mkdir $cwd/{{PROGRAM}}.bin
                                                   cp -rp * $cwd/{{PROGRAM}}.bin/
                                                 """;
        private const string MergeShTemplateDotnet = """
                                                   cd "$(dirname "$0")"
                                                   cwd=`pwd`
                                                   {{GENERATOR}} {{PROGRAM}}.cs
                                                   rm -rf {{ROOT_NAME_SPACE}}.exe {{ROOT_NAME_SPACE}}.dll {{ROOT_NAME_SPACE}}.pdb .build/{{PROGRAM}}/bin .build/{{PROGRAM}}/obj
                                                   dotnet publish -r linux-x64 -c release -p:SelfContained=true -p:PublishSingleFile=true .build/{{PROGRAM}}/{{PROGRAM}}.csproj
                                                   cd .build/{{PROGRAM}}/bin/release/net10.0/linux-x64/publish
                                                   if ls *.exe 1> /dev/null 2>&1; then
                                                      cp -p {{ROOT_NAME_SPACE}}.exe {{ROOT_NAME_SPACE}}.pdb $cwd/
                                                      cd $cwd
                                                      ls -lh {{ROOT_NAME_SPACE}}.exe {{ROOT_NAME_SPACE}}.pdb
                                                   else
                                                      cp -p {{ROOT_NAME_SPACE}}.dll {{ROOT_NAME_SPACE}}.pdb $cwd/
                                                      cd $cwd
                                                      ls -lh {{ROOT_NAME_SPACE}}.dll {{ROOT_NAME_SPACE}}.pdb
                                                   fi
                                                 """;
        private const string PackShTemplateDotnet = """
                                               cd "$(dirname "$0")"
                                               cwd=`pwd`
                                               {{GENERATOR}} {{PROGRAM}}.cs
                                               rm -rf {{PROGRAM}}.resource {{ROOT_NAME_SPACE}}.exe {{ROOT_NAME_SPACE}}.dll {{ROOT_NAME_SPACE}}.pdb .build/{{PROGRAM}}/bin .build/{{PROGRAM}}/obj
                                               dotnet build -c release .build/{{PROGRAM}}/{{PROGRAM}}.csproj
                                               #cd ./.build/{{PROGRAM}}/bin/release/{{NET_VSN}}
                                               exepack.exe -o {{ROOT_NAME_SPACE}}.{{EXT}}  -i ./.build/{{PROGRAM}}/bin/release/{{NET_VSN}}/{{ROOT_NAME_SPACE}}.{{EXT}}
                                             """;
    }
}
