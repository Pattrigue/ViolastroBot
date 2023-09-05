@echo off
setlocal

if exist ViolastroBot rmdir /s /q ViolastroBot

set "ASSET_NAME=ViolastroBot.zip"
set "EXTRACTED_FOLDER=ViolastroBot"

curl -s https://api.github.com/repos/Pattrigue/ViolastroBot/releases/latest > latest.json

for /f "delims=" %%i in ('findstr /R /C:"browser_download_url.*%ASSET_NAME%" latest.json') do set "URL_LINE=%%i"
set "DOWNLOAD_URL=%URL_LINE:*browser_download_url": =%"
set "DOWNLOAD_URL=%DOWNLOAD_URL:~1,-1%"

echo Latest release URL: %DOWNLOAD_URL%

curl -L -o %ASSET_NAME% %DOWNLOAD_URL%

del latest.json

echo Extracting %ASSET_NAME%...
powershell -command "Expand-Archive -Path .\%ASSET_NAME% -DestinationPath .\%EXTRACTED_FOLDER%"

echo Removing the downloaded ZIP file...
del %ASSET_NAME%

echo Running ViolastroBot...
cd .\%EXTRACTED_FOLDER%\
start ViolastroBot.exe

endlocal
