del hog_exe.zip 
copy "C:\Program Files\Microsoft\Microsoft Automatic Graph Layout\bin\*.dll" bin\Release\
7za -tzip -r a hog_exe.zip -xr!.svn bin\Release\*.exe README Changelog.txt bin\Release\*.dll bin\Release\Microsoft.MSAGL.* examples\
@if ERRORLEVEL 1 pause