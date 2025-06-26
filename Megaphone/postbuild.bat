@echo off

echo Running post-build script

REM === CONSTANTS ===
set GAME_DIR=Lethal Company\BepInEx\plugins
set MOD_NAME=Megaphone
set DLL_NAME=ESN.Megaphone.dll
set BUILD_CONFIG=Debug
set TARGET_FRAMEWORK=netstandard2.1
set GAME_EXE=Lethal Company\Lethal Company.exe

REM === DERIVED PATHS ===
set projDir=%cd%
set modDir=%GAME_DIR%\%MOD_NAME%
set assetsDir=%modDir%\assets
set buildDir=%projDir%\bin\%BUILD_CONFIG%\%TARGET_FRAMEWORK%

echo Project directory is %projDir%
cd ..
cd ..

REM Clean existing mod directory
IF exist "%modDir%" (
   echo Deleting mod directory: %modDir%
   rmdir /S /Q "%modDir%"
)

REM Create mod directory structure
echo Creating directory: %assetsDir%
mkdir "%assetsDir%"

REM Copy DLL
echo Copying DLL to mod directory...
copy "%buildDir%\%DLL_NAME%" "%modDir%"
if %errorlevel% neq 0 (
   echo Failed to copy DLL!
   exit /b %errorlevel%
)

REM Copy assets (if they exist)
if exist "%buildDir%\assets" (
   echo Copying assets...
   xcopy "%buildDir%\assets\*" "%assetsDir%\" /E /I /Y
   if %errorlevel% neq 0 (
       echo Failed to copy assets!
       exit /b %errorlevel%
   )
) else (
   echo No assets directory found, skipping...
)

echo Copy successful!

REM Launch game
echo Launching Lethal Company...
start "" "%GAME_EXE%"

echo Done!