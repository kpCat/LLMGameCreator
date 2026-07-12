@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-complete-test-suite.ps1" %*
exit /b %errorlevel%
