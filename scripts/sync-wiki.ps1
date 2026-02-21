#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Syncs wiki content from docs/wiki/ to the GitHub Wiki repository.

.DESCRIPTION
    This script copies markdown files from docs/wiki/ in the main repo
    to the GitHub Wiki git repository and pushes changes.

.PARAMETER WikiRepoPath
    Path to the cloned wiki repo. Default: ../GitHub_Copilot_Demo_20260221_wiki

.EXAMPLE
    ./scripts/sync-wiki.ps1
    ./scripts/sync-wiki.ps1 -WikiRepoPath "C:\repos\my-wiki"
#>
param(
    [string]$WikiRepoPath = "$PSScriptRoot/../../GitHub_Copilot_Demo_20260221_wiki"
)

$ErrorActionPreference = "Stop"

$wikiSource = Join-Path $PSScriptRoot "../docs/wiki"
$wikiRepo = Resolve-Path $WikiRepoPath -ErrorAction SilentlyContinue

if (-not $wikiRepo) {
    Write-Host "Wiki repo not found at $WikiRepoPath" -ForegroundColor Red
    Write-Host "Clone it first: git clone https://github.com/openaitx-system/GitHub_Copilot_Demo_20260221.wiki.git" -ForegroundColor Yellow
    exit 1
}

Write-Host "Syncing wiki content..." -ForegroundColor Cyan
Write-Host "  Source: $wikiSource" -ForegroundColor Gray
Write-Host "  Target: $wikiRepo" -ForegroundColor Gray

# Copy all markdown files
Copy-Item "$wikiSource/*.md" $wikiRepo -Force
Write-Host "  Copied $(( Get-ChildItem "$wikiSource/*.md" ).Count) files" -ForegroundColor Green

# Commit and push
Push-Location $wikiRepo
try {
    git add -A
    $status = git status --porcelain
    if ($status) {
        git commit -m "docs: sync wiki content from main repo"
        git push origin master
        Write-Host "Wiki updated and pushed!" -ForegroundColor Green
    } else {
        Write-Host "No changes to sync." -ForegroundColor Yellow
    }
} finally {
    Pop-Location
}
