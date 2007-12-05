del hog_exe.zip 
7z -tzip -r a hog_exe.zip bin\Release\hog.exe bin\ReleaseMono\hog.exe examples\*.rs examples\*.lmd
@if ERRORLEVEL 1 pause