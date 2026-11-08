@echo off
setlocal

set "SERVICE=Ollama"
set "MODEL=gemma4:e4b"
set "MODELS_DIR=C:\ollama\models"
set "LOG_DIR=C:\ollama"
set "OLD_MODELS_DIR=%USERPROFILE%\.ollama\models"
set "STARTUP_SHORTCUT=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\Ollama.lnk"

net session >nul 2>&1
if errorlevel 1 (
    powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

set "OLLAMA_EXE="
for /f "delims=" %%p in ('where ollama.exe 2^>nul') do if not defined OLLAMA_EXE set "OLLAMA_EXE=%%p"
if not defined OLLAMA_EXE if exist "%LOCALAPPDATA%\Programs\Ollama\ollama.exe" set "OLLAMA_EXE=%LOCALAPPDATA%\Programs\Ollama\ollama.exe"
if not defined OLLAMA_EXE (
    echo Could not find ollama.exe. Install Ollama first.
    pause
    exit /b 1
)

echo Using %OLLAMA_EXE%

if exist "%STARTUP_SHORTCUT%" del /q "%STARTUP_SHORTCUT%"
taskkill /im "ollama app.exe" /f >nul 2>&1
nssm stop "%SERVICE%" >nul 2>&1
taskkill /im ollama.exe /f >nul 2>&1

if not exist "%MODELS_DIR%" mkdir "%MODELS_DIR%"
if exist "%OLD_MODELS_DIR%" robocopy "%OLD_MODELS_DIR%" "%MODELS_DIR%" /E /NFL /NDL /NJH /NJS /NP >nul

sc query "%SERVICE%" >nul 2>&1
if errorlevel 1 nssm install "%SERVICE%" "%OLLAMA_EXE%" serve

nssm set "%SERVICE%" Application "%OLLAMA_EXE%"
nssm set "%SERVICE%" AppParameters serve
nssm set "%SERVICE%" Start SERVICE_AUTO_START
nssm set "%SERVICE%" AppEnvironmentExtra "OLLAMA_MODELS=%MODELS_DIR%" "OLLAMA_HOST=127.0.0.1:11434"
nssm set "%SERVICE%" AppStdout "%LOG_DIR%\ollama.log"
nssm set "%SERVICE%" AppStderr "%LOG_DIR%\ollama.log"

nssm start "%SERVICE%"
timeout /t 10 /nobreak >nul
nssm status "%SERVICE%"

"%OLLAMA_EXE%" pull %MODEL%
"%OLLAMA_EXE%" list
pause
