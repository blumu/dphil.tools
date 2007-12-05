(** $Id: $
	Description: Traversals window
	Author:		William Blum
**)

#light
open System.Drawing
open System.Windows.Forms
open Traversal
open Lnf
open GUI
 

(** Create the graph view of the computation graph
    @param graph the computation graph
    @return the GLEE graph object representing the computation graph
**)
let compgraph_to_graphview (nodes_content:cg_nodes,edges:cg_edges) =
    (* create a graph object *)
    let graph = new Microsoft.Glee.Drawing.Graph("graph") in
    
    for k = 0 to (Array.length nodes_content)-1 do
        let nodeid = string_of_int k in
        let node = graph.AddNode nodeid in
        match nodes_content.(k) with 
            NCntApp ->  node.Attr.Label <- "@"^" ["^nodeid^"]";
          | NCntTm(tm) -> node.Attr.Label <- tm^" ["^nodeid^"]";
                          node.Attr.Fillcolor <- Microsoft.Glee.Drawing.Color.Salmon;
                          node.Attr.Shape <- Microsoft.Glee.Drawing.Shape.Box;
                          node.Attr.LabelMargin <- 10;
          | NCntVar(x) -> node.Attr.Label <- x^" ["^nodeid^"]";
                          node.Attr.Fillcolor <- Microsoft.Glee.Drawing.Color.Green;
          | NCntAbs("",vars) -> node.Attr.Label <- "λ"^(String.concat " " vars)^" ["^nodeid^"]";
          | NCntAbs(nt,vars) -> node.Attr.Label <- "λ"^(String.concat " " vars)^" ["^nt^":"^nodeid^"]";
                                node.Attr.Fillcolor <- Microsoft.Glee.Drawing.Color.Yellow;
    done;

    let addtargets source targets =
        let source_id = string_of_int source in
        let aux i target = 
            let target_id = string_of_int target in
            let edge = graph.AddEdge(source_id,target_id) in
            (match nodes_content.(source) with
                NCntApp -> edge.EdgeAttr.Label <- string_of_int i;
                               (* Highlight the first edge if it is an @-node (going to the operator) *)
                               if i = 0 then edge.EdgeAttr.Color <- Microsoft.Glee.Drawing.Color.Green;
               | NCntAbs(_)  -> ();
               | _ -> edge.EdgeAttr.Label <- string_of_int (i+1);
            )

        in 
        Array.iteri aux targets
    in
    NodeEdgeMap.iter addtargets edges;
    graph
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
    buttonLatex.Click.Add(fun e -> Texexportform.LoadExportToLatexWindow mdiparent lnfrules )


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


let ShowCompGraphTraversalWindow mdiparent filename compgraph lnfrules =
    let form_trav = new Traversal.Traversal()
    form_trav.MdiParent <- mdiparent;
    ignore(form_trav.Show()) 
;;
