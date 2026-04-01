# /usr/bin/env bash.exe
# -*- mode: sh -*-
set -e
script_dir="$(dirname "$0")"
script_dir="$(realpath $script_dir)"
wingen.exe $script_dir/dnmerge.main.cs
if [ "$1" == "@vs" ]; then
  devenv.exe $script_dir/.build/dnmerge.main/dnmerge.main.csproj &
elif [ "$1" == "@rider" ]; then
  rider64.exe $script_dir/.build/dnmerge.main/dnmerge.main.csproj &
elif [ "$1" == "@clean" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  rm -rvf dnmerge.exe dnmerge.dll dnmerge.pdb .build/dnmerge.main/bin .build/dnmerge.main/obj
elif [ "$1" == "@build" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  wingen.exe dnmerge.main.cs
  rm -rf dnmerge.main.bin dnmerge.main.resource
  dotnet build -c release .build/dnmerge.main/dnmerge.main.csproj
  cd .build/dnmerge.main/bin/release/net462
  mkdir $cwd/dnmerge.main.bin
  cp -rp * $cwd/dnmerge.main.bin/
  #cd $cwd/dnmerge.main.bin
  #7z a -tzip -r ../dnmerge.main.resource *
elif [ "$1" == "@merge" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  if [ "$2" != "-f" ]; then
    if [ -f "dnmerge.exe" ]; then
      echo "dnmerge.exe exists...skipping build and merge."
      exit 0
    fi
  fi
  wingen.exe dnmerge.main.cs
  rm -rf dnmerge.main.resource dnmerge.exe dnmerge.dll dnmerge.pdb .build/dnmerge.main/bin .build/dnmerge.main/obj
  dotnet build -c release .build/dnmerge.main/dnmerge.main.csproj
  #cd ./.build/dnmerge.main/bin/release/net462
  dnmerge.exe ./.build/dnmerge.main/bin/release/net462/dnmerge.exe
elif [ "$1" == "@pack" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  wingen.exe dnmerge.main.cs
  rm -rf dnmerge.main.resource dnmerge.exe dnmerge.dll dnmerge.pdb .build/dnmerge.main/bin .build/dnmerge.main/obj
  dotnet build -c release .build/dnmerge.main/dnmerge.main.csproj
  #cd ./.build/dnmerge.main/bin/release/net462
  exepack.exe -o dnmerge.exe  -i ./.build/dnmerge.main/bin/release/net462/dnmerge.exe
elif [ "$1" == "@check" ]; then
  cd $script_dir/.build/dnmerge.main
  dotnet list package --outdated
elif [ "$1" == "@update" ]; then
  cd $script_dir/.build/dnmerge.main
  dotnet package update
else
  dotnet run --property WarningLevel=0 --project $script_dir/.build/dnmerge.main/dnmerge.main.csproj $*
fi
