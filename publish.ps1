$dotnet = "C:\Program Files\dotnet\dotnet.exe"
$project = Join-Path $PSScriptRoot "src\SubiektGtAddressManager\SubiektGtAddressManager.csproj"
$output = Join-Path $PSScriptRoot "publish"

& $dotnet publish $project `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $output

Write-Host ""
Write-Host "Gotowe. Katalog do dystrybucji: $output"
