param(
    [Parameter(Mandatory = $false)]
    [string] $ProjectName = "",

    [Parameter(Mandatory = $false)]
    [string] $CsprojPath = "",

    [Parameter(Mandatory = $false)]
    [string] $Destination = "D:\VisualStudio\LocalNuGetShare",

    [Parameter(Mandatory = $false)]
    [string] $Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [string] $Version = ""   # optional override, e.g. 1.2.3 ; leave empty to use csproj version
)

$ErrorActionPreference = "Stop"

function Find-RepoRootBySrc {
    param([string]$startDir)

    $dir = (Resolve-Path $startDir).Path
    while ($true) {
        if (Test-Path (Join-Path $dir "Src")) { return $dir }

        $parent = Split-Path $dir -Parent
        if ($parent -eq $dir -or [string]::IsNullOrWhiteSpace($parent)) {
            throw "Repo root not found. No 'Src' folder found when walking up from: $startDir"
        }
        $dir = $parent
    }
}

function Find-ProjectsByNameInCommon {
    param([string]$name, [string]$commonRoot)

    @(
        Get-ChildItem -Path $commonRoot -Recurse -Filter "*.csproj" -ErrorAction SilentlyContinue |
            Where-Object {
                $_.BaseName -eq $name -and
                $_.FullName -notmatch "\\bin\\" -and
                $_.FullName -notmatch "\\obj\\"
            } |
            Sort-Object FullName
    )
}

# --- Determine repo root based on script location (NOT $PWD) ---
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Find-RepoRootBySrc -startDir $scriptDir

# ONLY search in Src
$commonRoot = Join-Path $repoRoot "Src"

Write-Host "Script dir   : $scriptDir" -ForegroundColor DarkGray
Write-Host "Repo root    : $repoRoot" -ForegroundColor DarkGray
Write-Host "Search root  : $commonRoot" -ForegroundColor DarkGray

if (!(Test-Path $commonRoot)) {
    throw "Search root not found: $commonRoot"
}

# --- Ensure destination exists ---
if (!(Test-Path $Destination)) {
    Write-Host "Destination folder not found. Creating: $Destination" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    Write-Host "Destination folder created: $Destination" -ForegroundColor Green
}

# --- Choose project (.csproj path wins, otherwise search by name) ---
$selectedCsproj = $null

if ($CsprojPath) { $CsprojPath = $CsprojPath.Trim().Trim('"') }
if ($ProjectName) { $ProjectName = $ProjectName.Trim() }

if (-not [string]::IsNullOrWhiteSpace($CsprojPath)) {
    if (!(Test-Path $CsprojPath)) { throw "The .csproj path does not exist: $CsprojPath" }
    if ($CsprojPath -notmatch "\.csproj$") { throw "Please provide a .csproj file path." }

    $selectedCsproj = (Get-Item $CsprojPath).FullName
    $ProjectName = [System.IO.Path]::GetFileNameWithoutExtension($selectedCsproj)
}
else {
    $maxAttempts = 5
    $attempt = 0
    $projects = @()

    while ($projects.Count -eq 0 -and $attempt -lt $maxAttempts) {
        if ([string]::IsNullOrWhiteSpace($ProjectName)) {
            $ProjectName = (Read-Host "Enter the project name (without .csproj) or 'q' to quit").Trim()
        }

        if ($ProjectName -eq "q") {
            Write-Host "Aborted by user." -ForegroundColor Yellow
            return
        }

        Write-Host "Searching for '$ProjectName' in Src..." -ForegroundColor Blue
        $projects = Find-ProjectsByNameInCommon -name $ProjectName -commonRoot $commonRoot
        Write-Host "Matches found: $($projects.Count)" -ForegroundColor DarkGray

        if ($projects.Count -eq 0) {
            Write-Host "No project found for name: '$ProjectName' in Src" -ForegroundColor Red
            $ProjectName = ""
        }

        $attempt++
    }

    if ($projects.Count -eq 0) {
        throw "No project found after $maxAttempts attempts. Check the search root: $commonRoot"
    }

    # pick first match deterministically
    $selectedCsproj = $projects[0].FullName
}

Write-Host "Selected project : $ProjectName" -ForegroundColor Green
Write-Host "Selected .csproj : $selectedCsproj" -ForegroundColor DarkGray
Write-Host "Configuration    : $Configuration" -ForegroundColor DarkGray
Write-Host "Destination      : $Destination" -ForegroundColor DarkGray
if ($Version) { Write-Host "Version override : $Version" -ForegroundColor DarkGray }

# --- Pack (disable symbols, so you don't also get snupkg unless you want it) ---
Write-Host "Running: dotnet pack ..." -ForegroundColor Green

$packArgs = @(
    "pack", $selectedCsproj,
    "-c", $Configuration,
    "-o", $Destination,
    "--nologo",
    "/p:IncludeSymbols=false"
)

if (-not [string]::IsNullOrWhiteSpace($Version)) {
    $packArgs += "/p:PackageVersion=$Version"
}

& dotnet @packArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet pack failed."
}

Write-Host "Done. Packages were written to: $Destination" -ForegroundColor Green

# --- Show newest created nupkgs (best-effort) ---
Get-ChildItem -Path $Destination -Filter "*.nupkg" -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 10 FullName, LastWriteTime |
    ForEach-Object {
        Write-Host ("Created: {0} ({1})" -f $_.FullName, $_.LastWriteTime) -ForegroundColor DarkGray
    }