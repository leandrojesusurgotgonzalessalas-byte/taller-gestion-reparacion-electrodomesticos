$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
foreach($name in 'Taller.Web','Taller.Gateway') {
 $expected=[IO.Path]::GetFullPath((Join-Path $projectRoot "runtime\publish\$name\$name.exe"))
 Get-Process -Name $name -ErrorAction SilentlyContinue | Where-Object {$_.Path -eq $expected} | Stop-Process
}
Write-Output 'Procesos del taller detenidos.'
