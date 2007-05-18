@if "%_echo%"=="" echo off

:setenvir
setlocal
if "%FSHARP_HOME%"=="" ( set FSHARP_HOME=C:\Program Files\FSharp-1.9.1.9)
if "%FSC%"=="" ( set FSC=%FSHARP_HOME%\bin\fsc.exe )
if "%FSYACC%"=="" ( set FSYACC=%FSHARP_HOME%\bin\fsyacc.exe )
if "%FSLEX%"=="" ( set FSLEX=%FSHARP_HOME%\bin\fslex.exe )
if "%GLEE%"=="" ( set GLEE=C:\Program Files\Microsoft Research\GLEE)

:parserlexer
echo - Lex...
"""%FSLEX%""" -o hog_lexer.ml hog_lexer.mll
if ERRORLEVEL 1 goto Exit

echo - Yacc...
"""%FSYACC%""" -o hog_parser.ml hog_parser.mly
if ERRORLEVEL 1 goto Exit


:compile
echo - F# compilation...
"""%FSC%""" --target-winexe --fullpaths --progress -Ooff -I "%GLEE%\bin" -r Microsoft.GLEE.dll -r Microsoft.GLEE.GraphViewerGDI.dll -r Microsoft.GLEE.Drawing.dll -o hog.exe --no-warn 40 --target-winexe -g hog.mli hog.ml hocpda.ml cpdaform.fs horsform.fs horsform.resx cpdaform.resx parsing.ml hog_parser.mli hog_parser.ml hog_lexer.ml mainform.fs

if ERRORLEVEL 1 goto exit

echo ********************************************
echo Built ok, you may now run Hog.exe
echo ********************************************

:exit
endlocal
rem pause
exit /b %ERRORLEVEL%


