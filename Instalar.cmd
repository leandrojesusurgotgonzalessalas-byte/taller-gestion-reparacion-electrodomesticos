@echo off
rem Instala el taller en esta PC: comprueba .NET y SQL Server, compila, crea el acceso directo y lo abre.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Instalar.ps1" %*
pause
