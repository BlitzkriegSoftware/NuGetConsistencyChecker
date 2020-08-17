#!/bin/bash
# C:\code\blitz\NuGetConsistencyChecker\dotnetcore\ngcc2\singlefilebuild.sh
dotnet publish -r win-x64 -c Release --self-contained
TARGET=./bin/Release/netcoreapp3.1/win-x64/publish
cp ../README.md  ${TARGET}/ngcc2-README.md
cp ./ngcc2-urls.txt ${TARGET}/
ls ${TARGET}
