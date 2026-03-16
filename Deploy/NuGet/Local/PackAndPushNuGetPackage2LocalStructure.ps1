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
    [string] $Version = ""  # optional override (e.g. 1.0.0). Leave empty to use csproj version.
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

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

function Read-PackageIdAndVersionFromNupkg {
    param([string]$nupkgPath)

    if (!(Test-Path $nupkgPath)) { throw "nupkg not found: $nupkgPath" }

    $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkgPath)
    try {
        $nuspecEntry = $zip.Entries | Where-Object { $_.FullName -like "*.nuspec" } | Select-Object -First 1
        if (-not $nuspecEntry) { throw "No .nuspec found inside: $nupkgPath" }

        $stream = $nuspecEntry.Open()
        try {
            $reader = New-Object System.IO.StreamReader($stream)
            $xmlText = $reader.ReadToEnd()
        } finally {
            $reader.Dispose()
            $stream.Dispose()
        }

        [xml]$xml = $xmlText

        # nuspec can have default namespace, so use namespace-agnostic selection
        $idNode  = $xml.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='id']")
        $verNode = $xml.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='version']")

        if (-not $idNode -or [string]::IsNullOrWhiteSpace($idNode.InnerText)) {
            throw "Could not read package id from nuspec inside: $nupkgPath"
        }
        if (-not $verNode -or [string]::IsNullOrWhiteSpace($verNode.InnerText)) {
            throw "Could not read package version from nuspec inside: $nupkgPath"
        }

        return @{
            Id      = $idNode.InnerText.Trim()
            Version = $verNode.InnerText.Trim()
        }
    }
    finally {
        $zip.Dispose()
    }
}

# --- Determine repo root + search root (ONLY Src), based on script location ---
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Find-RepoRootBySrc -startDir $scriptDir
$commonRoot = Join-Path $repoRoot "Src"

Write-Host "Script dir    : $scriptDir" -ForegroundColor DarkGray
Write-Host "Repo root     : $repoRoot" -ForegroundColor DarkGray
Write-Host "Search root   : $commonRoot" -ForegroundColor DarkGray

if (!(Test-Path $commonRoot)) { throw "Search root not found: $commonRoot" }

# --- Ensure destination exists ---
if (!(Test-Path $Destination)) {
    Write-Host "Destination folder not found. Creating: $Destination" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    Write-Host "Destination folder created: $Destination" -ForegroundColor Green
}

# --- Temp pack dir (unique per run) ---
$tempPackDir = Join-Path $env:TEMP ("nupkg_pack_" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempPackDir -Force | Out-Null

try {
    # --- Choose project (.csproj path wins, otherwise search by name) ---
    $selectedCsproj = $null

    if ($CsprojPath)  { $CsprojPath  = $CsprojPath.Trim().Trim('"') }
    if ($ProjectName) { $ProjectName = $ProjectName.Trim() }

    if (-not [string]::IsNullOrWhiteSpace($CsprojPath)) {
        if (!(Test-Path $CsprojPath)) { throw "The .csproj path does not exist: $CsprojPath" }
        if ($CsprojPath -notmatch "\.csproj$") { throw "Please provide a .csproj file path." }
        $selectedCsproj = (Get-Item $CsprojPath).FullName
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

        $selectedCsproj = $projects[0].FullName
    }

    Write-Host "Selected .csproj : $selectedCsproj" -ForegroundColor Green
    Write-Host "Packing to temp  : $tempPackDir" -ForegroundColor DarkGray

    # --- Pack to temp (disable symbols to avoid snupkg) ---
    $packArgs = @(
        "pack", $selectedCsproj,
        "-c", $Configuration,
        "-o", $tempPackDir,
        "--nologo",
        "/p:IncludeSymbols=false"
    )
    if (-not [string]::IsNullOrWhiteSpace($Version)) {
        $packArgs += "/p:PackageVersion=$Version"
    }

    & dotnet @packArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed." }

    # --- Find created nupkg ---
    $nupkgs = @(Get-ChildItem -Path $tempPackDir -Filter "*.nupkg" -File -ErrorAction SilentlyContinue)
    if ($nupkgs.Count -eq 0) { throw "No .nupkg produced in: $tempPackDir" }

    # pick newest
    $pkg = $nupkgs | Sort-Object LastWriteTime -Descending | Select-Object -First 1

    # --- Read Id/Version from nupkg and create structured folder ---
    $meta = Read-PackageIdAndVersionFromNupkg -nupkgPath $pkg.FullName
    $id  = $meta.Id
    $ver = $meta.Version

    $targetDir = Join-Path (Join-Path $Destination $id) $ver
    if (!(Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }

    $targetPath = Join-Path $targetDir $pkg.Name
    Copy-Item -Path $pkg.FullName -Destination $targetPath -Force

    Write-Host "Structured output created:" -ForegroundColor Green
    Write-Host "  $targetPath" -ForegroundColor Green
}
finally {
    # Cleanup temp folder
    if (Test-Path $tempPackDir) {
        Remove-Item -Path $tempPackDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}