param(
    [Parameter(Mandatory=$true)]
    [string]$version
)

Write-Host "Building and packing Apparatus.Results version $version..." -ForegroundColor Green

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path "./nugets") {
    Remove-Item "./nugets" -Recurse -Force
}
New-Item -ItemType Directory -Path "./nugets" -Force | Out-Null

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore

# Build in Release configuration
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --no-restore --configuration Release

# Skip tests in pack script - tests should be run at pipeline level

# Pack the library
Write-Host "Packing Apparatus.Results..." -ForegroundColor Yellow
dotnet pack src/Apparatus.Results/Apparatus.Results.csproj --no-build --configuration Release --output ./nugets -p:PackageVersion=$version

Write-Host "Build and pack completed successfully!" -ForegroundColor Green
Write-Host "Package created in ./nugets/ directory" -ForegroundColor Cyan