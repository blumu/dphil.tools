pushd HOG
call build release
popd
@if ERRORLEVEL 1 goto exit

pushd HOG
call build mono_release
popd
@if ERRORLEVEL 1 goto exit

call zip_exe.cmd
@if ERRORLEVEL 1 goto exit

call zip_src.cmd
@if ERRORLEVEL 1 goto exit

pscp hog_exe.zip hog_src.zip blum@mercury.comlab.ox.ac.uk:/auto/fs/websrc/oucl/work/william.blum/
@if ERRORLEVEL 1 goto exit

plink blum@mercury.comlab.ox.ac.uk /users/blum/packbin/webpub /auto/fs/websrc/oucl/work/william.blum/hog_exe.zip /auto/fs/websrc/oucl/work/william.blum/hog_src.zip
@if ERRORLEVEL 1 goto exit

:success
echo **************** Program built and published.

:exit
endlocal
@if ERRORLEVEL 1 pause
exit /b %ERRORLEVEL%


