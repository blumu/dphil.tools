@echo OFF
rem created by William Blum on 22 fev 2006
rem modified on 26 may 2006
rem  Install a truetype unicode font for latex and pdflatex
rem requirement:
rem   - ttf2tfm, ttfucs.sty and utf8ttf.def

echo Truetype font installer for Latex by William Blum.
echo Version 1.0 - 22/02/2006
echo.

setlocal
set FONT=%1
set PID=3
set EID=1
set FONTTTF=%1.ttf
set TEXMF=\localtexmf

if [%1] EQU [] goto print_syntax
if [%2] NEQ [] set TEXMF=%2
if [%3] NEQ [] set PID=%3 
if [%4] NEQ [] set EID=%4

IF NOT EXIST %FONTTTF% goto filenotfound
IF NOT EXIST %TEXMF% goto dirnotfound


echo Install the truetype font "%FONTTTF%" in the latex directory "%TEXMF%" ...

echo ON
mkdir %TEXMF%\fonts\truetype\
copy %FONTTTF% %TEXMF%\fonts\truetype\
cd %TEXMF%\fonts\truetype
ttf2tfm %FONTTTF% -P %PID% -E %EID% -w %FONT%@Unicode@ ||  goto ttf2tfm_error
del %FONT%.map
for %%i in (*.enc) do @echo %%~ni ^<%FONTTTF% ^<%%~ni.enc >>%FONT%.map
@if not exist %FONT%.map goto nomap

@if not exist %TEXMF%\pdftex\config\ mkdir %TEXMF%\pdftex\config\
move %FONT%.map %TEXMF%\pdftex\config\

@if not exist %TEXMF%\fonts\tfm\%FONT% mkdir %TEXMF%\fonts\tfm\%FONT%
move %FONT%*.tfm %TEXMF%\fonts\tfm\%FONT%\

@if not exist %TEXMF%\pdftex\enc\%FONT% mkdir %TEXMF%\pdftex\enc\%FONT%
move %FONT%*.enc %TEXMF%\pdftex\enc\%FONT%\

@if not exist %TEXMF%\tex\latex\winfonts mkdir %TEXMF%\tex\latex\winfonts
@set FDFILE=%TEXMF%\tex\latex\winfonts\C70%FONT%.fd
@echo \ProvidesFile{C70%FONT%.fd}[%FONT%] > %FDFILE%
@echo \DeclareFontFamily{C70}{%FONT%}{\hyphenchar \font\m@ne} >> %FDFILE%
@echo \DeclareFontShape{C70}{%FONT%}{m}{n}{^<-^> CJK * %FONT%}{} >> %FDFILE%
@echo \DeclareFontShape{C70}{%FONT%}{bx}{n}{^<-^> CJKb * %FONT%}{\CJKbold}	>> %FDFILE%
@echo \endinput >> %FDFILE%

initexmf --update-fndb

goto succeed

:print_syntax
@echo off
echo The syntax is:
echo. %0 ttffile [texmfdir] [pid] [eid]
echo  . ttffile is the base name of the true type font (without .ttf)
echo  . texmfdir is the latex directory where you want to install the font (localtexmf for instance)
echo  . pid is the platform id of the font (used by ttf2tfm)
echo  . pid is the encoding id of the font (used by ttf2tfm)
echo.
goto end

:filenotfound
@echo off
echo File "%FONTTTF%" not found!
SET v_return=1
goto end

:dirnotfound
@echo off
echo The directory "%TEXMF%" does not exist!
SET v_return=2
goto end

:ttf2tfm_error
@echo off
echo Problem during the execution of ttf2tfm. Try to specify parameter PID and EID.
SET v_return=3
goto end

:nomap
@echo off
echo Problem during the generation of the map file.
SET v_return=4
goto end

:succeed
echo off
echo.
echo Operation succeeded. The truetype font has been installed!
echo You can use in as follow in a tex file:
echo   \usepackage[utf8ttf]{inputenc}
echo   \usepackage{ttfucs}
echo   \DeclareTruetypeFont{%FONT%}{%FONT%}
echo   \begin{document}
echo   \TruetypeFont{%FONT%}
echo   unicode texte
echo   \end{document}
SET v_return=0


:end

endlocal

rem Utilisation sous pdflatex:
rem necessite les fichiers ttfucs.sty et utf8ttf.def

rem \usepackage[utf8ttf]{inputenc}
rem \usepackage{ttfucs}
rem \DeclareTruetypeFont{cyberb}{cyberb}
rem \DeclareTruetypeFont{simkay}{simkay}

rem \begin{document}
rem \TruetypeFont{cyberbit}
rem 中文 大
rem \TruetypeFont{simkai}
rem 日本小
rem \end{document}