@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-gamepackage-projection-matrix.ps1" -ApplyCleanup %*
exit /b %ERRORLEVEL%
