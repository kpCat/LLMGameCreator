@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-unity-projection-verification.ps1" -Mode GenericFullPlaythrough -ApplyCleanup %*
exit /b %ERRORLEVEL%
