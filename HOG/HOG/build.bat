@if "%_echo%"=="" echo off
setlocal
if "%FSHARP_HOME%"=="" ( set FSHARP_HOME=C:\Program Files\FSharp-1.9.1.8)
if "%FSC%"=="" ( set FSC=%FSHARP_HOME%\bin\fsc.exe )
if "%FSYACC%"=="" ( set FSYACC=%FSHARP_HOME%\bin\fsyacc.exe )
if "%FSLEX%"=="" ( set FSLEX=%FSHARP_HOME%\bin\fslex.exe )



"""%FSC%""" --fullpaths --progress -Ooff -o Differentiate.exe --no-warn 40 --target-winexe -g hog.fsi form.fs hog.fs   form.resx

if ERRORLEVEL 1 goto Exit

echo ********************************************
echo Built ok, you may now run Hog.exe
echo ********************************************

:Exit
endlocal
pause
exit /b %ERRORLEVEL%


