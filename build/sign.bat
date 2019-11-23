@echo off
:: This script is a simple wrapper to be able to sign files just by dragging them to the batch file
powershell -File "%~dp0/sign.ps1" %*
pause
