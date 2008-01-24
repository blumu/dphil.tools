omake RELEASE=
@if ERRORLEVEL 1 goto error
call zip_src.cmd
@if ERRORLEVEL 1 goto error
call zip_exe.cmd
@if ERRORLEVEL 1 goto error
call publish
@if ERRORLEVEL 1 goto error

goto success

:error
echo **************** Error during packaging


:success
echo **************** Program built and published.

:exit
endlocal
@if ERRORLEVEL 1 pause
exit /b %ERRORLEVEL%


