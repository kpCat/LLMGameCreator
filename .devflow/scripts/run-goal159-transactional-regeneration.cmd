@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal159-transactional-regeneration.ps1" %*
exit /b %ERRORLEVEL%
