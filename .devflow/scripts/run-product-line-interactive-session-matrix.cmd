@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-product-line-interactive-session-matrix.ps1" %*
exit /b %ERRORLEVEL%
