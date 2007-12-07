(** $Id: $
	Description: Traversals window
	Author:		William Blum
**)

#light
open Common
open System.Drawing
open System.Windows.Forms
open System.Drawing
open Traversal
open Lnf
open GUI
 

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


let f2 = float32 2.0
let margin = 3
let internodesep = 10
let link_vertical_distance = 5

let penWidth = float32 1 
let pen = new Pen(Color.Black, penWidth)

let mutable selection_pen = new Pen(Color.Blue, float32 2.0)
selection_pen.DashStyle <-  System.Drawing.Drawing2D.DashStyle.Dash
selection_pen.DashOffset <- float32 2.0

let arrow_pen = new Pen(Color.Black, penWidth)
arrow_pen.EndCap <- System.Drawing.Drawing2D.LineCap.ArrowAnchor

let selection_arrow_pen = new Pen(Color.Blue, float32 2.0)
selection_arrow_pen.EndCap <- System.Drawing.Drawing2D.LineCap.ArrowAnchor


let fontHeight:float32 = float32 10.0
let font = new Font("Arial", fontHeight)

let backgroundColor = Color.Azure
let brush = new SolidBrush(backgroundColor)
let selection_brush = new SolidBrush(backgroundColor)
let textBrush = new SolidBrush(Color.Black);


let ShowCompGraphTraversalWindow mdiparent filename ((gr_nodes,gr_edges) as compgraph) lnfrules =
    let form_trav = new Traversal.Traversal()
    form_trav.MdiParent <- mdiparent;
    let trav = ref [||]
    let bboxes = ref [||]
    let link_pos = ref [||]
    
    let edited_node = ref 0
    let travnode_selection = ref 0
    
    let recreate_bbox (graphics:System.Drawing.Graphics) =
        if Array.length !trav > 0 then
            bboxes := Array.create (Array.length !trav) (System.Drawing.Rectangle(0,0,0,0))
            link_pos := Array.create (Array.length !trav) (Point(0,0))

            let Width = form_trav.picTrav.ClientSize.Width
            and Height = form_trav.picTrav.ClientSize.Height 
            let half_height = Height/2

            let x = ref (internodesep - margin)
            for i = 0 to (Array.length !trav)-1 do 
              let txt,link = (!trav).(i)
              let textdim = graphics.MeasureString(txt, font);
              let top_y = half_height+link_vertical_distance-(int (textdim.Height/f2))-margin
              (!bboxes).(i) <- Rectangle(!x, top_y, int textdim.Width + 2*margin, int textdim.Height + 2*margin)
              (!link_pos).(i) <- Point(!x+(int (textdim.Width/f2)), top_y)
              x:= !x + int textdim.Width + internodesep
            done;
            form_trav.pichScroll.Minimum <- 0
            form_trav.pichScroll.Maximum <- max 0 ((!bboxes).((Array.length !trav)-1).Right)
            form_trav.pichScroll.LargeChange <- Width;
            form_trav.pichScroll.SmallChange <- (!bboxes).(0).Width
        else 
            form_trav.pichScroll.Minimum <- 0
            form_trav.pichScroll.Maximum <- 0
                
    let NodeFromPosition (pos:Point) =
        Array.find_index (function (a:Rectangle) -> a.Contains(pos+Size(form_trav.pichScroll.Value,0))) !bboxes
        
    let traversal_add_node node = 
        trav := Array.concat [!trav; [|node|]];
        form_trav.pichScroll.Value <- max 0 (form_trav.pichScroll.Maximum-form_trav.pichScroll.LargeChange)
        bboxes:=[||]; // signal that the bbox need to be recomputed at the next repainting
        
    
    let last_hovered_node = ref null : Microsoft.Glee.Drawing.Node ref
    
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
    form_trav.gViewer.SelectionChanged.Add(fun _ -> let selection = form_trav.gViewer.SelectedObject
                                                    if selection = null then
                                                      last_hovered_node := null
                                                    else if (selection :? Microsoft.Glee.Drawing.Node) then
                                                      last_hovered_node := (selection :?> Microsoft.Glee.Drawing.Node)
                                          );
                                          
    form_trav.gViewer.MouseClick.Add(fun _ -> 
            let selected_node = !last_hovered_node in
            if selected_node <> null then
                begin
                    traversal_add_node (graph_node_label (gr_nodes.(int_of_string selected_node.Attr.Id)),0)
                    form_trav.picTrav.Invalidate();
                end
        );
    
    form_trav.picTrav.Resize.Add(fun _ -> bboxes:=[||]);
    
    form_trav.pichScroll.ValueChanged.Add( fun _ -> form_trav.picTrav.Invalidate());
    
    form_trav.btBackspace.Click.Add(fun _ -> if Array.length !trav > 0 then
                                               begin 
                                                trav := Array.sub !trav 0 ((Array.length !trav)-1)
                                                form_trav.picTrav.Invalidate()
                                               end
                                    )

    form_trav.btAdd.Click.Add(fun _ -> traversal_add_node ("...",0)
                                       form_trav.picTrav.Invalidate())
    
    form_trav.btExportGraph.Click.Add(fun _ -> Texexportform.LoadExportGraphToLatexWindow mdiparent lnfrules)
    form_trav.btExportTrav.Click.Add(fun _ -> Texexportform.LoadExportTraversalToLatexWindow mdiparent !trav)

    form_trav.picTrav.MouseClick.Add ( fun e -> 
                if e.Button = MouseButtons.Left then                   
                    try
                        travnode_selection:= NodeFromPosition e.Location;
                        form_trav.picTrav.Invalidate();
                    with Not_found -> ();
                    
                else if  e.Button = MouseButtons.Right 
                    && (!travnode_selection < Array.length !trav) then
                    begin
                        try
                            let sel = NodeFromPosition e.Location
                            if sel < !travnode_selection then
                              begin
                                let txt,_ = (!trav).(!travnode_selection) in
                                (!trav).(!travnode_selection) <- txt,!travnode_selection-sel
                              end
                        with Not_found -> 
                                let txt,_ = (!trav).(!travnode_selection) in
                                (!trav).(!travnode_selection) <- txt,0;                        
                        form_trav.picTrav.Invalidate()
                    end
                );
                
    form_trav.picTrav.MouseDoubleClick.Add(fun e -> 
                    try
                        let i = NodeFromPosition e.Location
                        edited_node := i
                        form_trav.nodeEditTextBox.Visible <- true 
                        form_trav.nodeEditTextBox.Width <- (!bboxes).(i).Width
                        form_trav.nodeEditTextBox.Left <- (!bboxes).(i).Left
                        form_trav.nodeEditTextBox.Top <- (!bboxes).(i).Top
                        form_trav.nodeEditTextBox.Select()
                    with Not_found -> ()
                    );

    form_trav.nodeEditTextBox.KeyUp.Add( fun e -> if e.KeyCode = Keys.Return then
                                                    if form_trav.nodeEditTextBox.Visible then
                                                        form_trav.nodeEditTextBox.Visible <- false
                                                        if !edited_node< Array.length !trav then
                                                            let _,lnk = (!trav).(!edited_node) in
                                                            (!trav).(!edited_node) <- form_trav.nodeEditTextBox.Text,lnk
                                                            bboxes:=[||]
                                                            form_trav.picTrav.Invalidate()
                                                            
                                         )
                
    form_trav.picTrav.Paint.Add( fun e -> 
      let graphics = e.Graphics      
      graphics.SmoothingMode <- System.Drawing.Drawing2D.SmoothingMode.AntiAlias

      if (Array.length !bboxes) = 0 then 
        recreate_bbox graphics
      
      let DrawNode i brush pen arrow_pen = 
          let delta = -form_trav.pichScroll.Value
          let txt,link = (!trav).(i)
          let mutable bbox = (!bboxes).(i)
          bbox.Offset(delta, 0);          
          graphics.FillEllipse(brush, bbox )
          graphics.DrawEllipse(pen, bbox )
          TextRenderer.DrawText(e.Graphics, txt, font, bbox, 
                                SystemColors.ControlText, (Enum.combine [TextFormatFlags.VerticalCenter;TextFormatFlags.HorizontalCenter]));

          if link<>0 then
            begin
                let src = (!link_pos).(i)+Size(delta,0)
                let dst = (!link_pos).(i-link)+Size(delta,0)
                let tmp = src + Size(dst)
                let mid = Point(tmp.X/2,tmp.Y/2- (link/2+1)*link_vertical_distance)
                graphics.DrawCurve(arrow_pen, [|src;mid;dst|])
            end
      
      for i = 0 to (Array.length !trav)-1 do 
        if i <> !travnode_selection then DrawNode i brush pen arrow_pen
      done
      // Draw the selected node after the other nodes have been drawn
      if !travnode_selection < Array.length !trav then
        DrawNode !travnode_selection selection_brush selection_pen selection_arrow_pen
    );
    ignore(form_trav.Show()) 
;;
