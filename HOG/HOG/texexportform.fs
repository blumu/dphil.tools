// Form for the exportation to Latex

#light

open System.Windows.Forms

let export_to_latex (lnfrules:Hog.lnfrule list) =
    let latexform = new System.Windows.Forms.Form()
    let latexoutput = new System.Windows.Forms.RichTextBox()
    latexform.Text <- "Latex output"
    latexform.Size <- new System.Drawing.Size(800, 600);
    latexoutput.Dock <- System.Windows.Forms.DockStyle.Fill
    latexoutput.Multiline <-  true
    latexoutput.ScrollBars <- RichTextBoxScrollBars.Vertical;
    //latexoutput.WordWrap <- false;
    let latex_preamb = "% Generated automatically by HOG
% -*- TeX -*- -*- Soft -*-
\\documentclass{article}
\\usepackage{pst-tree}

\\begin{document}
\\begin{center}
\\psset{levelsep=5ex,linewidth=0.5pt,nodesep=1pt,arrows=->,arcangle=-20,arrowsize=2pt 1}
\\setlength\fboxsep{2pt}

$\\rput[t](0,0){"

    let latex_post = "}$
\\end{center}
\\end{document}
"
    latexoutput.Text <- latex_preamb^(Hog.lnfrules_to_latexcompgraph lnfrules)^latex_post;
    latexform.Controls.Add(latexoutput)                                           
    ignore(latexform.Show())
