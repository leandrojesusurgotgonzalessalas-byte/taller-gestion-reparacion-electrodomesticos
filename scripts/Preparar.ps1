$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
Push-Location $projectRoot
try {
 dotnet build TallerElectrodomesticos.sln -c Release
 if($LASTEXITCODE -ne 0){throw 'Falló la compilación.'}
 foreach($name in 'Taller.Web','Taller.Gateway') {
  dotnet publish "src\$name\$name.csproj" -c Release -r win-x64 --self-contained true -o "runtime\publish\$name"
  if($LASTEXITCODE -ne 0){throw "Falló la publicación de $name."}
 }
} finally {Pop-Location}
