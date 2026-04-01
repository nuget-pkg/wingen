# /usr/bin/env bash.exe
# -*- mode: sh -*-
set -e
script_dir="$(dirname "$0")"
script_dir="$(realpath $script_dir)"
wingen.exe $script_dir/wingen.main.cs
if [ "$1" == "@vs" ]; then
  devenv.exe $script_dir/.build/wingen.main/wingen.main.csproj &
elif [ "$1" == "@rider" ]; then
  rider64.exe $script_dir/.build/wingen.main/wingen.main.csproj &
elif [ "$1" == "@clean" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  rm -rvf wingen.exe wingen.dll wingen.pdb .build/wingen.main/bin .build/wingen.main/obj
elif [ "$1" == "@build" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  wingen.exe wingen.main.cs
  rm -rf wingen.main.bin wingen.main.resource
  dotnet build -c release .build/wingen.main/wingen.main.csproj
  cd .build/wingen.main/bin/release/net462
  mkdir $cwd/wingen.main.bin
  cp -rp * $cwd/wingen.main.bin/
  #cd $cwd/wingen.main.bin
  #7z a -tzip -r ../wingen.main.resource *
elif [ "$1" == "@merge" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  if [ "$2" != "-f" ]; then
    if [ -f "wingen.exe" ]; then
      echo "wingen.exe exists...skipping build and merge."
      exit 0
    fi
  fi
  wingen.exe wingen.main.cs
  rm -rf wingen.main.resource wingen.exe wingen.dll wingen.pdb .build/wingen.main/bin .build/wingen.main/obj
  dotnet build -c release .build/wingen.main/wingen.main.csproj
  #cd ./.build/wingen.main/bin/release/net462
  dnmerge.exe ./.build/wingen.main/bin/release/net462/wingen.exe
elif [ "$1" == "@pack" ]; then
  cd "$(dirname "$0")"
  cwd=`pwd`
  wingen.exe wingen.main.cs
  rm -rf wingen.main.resource wingen.exe wingen.dll wingen.pdb .build/wingen.main/bin .build/wingen.main/obj
  dotnet build -c release .build/wingen.main/wingen.main.csproj
  #cd ./.build/wingen.main/bin/release/net462
  exepack.exe -o wingen.exe  -i ./.build/wingen.main/bin/release/net462/wingen.exe
elif [ "$1" == "@check" ]; then
  cd $script_dir/.build/wingen.main
  dotnet list package --outdated
elif [ "$1" == "@update" ]; then
  cd $script_dir/.build/wingen.main
  dotnet package update
else
  dotnet run --property WarningLevel=0 --project $script_dir/.build/wingen.main/wingen.main.csproj $*
fi
