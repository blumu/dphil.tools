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
        match nodes_content.(k) with 
            NCntApp -> node.Attr.Label <- "@"^" ["^nodeid^"]";
          | NCntTm(tm) -> node.Attr.Label <- tm^" ["^nodeid^"]";
                          node.Attr.LabelMargin <- 10;
          | NCntVar(x) -> node.Attr.Label <- x^" ["^nodeid^"]";
          | NCntAbs("",vars) -> node.Attr.Label <- LAMBDA_SYMBOL^(String.concat " " vars)^" ["^nodeid^"]";
          | NCntAbs(nt,vars) -> node.Attr.Label <- LAMBDA_SYMBOL^(String.concat " " vars)^" ["^nt^":"^nodeid^"]";
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
        (!new_pstr).MouseDown.Add(fun _ -> //match !selection with None -> () | Some(cursel) -> cursel.Deselection() );
                                           change_selection_pstrcontrol !new_pstr );
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
                      | Some(ctrl) -> let new_pstr = createAndAddPstringCtrl ctrl.Sequence
                                      change_selection_pstrcontrol new_pstr
                );
                
    form_trav.btOview.Click.Add(fun _ -> 
                    match !selection with 
                        None -> ()
                      | Some(ctrl) -> let new_pstr = createAndAddPstringCtrl ctrl.Sequence
                                      change_selection_pstrcontrol new_pstr                        
                );
                    
    form_trav.btDuplicate.Click.Add(fun _ -> 
                    match !selection with 
                        None -> ()
                      | Some(ctrl) -> let new_pstr = createAndAddPstringCtrl ctrl.Sequence
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
    form_trav.gViewer.BackwardEnabled <- false;
    form_trav.gViewer.Dock <- System.Windows.Forms.DockStyle.Fill;
    form_trav.gViewer.ForwardEnabled <- false;
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
    form_trav.gViewer.ZoomWindowThreshold <- 0.05;

    let gleegraph_last_hovered_node = ref null : Microsoft.Glee.Drawing.Node ref
    form_trav.gViewer.SelectionChanged.Add(fun _ -> let selection = form_trav.gViewer.SelectedObject
                                                    if selection = null then
                                                      gleegraph_last_hovered_node := null
                                                    else if (selection :? Microsoft.Glee.Drawing.Node) then
                                                      gleegraph_last_hovered_node := (selection :?> Microsoft.Glee.Drawing.Node)
                                          );
                                          
    form_trav.gViewer.MouseClick.Add(fun e -> 
            match !selection with 
                None -> ()
               | Some(ctrl) ->
                let gleegraph_selected_node = !gleegraph_last_hovered_node in
                if gleegraph_selected_node <> null then
                    begin
                        let gr_nodeindex = int_of_string gleegraph_selected_node.Attr.Id
                        
                        // add an internal node to the traversal
                        if e.Button = MouseButtons.Left then
                            let trav_node = Internal(gr_nodeindex)
                            let player = travnode_player gr_nodes trav_node
                            ctrl.add_node { label = graph_node_label gr_nodes.(gr_nodeindex);
                                            tag = box trav_node;
                                            link = 0;
                                            shape=player_to_shape player;
                                            color=player_to_color player
                                            }
                        // add a value-leaf node to the traversal
                        else
                            let trav_node = ValueLeaf(gr_nodeindex,1)
                            let player = travnode_player gr_nodes trav_node
                            ctrl.add_node { label = "1_{"^(graph_node_label gr_nodes.(gr_nodeindex))^"}";
                                            tag = box trav_node;
                                            link = 0;
                                            shape=player_to_shape player;
                                            color=player_to_color player
                                           }                          
                    end
        );
    

    
    let pstrnode_to_latex travnode =
        if travnode.tag = null then
          travnode.label
        else
          travnode_to_latex gr_nodes ((unbox travnode.tag) : trav_node)
    

    form_trav.btExportGraph.Click.Add(fun _ -> Texexportform.LoadExportGraphToLatexWindow mdiparent lnfrules)
    form_trav.btExportTrav.Click.Add(fun _ -> match !selection with 
                                                None -> ()
                                              | Some(ctrl) -> Texexportform.LoadExportTraversalToLatexWindow mdiparent pstrnode_to_latex ctrl.Sequence)

    ignore(form_trav.Show()) 
;;

