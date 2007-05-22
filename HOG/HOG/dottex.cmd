dot -Txdot %1.dot | dot2tex > %1.tex
pdflatex %1.tex