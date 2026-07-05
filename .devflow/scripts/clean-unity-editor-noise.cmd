@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0clean-unity-editor-noise.ps1" -Apply %*
exit /b %ERRORLEVEL%
