@if "%_echo%"=="" echo off
setlocal
if "%FSHARP_HOME%"=="" ( set FSHARP_HOME=C:\Program Files\FSharp-1.9.1.8)
if "%FSC%"=="" ( set FSC=%FSHARP_HOME%\bin\fsc.exe )
if "%FSYACC%"=="" ( set FSYACC=%FSHARP_HOME%\bin\fsyacc.exe )
if "%FSLEX%"=="" ( set FSLEX=%FSHARP_HOME%\bin\fslex.exe )
if "%GLEE%"=="" ( set GLEE=C:\Program Files\Microsoft Research\GLEE)

"""%FSC%""" --fullpaths --progress -Ooff -I "%GLEE%\bin" -r Microsoft.GLEE.dll -r Microsoft.GLEE.GraphViewerGDI.dll -r Microsoft.GLEE.Drawing.dll -o hog.exe --no-warn 40 --target-winexe -g hog.mli hog.ml hocpda.ml myform.fs cpdaform.fs myform.resx

if ERRORLEVEL 1 goto Exit

echo ********************************************
echo Built ok, you may now run Hog.exe
echo ********************************************

:Exit
endlocal
rem pause
exit /b %ERRORLEVEL%


