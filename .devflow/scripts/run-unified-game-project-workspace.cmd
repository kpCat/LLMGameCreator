@echo off
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-unified-game-project-workspace.ps1" %*
exit /b %errorlevel%
