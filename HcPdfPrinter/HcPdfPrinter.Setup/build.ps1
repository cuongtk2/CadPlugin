# Chạy trong thư mục HcPdfPrinter.Setup:  .\build.ps1
#
# Cài WiX một lần (dùng v5.0.2, xem ghi chú giấy phép):
#   dotnet tool install --global wix --version 5.0.2
#   wix extension add -g WixToolset.Util.wixext/5.0.2

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$proj    = Resolve-Path '..\HcPdfPrinter\HcPdfPrinter.csproj'
$publish = Join-Path $PSScriptRoot 'publish'
if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }

# .NET Framework 4.8: máy đích (Windows 10 1903 trở lên) đã có sẵn runtime, không cần kèm theo.
dotnet publish $proj -c Release -o $publish
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish lỗi' }

wix build Package.wxs -arch x64 -ext WixToolset.Util.wixext -d "PublishDir=$publish" -o setup.msi
if ($LASTEXITCODE -ne 0) { throw 'wix build lỗi' }

Write-Host 'Xong: setup.msi'
