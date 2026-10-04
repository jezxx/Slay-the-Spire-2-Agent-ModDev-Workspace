@echo off
setlocal
set "WORKFLOW_ROOT=%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%WORKFLOW_ROOT%dev.ps1" %*
exit /b %errorlevel%
