#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
RID="${1:-osx-arm64}"

cmake -S "$ROOT/native" -B "$ROOT/native/build-macos" -DCMAKE_BUILD_TYPE=Release
cmake --build "$ROOT/native/build-macos" --config Release
mkdir -p "$ROOT/BCellResearchApp/runtimes/osx/native"
cp "$ROOT/native/build-macos/libbcell_native.dylib" "$ROOT/BCellResearchApp/runtimes/osx/native/"

dotnet restore "$ROOT/BCellResearchApp/BCellResearchApp.csproj"
dotnet publish "$ROOT/BCellResearchApp/BCellResearchApp.csproj" \
  -c Release \
  -r "$RID" \
  --self-contained true \
  -p:PublishSingleFile=false \
  -o "$ROOT/release/$RID"

echo
echo "Published Release build: $ROOT/release/$RID"
echo "Run with: $ROOT/release/$RID/BCellResearchApp"
