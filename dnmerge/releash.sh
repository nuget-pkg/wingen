#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`
wingen.exe
./dnmerge.task @merge -f
cp -rvp dnmerge.??? C:/env/+cmd/
