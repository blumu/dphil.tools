open System
(* open System.Collections.Generic *)
open System.ComponentModel
open System.Data
open System.Drawing
open System.Text
open System.Windows.Forms
open System.IO
open Printf
open Type
open Lnf
open Hog
open Hocpda



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


let appterm_of_treeviewnode (node:TreeNode) =
  node.Tag:?>appterm
;;

let is_terminal_treeviewnode (node:TreeNode) =
  node.Tag <> null &&  (match applicative_decomposition (appterm_of_treeviewnode node) with Tm(_),_ -> true  | _ -> false)
;;

let is_expandable_treeviewnode (node:TreeNode) =
  node.Tag <> null && (match applicative_decomposition (appterm_of_treeviewnode node) with Tm(_),_ -> false  | _ -> true)
;;


(* Expand the node of the treeview by performing one step of the CPDA.
   Return true if the CPDA has emitted a terminal, false otherwise. *)
let expand_term_in_treeview hors (treeview_node:TreeNode) =
    let rec expand_term_in_treeview_aux (rootnode:TreeNode) t = 
      let op,operands = applicative_decomposition t in
      match op with
        Tm(f) -> rootnode.Text <- f;
                 rootnode.ImageKey <- "BookClosed";
                 rootnode.SelectedImageKey <- "BookClosed";
                 rootnode.Tag <- t;
                 List.iter (function operand -> let newNode = new TreeNode((string_of_appterm operand), ImageKey = "Help", SelectedImageKey = "Help") in
                                                  newNode.Tag <- operand;
                                                  ignore(rootnode.Nodes.Add(newNode));
                                                  ignore(expand_term_in_treeview_aux newNode operand);
                                                ) operands;
                 rootnode.Expand();                                                             
                 true;
            (* The leftmost operator is not a terminal: we just replace the Treeview node label
               by the reduced term. *)
       | Nt(nt) -> rootnode.Text <- string_of_appterm t;
                   rootnode.Tag <- t;
                   false;
       | _ -> failwith "bug in applicative_decomposition!";
    in
    let _,redterm = step_reduce hors (appterm_of_treeviewnode treeview_node) in
    expand_term_in_treeview_aux treeview_node redterm;
;;


#light

type HorsForm = 
  class
    inherit Form as base

    val mutable components: System.ComponentModel.Container;
    override this.Dispose(disposing) =
        if (disposing && (match this.components with null -> false | _ -> true)) then
          this.components.Dispose();
        base.Dispose(disposing)

    member this.validate_valuetree_path (node:TreeNode) =
      if node.Tag <> null then
          let nodepath = Cpdaform.TreeNode_get_path (if is_terminal_treeviewnode node then node else node.Parent)
          let path = List.map (function (n:TreeNode) -> n.Text) nodepath
          let v,c = this.hors.rs_path_validator path
          if v then
            begin
              this.pathTextBox.Text <- c;
              this.pathTextBox.ForeColor <- System.Drawing.Color.Green
            end
          else
            begin
              this.pathTextBox.Text <- c;
              this.pathTextBox.ForeColor <- System.Drawing.Color.Red
            end
      else
        this.pathTextBox.Clear();

    member this.InitializeComponent() =
        this.components <- new System.ComponentModel.Container();
        let resources = new System.ComponentModel.ComponentResourceManager((type HorsForm)) 
        
        this.outerSplitContainer <- new System.Windows.Forms.SplitContainer();
        this.valueTreeView <- new System.Windows.Forms.TreeView();
        this.imageList <- new System.Windows.Forms.ImageList(this.components);
        this.treeviewLabel <- new System.Windows.Forms.Label();
        this.rightContainer <- new System.Windows.Forms.SplitContainer();
        this.rightUpperSplitContainer <- new System.Windows.Forms.SplitContainer();
        this.pathTextBox <- new System.Windows.Forms.TextBox();
        this.pathLabel <- new System.Windows.Forms.Label();
        this.codeRichTextBox <- new System.Windows.Forms.RichTextBox();
        this.codeLabel <- new System.Windows.Forms.Label();
        this.runButton <- new System.Windows.Forms.Button();
        this.graphButton <- new System.Windows.Forms.Button();
        this.cpdaButton <- new System.Windows.Forms.Button();
        this.pdaButton <- new System.Windows.Forms.Button();
        this.np1pdaButton <- new System.Windows.Forms.Button();
        this.outputTextBox <- new System.Windows.Forms.RichTextBox();
        this.outputLabel <- new System.Windows.Forms.Label();
        this.outerSplitContainer.Panel1.SuspendLayout();
        this.outerSplitContainer.Panel2.SuspendLayout();
        this.outerSplitContainer.SuspendLayout();
        this.rightContainer.Panel1.SuspendLayout();
        this.rightContainer.Panel2.SuspendLayout();
        this.rightContainer.SuspendLayout();
        this.rightUpperSplitContainer.Panel1.SuspendLayout();
        this.rightUpperSplitContainer.Panel2.SuspendLayout();
        this.rightUpperSplitContainer.SuspendLayout();
        this.SuspendLayout();
        (* 
         * outerSplitContainer
         *) 
        this.outerSplitContainer.Dock <- System.Windows.Forms.DockStyle.Fill;
        this.outerSplitContainer.FixedPanel <- System.Windows.Forms.FixedPanel.Panel1;
        this.outerSplitContainer.Location <- new System.Drawing.Point(0, 0);
        this.outerSplitContainer.Name <- "outerSplitContainer";
        // 
        // outerSplitContainer.Panel1
        // 
        this.outerSplitContainer.Panel1.Controls.Add(this.valueTreeView);
        this.outerSplitContainer.Panel1.Controls.Add(this.treeviewLabel);
        // 
        // outerSplitContainer.Panel2
        // 
        this.outerSplitContainer.Panel2.Controls.Add(this.rightContainer);
        this.outerSplitContainer.Size <- new System.Drawing.Size(952, 682);
        this.outerSplitContainer.SplitterDistance <- 450;
        this.outerSplitContainer.TabIndex <- 0;
        // 
        // valueTreeView
        // 
        this.valueTreeView.Anchor <- 
          Enum.combine [ System.Windows.Forms.AnchorStyles.Top; 
                        System.Windows.Forms.AnchorStyles.Bottom;
                        System.Windows.Forms.AnchorStyles.Left;
                        System.Windows.Forms.AnchorStyles.Right];
        this.valueTreeView.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.valueTreeView.HideSelection <- false;
        this.valueTreeView.ImageKey <- "";
        this.valueTreeView.SelectedImageKey <- "";
        this.valueTreeView.ImageList <- this.imageList;
        this.valueTreeView.Location <- new System.Drawing.Point(0, 28);
        this.valueTreeView.Name <- "valueTreeView";
        // Treeview.ShowNodeToolTips Not implemented in  Mono
        if not Common.IsRunningOnMono then
            (); //this.valueTreeView.ShowNodeToolTips <- true;
        this.valueTreeView.ShowRootLines <- false;
        this.valueTreeView.Size <- new System.Drawing.Size(450, 654);
        this.valueTreeView.TabIndex <- 1;
        this.valueTreeView.add_NodeMouseDoubleClick(fun  _ e -> 
                if is_expandable_treeviewnode e.Node then
                  begin
                    ignore(expand_term_in_treeview this.hors e.Node);
                    this.validate_valuetree_path e.Node;
                  end
              );
                        
        this.valueTreeView.add_BeforeCollapse(fun _ e -> 
              // e.Node.Level is incompatible with Mono
              if e.Node.Parent = null then
                e.Cancel <- true;
            );
            
        this.valueTreeView.add_AfterSelect(fun _ e -> 
            let currentNode = this.valueTreeView.SelectedNode  
            this.runButton.Enabled <- is_expandable_treeviewnode currentNode;
            this.validate_valuetree_path currentNode;
        );
 
        // 
        // imageList
        // 
        
        this.imageList.ImageStream <- (resources.GetObject("imageList.ImageStream") :?> System.Windows.Forms.ImageListStreamer);
        this.imageList.Images.SetKeyName(0, "Help");
        this.imageList.Images.SetKeyName(1, "BookStack");
        this.imageList.Images.SetKeyName(2, "BookClosed");
        this.imageList.Images.SetKeyName(3, "BookOpen");
        this.imageList.Images.SetKeyName(4, "Item");
        this.imageList.Images.SetKeyName(5, "Run");
        
        // 
        // treeviewLabel
        // 
        this.treeviewLabel.AutoSize <- true;
        this.treeviewLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.treeviewLabel.Location <- new System.Drawing.Point(3, 9);
        this.treeviewLabel.Name <- "treeviewLabel";
        this.treeviewLabel.Size <- new System.Drawing.Size(58, 16);
        this.treeviewLabel.TabIndex <- 0;
        this.treeviewLabel.Text <- "The lazy value-tree:";
        // 
        // rightContainer
        // 
        this.rightContainer.Dock <- System.Windows.Forms.DockStyle.Fill;
        this.rightContainer.Location <- new System.Drawing.Point(0, 0);
        this.rightContainer.Name <- "rightContainer";
        this.rightContainer.Orientation <- System.Windows.Forms.Orientation.Horizontal;
        // 
        // rightContainer.Panel1
        // 
        this.rightContainer.Panel1.Controls.Add(this.rightUpperSplitContainer);
        // 
        // rightContainer.Panel2
        // 
        this.rightContainer.Panel2.Controls.Add(this.runButton);        
        this.rightContainer.Panel2.Controls.Add(this.graphButton);
        this.rightContainer.Panel2.Controls.Add(this.cpdaButton);
        this.rightContainer.Panel2.Controls.Add(this.pdaButton);
        this.rightContainer.Panel2.Controls.Add(this.np1pdaButton);
        this.rightContainer.Panel2.Controls.Add(this.outputTextBox);
        this.rightContainer.Panel2.Controls.Add(this.outputLabel);
        this.rightContainer.Size <- new System.Drawing.Size(680, 682);
        this.rightContainer.SplitterDistance <- 357;
        this.rightContainer.TabIndex <- 0;
        // 
        // rightUpperSplitContainer
        // 
        this.rightUpperSplitContainer.Dock <- System.Windows.Forms.DockStyle.Fill;
        this.rightUpperSplitContainer.FixedPanel <- System.Windows.Forms.FixedPanel.Panel1;
        this.rightUpperSplitContainer.Location <- new System.Drawing.Point(0, 0);
        this.rightUpperSplitContainer.Name <- "rightUpperSplitContainer";
        this.rightUpperSplitContainer.Orientation <- System.Windows.Forms.Orientation.Horizontal;
        // 
        // rightUpperSplitContainer.Panel1
        // 
        this.rightUpperSplitContainer.Panel1.Controls.Add(this.pathTextBox);
        this.rightUpperSplitContainer.Panel1.Controls.Add(this.pathLabel);
        // 
        // rightUpperSplitContainer.Panel2
        // 
        this.rightUpperSplitContainer.Panel2.Controls.Add(this.codeRichTextBox);
        this.rightUpperSplitContainer.Panel2.Controls.Add(this.codeLabel);
        this.rightUpperSplitContainer.Size <- new System.Drawing.Size(680, 357);
        this.rightUpperSplitContainer.SplitterDistance <- 95;
        this.rightUpperSplitContainer.TabIndex <- 0;
        // 
        // pathTextBox
        // 
        this.pathTextBox.Anchor<- 
          Enum.combine [ System.Windows.Forms.AnchorStyles.Top;
                    System.Windows.Forms.AnchorStyles.Bottom;
                    System.Windows.Forms.AnchorStyles.Left;
                    System.Windows.Forms.AnchorStyles.Right ];
        this.pathTextBox.BackColor <- System.Drawing.SystemColors.ControlLight;
        this.pathTextBox.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.pathTextBox.Location <- new System.Drawing.Point(0, 28);
        this.pathTextBox.Multiline <- true;
        this.pathTextBox.Name <- "pathTextBox";
        this.pathTextBox.ReadOnly <- true;
        this.pathTextBox.ScrollBars <- System.Windows.Forms.ScrollBars.Vertical;
        this.pathTextBox.Size <- new System.Drawing.Size(680, 67);
        this.pathTextBox.TabIndex <- 1;
        // 
        // pathLabel
        // 
        this.pathLabel.AutoSize <- true;
        this.pathLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.pathLabel.Location <- new System.Drawing.Point(3, 9);
        this.pathLabel.Name <- "pathLabel";
        this.pathLabel.Size <- new System.Drawing.Size(72, 16);
        this.pathLabel.TabIndex <- 0;
        this.pathLabel.Text <- "Result of the value tree path validation:";
        // 
        // codeRichTextBox
        // 
        this.codeRichTextBox.Anchor <- 
          Enum.combine [ System.Windows.Forms.AnchorStyles.Top;
                    System.Windows.Forms.AnchorStyles.Bottom;
                    System.Windows.Forms.AnchorStyles.Left;
                    System.Windows.Forms.AnchorStyles.Right ];
        this.codeRichTextBox.BackColor <- System.Drawing.SystemColors.ControlLight;
        //this.codeRichTextBox.ForeColor <- System.Drawing.Color.Blue;
        this.codeRichTextBox.BorderStyle <- System.Windows.Forms.BorderStyle.FixedSingle;
        this.codeRichTextBox.Font <- new System.Drawing.Font("Lucida Console", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.codeRichTextBox.Location <- new System.Drawing.Point(0, 18);
        this.codeRichTextBox.Name <- "codeRichTextBox";
        this.codeRichTextBox.ReadOnly <- true;
        this.codeRichTextBox.Size <- new System.Drawing.Size(680, 240);
        this.codeRichTextBox.TabIndex <- 1;
        this.codeRichTextBox.Text <- (string_of_rs this.hors) ;
        //this.codeRichTextBox.Dock <- DockStyle.Fill;
        this.codeRichTextBox.WordWrap <- false;
        // 
        // codeLabel
        // 
        this.codeLabel.AutoSize <- true;
        this.codeLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.codeLabel.Location <- new System.Drawing.Point(3, -1);
        this.codeLabel.Name <- "codeLabel";
        this.codeLabel.Size <- new System.Drawing.Size(100, 16);
        this.codeLabel.TabIndex <- 0;
        this.codeLabel.Text <- "Description of the recursion scheme:";
        
        
          // 
        // runButton
        // 
        this.runButton.Enabled <- true;
        this.runButton.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.runButton.ImageAlign <- System.Drawing.ContentAlignment.MiddleRight;
        this.runButton.ImageKey <- "Run";
        this.runButton.ImageList <- this.imageList;
        this.runButton.Location <- new System.Drawing.Point(0, -1);
        this.runButton.Name <- "runButton";
        this.runButton.Size <- new System.Drawing.Size(90, 27);
        this.runButton.TabIndex <- 0;
        this.runButton.Text <- "Run";
        this.runButton.TextImageRelation <- System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.runButton.Click.Add(fun e -> let node = this.valueTreeView.SelectedNode
                                          if is_expandable_treeviewnode node then
                                            begin
                                             while not (expand_term_in_treeview this.hors node) do () done;
                                             this.validate_valuetree_path node;
                                            end
        );
        
        
        // 
        // graphButton
        // 
        this.graphButton.Enabled <- true;
        this.graphButton.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        //this.graphButton.ImageAlign <- System.Drawing.ContentAlignment.MiddleRight;
        //this.graphButton.ImageKey <- "Run";
        //this.graphButton.ImageList <- this.imageList;
        this.graphButton.Location <- new System.Drawing.Point(100, -1);
        this.graphButton.Name <- "graphButton";
        this.graphButton.Size <- new System.Drawing.Size(140, 27);
        this.graphButton.TabIndex <- 0;
        this.graphButton.Text <- "Computation graph";
        this.graphButton.TextImageRelation <- System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.graphButton.Click.Add(fun e -> 
            // create a form
            let form = new System.Windows.Forms.Form()
            let viewer = new Microsoft.Glee.GraphViewerGdi.GViewer()
            let panel1 = new System.Windows.Forms.Panel();
            let buttonLatex = new System.Windows.Forms.Button()

            form.SuspendLayout(); 

            form.Text <- "Computation graph of "^this.filename;
            form.Size <- Size(700,800);
            this.outputTextBox.Text <- "Rules in eta-long normal form:\n"^(String.concat "\n" (List.map lnfrule_to_string this.lnfrules));

            buttonLatex.Location <- new System.Drawing.Point(1, 1)
            buttonLatex.Name <- "button1"
            buttonLatex.Size <- new System.Drawing.Size(267, 23)
            buttonLatex.TabIndex <- 2
            buttonLatex.Text <- "Export to Latex"
            buttonLatex.UseVisualStyleBackColor <- true
            buttonLatex.Click.Add(fun e -> Texexportform.export_to_latex this.lnfrules )


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
            viewer.Graph <- compgraph_to_graphview this.compgraph;
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
            ignore(form.Show()); 
        );
        
      
        
        //
        // cpdaButton
        //
        this.cpdaButton.Enabled <- true;
        this.cpdaButton.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.cpdaButton.Location <- new System.Drawing.Point(250, -1);
        this.cpdaButton.Name <- "cpdaButton";
        this.cpdaButton.Size <- new System.Drawing.Size(100, 27);
        this.cpdaButton.TabIndex <- 0;
        this.cpdaButton.Text <- "Build n-CPDA";
        this.cpdaButton.TextImageRelation <- System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.cpdaButton.Click.Add( fun e -> //create the cpda form
                                            let form = new Cpdaform.CpdaForm("CPDA built from the recursion scheme "^this.filename,
                                                                             Hocpda.hors_to_cpda Ncpda this.hors this.compgraph this.vartmtypes)
                                            ignore(form.Show());
                                 );

        //
        // pdaButton
        //
        this.pdaButton.Enabled <- true;
        this.pdaButton.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.pdaButton.Location <- new System.Drawing.Point(360, -1);
        this.pdaButton.Name <- "pdaButton";
        this.pdaButton.Size <- new System.Drawing.Size(150, 27);
        this.pdaButton.TabIndex <- 0;
        this.pdaButton.Text <- "Build n-PDA (safe RS)";
        this.pdaButton.TextImageRelation <- System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.pdaButton.Click.Add( fun e -> //create the pda
                                            let form = new Cpdaform.CpdaForm("PDA built from the recursion scheme "^this.filename,
                                                                             Hocpda.hors_to_cpda Npda this.hors this.compgraph this.vartmtypes)
                                            ignore(form.Show());
                                 );
            

        //
        // np1pdaButton
        //
        this.np1pdaButton.Enabled <- true;
        this.np1pdaButton.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.np1pdaButton.Location <- new System.Drawing.Point(520, -1);
        this.np1pdaButton.Name <- "np1pdaButton";
        this.np1pdaButton.Size <- new System.Drawing.Size(120, 27);
        this.np1pdaButton.TabIndex <- 0;
        this.np1pdaButton.Text <- "Build (n+1)-PDA";
        this.np1pdaButton.TextImageRelation <- System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.np1pdaButton.Click.Add( fun e -> //create the pda
                                            let form = new Cpdaform.CpdaForm("n+1-PDA built from the recursion scheme "^this.filename,
                                                                             Hocpda.hors_to_cpda Np1pda this.hors this.compgraph this.vartmtypes)
                                            ignore(form.Show());
                                 );        
        
      
        // 
        // outputTextBox
        // 
        this.outputTextBox.Anchor <- 
          Enum.combine [ System.Windows.Forms.AnchorStyles.Top;
                    System.Windows.Forms.AnchorStyles.Bottom;
                    System.Windows.Forms.AnchorStyles.Left;
                    System.Windows.Forms.AnchorStyles.Right ];
        this.outputTextBox.BackColor <- System.Drawing.SystemColors.ControlLight;
        this.outputTextBox.Font <- new System.Drawing.Font("Lucida Console", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.outputTextBox.Location <- new System.Drawing.Point(0, 48);
        this.outputTextBox.Multiline <- true;
        this.outputTextBox.Name <- "outputTextBox";
        this.outputTextBox.ReadOnly <- true;
        this.outputTextBox.ScrollBars <- System.Windows.Forms.RichTextBoxScrollBars.Both;
        this.outputTextBox.Size <- new System.Drawing.Size(680, 273);
        this.outputTextBox.TabIndex <- 2;
        this.outputTextBox.WordWrap <- false;
        // 
        // outputLabel
        // 
        this.outputLabel.AutoSize <- true;
        this.outputLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.outputLabel.Location <- new System.Drawing.Point(3, 29);
        this.outputLabel.Name <- "outputLabel";
        this.outputLabel.Size <- new System.Drawing.Size(47, 16);
        this.outputLabel.TabIndex <- 1;
        this.outputLabel.Text <- "Output:";
        // 
        // DisplayForm
        // 
        this.AcceptButton <- this.runButton;
        this.AutoScaleDimensions <- new System.Drawing.SizeF(6.0F, 13.0F);
        this.AutoScaleMode <- System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize <- new System.Drawing.Size(1100, 682);
        this.Controls.Add(this.outerSplitContainer);
        this.Font <- new System.Drawing.Font("Tahoma", 8.25F);
        //this.Icon <- (resources.GetObject("$this.Icon") :?> System.Drawing.Icon);
        this.Name <- "DisplayForm";
        this.Text <- "Samples";
        this.outerSplitContainer.Panel1.ResumeLayout(false);
        this.outerSplitContainer.Panel1.PerformLayout();
        this.outerSplitContainer.Panel2.ResumeLayout(false);
        this.outerSplitContainer.ResumeLayout(false);
        this.rightContainer.Panel1.ResumeLayout(false);
        this.rightContainer.Panel2.ResumeLayout(false);
        this.rightContainer.Panel2.PerformLayout();
        this.rightContainer.ResumeLayout(false);
        this.rightUpperSplitContainer.Panel1.ResumeLayout(false);
        this.rightUpperSplitContainer.Panel1.PerformLayout();
        this.rightUpperSplitContainer.Panel2.ResumeLayout(false);
        this.rightUpperSplitContainer.Panel2.PerformLayout();
        this.rightUpperSplitContainer.ResumeLayout(false);
        this.ResumeLayout(false)


    val mutable outerSplitContainer : System.Windows.Forms.SplitContainer;
    val mutable treeviewLabel : System.Windows.Forms.Label;
    val mutable rightContainer : System.Windows.Forms.SplitContainer;
    val mutable outputTextBox : System.Windows.Forms.RichTextBox;
    val mutable outputLabel : System.Windows.Forms.Label;
    val mutable runButton : System.Windows.Forms.Button;
    val mutable graphButton : System.Windows.Forms.Button;
    val mutable cpdaButton : System.Windows.Forms.Button;
    val mutable pdaButton : System.Windows.Forms.Button;
    val mutable np1pdaButton : System.Windows.Forms.Button;
    val mutable rightUpperSplitContainer : System.Windows.Forms.SplitContainer;
    val mutable pathTextBox : System.Windows.Forms.TextBox;
    val mutable pathLabel : System.Windows.Forms.Label;
    val mutable codeLabel : System.Windows.Forms.Label;
    val mutable valueTreeView : System.Windows.Forms.TreeView;
    val mutable imageList : System.Windows.Forms.ImageList;
    val mutable codeRichTextBox : System.Windows.Forms.RichTextBox;
    val mutable hors : recscheme;
    val mutable lnfrules : lnfrule list;
    val mutable vartmtypes : (ident*typ) list;
    val mutable compgraph : computation_graph;
    val mutable filename : string;

    new (filename,newhors) as this =
       { outerSplitContainer = null;
         treeviewLabel = null;
         rightContainer = null;
         outputTextBox = null;
         outputLabel =null;
         cpdaButton = null;
         pdaButton = null;
         np1pdaButton = null;
         graphButton = null;
         runButton =null;
         rightUpperSplitContainer =null;
         pathTextBox =null;
         pathLabel = null;
         codeLabel = null;
         valueTreeView = null;
         imageList = null;
         codeRichTextBox = null;
         components = null;
         hors = newhors;
         lnfrules = [];
         vartmtypes = [];
         compgraph = [||],NodeEdgeMap.empty;
         filename = "";
         }
       
       then 
        this.InitializeComponent();

        this.Text <- ("Higher-order recursion scheme - "^filename);
        this.filename <- filename;

        let rootNode = new TreeNode("Value tree", Tag = (null : obj), ImageKey = "BookStack", SelectedImageKey = "BookStack")
        ignore(this.valueTreeView.Nodes.Add(rootNode));
        rootNode.Expand();
              
        // check that the hors is well-defined
        let errors = rs_check this.hors in
        if errors <> [] then
          begin
            let msg = "Inconsistent HORS definition. Please check types and definitions of terminals, non-terminals and variables.\nList of errors:\n  "^(String.concat "\n  " errors) in
            //Mainform.Debug_print msg;
            failwith msg
          end
          
        // convert the rules to LNF
        let r,v = rs_to_lnf this.hors in

        this.lnfrules <- r;
        this.vartmtypes <- v;
        
        // create the computation graph from the rules of the recursion scheme in in LNF
        this.compgraph <- lnfrs_to_graph this.lnfrules;

        let SNode = new TreeNode("S")  
        SNode.ImageKey <- "Help";
        SNode.SelectedImageKey <- "Help";
        SNode.Tag <- Nt("S");
        ignore(rootNode.Nodes.Add(SNode)); 
        this.valueTreeView.SelectedNode <- SNode;      
  end
