call "D:\Microsoft Visual Studio 8\VC\vcvarsall.bat" x86
ocamlc.opt anagrammes.ml -o anagrammes-native.exe
ocamlopt.opt anagrammes.ml -o anagrammes-native.exe
del anagrammes.zip
7z a -tzip anagrammes.zip anagrammes.ml *.exe *.cmd 
pscp anagrammes.zip william@www.famille-blum.org:public_html/software/
pause