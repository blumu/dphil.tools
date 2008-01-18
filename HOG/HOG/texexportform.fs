(** $Id$
	Description: Exportation to Latex
	Author:		William Blum
**)
open Traversal
open Lnf
open Pstring


  
(** [hors_to_latexcompgraph lnfrules] Create Latex code (using the pstricks package) to draw the computation graph
    of the rules in eta normalform [lnfrules].
    @param lnfrules rewriting rules in eta-long normal form
    @return a string containing the latex code to draw the compuation graph.
**)
let lnfrules_to_latexcompgraph (lnfrules:lnfrule list) =
    
    let ar_lnfrules = Array.of_list lnfrules in
    let nrules = Array.length ar_lnfrules in
    (* return the index in ar_lnfrules from the NT ident *)
    let index_of_nt name = Common.array_find_index (function (n,_) -> name = n)  ar_lnfrules in
    
    (** Determine the NT that appears twice in the computation tree. Only the nodes
        corresponding to those NT will be named in the pstricks code.
        (It is necessary to name them in order to create the links in pstricks.) **)
    let nt_occs = Array.create nrules 0 in 
    
    (** list of all the reachable non-terminals (from the initial non-term S) occurring more than once in the tree. **)
    let rec calc_occ_in_rule i_rule =
        (** count the occurrences of NTs in the right-hand side of the rule **)
        let rec aux (_,app) =
            match app with
                 LnfAppNt(nt, operands) -> 
                   let i_nt = index_of_nt nt in
                   if nt_occs.(i_nt) = 0 then (** the NT has not been visited yet **)
                        calc_occ_in_rule i_nt
                   else (** the NT has already been visited therefore it appears twice in the comptree **)
                        nt_occs.(i_nt) <- nt_occs.(i_nt) + 1;
                   List.iter aux operands;
               
               | LnfAppTm(_, operands) | LnfAppVar(_, operands) -> List.iter aux operands 
               | LnfAppAbs(abs, operands)  -> aux abs; List.iter aux operands 
                
        in nt_occs.(i_rule) <- 1;
           aux (snd (ar_lnfrules.(i_rule)))
    in
      calc_occ_in_rule 0; (** start with the initial non-terminal **)
        
    
    let format_var x = if String.get x 0 = '#' then "\\theta_{"^(String.sub x 1 ((String.length x)-1))^"}" else x in

    let nt_visited = Array.create nrules false in 

    let name_cnt = ref (-1) in
    let newname() = incr name_cnt; "n"^(string_of_int !name_cnt) in
    (* list of arcs to be drawn with pstrick. It is a list of pairs of the form (source,target) *)
    let arcs_list = ref [] in
    
    (* creation of pstricks node for variable and constant *)
    let pst_node_var x = "\\TR{"^(format_var x)^"}"
    and pst_node_const c = "\\TR{\\framebox{"^c^"}}" in
        
    (* [build_subgraph nt rhs] creates the latex code for the subgraph corresponding to the
       lnf term [rhs]. *)
    let rec build_nt_subgraph incomingedge_label i_nt =
        let nt,(abs_part, app_part) = ar_lnfrules.(i_nt) in
        nt_visited.(i_nt) <- true;
        let optlambda = if nt_occs.(i_nt) > 1 then "[name="^(string_of_int i_nt)^"]" else ""
        and edgelabel = if incomingedge_label = "" then "" else "\\ncput*{\\scriptstyle "^incomingedge_label^"}" in
        let prefix = if nt = "" then "" else "["^nt^"]" in
        "\\pstree{\\TR"^optlambda ^"{"^prefix^"\lambda "^(String.concat " " (List.map format_var abs_part))
            ^"}"^edgelabel^"}{"^(build_subgraph_app app_part)^"}"

    and build_subgraph_lmd (abs_part, app_part) =
        match app_part with 
        (* Special case when the applicative part is a ground type non-terminal *)
        | LnfAppNt(nt, []) -> 
            let i_nt = index_of_nt nt in
            build_nt_subgraph "" i_nt
        | _ -> "\\pstree{\\TR{\lambda "^(String.concat " " (List.map format_var abs_part))^"}}{"^(build_subgraph_app app_part)^"}"

    and build_subgraph_app app_part =
        match app_part with
        (* ground type variable or constant *)
          LnfAppVar(x, []) -> pst_node_var x
        
        | LnfAppTm(c, []) -> pst_node_const c
               
        (* An Non-Terminal node is like an abstraction node except that it has a name that can be referenced
           by a node in some othere place in the graph *)
        | LnfAppNt(nt, operands) ->
            let i_nt = index_of_nt nt in
            if nt_visited.(i_nt) then
              begin
                let atnode_name = newname() in
                arcs_list := (atnode_name, (string_of_int i_nt))::!arcs_list;
                "\\pstree{\\TR[name="^atnode_name^"]{@}}{"^(String.concat " " (List.map build_subgraph_lmd operands))^"}"
              end
            else
                "\\pstree{\\TR{@}}{"^(build_nt_subgraph "0" i_nt)^(String.concat " " (List.map build_subgraph_lmd operands))^"}"
        
        | LnfAppAbs(abs,operands) ->
                "\\pstree{\\TR{@}}{"^(build_subgraph_lmd abs)^(String.concat " " (List.map build_subgraph_lmd operands))^"}"
        
        | LnfAppTm(c, operands) -> 
            "\\pstree{"^(pst_node_const c)^"}"^"{"^(String.concat " " (List.map build_subgraph_lmd operands))^"}"
        | LnfAppVar(x, operands) ->
            "\\pstree{"^(pst_node_var x)^"}"^"{"^(String.concat " " (List.map build_subgraph_lmd operands))^"}"
    in
    
    (* process the tree starting at its root *)
    (build_nt_subgraph "" 0)^"\n"
    (* create the latex code to draw the arcs *)
    ^(String.concat "\n" (List.map (function (s,t) -> "\\ncarc{->}{"^s^"}{"^t^"}\\ncput*{\\scriptstyle 0}") !arcs_list))
;;

#light

open System.Windows.Forms

let LoadExportToLatexWindow mdiparent preamble body postamble  =
    let latexform = new System.Windows.Forms.Form()
    let latexoutput = new System.Windows.Forms.RichTextBox()
    latexform.MdiParent <- mdiparent
    latexform.Text <- "Latex output"
    latexform.Size <- new System.Drawing.Size(800, 600);
    latexoutput.Dock <- System.Windows.Forms.DockStyle.Fill
    latexoutput.Multiline <-  true
    latexoutput.ScrollBars <- RichTextBoxScrollBars.Vertical;
    //latexoutput.WordWrap <- false;  
    latexoutput.Text <- preamble^body^postamble;
    latexform.Controls.Add(latexoutput)                                           
    ignore(latexform.Show())
;;



let LoadExportGraphToLatexWindow mdiparent (lnfrules:Lnf.lnfrule list) =
    let latex_preamb = "% Generated automatically by HOG
% -*- TeX -*- -*- Soft -*-
\\documentclass{article}
\\usepackage{pst-tree}

\\begin{document}
\\begin{center}
\\psset{levelsep=5ex,linewidth=0.5pt,nodesep=1pt,arcangle=-20,arrowsize=2pt 1}
\\setlength\fboxsep{2pt}

$\\rput[t](0,0){"

    let latex_post = "}$
\\end{center}
\\end{document}
"
    LoadExportToLatexWindow mdiparent latex_preamb (lnfrules_to_latexcompgraph lnfrules) latex_post


let IntSet = Set.Make((Pervasives.compare : int -> int -> int))

let t = IntSet.empty

let traversal_to_latex travnode_to_latex (trav:Pstring.pstring) = 
    let name_node i = "n"^(string_of_int i) in
    let body = 
    //for i= Array.length trav-1)-i to 0 do
        (fst (Common.array_fold_lefti (fun i (acc,named_nodes_set) (travnode:pstring_occ) ->
                                                                               let src = (Array.length trav-1)-i
                                                                               let latex_label = "{"^(travnode_to_latex travnode)^"}"
                                                                               match travnode.link with
                                                                                      0 when IntSet.mem src named_nodes_set -> ("("^(name_node src)^")"^latex_label^"\ "^acc), named_nodes_set
                                                                                    | 0 -> (latex_label^"\ "^acc), named_nodes_set
                                                                                    | _ -> let dst = src -travnode.link
                                                                                           ("("^(name_node src)^"-"^(name_node dst)^")"^latex_label^"\ "^acc), (IntSet.add dst named_nodes_set)
                                   ) ("",IntSet.empty) (Array.rev trav))
        )
    "\\Pstr[0.7cm]{"^body^"}"
;;

let LoadExportTraversalToLatexWindow mdiparent travocc_to_latex (trav:Pstring.pstring) =
    let latex_preamb = "% Generated automatically by HOG
% -*- TeX -*- -*- Soft -*-
\\documentclass{article}
\\usepackage{pstring}

\\begin{document}

"

    let latex_post = "

\\end{document}
"
    LoadExportToLatexWindow mdiparent latex_preamb ("$"^(traversal_to_latex travocc_to_latex trav)^"$") latex_post
;;