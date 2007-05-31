del hog_src.zip 
7z -tzip -r a hog_src.zip *.fs *.fsi *.ml *.mli *.mll *.mly *.bat Hog\*.cmd *.txt *.rs *.fsharp *.csproj *.cs *.settings *.resx *.sln 
@if ERRORLEVEL 1 pause