#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cmake -S "$ROOT/native" -B "$ROOT/native/build-macos" -DCMAKE_BUILD_TYPE=Release
cmake --build "$ROOT/native/build-macos" --config Release
mkdir -p "$ROOT/BCellResearchApp/runtimes/osx/native"
cp "$ROOT/native/build-macos/libbcell_native.dylib" "$ROOT/BCellResearchApp/runtimes/osx/native/"
dotnet restore "$ROOT/BCellResearchApp/BCellResearchApp.csproj"
dotnet build "$ROOT/BCellResearchApp/BCellResearchApp.csproj" -c Release
