7z -tzip -r a fcsharp_omake.zip *.om
copy fcsharp_omake.zip c:\users\william\documents\website\research
pscp fcsharp_omake.zip william@www.famille-blum.org:public_html/research/
@if ERRORLEVEL 1 goto exit

:success
echo **************** fcsharp_omake has been published successfuly.

:exit
endlocal
@if ERRORLEVEL 1 pause
exit /b %ERRORLEVEL%


