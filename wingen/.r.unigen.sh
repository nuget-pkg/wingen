# /usr/bin/env bash.exe
# -*- mode: sh -*-
set -e
script_dir="$(dirname "$0")"
script_dir="$(realpath $script_dir)"
wingen.exe $script_dir/unigen.main.cs
if [ "$1" == "@vs" ]; then
  devenv.exe $script_dir/.build/unigen.main/unigen.main.csproj &
elif [ "$1" == "@rider" ]; then
  rider64.exe $script_dir/.build/unigen.main/unigen.main.csproj &
elif [ "$1" == "@clean" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  rm -rvf unigen.exe unigen.dll unigen.pdb .build/unigen.main/bin .build/unigen.main/obj
elif [ "$1" == "@build" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  wingen.exe unigen.main.cs
  rm -rf unigen.main.bin unigen.main.resource
  dotnet build -c release .build/unigen.main/unigen.main.csproj
  cd .build/unigen.main/bin/release/net462
  mkdir $cwd/unigen.main.bin
  cp -rp * $cwd/unigen.main.bin/
  #cd $cwd/unigen.main.bin
  #7z a -tzip -r ../unigen.main.resource *
elif [ "$1" == "@merge" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  if [ "$2" != "-f" ]; then
    if [ -f "unigen.exe" ]; then
      echo "unigen.exe exists...skipping build and merge."
      exit 0
    fi
  fi
  wingen.exe unigen.main.cs
  rm -rf unigen.main.resource unigen.exe unigen.dll unigen.pdb .build/unigen.main/bin .build/unigen.main/obj
  dotnet build -c release .build/unigen.main/unigen.main.csproj
  #cd ./.build/unigen.main/bin/release/net462
  dnmerge.exe ./.build/unigen.main/bin/release/net462/unigen.exe
elif [ "$1" == "@pack" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  wingen.exe unigen.main.cs
  rm -rf unigen.main.resource unigen.exe unigen.dll unigen.pdb .build/unigen.main/bin .build/unigen.main/obj
  dotnet build -c release .build/unigen.main/unigen.main.csproj
  #cd ./.build/unigen.main/bin/release/net462
  exepack.exe -o unigen.exe  -i ./.build/unigen.main/bin/release/net462/unigen.exe
elif [ "$1" == "@check" ]; then
  cd $script_dir/.build/unigen.main
  dotnet list package --outdated
elif [ "$1" == "@update" ]; then
  cd $script_dir/.build/unigen.main
  dotnet package update
else
  dotnet run --property WarningLevel=0 --project $script_dir/.build/unigen.main/unigen.main.csproj $*
fi
