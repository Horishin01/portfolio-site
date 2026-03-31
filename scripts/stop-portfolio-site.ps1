param(
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot)
)

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$escapedRepoRoot = [Regex]::Escape($resolvedRepoRoot)

$targets = Get-CimInstance Win32_Process | Where-Object {
    $_.Name -eq "dotnet.exe" -and
    $_.CommandLine -and
    $_.CommandLine -match $escapedRepoRoot -and
    ($_.CommandLine -like "*PortfolioSite.dll*" -or $_.CommandLine -like "*PortfolioSite.exe*")
}

foreach ($target in $targets) {
    Stop-Process -Id $target.ProcessId -Force -ErrorAction SilentlyContinue
}

exit 0
