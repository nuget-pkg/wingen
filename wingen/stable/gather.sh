#! bash
# -*- mode: sh -*-
script_dir="$(realpath `dirname "$0"`)"
cd "$script_dir"
#set -uvx
set -e
cwd=$(pwd)
ts=$(date "+%Y.%m%d.%H%M.%S")
ver=$(echo $ts | sed -e "s/[.]0/./g")

cp -pv C:/env/+cmd/wingen.??? .
cp -pv C:/env/+cmd/unigen.??? .
