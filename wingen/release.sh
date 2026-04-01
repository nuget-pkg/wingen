#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
cd $cwd
./stable/wingen.exe
export PATH="$(realpath ./stable):$PATH"
./do.wingen.cmd @merge -f
cp -rvp wingen.??? C:/env/+cmd/
./do.unigen.cmd @merge -f
cp -rvp unigen.??? C:/env/+cmd/
