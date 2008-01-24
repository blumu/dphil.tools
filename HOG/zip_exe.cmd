del hog_exe.zip 
7z -tzip -r a hog_exe.zip -xr!.svn bin\Release\*.exe bin\Release\*.dll bin\Release\Microsoft.GLEE.* examples\
@if ERRORLEVEL 1 pause