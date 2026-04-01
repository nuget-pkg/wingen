namespace Local
{
    internal static class Const
    {
        public static string _generator = "wingen.exe";
        public const string _runtime = "win-x64";
        public const string NetVsn = "net462";
        public const string CleanShTemplate = CleanShTemplateCs;
        public const string BuildShTemplate = BuildShTemplateCs;
        public const string MergeShTemplate = MergeShTemplateCs;
        public const string PackShTemplate = PackShTemplateCs;
        private const string CleanShTemplateCs = """
                                               cd "$(dirname "$0")"
                                               cwd=`pwd`
                                               rm -rvf {{ROOT_NAME_SPACE}}.exe {{ROOT_NAME_SPACE}}.dll {{ROOT_NAME_SPACE}}.pdb .build/{{PROGRAM}}/bin .build/{{PROGRAM}}/obj
                                             """;
        private const string BuildShTemplateCs = """
                                               cd "$(dirname "$0")"
                                               cwd=`pwd`
                                               {{GENERATOR}} {{PROGRAM}}.cs
                                               rm -rf {{PROGRAM}}.bin {{PROGRAM}}.resource
                                               dotnet build -c release .build/{{PROGRAM}}/{{PROGRAM}}.csproj
                                               cd .build/{{PROGRAM}}/bin/release/{{NET_VSN}}
                                               mkdir $cwd/{{PROGRAM}}.bin
                                               cp -rp * $cwd/{{PROGRAM}}.bin/
                                               #cd $cwd/{{PROGRAM}}.bin
                                               #7z a -tzip -r ../{{PROGRAM}}.resource *
                                             """;
        private const string MergeShTemplateCs = """
                                               cd "$(dirname "$0")"
                                               cwd=`pwd`
                                               if [ "$2" != "-f" ]; then
                                                 if [ -f "{{ROOT_NAME_SPACE}}.exe" ]; then
                                                   echo "{{ROOT_NAME_SPACE}}.exe exists...skipping build and merge."
                                                   exit 0
                                                 fi
                                               fi
                                               {{GENERATOR}} {{PROGRAM}}.cs
                                               rm -rf {{PROGRAM}}.resource {{ROOT_NAME_SPACE}}.exe {{ROOT_NAME_SPACE}}.dll {{ROOT_NAME_SPACE}}.pdb .build/{{PROGRAM}}/bin .build/{{PROGRAM}}/obj
                                               dotnet build -c release .build/{{PROGRAM}}/{{PROGRAM}}.csproj
                                               #cd ./.build/{{PROGRAM}}/bin/release/{{NET_VSN}}
                                               dnmerge.exe ./.build/{{PROGRAM}}/bin/release/{{NET_VSN}}/{{ROOT_NAME_SPACE}}.{{EXT}}
                                             """;
        private const string PackShTemplateCs = """
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
