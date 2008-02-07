del hog_src.zip 
7za -tzip -r a -xr!.svn hog_src.zip *.fs *.fsi *.ml *.mli *.mll *.mly *.bat Hog\*.cmd *.txt *.rs *.fsharp *.csproj *.cs *.settings *.resx *.sln *.om *.lmd OMakefile OMakeroot README Changelog.txt GUI\Resources\*
@if ERRORLEVEL 1 pause