@echo off
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-character-attributes-level-progression-slice.ps1" %*
exit /b %errorlevel%
