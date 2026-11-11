$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$desktopPath=[Environment]::GetFolderPath('Desktop')
$title='TALLER DE GESTION DE REPARACION DE ELECTRODOMESTICOS'
$icon=Join-Path $projectRoot 'src\Taller.Web\wwwroot\branding\taller.ico'
$launcher=Join-Path $projectRoot 'Iniciar.cmd'
if(-not(Test-Path -LiteralPath $icon) -or -not(Test-Path -LiteralPath $launcher)){throw 'Falta el icono o el iniciador del taller.'}
$shortcutPath=Join-Path $desktopPath ($title+'.lnk')
$shortcutShell=New-Object -ComObject WScript.Shell
$shortcut=$shortcutShell.CreateShortcut($shortcutPath)
$shortcut.TargetPath=$launcher
$shortcut.WorkingDirectory=$projectRoot
$shortcut.IconLocation=$icon+',0'
$shortcut.WindowStyle=7
$shortcut.Description='Abrir el taller local con SQL Server y YARP'
$shortcut.Save()
if(-not(Test-Path -LiteralPath $shortcutPath)){throw 'No se pudo guardar el acceso directo.'}
# Preserve the former desktop CMD before replacing it with the icon-bearing shortcut.
$previous=Join-Path $desktopPath ($title+'.cmd')
if(Test-Path -LiteralPath $previous){
 $archive=Join-Path $projectRoot ('runtime\Iniciador-escritorio-anterior-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.cmd')
 if(-not([IO.Path]::GetFullPath($archive).StartsWith($projectRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase))){throw 'La ruta de respaldo no pertenece al proyecto.'}
 Move-Item -LiteralPath $previous -Destination $archive
}
$verification=$shortcutShell.CreateShortcut($shortcutPath)
if($verification.IconLocation -ne ($icon+',0') -or $verification.TargetPath -ne $launcher){throw 'El acceso directo no conserva el icono o el destino esperado.'}
Write-Output "Acceso directo instalado: $shortcutPath"
Write-Output "Icono: $($verification.IconLocation)"
