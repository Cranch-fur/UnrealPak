@echo off
setlocal enabledelayedexpansion

set /p "startIndex=Specify starting index: "

REM Check for arguments
if "%~1"=="" (
    echo Usage: %~nx0 "folder_path1" "folder_path2" ...
    exit /b 1
)

set loopCount=0

:loop
if "%~1"=="" goto end

REM Calculate the current index
set /a calculatedIndex=!startIndex! + !loopCount!

echo Running program with argument: "%~1" -forcedIndex -index=!calculatedIndex!
UnrealPak.exe "%~1" -forcedIndex -index=!calculatedIndex!

REM Increment the counter and move to the next argument
set /a loopCount+=1
shift
goto loop

:end
echo All folders processed.
pause
endlocal
