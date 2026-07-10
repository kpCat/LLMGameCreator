@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-runtime-backed-player-command-roundtrip.ps1" -ApplyCleanup %*
