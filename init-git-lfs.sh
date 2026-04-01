#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`

git lfs install
git lfs track "*.exe"
git lfs track "*.dll"
git lfs track "*.pdb"
git lfs track "*.zip"
git lfs track "*.7z"
git lfs track "*.AppImage"
