pushd utf_example
latex latex_utf8.tex
pdflatex pdflatex_utf8.tex
popd
del ttffortex.zip 
7za -tzip a ttffortex.zip ttffortex.cmd utf_example\utf8ttf.def utf_example\ttfucs.sty utf_example\latex_utf8.tex utf_example\pdflatex_utf8.tex utf_example\pdflatex_utf8.pdf utf_example\latex_utf8.dvi
@IF ERRORLEVEL 1 pause