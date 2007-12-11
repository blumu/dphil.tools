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

(** Create the graph view of the computation graph
    @param graph the computation graph
    @return the GLEE graph object representing the computation graph
**)
let compgraph_to_graphview (nodes_content:cg_nodes,edges:cg_edges) =
    (* create a graph object *)
    let gleegraph = new Microsoft.Glee.Drawing.Graph("graph") in
    
    for k = 0 to (Array.length nodes_content)-1 do
        let nodeid = string_of_int k in
        let node = gleegraph.AddNode nodeid in
        node.Attr.Id <- string_of_int k
        match nodes_content.(k) with 
            NCntApp -> node.Attr.Label <- "@"^" ["^nodeid^"]";
          | NCntTm(tm) -> node.Attr.Label <- tm^" ["^nodeid^"]";
                          node.Attr.Fillcolor <- Microsoft.Glee.Drawing.Color.Salmon;
                          node.Attr.Shape <- Microsoft.Glee.Drawing.Shape.Box;
                          node.Attr.LabelMargin <- 10;
          | NCntVar(x) -> node.Attr.Label <- x^" ["^nodeid^"]";
                          node.Attr.Fillcolor <- Microsoft.Glee.Drawing.Color.Green;
          | NCntAbs("",vars) -> node.Attr.Label <- LAMBDA_SYMBOL^(String.concat " " vars)^" ["^nodeid^"]";
          | NCntAbs(nt,vars) -> node.Attr.Label <- LAMBDA_SYMBOL^(String.concat " " vars)^" ["^nt^":"^nodeid^"]";
                                node.Attr.Fillcolor <- Microsoft.Glee.Drawing.Color.Yellow;
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
    viewer.Graph <- compgraph_to_graphview compgraph;
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






let ShowCompGraphTraversalWindow mdiparent filename ((gr_nodes,gr_edges) as compgraph) lnfrules =
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

    // change the current selection
    let change_selection_pstrcontrol (ctrl:Pstring.PstringControl) =
        match !selection with
              None -> select_pstrcontrol ctrl
            | Some(cursel) -> cursel.Deselection();
                              ctrl.Selection();
                              selection := Some(ctrl)
    
    // add an event handler to the a given pstring control in order to detect selection
    // of the control by the user
    let add_selection_eventhandler (ctrl: Pstring.PstringControl) =
                        ctrl.Click.Add(fun _ -> match !selection with
                                                     Some(cursel) when cursel = ctrl -> ()
                                                   | None -> select_pstrcontrol ctrl;
                                                   | Some(cursel) -> change_selection_pstrcontrol ctrl
                                                    );
    


    
    // create a default pstring
    let first = ref (new Pstring.PstringControl([||]))
    (!first).AutoSize <- true
    add_selection_eventhandler !first
    select_pstrcontrol !first
    //(!first).nodeClick.Add(fun _ -> 
    //        ignore( MessageBox.Show("salut","test", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)))
    
    form_trav.seqflowPanel.Controls.Add (!first)
    
    form_trav.seqflowPanel.Click.Add(fun _ -> unselect_pstrcontrol())
     
    form_trav.btPview.Click.Add(fun _ -> 
                    match !selection with 
                        None -> ()
                      | Some(ctrl) ->
                        let new_pstr = ref (new Pstring.PstringControl(ctrl.Sequence))
                        (!new_pstr).AutoSize <- true
                        add_selection_eventhandler !new_pstr
                        form_trav.seqflowPanel.Controls.Add !new_pstr
                        change_selection_pstrcontrol !new_pstr
                );
                
    
    form_trav.btDuplicate.Click.Add(fun _ -> 
                    match !selection with 
                        None -> ()
                      | Some(ctrl) ->
                        let new_pstr = ref (new Pstring.PstringControl(ctrl.Sequence))
                        (!new_pstr).AutoSize <- true
                        add_selection_eventhandler !new_pstr
                        form_trav.seqflowPanel.Controls.Add !new_pstr
                        change_selection_pstrcontrol !new_pstr
                );

    form_trav.btDelete.Click.Add(fun _ -> 
                    match !selection with 
                        None -> ()
                      | Some(ctrl) ->
                        unselect_pstrcontrol()
                        form_trav.seqflowPanel.Controls.Remove(ctrl)
                );

    form_trav.btNew.Click.Add(fun _ -> 
                        let new_pstr = ref (new Pstring.PstringControl([||]))
                        (!new_pstr).AutoSize <- true
                        add_selection_eventhandler !new_pstr
                        form_trav.seqflowPanel.Controls.Add !new_pstr
                        change_selection_pstrcontrol !new_pstr
                );
                
    form_trav.btOview.Click.Add(fun _ -> 
                    match !selection with 
                        None -> ()
                      | Some(ctrl) ->
                        let mutable new_pstr = new Pstring.PstringControl([||]);
                        new_pstr.AutoSize <- true
                        add_selection_eventhandler new_pstr
                        form_trav.seqflowPanel.Controls.Add (new_pstr)
                        change_selection_pstrcontrol new_pstr
                );
    
    // bind the graph to the viewer
    form_trav.gViewer.Graph <- compgraph_to_graphview compgraph;
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

    let last_hovered_node = ref null : Microsoft.Glee.Drawing.Node ref
    form_trav.gViewer.SelectionChanged.Add(fun _ -> let selection = form_trav.gViewer.SelectedObject
                                                    if selection = null then
                                                      last_hovered_node := null
                                                    else if (selection :? Microsoft.Glee.Drawing.Node) then
                                                      last_hovered_node := (selection :?> Microsoft.Glee.Drawing.Node)
                                          );
                                          
    form_trav.gViewer.MouseClick.Add(fun _ -> 
            match !selection with 
                None -> ()
               | Some(ctrl) ->
                let selected_node = !last_hovered_node in
                if selected_node <> null then
                    begin
                        let compgraph_nodeindex = int_of_string selected_node.Attr.Id
                        ctrl.add_node { label = graph_node_label gr_nodes.(compgraph_nodeindex);
                                             tag = box compgraph_nodeindex;
                                             link = 0;}
                    end
        );
    
    
    form_trav.btBackspace.Click.Add(fun _ -> match !selection with 
                                                None -> ()
                                              | Some(ctrl) -> ctrl.remove_last_node())

    form_trav.btAdd.Click.Add(fun _ ->  match !selection with 
                                                None -> ()
                                              | Some(ctrl) -> ctrl.add_node {label="..."; link=0; tag = box (-1)} )

    let travnode_to_latex travnode =
        let gr_inode = (unbox travnode.tag) : int
        match gr_inode with
         -1 -> travnode.label
        | _ -> graphnodelabel_to_latex gr_nodes.(gr_inode)

    form_trav.btExportGraph.Click.Add(fun _ -> Texexportform.LoadExportGraphToLatexWindow mdiparent lnfrules)
    form_trav.btExportTrav.Click.Add(fun _ -> match !selection with 
                                                None -> ()
                                              | Some(ctrl) -> Texexportform.LoadExportTraversalToLatexWindow mdiparent travnode_to_latex ctrl.Sequence)

    ignore(form_trav.Show()) 
;;

