$ErrorActionPreference = "Stop"
$Root = Resolve-Path "$PSScriptRoot\.."
cmake -S "$Root\native" -B "$Root\native\build-windows" -A x64
cmake --build "$Root\native\build-windows" --config Release
New-Item -ItemType Directory -Force "$Root\BCellResearchApp\runtimes\win-x64\native" | Out-Null
Copy-Item "$Root\native\build-windows\Release\bcell_native.dll" "$Root\BCellResearchApp\runtimes\win-x64\native\bcell_native.dll" -Force
dotnet restore "$Root\BCellResearchApp\BCellResearchApp.csproj"
dotnet build "$Root\BCellResearchApp\BCellResearchApp.csproj" -c Release
