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
if %1 == release goto release
if %1 == debug goto debug
if %1 == mono goto mono
if %1 == mono_release goto mono
if %1 == "" goto debug

echo.
echo Bad build paramter!
echo Valid parameters are: debug release mono mono_release
exit /b 10


:debug
echo   * Debug mode
"""%FSC%""" --fullpaths --progress -Ooff -I "%GLEE%\bin" -r Microsoft.GLEE.dll -r Microsoft.GLEE.GraphViewerGDI.dll -r Microsoft.GLEE.Drawing.dll -o hog.exe --no-warn 40 --target-winexe -g common.ml hog.mli hog.ml hocpda.ml cpdaform.fs horsform.fs horsform.resx cpdaform.resx parsing.ml hog_parser.mli hog_parser.ml hog_lexer.ml mainform.fs
if ERRORLEVEL 1 goto exit
goto success


:release
echo   * Release mode
"""%FSC%""" --fullpaths --progress --standalone -O3 -I "%GLEE%\bin" -r Microsoft.GLEE.dll -r Microsoft.GLEE.GraphViewerGDI.dll -r Microsoft.GLEE.Drawing.dll -o hog.exe --no-warn 40 --target-winexe common.ml hog.mli hog.ml hocpda.ml cpdaform.fs horsform.fs horsform.resx cpdaform.resx parsing.ml hog_parser.mli hog_parser.ml hog_lexer.ml mainform.fs
if ERRORLEVEL 1 goto exit
goto success


:mono
echo   * Mono build
echo     - Duplicate the resource files (to fix a bug in Mono)
copy /y horsform.resx "horsform+horsform.resx"
copy /y cpdaform.resx "cpdaform+cpdaform.resx"

if %1 == mono goto mono_debug
if %1 == mono_release goto mono_release

:mono_debug
echo   * Debug mode
"""%FSC%""" --fullpaths --progress -Ooff -I "%GLEE%\bin" -r Microsoft.GLEE.dll -r Microsoft.GLEE.GraphViewerGDI.dll -r Microsoft.GLEE.Drawing.dll -o hog.exe --no-warn 40 -g common.ml hog.mli hog.ml hocpda.ml cpdaform.fs horsform.fs horsform+horsform.resx horsform.resx cpdaform.resx cpdaform+cpdaform.resx parsing.ml hog_parser.mli hog_parser.ml hog_lexer.ml mainform.fs
if ERRORLEVEL 1 goto exit
goto success


:mono_release
echo   * Release mode
"""%FSC%""" --fullpaths --progress --standalone -O3 -I "%GLEE%\bin" -r Microsoft.GLEE.dll -r Microsoft.GLEE.GraphViewerGDI.dll -r Microsoft.GLEE.Drawing.dll -o hog.exe --no-warn 40 common.ml hog.mli hog.ml hocpda.ml cpdaform.fs horsform.fs horsform+horsform.resx horsform.resx cpdaform.resx cpdaform+cpdaform.resx parsing.ml hog_parser.mli hog_parser.ml hog_lexer.ml mainform.fs
if ERRORLEVEL 1 goto exit
goto success

:success
echo ********************************************
echo Built ok, you may now run Hog.exe
echo ********************************************

:exit
endlocal
rem if ERRORLEVEL 1 pause
exit /b %ERRORLEVEL%


