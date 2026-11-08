@echo off
setlocal

set "SERVICE=ViolastroBot"
set "BASE_DIR=%~dp0"
set "BASE_DIR=%BASE_DIR:~0,-1%"
set "BOT_DIR=%BASE_DIR%\ViolastroBot"
set "LOG=%BOT_DIR%\ViolastroBot.log"

if /i "%~1"=="run" goto :run

net session >nul 2>&1
if errorlevel 1 (
    powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

if not exist "%BOT_DIR%" mkdir "%BOT_DIR%"

sc query "%SERVICE%" >nul 2>&1
if errorlevel 1 nssm install "%SERVICE%" "%~f0" run

nssm set "%SERVICE%" Application "%~f0"
nssm set "%SERVICE%" AppParameters run
nssm set "%SERVICE%" AppDirectory "%BASE_DIR%"
nssm set "%SERVICE%" AppStdout "%LOG%"
nssm set "%SERVICE%" AppStderr "%LOG%"

echo Restarting %SERVICE%...
nssm stop "%SERVICE%" >nul 2>&1
nssm start "%SERVICE%"
timeout /t 15 /nobreak >nul
nssm status "%SERVICE%"
if exist "%LOG%" powershell -NoProfile -Command "$m = Select-String -Path '%LOG%' -Pattern 'Fetching latest build' | Select-Object -Last 1; if ($m) { (Get-Content '%LOG%' | Select-Object -Skip ($m.LineNumber - 1)) -replace '^Terminate batch job \(Y/N\)\? ', '' }"
pause
exit /b

:run
cd /d "%BASE_DIR%"

echo Fetching latest build...
set "ASSET_URL="
set "DOWNLOADED="
for /f "usebackq delims=" %%u in (`powershell -NoProfile -Command "[Net.ServicePointManager]::SecurityProtocol='Tls12'; (Invoke-RestMethod 'https://api.github.com/repos/Pattrigue/ViolastroBot/releases/latest').assets[0].url"`) do set "ASSET_URL=%%u"
if defined ASSET_URL curl -fsSL --retry 3 --retry-delay 5 -H "Accept: application/octet-stream" -o ViolastroBot.zip "%ASSET_URL%" && set "DOWNLOADED=1"
if not defined DOWNLOADED (
    echo Download failed. Keeping the current build.
    del /q ViolastroBot.zip 2>nul
    goto :launch
)

if exist ViolastroBot.new rmdir /s /q ViolastroBot.new
powershell -NoProfile -Command "Expand-Archive -Path .\ViolastroBot.zip -DestinationPath .\ViolastroBot.new"
del /q ViolastroBot.zip

if not exist ViolastroBot.new\ViolastroBot.exe (
    echo The download did not contain ViolastroBot.exe. Keeping the current build.
    rmdir /s /q ViolastroBot.new 2>nul
    goto :launch
)

robocopy ViolastroBot.new ViolastroBot /MIR /XF ViolastroBot.log /NFL /NDL /NJH /NJS /NP >nul
if errorlevel 8 echo Could not update the ViolastroBot folder.
rmdir /s /q ViolastroBot.new 2>nul

:launch
cd /d "%BOT_DIR%"
ViolastroBot.exe
exit /b %errorlevel%
