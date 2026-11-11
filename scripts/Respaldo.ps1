$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
$dest=[IO.Path]::GetFullPath((Join-Path $projectRoot 'backups'))
New-Item -ItemType Directory -Force -Path $dest | Out-Null
# SQL Server writes within this folder using its service identity.
$sqlService=Get-CimInstance Win32_Service -Filter "Name='MSSQLSERVER'"
$serviceIdentity=$sqlService.StartName
$acl=Get-Acl -LiteralPath $dest
$rule=New-Object System.Security.AccessControl.FileSystemAccessRule($serviceIdentity,'Modify','ContainerInherit,ObjectInherit','None','Allow')
$acl.SetAccessRule($rule)
Set-Acl -LiteralPath $dest -AclObject $acl
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss'
$backupFile=Join-Path $dest "TallerElectrodomesticos-$stamp.bak"
$sqlPath=$backupFile.Replace("'","''")
& sqlcmd -S localhost -E -b -Q "BACKUP DATABASE [TallerElectrodomesticos] TO DISK=N'$sqlPath' WITH COPY_ONLY, CHECKSUM; RESTORE VERIFYONLY FROM DISK=N'$sqlPath' WITH CHECKSUM;"
if($LASTEXITCODE -ne 0){throw 'SQL Server no pudo crear/verificar el respaldo.'}
Write-Output "Respaldo verificado: $backupFile"