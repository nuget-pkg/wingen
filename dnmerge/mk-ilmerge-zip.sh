#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`

rm -rf tmp
mkdir -p tmp
cd tmp
cp -pv C:/env/+cmd/ILMerge.exe C:/env/+cmd/System.Compiler.dll .
7z a -tzip ../ILMerge.zip ILMerge.exe System.Compiler.dll
