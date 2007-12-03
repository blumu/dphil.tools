:: by William Blum, 3/12/2007
:: This file is deprecated, use Omake instead to build the project. (http://omake.metaprl.org/index.html)
@if "%_echo%"=="" echo off

if "%1"=="release" goto set_release
if "%1"=="debug" goto set_debug
goto bad_parameter

:bad_parameter
echo.
echo Bad build paramter!
echo Valid value for the first argument are: debug release
echo Valid value for the second argument (optional): mono
exit /b 10

:set_release
echo   * Release mode
set FSC_OPTIONS=--standalone -O3 
goto testmonomode

:set_debug
echo   * Debug mode
set FSC_OPTIONS=-Ooff -g
goto testmonomode


:testmonomode
if "%2" EQU "mono" goto set_mono

:no_mono
echo Compilation for .NET
set mono_mode=0
set RESOURCES_ARG=--resource horsform.resources --resource cpdaform.resources 
set OUTPUT_EXE=hog.exe
goto setenvir

:set_mono
echo Compilation for Mono
set mono_mode=1
set RESOURCES_ARG=horsform+horsform.resx horsform.resx cpdaform.resx cpdaform+cpdaform.resx
set OUTPUT_EXE=hog-mono.exe


:setenvir
setlocal
if "%FSHARP_HOME%"=="" ( set FSHARP_HOME=C:\Program Files\FSharp-1.9.3.7)
if "%FSC%"=="" ( set FSC=%FSHARP_HOME%\bin\fsc.exe)
if "%FSYACC%"=="" ( set FSYACC=%FSHARP_HOME%\bin\fsyacc.exe)
if "%FSLEX%"=="" ( set FSLEX=%FSHARP_HOME%\bin\fslex.exe)
if "%GLEE%"=="" ( set GLEE=C:\Program Files\Microsoft Research\GLEE)
if "%RESXC%"=="" ( set RESXC=%FSHARP_HOME%\bin\resxc.exe)


set SOURCE_FILES= common.ml type.ml hog.mli hog.ml hocpda.ml cpdaform.fs texexportform.fs horsform.fs parsing.ml hog_parser.mli hog_parser.ml hog_lexer.ml ml_structs.mli ml_structs.ml ml_parser.mli ml_parser.ml ml_lexer.ml lmdtermform.fs  mainform.fs 

set RESOURCES_FILES=horsform.resx cpdaform.resx lmdtermform.resx lmdtermform.resx


:parserlexer
echo   - Lex for HORS...
"""%FSLEX%""" -o hog_lexer.ml hog_lexer.mll
echo   - Lex for CoreML...
"""%FSLEX%""" -o ml_lexer.ml ml_lexer.mll
if ERRORLEVEL 1 goto Exit

echo   - Yacc for HORS...
"""%FSYACC%""" -o hog_parser.ml hog_parser.mly
echo   - Yacc for CoreML...
"""%FSYACC%""" -o ml_parser.ml ml_parser.mly
if ERRORLEVEL 1 goto Exit


:resources
echo     - compiling resources
"""%RESXC%""" %RESOURCES_FILES%


:compile
echo - F# compilation...
echo     - compiling sources
if %mono_mode% == 0 goto fsc_compile
echo     - Create duplicate resource files (this is necessary because of a bug in Mono)
copy /y horsform.resx "horsform+horsform.resx"
copy /y cpdaform.resx "cpdaform+cpdaform.resx"

:fsc_compile
"""%FSC%""" %FSC_OPTIONS% --fullpaths --progress -I "%GLEE%\bin" -r Microsoft.GLEE.dll -r Microsoft.GLEE.GraphViewerGDI.dll -r Microsoft.GLEE.Drawing.dll --no-warn 40 --target-winexe -o %OUTPUT_EXE% %RESOURCES_ARG% %SOURCE_FILES%
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


