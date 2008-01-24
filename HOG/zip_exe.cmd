del hog_exe.zip 
7z -tzip -r a hog_exe.zip -xr!.svn bin\Release\hog.exe bin\Release\Microsoft.GLEE.* examples\
@if ERRORLEVEL 1 pause