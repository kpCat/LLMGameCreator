@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-product-line-runtime-variant-matrix.ps1" -ApplyCleanup %*
