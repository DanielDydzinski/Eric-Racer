# Launches a host plus N clients of the dev build on this PC, lets them auto-drive, then stops them
# and prints the interesting log lines from each window.
# Usage: powershell -File run-local.ps1 [-Clients 2] [-Seconds 25] [-Build "Builds\Dev\EricRacer.exe"]
param(
    [int]$Clients = 2,
    [int]$Seconds = 25,
    [string]$Build = "Builds\Dev\EricRacer.exe",
    [int]$Laps = 1
)

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
$exe = Join-Path $root $Build
$logDir = Join-Path (Split-Path $exe) "logs"
New-Item -ItemType Directory -Force $logDir | Out-Null
Get-ChildItem $logDir -Filter *.log -ErrorAction SilentlyContinue | Remove-Item

$common = @("-screen-fullscreen", "0", "-screen-width", "640", "-screen-height", "360", "-autodrive", "-netlog", "-laps", "$Laps")
$procs = @()
# Paths contain spaces ("Unity projects"), so the log path must be quoted inside the argument.
$procs += Start-Process $exe -PassThru -ArgumentList ($common + @("-host", "-players", "$($Clients + 1)", "-name", "Host", "-logFile", "`"$(Join-Path $logDir 'host.log')`""))
Start-Sleep -Seconds 4
for ($i = 1; $i -le $Clients; $i++) {
    $procs += Start-Process $exe -PassThru -ArgumentList ($common + @("-join", "127.0.0.1", "-name", "Client$i", "-logFile", "`"$(Join-Path $logDir "client$i.log")`""))
    Start-Sleep -Seconds 1
}

Start-Sleep -Seconds $Seconds
foreach ($p in $procs) { if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force } }
Start-Sleep -Seconds 1

foreach ($log in Get-ChildItem $logDir -Filter *.log) {
    Write-Output "===== $($log.Name) ====="
    Select-String -Path $log.FullName -Pattern "\[Connection\]|\[Race\]|Exception|Error|\[Netcode\]" | ForEach-Object { $_.Line }
    Select-String -Path $log.FullName -Pattern "\[NetSync\]" | Select-Object -Last 2 | ForEach-Object { $_.Line }
}
