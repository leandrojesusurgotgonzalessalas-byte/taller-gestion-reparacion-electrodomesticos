# Instala el taller en una PC nueva después de descargarlo de GitHub.
# - Comprueba el SDK de .NET y SQL Server.
# - Si SQL Server no es la instancia predeterminada (por ejemplo SQL Server Express), guarda la conexión
#   en src\Taller.Web\appsettings.Local.json (archivo propio de esa PC, excluido de Git).
# - Compila y publica la aplicación, crea el acceso directo y abre el taller.
# La base TallerElectrodomesticos se crea vacía la primera vez que inicia la aplicación.
# Si la base ya existe, NO se modifica ni se borra ningún dato.
param(
 [string]$Servidor,          # Opcional: por ejemplo "localhost\SQLEXPRESS". Si se omite se detecta solo.
 [switch]$SinAccesoDirecto,
 [switch]$SinAbrir
)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Servidor.ps1')
$configLocal=Join-Path $projectRoot 'src\Taller.Web\appsettings.Local.json'

function Paso([string]$texto){Write-Host ''; Write-Host "==> $texto" -ForegroundColor Cyan}

try {
 Paso 'Comprobando el SDK de .NET (se necesita la versión 9 o superior)'
 if(-not(Get-Command dotnet -ErrorAction SilentlyContinue)){
  throw 'No se encontró .NET. Instale el SDK de .NET 9 desde https://dotnet.microsoft.com/download/dotnet/9.0 y vuelva a ejecutar Instalar.cmd.'
 }
 $sdks=@(& dotnet --list-sdks 2>$null | ForEach-Object { [int](($_ -split '\.')[0]) } | Where-Object { $_ -ge 9 })
 if($sdks.Count -eq 0){
  throw 'Falta el SDK de .NET 9 o superior (solo hay runtimes o versiones antiguas). Descárguelo de https://dotnet.microsoft.com/download/dotnet/9.0.'
 }
 Write-Host "SDK encontrado: $(& dotnet --version)"

 Paso 'Buscando SQL Server en esta PC'
 if(-not $Servidor){
  if(Test-Path -LiteralPath $configLocal){
   $Servidor=Get-TallerServidor
   Write-Host "Se conserva la configuración existente de esta PC: $Servidor"
  } else {
   $servicios=@(Get-Service -Name 'MSSQLSERVER','MSSQL$*' -ErrorAction SilentlyContinue)
   if($servicios.Count -eq 0){
    throw 'No se encontró SQL Server. Instale SQL Server Express (gratuito) desde https://www.microsoft.com/sql-server/sql-server-downloads con la instancia predeterminada o SQLEXPRESS, y vuelva a ejecutar Instalar.cmd.'
   }
   $elegido=$servicios | Where-Object Name -eq 'MSSQLSERVER' | Select-Object -First 1
   if(-not $elegido){$elegido=$servicios | Where-Object Name -eq 'MSSQL$SQLEXPRESS' | Select-Object -First 1}
   if(-not $elegido){$elegido=$servicios | Select-Object -First 1}
   if($elegido.Name -eq 'MSSQLSERVER'){$Servidor='localhost'} else {$Servidor='localhost\'+$elegido.Name.Substring(6)}
  }
 }
 $servicioSql=Get-TallerServicioSql $Servidor
 $estado=Get-Service -Name $servicioSql -ErrorAction SilentlyContinue
 if(-not $estado){throw "No existe el servicio $servicioSql para el servidor '$Servidor'."}
 if($estado.Status -ne 'Running'){
  Write-Host "Iniciando el servicio $servicioSql..."
  try {Start-Service -Name $servicioSql} catch {throw "SQL Server ($servicioSql) está detenido y no se pudo iniciar. Ábralo como administrador o inícielo desde 'Servicios' de Windows."}
 }
 Write-Host "Servidor SQL que usará el taller: $Servidor"

 if($Servidor -ne 'localhost'){
  $cadena="Server=$Servidor;Database=TallerElectrodomesticos;Integrated Security=True;TrustServerCertificate=True;Encrypt=True"
  $json=@{ConnectionStrings=@{Taller=$cadena}} | ConvertTo-Json -Depth 3
  [IO.File]::WriteAllText($configLocal,$json,(New-Object System.Text.UTF8Encoding($false)))
  Write-Host "Conexión guardada en src\Taller.Web\appsettings.Local.json (solo para esta PC)."
 }

 Paso 'Compilando y publicando la aplicación (la primera vez descarga paquetes de Internet)'
 & (Join-Path $PSScriptRoot 'Detener.ps1') | Out-Null
 & (Join-Path $PSScriptRoot 'Preparar.ps1')

 if(-not $SinAccesoDirecto){
  Paso 'Creando el acceso directo en el escritorio'
  & (Join-Path $PSScriptRoot 'InstalarAccesoDirecto.ps1')
 }

 Paso 'Iniciando el taller (crea la base vacía si todavía no existe)'
 if($SinAbrir){& (Join-Path $PSScriptRoot 'Iniciar.ps1') -NoBrowser} else {& (Join-Path $PSScriptRoot 'Iniciar.ps1')}

 Write-Host ''
 Write-Host 'Instalación terminada. Abra el taller en http://127.0.0.1:7180 o con el acceso directo del escritorio.' -ForegroundColor Green
} catch {
 Write-Host ''
 Write-Host ('ERROR: '+$_.Exception.Message) -ForegroundColor Red
 Write-Host 'Si el problema es al crear la base, revise runtime\Taller.Web.error.log.'
 exit 1
}
