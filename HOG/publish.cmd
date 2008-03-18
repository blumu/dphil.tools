pscp hog_exe.zip hog_src.zip blum@mercury.comlab.ox.ac.uk:/auto/fs/website/people/william.blum/
@if ERRORLEVEL 1 goto exit

pscp hog_exe.zip hog_src.zip william@www.famille-blum.org:public_html/research/


:success
echo **************** Program published.

:exit
endlocal
@if ERRORLEVEL 1 pause
exit /b %ERRORLEVEL%


