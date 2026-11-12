# Funciones compartidas: averiguan qué instancia de SQL Server usa el taller en esta PC.
# Si existe src\Taller.Web\appsettings.Local.json (lo crea Instalar.ps1 en otras PC), se usa ese servidor.
# Si no existe, se usa la instancia predeterminada "localhost", como en la PC original.
$script:TallerConfigLocal=Join-Path (Split-Path $PSScriptRoot -Parent) 'src\Taller.Web\appsettings.Local.json'

function Get-TallerServidor {
 if(Test-Path -LiteralPath $script:TallerConfigLocal){
  $config=Get-Content -LiteralPath $script:TallerConfigLocal -Raw -Encoding UTF8 | ConvertFrom-Json
  $cadena=$config.ConnectionStrings.Taller
  if($cadena -match '(?i)(?:^|;)\s*(?:Server|Data Source)\s*=\s*([^;]+)'){return $Matches[1].Trim()}
 }
 return 'localhost'
}

function Get-TallerServicioSql([string]$servidor) {
 # localhost, . o (local) = instancia predeterminada MSSQLSERVER; "equipo\NOMBRE" = servicio MSSQL$NOMBRE.
 if($servidor -match '\\(.+)$'){return 'MSSQL$'+$Matches[1]}
 return 'MSSQLSERVER'
}
