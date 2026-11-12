param([switch]$NoBrowser)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
$runtime=Join-Path $projectRoot 'runtime'
New-Item -ItemType Directory -Path $runtime -Force | Out-Null
$mutex=New-Object System.Threading.Mutex($false,'Local\TallerElectrodomesticos.Iniciar')
$acquired=$false
function Get-Health([string]$url,[string]$app) {
 try { $r=Invoke-RestMethod -Uri $url -TimeoutSec 2; return $r.application -eq $app -and $r.status -eq 'ok' } catch {return $false}
}
try {
 $acquired=$mutex.WaitOne(90000)
 if(-not $acquired){throw 'Otra instancia está iniciando el taller. Espere unos segundos.'}
 . (Join-Path $PSScriptRoot 'Servidor.ps1')
 $servicioSql=Get-TallerServicioSql (Get-TallerServidor)
 $estadoSql=Get-Service -Name $servicioSql -ErrorAction SilentlyContinue
 if(-not $estadoSql){throw "No se encontró SQL Server ($servicioSql) en esta PC. Instale SQL Server Express y ejecute Instalar.cmd."}
 if($estadoSql.Status -ne 'Running'){throw "SQL Server está detenido. Inicie el servicio $servicioSql y vuelva a abrir el taller."}
 foreach($item in @(@{Name='Taller.Web';Port=7181;Health='health'},@{Name='Taller.Gateway';Port=7180;Health='gateway-health'})) {
  $url='http://127.0.0.1:'+ $item.Port +'/'+$item.Health
  if(Get-Health $url $item.Name){continue}
  if(Get-NetTCPConnection -State Listen -LocalPort $item.Port -ErrorAction SilentlyContinue){throw "El puerto $($item.Port) está ocupado por otra aplicación."}
  $folder=Join-Path $projectRoot ('runtime\publish\'+$item.Name)
  $exe=Join-Path $folder ($item.Name+'.exe')
  if(-not(Test-Path -LiteralPath $exe)){throw 'Falta la aplicación compilada. Si es una PC nueva ejecute Instalar.cmd; si no, scripts\Preparar.ps1.'}
  $process=Start-Process -FilePath $exe -WorkingDirectory $folder -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtime ($item.Name+'.log')) -RedirectStandardError (Join-Path $runtime ($item.Name+'.error.log'))
  $ready=$false
  for($attempt=0;$attempt -lt 40;$attempt++) {
   if(Get-Health $url $item.Name){$ready=$true;break}
   if($process.HasExited){throw "No pudo iniciar $($item.Name). Consulte runtime\$($item.Name).error.log."}
   Start-Sleep -Milliseconds 500
  }
  if(-not $ready){throw "El inicio de $($item.Name) excedió el tiempo previsto. Consulte runtime."}
 }
 if(-not(Get-Health 'http://127.0.0.1:7180/health' 'Taller.Web')){throw 'YARP no pudo conectar la aplicación con SQL Server.'}
 if(-not $NoBrowser){Start-Process 'http://127.0.0.1:7180'}
 Write-Output 'Taller disponible en http://127.0.0.1:7180'
} catch {
 $_ | Out-String | Set-Content -LiteralPath (Join-Path $runtime 'ultimo-error.txt') -Encoding UTF8
 if(-not $NoBrowser){Add-Type -AssemblyName System.Windows.Forms;[System.Windows.Forms.MessageBox]::Show($_.Exception.Message,'TALLER DE GESTION DE REPARACION DE ELECTRODOMESTICOS') | Out-Null}
 throw
} finally {if($acquired){$mutex.ReleaseMutex()};$mutex.Dispose()}
