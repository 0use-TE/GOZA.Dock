@echo off
cd /d "C:\Users\Ouse\Desktop\Projects\GOZA.Dock"
echo Launching docfx on port 8090 ...
start "docfx-goza" /B "C:\Users\Ouse\.dotnet\tools\docfx.exe" serve _site --port 8090 --open false