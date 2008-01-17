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
open GUI
open Pstring

(** Convert a colors of type System.Drawing.Color to a color of type Microsoft.Glee.Drawing.Color **)
let sysdrawingcolor_2_gleecolor (color:System.Drawing.Color) :Microsoft.Glee.Drawing.Color = 
    Microsoft.Glee.Drawing.Color(color.R,color.G,color.B)

(** Convert a shape of type Pstring.Shapes to a shape of type Microsoft.Glee.Drawing.Shape **)
let pstringshape_2_gleeshape =
    function  Pstring.Shapes.ShapeRectangle -> Microsoft.Glee.Drawing.Shape.Box
             | Pstring.Shapes.ShapeOval -> Microsoft.Glee.Drawing.Shape.Circle


                                
(** Create the graph view of the computation graph
    @param node_2_color a mapping from nodes to color
    @param node_2_shape a mapping from nodes to shapes
    @param graph the computation graph    
    @return the GLEE graph object representing the computation graph
**)
let compgraph_to_graphview node_2_color node_2_shape (nodes_content:cg_nodes,edges:cg_edges) =
    (* create a graph object *)
    let gleegraph = new Microsoft.Glee.Drawing.Graph("graph") in
    
    for k = 0 to (Array.length nodes_content)-1 do
        let nodeid = string_of_int k in
        let node = gleegraph.AddNode nodeid in
        node.Attr.Id <- string_of_int k
        node.Attr.Fillcolor <- sysdrawingcolor_2_gleecolor (node_2_color nodes_content.(k))
        node.Attr.Shape <- pstringshape_2_gleeshape (node_2_shape nodes_content.(k))
        node.Attr.Label <- graph_node_label_with_idsuffix nodes_content k
        match nodes_content.(k) with 
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
            (match nodes_content.(source) with
                NCntApp -> edge.EdgeAttr.Label <- string_of_int i;
                               (* Highlight the first edge if it is an @-node (going to the operator) *)
                               if i = 0 then edge.EdgeAttr.Color <- Microsoft.Glee.Drawing.Color.Green;
               | NCntAbs(_)  -> ();
               | _ -> edge.EdgeAttr.Label <- string_of_int (i+1);
            )

        in 
        List.iteri aux targets
    in
    Array.iteri addtargets edges;
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


/////////////////////// Sequence transformations

// get_gennode function for sequences of type Pstringcontrol.pstring_node
let pstrseq_gennode_get nd = if nd.tag = null then
                                failwith "This is not a valid traversal: there is a node in the sequence that does not belong to the computation graph/tree."
                             else 
                                (unbox nd.tag:gen_node)

// getlink function for sequences of type Pstringcontrol.pstring_node
let pstrseq_gennode_getlink nd = nd.link

// updatelink function for sequences of type Pstringcontrol.pstring_node
let pstrseq_gennode_updatelink nd newlink =
     {tag=nd.tag;color=nd.color;shape=nd.shape;label=nd.label;link=newlink}
     
// create a dummy gennode (which does not correspond to any node of the computation tree)
let pstrseq_gennode_createdummy newlabel newlink = 
    {tag=null;color=Color.Beige;shape=ShapeRectangle;label=newlabel;link=newlink}
    
     
(** P-View, specialized for sequences of type Pstringcontrol.pstring_node **) 
let pstrseq_pview gr_nodes seq = seq_Xview gr_nodes
                                   Proponent 
                                   pstrseq_gennode_get        // get_gennode function                                   
                                   pstrseq_gennode_getlink    // get_link function
                                   pstrseq_gennode_updatelink // update link function                                   
                                   seq
                                   ((Array.length seq)-1)
                                    

(** O-View, specialized for sequences of type Pstringcontrol.pstring_node **) 
let pstrseq_oview gr_nodes seq = seq_Xview gr_nodes
                                   Opponent
                                   pstrseq_gennode_get        // get_gennode function                                   
                                   pstrseq_gennode_getlink    // get_link function
                                   pstrseq_gennode_updatelink // update link function                                   
                                   seq
                                   ((Array.length seq)-1)

(** Subterm projection with respect to a reference root node.
    Specialized for sequences of type Pstringcontrol.pstring_node **) 
let pstrseq_subtermproj gr_nodes = subtermproj gr_nodes
                                               pstrseq_gennode_get
                                               pstrseq_gennode_getlink
                                               pstrseq_gennode_updatelink

(** Hereditary projection,
    specialized for sequences of type Pstringcontrol.pstring_node **) 
let pstrseq_herproj = heredproj pstrseq_gennode_getlink
                                pstrseq_gennode_updatelink
                                
(** Prefixing **)
let pstrseq_prefix seq at = Array.sub seq 0 (at+1)


(** Traversal star **)
let pstrseq_star gr_nodes = star gr_nodes
                                 pstrseq_gennode_get
                                 pstrseq_gennode_getlink
                                 pstrseq_gennode_updatelink


(** Traversal extension **)
let pstrseq_ext gr_nodes = extension gr_nodes
                                     pstrseq_gennode_get
                                     pstrseq_gennode_getlink
                                     pstrseq_gennode_updatelink
                                     pstrseq_gennode_createdummy


(** Map a player to a node shape **)
let player_to_shape = function Proponent -> ShapeRectangle
                               | Opponent -> ShapeOval
(** Map a player to a node color **)
let player_to_color = function Proponent -> Color.Coral
                               | Opponent -> Color.CornflowerBlue

(** Loads the traversal calculator window for a given computation graph. **)
let ShowTraversalCalculatorWindow mdiparent filename ((gr_nodes,gr_edges) as compgraph) lnfrules =
    let form_trav = new Traversal.Traversal()

    form_trav.MdiParent <- mdiparent;
    
    // this holds the Pstring control that is currently selected.
    let selection : Pstring.PstringControl option ref = ref None
    
    // unselect the currently selected pstring control
    let unselect_pstrcontrol() =
        match !selection with
          None -> ()
        | Some(ctrl) -> 
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
            ctrl.Deselection();
            selection := None

    // select a pstring control
    let select_pstrcontrol (ctrl:Pstring.PstringControl) =
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
        ctrl.Selection();
        selection := Some(ctrl)
        ctrl.Select();

    // change the current selection
    let change_selection_pstrcontrol (ctrl:Pstring.PstringControl) =
        match !selection with None -> () | Some(cursel) -> cursel.Deselection()
        select_pstrcontrol ctrl

    // Give the focus back to the currently selected line
    let refocus_pstrcontrol() =
        match !selection with
            None -> ()
            | Some(cursel) -> cursel.Select();
    

    // Create a new pstring control and add it to the list
    let createAndAddPstringCtrl seq =
        let new_pstr = ref (new Pstring.PstringControl(seq))
        (!new_pstr).AutoSize <- true
        (!new_pstr).TabStop <- false
        (!new_pstr).BackColor <- form_trav.seqflowPanel.BackColor
        // add an event handler to the a given pstring control in order to detect selection
        // of the control by the user
        (!new_pstr).MouseDown.Add(fun _ -> change_selection_pstrcontrol !new_pstr );
        (!new_pstr).KeyDown.Add( fun e -> match e.KeyCode with 
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
        form_trav.seqflowPanel.Controls.Add !new_pstr
        !new_pstr
    
    // create a default pstring control
    select_pstrcontrol (createAndAddPstringCtrl [||])
    //(!first).nodeClick.Add(fun _ -> 
    //        ignore( MessageBox.Show("salut","test", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)))
    
    // The user has clicked in the void.
    form_trav.seqflowPanel.MouseDown.Add(fun _ -> // if the control (or one of the controls it contains) already has the focus
                                                  if form_trav.seqflowPanel.ContainsFocus then
                                                     unselect_pstrcontrol() // then unselect the currently selected line
                                                  else
                                                     // otherwise gives the focus back to the currently selected line
                                                     refocus_pstrcontrol()
                                              )
    
    form_trav.seqflowPanel.Enter.Add( fun _ -> match !selection with
                                                  None -> ()
                                                | Some(cursel) -> cursel.Invalidate();
                                    )
    form_trav.seqflowPanel.Leave.Add( fun _ -> match !selection with
                                                  None -> ()
                                                | Some(cursel) -> cursel.Invalidate();
                                    )
        
    form_trav.btPview.Click.Add(fun _ -> 
                    match !selection with 
                        None -> ()
                      | Some(ctrl) -> let new_pstr = createAndAddPstringCtrl (pstrseq_pview gr_nodes ctrl.Sequence)
                                      change_selection_pstrcontrol new_pstr
                );
                
    form_trav.btOview.Click.Add(fun _ -> 
                    match !selection with 
                        None -> ()
                      | Some(ctrl) -> let new_pstr = createAndAddPstringCtrl (pstrseq_oview gr_nodes ctrl.Sequence)
                                      change_selection_pstrcontrol new_pstr                        
                );
                
    form_trav.btHerProj.Click.Add(fun _ -> 
                    match !selection with 
                        None -> ()
                      | Some(ctrl) -> let new_pstr = createAndAddPstringCtrl (pstrseq_herproj ctrl.Sequence ctrl.SelectedNodeIndex)
                                      change_selection_pstrcontrol new_pstr                        
                );
                                
    form_trav.btSubtermProj.Click.Add(fun _ -> 
                    match !selection with 
                        Some(ctrl) when ctrl.SelectedNodeIndex >= 0 ->
                            let new_pstr = createAndAddPstringCtrl (pstrseq_subtermproj gr_nodes ctrl.Sequence ctrl.SelectedNodeIndex)
                            change_selection_pstrcontrol new_pstr
                      | _ -> ()
                );
                    
    form_trav.btDuplicate.Click.Add(fun _ -> 
                    match !selection with 
                        None -> ()
                      | Some(ctrl) -> let new_pstr = createAndAddPstringCtrl ctrl.Sequence
                                      change_selection_pstrcontrol new_pstr
                );
                
    form_trav.btPrefix.Click.Add(fun _ ->  match !selection with 
                                                None -> ()
                                              | Some(ctrl) -> let new_pstr = createAndAddPstringCtrl (pstrseq_prefix ctrl.Sequence ctrl.SelectedNodeIndex)
                                                              change_selection_pstrcontrol new_pstr
                                        );                
                                        
    form_trav.btExt.Click.Add(fun _ ->  match !selection with 
                                                None -> ()
                                              | Some(ctrl) -> let new_pstr = createAndAddPstringCtrl (pstrseq_ext gr_nodes ctrl.Sequence ctrl.SelectedNodeIndex)
                                                              change_selection_pstrcontrol new_pstr
                                        );
                                             
    form_trav.btStar.Click.Add(fun _ ->  match !selection with 
                                                None -> ()
                                              | Some(ctrl) -> let new_pstr = createAndAddPstringCtrl (pstrseq_star gr_nodes ctrl.Sequence ctrl.SelectedNodeIndex)
                                                              change_selection_pstrcontrol new_pstr
                                        );
                                        
    form_trav.btDelete.Click.Add(fun _ -> 
                    match !selection with 
                        None -> ()
                      | Some(ctrl) ->
                        let i = form_trav.seqflowPanel.Controls.GetChildIndex(ctrl)
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
                        
                        form_trav.seqflowPanel.Controls.Remove(ctrl)
                );

    form_trav.btNew.Click.Add(fun _ -> 
                        let new_pstr = createAndAddPstringCtrl [||]
                        change_selection_pstrcontrol new_pstr
                );
                
    form_trav.btBackspace.Click.Add(fun _ -> match !selection with 
                                                None -> ()
                                              | Some(ctrl) -> ctrl.remove_last_node())

    form_trav.btAdd.Click.Add(fun _ ->  match !selection with 
                                                None -> ()
                                              | Some(ctrl) -> ctrl.add_node {label="..."; link=0; tag = null; shape = ShapeRectangle; color = Color.White} )

    form_trav.btEditLabel.Click.Add(fun _ ->  match !selection with 
                                                None -> ()
                                              | Some(ctrl) -> ctrl.EditLabel(); )
    
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
                            let gen_node = Internal(gr_nodeindex)
                            let player = gennode_player gr_nodes gen_node
                            ctrl.add_node { label = graph_node_label_with_idsuffix gr_nodes gr_nodeindex;
                                            tag = box gen_node; // Use the tag field to store the generalized graph node
                                            link = 0;
                                            shape=player_to_shape player;
                                            color=player_to_color player
                                            }
                        // add a value-leaf node to the traversal
                        else
                            let gen_node = ValueLeaf(gr_nodeindex,1)
                            let player = gennode_player gr_nodes gen_node
                            ctrl.add_node { label = "1_{"^(graph_node_label_with_idsuffix gr_nodes gr_nodeindex)^"}";
                                            tag = box gen_node;
                                            link = 0;
                                            shape=player_to_shape player;
                                            color=player_to_color player
                                           }       
                        refocus_pstrcontrol()
                    end
              | _ -> ()
        );
    

    // convert a pstrnode to latex code
    let pstrnode_to_latex pstrnode =
        if pstrnode.tag = null then
          pstrnode.label
        else
          gennode_to_latex gr_nodes ((unbox pstrnode.tag) : gen_node)
    
    form_trav.btExportGraph.Click.Add(fun _ -> Texexportform.LoadExportGraphToLatexWindow mdiparent lnfrules)
    form_trav.btExportTrav.Click.Add(fun _ -> match !selection with 
                                                None -> ()
                                              | Some(ctrl) -> Texexportform.LoadExportTraversalToLatexWindow mdiparent pstrnode_to_latex ctrl.Sequence)

    ignore(form_trav.Show()) 
;;

