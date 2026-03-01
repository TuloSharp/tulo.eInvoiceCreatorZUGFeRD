param(
    [Parameter(Mandatory = $false)]
    [string] $ProjectName = "",

    [Parameter(Mandatory = $false)]
    [string] $CsprojPath = "",

    [Parameter(Mandatory = $false)]
    [string] $Destination = "D:\LocalNuGetShare",

    [Parameter(Mandatory = $false)]
    [string] $Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [string] $Version = ""   # optional, e.g. 1.2.3 ; leave empty to use csproj version
)

$ErrorActionPreference = "Stop"

function Find-SolutionRoot {
    param([string]$startDir)

    $dir = (Resolve-Path $startDir).Path
    while ($true) {
        $sln = Get-ChildItem -Path $dir -Filter *.sln -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($sln) { return $dir }

        $parent = Split-Path $dir -Parent
        if ($parent -eq $dir -or [string]::IsNullOrWhiteSpace($parent)) {
            throw "Solution root not found. No .sln found when walking up from: $startDir"
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

# --- Determine solution root based on current terminal location ---
$solutionDir = Find-SolutionRoot -startDir $PWD.Path

# ONLY search in Src\Common (as requested)
$commonRoot = Join-Path $solutionDir "Src\Common"

Write-Host "Solution root : $solutionDir" -ForegroundColor DarkGray
Write-Host "Search root   : $commonRoot" -ForegroundColor DarkGray

if (!(Test-Path $commonRoot)) {
    throw "Search root not found: $commonRoot"
}

# --- Create destination folder if missing ---
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

        Write-Host "Searching for '$ProjectName' in Src\Common..." -ForegroundColor Blue
        $projects = Find-ProjectsByNameInCommon -name $ProjectName -commonRoot $commonRoot
        Write-Host "Matches found: $($projects.Count)" -ForegroundColor DarkGray

        if ($projects.Count -eq 0) {
            Write-Host "No project found for name: '$ProjectName' in Src\Common" -ForegroundColor Red
            $ProjectName = ""
        }

        $attempt++
    }

    if ($projects.Count -eq 0) {
        throw "No project found after $maxAttempts attempts. Check the search root: $commonRoot"
    }

    $selectedCsproj = $projects[0].FullName
}

Write-Host "Selected project : $ProjectName" -ForegroundColor Green
Write-Host "Selected .csproj : $selectedCsproj" -ForegroundColor DarkGray
Write-Host "Configuration    : $Configuration" -ForegroundColor DarkGray
Write-Host "Destination      : $Destination" -ForegroundColor DarkGray
if ($Version) { Write-Host "Version          : $Version" -ForegroundColor DarkGray }

# --- Pack ---
Write-Host "Running: dotnet pack ..." -ForegroundColor Green

$packArgs = @(
    "pack", $selectedCsproj,
    "-c", $Configuration,
    "-o", $Destination,
    "--nologo"
)

if (-not [string]::IsNullOrWhiteSpace($Version)) {
    $packArgs += "/p:PackageVersion=$Version"
}

& dotnet @packArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet pack failed."
}

Write-Host "Done. Packages were written to: $Destination" -ForegroundColor Green

# Show newest packages for this project
Get-ChildItem -Path $Destination -Filter "$ProjectName*.nupkg" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 5 FullName, LastWriteTime |
    ForEach-Object { Write-Host ("Created: {0} ({1})" -f $_.FullName, $_.LastWriteTime) -ForegroundColor DarkGray }
