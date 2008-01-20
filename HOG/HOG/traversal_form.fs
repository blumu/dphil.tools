(** $Id: $
	Description: Traversals window
	Author:		William Blum
**)

#light
open Common
open System.Drawing
open System.Windows.Forms
open Traversal
open Lnf
open Compgraph
open GUI
open Pstring


(** Map a player to a node shape **)
let player_to_shape = function Proponent -> ShapeRectangle
                               | Opponent -> ShapeOval
(** Map a player to a node color **)
let player_to_color = function Proponent -> Color.Coral
                               | Opponent -> Color.CornflowerBlue
(***** Pstring functions *****)

(** Convert a colors of type System.Drawing.Color to a color of type Microsoft.Glee.Drawing.Color **)
let sysdrawingcolor_2_gleecolor (color:System.Drawing.Color) :Microsoft.Glee.Drawing.Color = 
    Microsoft.Glee.Drawing.Color(color.R,color.G,color.B)

(** Convert a shape of type Pstring.Shapes to a shape of type Microsoft.Glee.Drawing.Shape **)
let pstringshape_2_gleeshape =
    function  Pstring.Shapes.ShapeRectangle -> Microsoft.Glee.Drawing.Shape.Box
             | Pstring.Shapes.ShapeOval -> Microsoft.Glee.Drawing.Shape.Circle

(** Create a pstring occurrence from a gennode **)
let occ_from_gennode (compgraph:computation_graph) gennode lnk =
    let player = compgraph.gennode_player gennode    
    let lab =
        match gennode with
          Custom -> "?"
        | ValueLeaf(nodeindex,v) -> (string_of_int nodeindex)^"_{"^(compgraph.node_label_with_idsuffix nodeindex)^"}"
        | InternalNode(nodeindex) -> compgraph.node_label_with_idsuffix nodeindex
    { label= lab;
      tag = box gennode; // Use the tag field to store the generalized graph node
      link = lnk;
      shape=player_to_shape player;
      color=player_to_color player
    } 
    (*ctrl.add_node (new pstring_occ(box gennode, // Use the tag field to store the generalized graph node,
                                        (compgraph.node_label_with_idsuffix gr_nodeindex),
                                        0,
                                        player_to_shape player,
                                        player_to_color player))*)
                                        

(** Return the generalized graph node of a given occurrence in a Pstringcontrol.pstring sequence **)
let pstr_occ_getnode (nd:pstring_occ) =
    if nd.tag = null then
        failwith "This is not a valid justified sequence. Some node-occurrence does not belong to the computation graph/tree."
    else 
        (unbox nd.tag:gen_node)

//// Sequence transformations

(**** Sequence transformations specialized for sequences of type Pstringcontrol.pstring  ****)
     
(** P-View **) 
let pstrseq_pview gr seq = seq_Xview gr
                                     Proponent 
                                     pstr_occ_getnode        // getnode function
                                     pstr_occ_getlink        // getlink function
                                     pstr_occ_updatelink     // update link function
                                     seq
                                     ((Array.length seq)-1) // compute the P-view at the last occurrence
                                    

(** O-View **) 
let pstrseq_oview gr seq = seq_Xview gr
                                     Opponent
                                     pstr_occ_getnode        // getnode function
                                     pstr_occ_getlink        // getlink function
                                     pstr_occ_updatelink     // update link function
                                     seq
                                     ((Array.length seq)-1)  // compute the O-view at the last occurrence

(** Subterm projection with respect to a reference root node **) 
let pstrseq_subtermproj gr = subtermproj gr
                                         pstr_occ_getnode
                                         pstr_occ_getlink
                                         pstr_occ_updatelink

(** Hereditary projection **) 
let pstrseq_herproj = heredproj pstr_occ_getlink
                                pstr_occ_updatelink
                                
(** Prefixing **)
let pstrseq_prefix seq at = Array.sub seq 0 (at+1)


(** Traversal star **)
let pstrseq_star gr = star gr
                           pstr_occ_getnode
                           pstr_occ_getlink
                           pstr_occ_updatelink


(** Traversal extension **)
let pstrseq_ext gr = extension gr
                               pstr_occ_getnode
                               pstr_occ_getlink
                               pstr_occ_updatelink
                               Pstring.create_dummy_occ



/////////////////////// Traversal control

(** The type of a traversal sequence **)
type TraversalControl = 
  class
    inherit Pstring.PstringControl as base

    val mutable compgraph : computation_graph;
    
    // Constructor
    new (ncompgr,pstr:pstring) as this = {inherit PstringControl(pstr);
                                            compgraph = create_empty_graph();} then this.compgraph <- ncompgr
    
    // Add an occurrence of a gennode to the end of the traversal
    member private x.add_gennode gennode link =
        x.add_node (occ_from_gennode x.compgraph gennode link)
    
    // Play the next move for P according to the strategy given by the term
    member x.play_for_p() =
        let l = Array.length x.Sequence
        if l = 0 then
            failwith "It is your turn to play (for the Opponent)!"
            
        let last = x.Sequence.(l-1)
        let gennode = pstr_occ_getnode last
        // What is the type of the last occurrence?
        match gennode with
        
              Custom -> failwith "Bad traversal! There is an occurence of a node that does not belong to the computation graph!"
            
            // it is an intenal-node
            | InternalNode(i) -> match x.compgraph.nodes.(i) with
                                    NCntApp | NCntVar(_) | NCntTm(_) 
                                        -> failwith "It is your turn. You are playing the Opponent!"
                                  // it is a lambda node: Play using the rule (Lmd)
                                  | NCntAbs(_,_) ->  // find the child node of the lambda node
                                                     let firstchild = List.hd x.compgraph.edges.(i)                                                   
                                                     x.add_gennode (InternalNode(firstchild)) 0

                                  
            // it is a value leaf
            | ValueLeaf(i,v) -> // play using the copycat rule (Value)
                                ()
  end



                                
(** [compgraph_to_graphview node_2_color node_2_shape gr] creates the GLEE graph view of a computation graph.
    @param node_2_color a mapping from nodes to color
    @param node_2_shape a mapping from nodes to shapes
    @param gr the computation graph    
    @return the GLEE graph object representing the computation graph
**)
let compgraph_to_graphview node_2_color node_2_shape (gr:computation_graph) =
    (* create a graph object *)
    let gleegraph = new Microsoft.Glee.Drawing.Graph("graph") in
    
    for k = 0 to (Array.length gr.nodes)-1 do
        let nodeid = string_of_int k in
        let node = gleegraph.AddNode nodeid in
        node.Attr.Id <- string_of_int k
        node.Attr.Fillcolor <- sysdrawingcolor_2_gleecolor (node_2_color gr.nodes.(k))
        node.Attr.Shape <- pstringshape_2_gleeshape (node_2_shape gr.nodes.(k))
        node.Attr.Label <- gr.node_label_with_idsuffix k
        match gr.nodes.(k) with 
          | NCntTm(tm) -> node.Attr.LabelMargin <- 10;
          | NCntApp 
          | NCntVar(_)
          | NCntAbs(_,_) -> ()
    done;

    let addtargets source targets =
        let source_id = string_of_int source in
        let aux i target = 
            let target_id = string_of_int target in
            let edge = gleegraph.AddEdge(source_id,target_id) in
            (match gr.nodes.(source) with
                NCntApp -> edge.EdgeAttr.Label <- string_of_int i;
                               (* Highlight the first edge if it is an @-node (going to the operator) *)
                               if i = 0 then edge.EdgeAttr.Color <- Microsoft.Glee.Drawing.Color.Green;
               | NCntAbs(_)  -> ();
               | _ -> edge.EdgeAttr.Label <- string_of_int (i+1);
            )

        in 
        List.iteri aux targets
    in
    Array.iteri addtargets gr.edges;
    gleegraph
;;


(** Loads a window showing a computation graph and permitting the user to export it to latex. **)
let ShowCompGraphWindow mdiparent filename compgraph lnfrules =
    // create a form
    let form = new System.Windows.Forms.Form()
    let viewer = new Microsoft.Glee.GraphViewerGdi.GViewer()
    let panel1 = new System.Windows.Forms.Panel();
    let buttonLatex = new System.Windows.Forms.Button()

    form.SuspendLayout(); 
    form.MdiParent <- mdiparent;
    form.Text <- "Computation graph of "^filename;
    form.Size <- Size(700,800);
    buttonLatex.Location <- new System.Drawing.Point(1, 1)
    buttonLatex.Name <- "button1"
    buttonLatex.Size <- new System.Drawing.Size(267, 23)
    buttonLatex.TabIndex <- 2
    buttonLatex.Text <- "Export to Latex"
    buttonLatex.UseVisualStyleBackColor <- true
    buttonLatex.Click.Add(fun e -> Texexportform.LoadExportGraphToLatexWindow mdiparent lnfrules )


    // create a viewer object
    panel1.SuspendLayout();
    panel1.Anchor <- Enum.combine [ System.Windows.Forms.AnchorStyles.Top ; 
                                    System.Windows.Forms.AnchorStyles.Bottom ; 
                                    System.Windows.Forms.AnchorStyles.Left ; 
                                    System.Windows.Forms.AnchorStyles.Right ];
    panel1.Controls.Add(viewer);
    panel1.Location <- new System.Drawing.Point(1, 29);
    panel1.Margin <- new System.Windows.Forms.Padding(2, 2, 2, 2);
    panel1.Name <- "panel1";
    panel1.Size <- new System.Drawing.Size(972, 505);
    panel1.TabIndex <- 4;

    // bind the graph to the viewer
    let nd_2_col = function  NCntApp -> System.Drawing.Color.White
                            | NCntTm(tm) -> System.Drawing.Color.Salmon;
                            | NCntVar(x) -> System.Drawing.Color.Green;
                            | NCntAbs("",vars) -> System.Drawing.Color.White
                            | NCntAbs(nt,vars) -> System.Drawing.Color.Yellow;


    let nd_2_shape = function NCntVar(_) | NCntAbs(_,_) | NCntApp -> Pstring.Shapes.ShapeOval
                                | NCntTm(_) -> Pstring.Shapes.ShapeRectangle
          
    viewer.Graph <- compgraph_to_graphview nd_2_col nd_2_shape compgraph;
    viewer.AsyncLayout <- false;
    viewer.AutoScroll <- true;
    viewer.BackwardEnabled <- false;
    viewer.Dock <- System.Windows.Forms.DockStyle.Fill;
    viewer.ForwardEnabled <- false;
    viewer.Location <- new System.Drawing.Point(0, 0);
    viewer.MouseHitDistance <- 0.05;
    viewer.Name <- "gViewer";
    viewer.NavigationVisible <- true;
    viewer.PanButtonPressed <- false;
    viewer.SaveButtonVisible <- true;
    viewer.Size <- new System.Drawing.Size(674, 505);
    viewer.TabIndex <- 3;
    viewer.ZoomF <- 1.0;
    viewer.ZoomFraction <- 0.5;
    viewer.ZoomWindowThreshold <- 0.05;

    //associate the viewer with the form
    form.ClientSize <- new System.Drawing.Size(970, 532);

    form.Controls.Add(buttonLatex);
    form.Controls.Add(panel1);            
    panel1.ResumeLayout(false);
    form.ResumeLayout(false);            

    //show the form
    ignore(form.Show())
;;



(**** Traversal calculator window ***)

(** Loads the traversal calculator window for a given computation graph. **)
let ShowTraversalCalculatorWindow mdiparent graphsource_filename (compgraph:computation_graph) lnfrules initialize_ws =
    let form_trav = new Traversal.Traversal()
    form_trav.MdiParent <- mdiparent;
    form_trav.WindowState<-FormWindowState.Maximized;    
    
    // this holds the Pstring control that is currently selected.
    let selection : Pstring.PstringControl option ref = ref None
    
    // execute a function on the current selection if there is one
    let do_on_selection f = match !selection with 
                                None -> ()
                              | Some(ctrl) -> f ctrl

    // execute a function on the current selection if it is a traversal
    let do_on_traversal_selection f = match !selection with 
                                        Some(ctrl) when (ctrl:?TraversalControl) -> let trav = (ctrl:?>TraversalControl)
                                                                                    f trav
                                        | _ -> ()
    
    // unselect the currently selected pstring control
    let unselect_pstrcontrol() =
        do_on_selection
          (fun ctrl ->
            form_trav.btDelete.Enabled <- false
            form_trav.btDuplicate.Enabled <- false
            form_trav.btOview.Enabled <- false
            form_trav.btPview.Enabled <- false
            form_trav.btBackspace.Enabled <- false
            form_trav.btEditLabel.Enabled <- false
            form_trav.btExportTrav.Enabled <- false
            form_trav.btHerProj.Enabled <- false
            form_trav.btAdd.Enabled <- false
            form_trav.btSubtermProj.Enabled <- false
            form_trav.btPrefix.Enabled <- false
            form_trav.btStar.Enabled <- false
            form_trav.btExt.Enabled <- false
            ctrl.Deselection();
            selection := None)

    // select a pstring control
    let select_pstrcontrol (ctrl:Pstring.PstringControl) =
        let btrav = ctrl:?TraversalControl in
        form_trav.grpNode.Enabled <- not btrav
        form_trav.grpTrav.Enabled <- btrav
    
        form_trav.btDelete.Enabled <- true
        form_trav.btDuplicate.Enabled <- true
        form_trav.btOview.Enabled <- true
        form_trav.btPview.Enabled <- true
        form_trav.btBackspace.Enabled <- true
        form_trav.btEditLabel.Enabled <- true
        form_trav.btExportTrav.Enabled <- true
        form_trav.btHerProj.Enabled <- true
        form_trav.btAdd.Enabled <- true
        form_trav.btSubtermProj.Enabled <- true
        form_trav.btPrefix.Enabled <- true
        form_trav.btStar.Enabled <- true
        form_trav.btExt.Enabled <- true
        ctrl.Selection();
        selection := Some(ctrl)
        ctrl.Select();

    // change the current selection
    let change_selection_pstrcontrol (ctrl:Pstring.PstringControl) =
        do_on_selection (fun cursel -> cursel.Deselection() )
        select_pstrcontrol ctrl

    // Give the focus back to the currently selected line
    let refocus_pstrcontrol() =
        do_on_selection (fun cursel -> cursel.Select() )
    
    // Add a pstring control to the list
    let AddPstringCtrl (new_pstr:PstringControl) =
        new_pstr.AutoSize <- true
        new_pstr.TabStop <- false
        new_pstr.BackColor <- form_trav.seqflowPanel.BackColor
        // add an event handler to the a given pstring control in order to detect selection
        // of the control by the user
        new_pstr.MouseDown.Add(fun _ -> change_selection_pstrcontrol new_pstr );
        new_pstr.KeyDown.Add( fun e -> match e.KeyCode with 
                                              Keys.Up -> let i = match !selection with 
                                                                          None -> -1
                                                                        | Some(ctrl) -> form_trav.seqflowPanel.Controls.GetChildIndex(ctrl)
                                                         if i-1 >=0 then
                                                             change_selection_pstrcontrol (form_trav.seqflowPanel.Controls.Item(i-1):?> Pstring.PstringControl)
                                            | Keys.Down -> let i = match !selection with 
                                                                          None -> -1
                                                                        | Some(ctrl) -> form_trav.seqflowPanel.Controls.GetChildIndex(ctrl)
                                                           if i+1 < form_trav.seqflowPanel.Controls.Count then
                                                             change_selection_pstrcontrol (form_trav.seqflowPanel.Controls.Item(i+1):?> Pstring.PstringControl)
                                            | _ -> ()
                               ); 
        form_trav.seqflowPanel.Controls.Add new_pstr
        new_pstr

    // Create a new pstring control and add it to the list
    let createAndAddPstringCtrl seq =
        AddPstringCtrl (new Pstring.PstringControl(seq))
    
    // add a handler for click on the nodes of the sequence
    //(!first).nodeClick.Add(fun _ -> 
    //        ignore( MessageBox.Show("salut","test", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)))

    // execute the worksheet initialization function
    initialize_ws createAndAddPstringCtrl
    
    // select the last line
    let p = form_trav.seqflowPanel.Controls.Count
    if p > 0 then
        select_pstrcontrol (form_trav.seqflowPanel.Controls.Item(p-1):?>PstringControl)
      
    
    // The user has clicked in the void.
    form_trav.seqflowPanel.MouseDown.Add(fun _ -> // if the control (or one of the controls it contains) already has the focus
                                                  if form_trav.seqflowPanel.ContainsFocus then
                                                     unselect_pstrcontrol() // then unselect the currently selected line
                                                  else
                                                     // otherwise gives the focus back to the currently selected line
                                                     refocus_pstrcontrol()
                                              )
    
    form_trav.seqflowPanel.Enter.Add( fun _ -> do_on_selection (fun cursel -> cursel.Invalidate() )
                                    )
    form_trav.seqflowPanel.Leave.Add( fun _ -> do_on_selection (fun cursel -> cursel.Invalidate() )
                                    )
        
    form_trav.btPview.Click.Add(fun _ -> 
                    do_on_selection (fun cursel -> let new_pstr = createAndAddPstringCtrl (pstrseq_pview compgraph cursel.Sequence)
                                                   change_selection_pstrcontrol new_pstr)
                );
                
    form_trav.btOview.Click.Add(fun _ -> 
                    do_on_selection (fun cursel -> let new_pstr = createAndAddPstringCtrl (pstrseq_oview compgraph cursel.Sequence)
                                                   change_selection_pstrcontrol new_pstr)
                );
                
    form_trav.btHerProj.Click.Add(fun _ -> 
                        do_on_selection (fun cursel -> let new_pstr = createAndAddPstringCtrl (pstrseq_herproj cursel.Sequence cursel.SelectedNodeIndex)
                                                       change_selection_pstrcontrol new_pstr  )
                );
                                
    form_trav.btSubtermProj.Click.Add(fun _ -> 
                    match !selection with 
                        Some(cursel) when cursel.SelectedNodeIndex >= 0 ->
                            let new_pstr = createAndAddPstringCtrl (pstrseq_subtermproj compgraph cursel.Sequence cursel.SelectedNodeIndex)
                            change_selection_pstrcontrol new_pstr
                      | _ -> ()
                );
                    
    form_trav.btDuplicate.Click.Add(fun _ -> 
                    do_on_selection (fun cursel ->
                         let new_pstr = if (cursel:?TraversalControl) then
                                            new TraversalControl(compgraph,cursel.Sequence):>Pstring.PstringControl
                                        else
                                            new Pstring.PstringControl(cursel.Sequence)
                         change_selection_pstrcontrol (AddPstringCtrl new_pstr) )
                );
                
    form_trav.btPrefix.Click.Add(fun _ -> do_on_selection (fun cursel -> let new_pstr = createAndAddPstringCtrl (pstrseq_prefix cursel.Sequence cursel.SelectedNodeIndex)
                                                                         change_selection_pstrcontrol new_pstr  )
                                        );                
                                        
    form_trav.btExt.Click.Add(fun _ ->  do_on_selection (fun cursel -> let new_pstr = createAndAddPstringCtrl (pstrseq_ext compgraph cursel.Sequence cursel.SelectedNodeIndex)
                                                                       change_selection_pstrcontrol new_pstr)
                                        );
                                             
    form_trav.btStar.Click.Add(fun _ ->  do_on_selection (fun cursel -> let new_pstr = createAndAddPstringCtrl (pstrseq_star compgraph cursel.Sequence cursel.SelectedNodeIndex)
                                                                        change_selection_pstrcontrol new_pstr)
                                        );
                                        
    form_trav.btDelete.Click.Add( fun _ -> do_on_selection
                                             (fun cursel -> 
                                                let i = form_trav.seqflowPanel.Controls.GetChildIndex(cursel)
                                                // last line removed?
                                                if i = form_trav.seqflowPanel.Controls.Count-1 then
                                                  if i > 0 then
                                                    // select the previous line if there is one
                                                    change_selection_pstrcontrol (form_trav.seqflowPanel.Controls.Item(i-1):?>Pstring.PstringControl)
                                                  else
                                                    unselect_pstrcontrol() // removing the only remaining line: just unselect it
                                                else // it's not the last line that is removed
                                                    // select the next line
                                                    change_selection_pstrcontrol (form_trav.seqflowPanel.Controls.Item(i+1):?>Pstring.PstringControl)
                                                
                                                form_trav.seqflowPanel.Controls.Remove(cursel)
                                              )
                                       );

    form_trav.btNew.Click.Add(fun _ -> change_selection_pstrcontrol (createAndAddPstringCtrl [||])
                );
                
    form_trav.btBackspace.Click.Add(fun _ -> do_on_selection
                                                (fun cursel -> cursel.remove_last_node()))

    form_trav.btAdd.Click.Add(fun _ -> do_on_selection
                                           (fun cursel -> cursel.add_node (create_blank_occ())))

    form_trav.btEditLabel.Click.Add(fun _ -> do_on_selection
                                                (fun cursel -> cursel.EditLabel()))
                                              

    let filter = "Traversal worksheet *.xml|*.xml|All files *.*|*.*" in
    form_trav.btSave.Click.Add(fun _ -> // savefile dialog box
                                        let d = new SaveFileDialog() in 
                                        d.Filter <- filter;
                                        d.FilterIndex <- 1;
                                        if d.ShowDialog() = DialogResult.OK then
                                            Worksheetfile.save_worksheet d.FileName graphsource_filename lnfrules form_trav.seqflowPanel)

    form_trav.btImport.Click.Add(fun _ -> // openfile dialog box
                                          let d = new OpenFileDialog() in 
                                          d.Filter <- filter;
                                          d.FilterIndex <- 1;
                                          d.Title <- "Import a worksheet..."
                                          if d.ShowDialog() = DialogResult.OK then
                                            (Worksheetfile.import_worksheet d.FileName lnfrules createAndAddPstringCtrl)
                                )
    // Traversal game buttons
    form_trav.btNewGame.Click.Add(fun _ -> change_selection_pstrcontrol (AddPstringCtrl ((new TraversalControl(compgraph,[||])):>PstringControl))
                                );

    form_trav.btPlay.Click.Add(fun _ -> do_on_traversal_selection
                                         (fun travsel -> travsel.play_for_p()
                                         )
                                );

    // bind the graph to the viewer
    let nd_2_col nd = player_to_color (graphnode_player nd)
    let nd_2_shape nd = player_to_shape (graphnode_player nd)
    form_trav.gViewer.Graph <- compgraph_to_graphview nd_2_col nd_2_shape compgraph;
    form_trav.gViewer.AsyncLayout <- false;
    form_trav.gViewer.AutoScroll <- true;
    form_trav.gViewer.BackwardEnabled <- true;
    form_trav.gViewer.Dock <- System.Windows.Forms.DockStyle.Fill;
    form_trav.gViewer.ForwardEnabled <- true;
    form_trav.gViewer.Location <- new System.Drawing.Point(0, 0);
    form_trav.gViewer.MouseHitDistance <- 0.05;
    form_trav.gViewer.Name <- "gViewer";
    form_trav.gViewer.NavigationVisible <- true;
    form_trav.gViewer.PanButtonPressed <- false;
    form_trav.gViewer.SaveButtonVisible <- true;
    form_trav.gViewer.Size <- new System.Drawing.Size(674, 505);
    form_trav.gViewer.TabIndex <- 3;
    form_trav.gViewer.ZoomF <- 1.0;
    form_trav.gViewer.ZoomFraction <- 0.5;
    form_trav.gViewer.ZoomWindowThreshold <- 0.10;

    let (gleegraph_last_hovered_node: Microsoft.Glee.Drawing.Node option ref) = ref None
    form_trav.gViewer.SelectionChanged.Add(fun _ -> if form_trav.gViewer.SelectedObject = null then
                                                      gleegraph_last_hovered_node := None
                                                    else if (form_trav.gViewer.SelectedObject :? Microsoft.Glee.Drawing.Node) then
                                                      gleegraph_last_hovered_node := Some(form_trav.gViewer.SelectedObject :?> Microsoft.Glee.Drawing.Node)
                                                    else
                                                      gleegraph_last_hovered_node := None
                                          );
    
    form_trav.gViewer.MouseDown.Add(fun e -> 
            match !selection, !gleegraph_last_hovered_node  with 
                Some(ctrl), Some(nd) ->
                    begin
                        let gr_nodeindex = int_of_string nd.Attr.Id
                        
                        // add an internal node to the traversal
                        if e.Button = MouseButtons.Left then
                            ctrl.add_node (occ_from_gennode compgraph (InternalNode(gr_nodeindex)) 0 )
                        // add a value-leaf node to the traversal
                        else
                            ctrl.add_node (occ_from_gennode compgraph (ValueLeaf(gr_nodeindex,1)) 0 )

                        refocus_pstrcontrol()
                    end
              | _ -> ()
        );
    
    // convert an node-occurrence to latex code
    let pstrocc_to_latex (pstrnode:pstring_occ) =
        if pstrnode.tag = null then
          pstrnode.label
        else
          compgraph.gennode_to_latex ((unbox pstrnode.tag) : gen_node)
    
    form_trav.btExportGraph.Click.Add(fun _ -> Texexportform.LoadExportGraphToLatexWindow mdiparent lnfrules)
    form_trav.btExportTrav.Click.Add(fun _ -> do_on_selection (fun cursel -> Texexportform.LoadExportTraversalToLatexWindow mdiparent pstrocc_to_latex cursel.Sequence) ) 
    
    form_trav.btExportWS.Click.Add(fun _ -> let p = form_trav.seqflowPanel.Controls.Count
                                            let exp = ref ""
                                            for i = 0 to p-1 do
                                                exp := !exp^eol^("\\item $"^(Texexportform.traversal_to_latex pstrocc_to_latex (form_trav.seqflowPanel.Controls.Item(i):?>PstringControl).Sequence)^"$")
                                            done;
                                            let latex_preamb = "% Generated automatically by HOG
% -*- TeX -*- -*- Soft -*-
\\documentclass{article}
\\usepackage{pstring}

\\begin{document}
\\begin{itemize}
"
                                            and latex_post = "
\\end{itemize}
\\end{document}
"
                                            Texexportform.LoadExportToLatexWindow mdiparent latex_preamb !exp latex_post)

    ignore(form_trav.Show()) 
;;

