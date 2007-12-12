(** $Id$
	Description: Window for Higher-order collapsible pushdown automata
	Author:		William Blum
**)

open System
//open System.Collections.Generic
open System.ComponentModel
open System.Data
open System.Drawing
open System.Text
open System.Windows.Forms
open System.IO
open Printf
open Hog
open Hocpda



#light;;

let RichTextbox_SelectLine cum (rtb:RichTextBox) line  = 
  if line < Array.length cum then
    begin
      // selection of negative length are not compatible with Mono
      //ignore(rtb.Select( cum.(line) + rtb.Lines.(line).Length , -rtb.Lines.(line).Length ));
      ignore(rtb.Select( cum.(line) , rtb.Lines.(line).Length+1 ));
//      rtb.ScrollToCaret();
    end
;;

let RichTextBox_CumulLinesLength (rtb:RichTextBox) = 
    let cum = Array.create rtb.Lines.Length 0 in
    for i = 0 to rtb.Lines.Length -2 do
        cum .(i+1) <- cum .(i) + rtb.Lines.(i).Length + 1;
    done;
    cum
;;
(** The Tag field of the Treeview nodes are of the form [(alive,hist)] where
    [alive] tells whether the CPDA is still running at that node (i.e. has not halted nor has emitted any constant)
    and [hist] contains the history of all visited configurations.
    (stored as a list of gen_configuration, the head of the list being the current configuration). **)
let cpda_confhistory_from_treeviewnode (node:TreeNode) = snd (node.Tag:?>(bool * gen_configuration list)) ;;

let cpda_conf_from_treeviewnode (node:TreeNode) = 
    try List.hd (snd (node.Tag:?>bool * gen_configuration list))
    with Failure("hd") -> failwith "wrong treeviewnode tag"
    ;;

let is_cpda_alive_at_treeviewnode (node:TreeNode) = 
    (node.Tag<>null) && (fst (node.Tag:?>bool * gen_configuration list)) ;;

let is_cpda_emitting_at_treeviewnode (node:TreeNode) = 
    (node.Tag<>null) && ( match (node.Tag:?>bool * gen_configuration list) with 
                                  _,(TmState(_),_)::q -> true
                                | _ -> false
                        ) ;;


let is_configuration_treeviewnode (node:TreeNode) = node<> null && node.Tag<>null;;

(* Add a configuration to the history.
   When consecutive configurations share the same stack, only the last one is kept in the history.
   Also terminal configurastion are not added to the history. *)
let add_conf_to_treeviewnode_history (node:TreeNode) (state,stk as newconf :gen_configuration)  = 
  let _,history = node.Tag:?>bool * gen_configuration list
  node.Tag <- match state with
                TmState(_) -> false, history
                | _ ->  true,(match history with
                               (_,prevstk)::q when prevstk = stk -> newconf::q
                                 | _ ->  newconf::history
                             )
;;

let init_treeviewnode_history (node:TreeNode) (newconf:gen_configuration) = 
   let tag = true,[newconf]
   node.Tag <- tag;;


let TreeNode_get_path (node:TreeNode) =
   let rec aux (node:TreeNode) = 
     // incompatible with Mono
     //if node.Level = 0 then
     if node.Parent = null then
       []
     else 
       (aux node.Parent)@[node]
   in
     aux node
;;
         
type CpdaForm = 
  class
    inherit Form as base

    val mutable components: System.ComponentModel.Container;
    override this.Dispose(disposing) =
        if (disposing && (match this.components with null -> false | _ -> true)) then
          this.components.Dispose();
        base.Dispose(disposing)
        
    member this.selectTreeViewNodeSourceline() =
      let node = this.valueTreeView.SelectedNode
      if is_configuration_treeviewnode node then
          match cpda_conf_from_treeviewnode node with 
              TmState(_),_ -> ();
            | State(ip),_ -> RichTextbox_SelectLine this.cumul_linelength this.codeRichTextBox (ip+this.codestartline);
    
    member this.validate_valuetree_path (node:TreeNode) =
      if is_configuration_treeviewnode node then
          let nodepath = TreeNode_get_path (if is_cpda_alive_at_treeviewnode node then node.Parent else node)
          let path = List.map (function (n:TreeNode) -> n.Text) nodepath
          let v,c = this.cpda.cpda_path_validator path
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

    member this.expand_selected_treeviewconfiguration (node:TreeNode) =
        if is_cpda_alive_at_treeviewnode node then 
            let conf = cpda_conf_from_treeviewnode node
            try 
                  let newconf = hocpda_step this.cpda conf
                  add_conf_to_treeviewnode_history node newconf;
                  this.selectTreeViewNodeSourceline();
                  this.confhistoryTextBox.Text <- String.concat "\n\n" (List.map string_of_genconfiguration (cpda_confhistory_from_treeviewnode node));                  
                  match newconf with 
                    State(ip),stk ->  node.Text <- string_of_int ip;
                                      this.validate_valuetree_path node;
                                      true;
                  | TmState(f,ql),stk ->  node.Text <- f;
                                          node.ImageKey <- "BookClosed";
                                          node.SelectedImageKey <- "BookClosed";
                                          List.iter (function ip_param -> let newNode = new TreeNode((string_of_int ip_param), ImageKey = "Help", SelectedImageKey = "Help")
                                                                          let conf = State(ip_param),stk
                                                                          init_treeviewnode_history newNode conf;
                                                                          ignore(node.Nodes.Add(newNode));
                                                    ) ql;
                                          node.Expand();
                                          this.validate_valuetree_path node;
                                          this.runButton.Enabled <- is_cpda_alive_at_treeviewnode node;
                                          this.stepButton.Enabled <- is_cpda_alive_at_treeviewnode node;                                          
                                          false;
                  
            with CpdaHalt -> node.Tag <- null;
                             node.Text <- "halted";
                             node.ImageKey <- "BookClosed";
                             node.SelectedImageKey <- "BookClosed";
                             this.validate_valuetree_path node;
                             this.selectTreeViewNodeSourceline();
                             false;
        else
          false;
        

    member this.InitializeComponent() =
        this.components <- new System.ComponentModel.Container();
        this.outerSplitContainer <- new System.Windows.Forms.SplitContainer();
        this.valueTreeView <- new System.Windows.Forms.TreeView();
        this.imageList <- new System.Windows.Forms.ImageList(this.components);
        this.treeviewheadLabel <- new System.Windows.Forms.Label();
        this.rightContainer <- new System.Windows.Forms.SplitContainer();
        this.rightUpperSplitContainer <- new System.Windows.Forms.SplitContainer();
        this.pathTextBox <- new System.Windows.Forms.TextBox();
        this.pathLabel <- new System.Windows.Forms.Label();
        this.codeRichTextBox <- new System.Windows.Forms.RichTextBox();
        this.codeLabel <- new System.Windows.Forms.Label();
        this.runButton <- new System.Windows.Forms.Button();
        this.stepButton <- new System.Windows.Forms.Button();
        this.confhistoryTextBox <- new System.Windows.Forms.RichTextBox();
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
        // 
        // outerSplitContainer
        // 
        this.outerSplitContainer.Dock <- System.Windows.Forms.DockStyle.Fill;
        this.outerSplitContainer.FixedPanel <- System.Windows.Forms.FixedPanel.Panel1;
        this.outerSplitContainer.Location <- new System.Drawing.Point(0, 0);
        this.outerSplitContainer.Name <- "outerSplitContainer";
        // 
        // outerSplitContainer.Panel1
        // 
        this.outerSplitContainer.Panel1.Controls.Add(this.valueTreeView);
        this.outerSplitContainer.Panel1.Controls.Add(this.treeviewheadLabel);
        // 
        // outerSplitContainer.Panel2
        // 
        this.outerSplitContainer.Panel2.Controls.Add(this.rightContainer);
        this.outerSplitContainer.Size <- new System.Drawing.Size(952, 682);
        this.outerSplitContainer.SplitterDistance <- 268;
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
        this.valueTreeView.PathSeparator <- " ";
        // Treeview..ShowNodeToolTips Not implemented in  Mono
        if not Common.IsRunningOnMono then
            (); // this.valueTreeView.ShowNodeToolTips <- true;
        
        this.valueTreeView.ShowRootLines <- false;
        this.valueTreeView.Size <- new System.Drawing.Size(266, 654);
        this.valueTreeView.TabIndex <- 1;
        this.valueTreeView.add_KeyPress(fun  _ k ->  if k.KeyChar = ' ' then 
                                                        ignore(this.expand_selected_treeviewconfiguration this.valueTreeView.SelectedNode)
                                       );
        this.valueTreeView.add_NodeMouseDoubleClick(fun  _ e ->
                ignore(this.expand_selected_treeviewconfiguration e.Node);
              );
        this.valueTreeView.add_GotFocus(fun  _ _ -> 
               this.selectTreeViewNodeSourceline();
              );
        this.valueTreeView.add_BeforeCollapse(fun _ e -> 
              // e.Node.Level is incompatible with Mono
              if e.Node.Parent = null then
                e.Cancel <- true;
              );

        this.valueTreeView.add_AfterSelect(fun _ e -> 
            let currentNode = this.valueTreeView.SelectedNode  
            this.runButton.Enabled <- is_cpda_alive_at_treeviewnode currentNode;
            this.stepButton.Enabled <- is_cpda_alive_at_treeviewnode currentNode;
            this.validate_valuetree_path currentNode;
            this.selectTreeViewNodeSourceline();
            if is_configuration_treeviewnode currentNode then
                this.confhistoryTextBox.Text <- String.concat "\n\n" (List.map string_of_genconfiguration (cpda_confhistory_from_treeviewnode e.Node));
            else
                this.confhistoryTextBox.Clear();
            );
              
        // 
        // imageList
        // 
        
        this.imageList.Images.Add((GUI.Properties.Resources.roundquestionmark:>System.Drawing.Image));
        this.imageList.Images.Add((GUI.Properties.Resources.books:>System.Drawing.Image));
        this.imageList.Images.Add((GUI.Properties.Resources.closedbook:>System.Drawing.Image));
        this.imageList.Images.Add((GUI.Properties.Resources.openbook:>System.Drawing.Image));
        this.imageList.Images.Add((GUI.Properties.Resources.questionmark:>System.Drawing.Image));
        this.imageList.Images.Add((GUI.Properties.Resources.run:>System.Drawing.Image));
        this.imageList.Images.SetKeyName(0, "Help");
        this.imageList.Images.SetKeyName(1, "BookStack");
        this.imageList.Images.SetKeyName(2, "BookClosed");
        this.imageList.Images.SetKeyName(3, "BookOpen");
        this.imageList.Images.SetKeyName(4, "Item");
        this.imageList.Images.SetKeyName(5, "Run");

        
        // 
        // treeviewheadLabel
        // 
        this.treeviewheadLabel.AutoSize <- true;
        this.treeviewheadLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.treeviewheadLabel.Location <- new System.Drawing.Point(3, 9);
        this.treeviewheadLabel.Name <- "treeviewheadLabel";
        this.treeviewheadLabel.Size <- new System.Drawing.Size(58, 16);
        this.treeviewheadLabel.TabIndex <- 0;
        this.treeviewheadLabel.Text <- "Value tree:";
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
        this.rightContainer.Panel2.Controls.Add(this.stepButton);
        this.rightContainer.Panel2.Controls.Add(this.confhistoryTextBox);
        this.rightContainer.Panel2.Controls.Add(this.outputLabel);
        this.rightContainer.Size <- new System.Drawing.Size(680, 682);
        this.rightContainer.SplitterDistance <- 550;
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
        this.rightUpperSplitContainer.SplitterDistance <- 50;
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
        this.pathTextBox.Size <- new System.Drawing.Size(150, 25);
        this.pathTextBox.TabIndex <- 1;
        this.pathTextBox.WordWrap <- true;
        //this.pathTextBox.Text <- "Double-click on a node of the treeview to execute the corresponding transition of the CPDA.";
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
        this.codeRichTextBox.Size <- new System.Drawing.Size(680, 500);
        this.codeRichTextBox.TabIndex <- 1;
        this.codeRichTextBox.Text <- string_of_hocpda this.cpda ;
        this.codeRichTextBox.Dock <- DockStyle.Fill;
        this.codeRichTextBox.WordWrap <- false;
        this.codeRichTextBox.HideSelection <- false;
        // compute lines length and the position where the code dump start
        this.cumul_linelength <- (RichTextBox_CumulLinesLength this.codeRichTextBox);
        this.codestartline <- 2+this.codeRichTextBox.GetLineFromCharIndex(this.codeRichTextBox.Find("Code:"))
        
        // 
        // codeLabel
        // 
        this.codeLabel.AutoSize <- true;
        this.codeLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.codeLabel.Location <- new System.Drawing.Point(3, -1);
        this.codeLabel.Name <- "codeLabel";
        this.codeLabel.Size <- new System.Drawing.Size(38, 16);
        this.codeLabel.TabIndex <- 0;
        this.codeLabel.Text <- "CPDA description:";
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
        this.runButton.Size <- new System.Drawing.Size(100, 27);
        this.runButton.TabIndex <- 0;
        this.runButton.Text <- " Run!";
        this.runButton.TextImageRelation <- System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.runButton.Click.Add(fun e -> let node = this.valueTreeView.SelectedNode
                                          while this.expand_selected_treeviewconfiguration node do () done;
        );        
         

        // 
        // stepButton
        // 
        this.stepButton.Enabled <- true;
        this.stepButton.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.stepButton.ImageAlign <- System.Drawing.ContentAlignment.MiddleRight;
        this.stepButton.ImageKey <- "Run";
        this.stepButton.ImageList <- this.imageList;
        this.stepButton.Location <- new System.Drawing.Point(110, -1);
        this.stepButton.Name <- "runButton";
        this.stepButton.Size <- new System.Drawing.Size(100, 27);
        this.stepButton.TabIndex <- 0;
        this.stepButton.Text <- " Step";
        this.stepButton.TextImageRelation <- System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.stepButton.Click.Add(fun e -> let node = this.valueTreeView.SelectedNode
                                           ignore(this.expand_selected_treeviewconfiguration node);
        );        

        // 
        // confhistoryTextBox
        // 
        this.confhistoryTextBox.Anchor <- 
          Enum.combine [ System.Windows.Forms.AnchorStyles.Top;
                    System.Windows.Forms.AnchorStyles.Bottom;
                    System.Windows.Forms.AnchorStyles.Left;
                    System.Windows.Forms.AnchorStyles.Right ];
        this.confhistoryTextBox.BackColor <- System.Drawing.SystemColors.ControlLight;
        this.confhistoryTextBox.Font <- new System.Drawing.Font("Lucida Console", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.confhistoryTextBox.Location <- new System.Drawing.Point(0, 48);
        this.confhistoryTextBox.Multiline <- true;
        this.confhistoryTextBox.Name <- "confhistoryTextBox";
        this.confhistoryTextBox.ReadOnly <- true;
        this.confhistoryTextBox.ScrollBars <- System.Windows.Forms.RichTextBoxScrollBars.Both;
        this.confhistoryTextBox.Size <- new System.Drawing.Size(680, 70);
        this.confhistoryTextBox.TabIndex <- 2;
        this.confhistoryTextBox.WordWrap <- true;
        // 
        // outputLabel
        // 
        this.outputLabel.AutoSize <- true;
        this.outputLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.outputLabel.Location <- new System.Drawing.Point(3, 29);
        this.outputLabel.Name <- "outputLabel";
        this.outputLabel.Size <- new System.Drawing.Size(47, 16);
        this.outputLabel.TabIndex <- 1;
        this.outputLabel.Text <- "Configuration at the selected node:";
        // 
        // DisplayForm
        // 
        this.AcceptButton <- this.runButton;
        this.AutoScaleDimensions <- new System.Drawing.SizeF(6.0F, 13.0F);
        this.AutoScaleMode <- System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize <- new System.Drawing.Size(952, 682);
        this.Controls.Add(this.outerSplitContainer);
        this.Font <- new System.Drawing.Font("Tahoma", 8.25F);        
        //this.Icon <- (GUI.Properties.Resources.app:>System.Drawing.Icon)
        this.Name <- "DisplayForm";
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
    val mutable treeviewheadLabel : System.Windows.Forms.Label;
    val mutable rightContainer : System.Windows.Forms.SplitContainer;
    val mutable confhistoryTextBox : System.Windows.Forms.RichTextBox;
    val mutable outputLabel : System.Windows.Forms.Label;
    val mutable runButton : System.Windows.Forms.Button;
    val mutable stepButton : System.Windows.Forms.Button;
    val mutable rightUpperSplitContainer : System.Windows.Forms.SplitContainer;
    val mutable pathTextBox : System.Windows.Forms.TextBox;
    val mutable pathLabel : System.Windows.Forms.Label;
    val mutable codeLabel : System.Windows.Forms.Label;
    val mutable valueTreeView : System.Windows.Forms.TreeView;
    val mutable imageList : System.Windows.Forms.ImageList;
    val mutable codeRichTextBox : System.Windows.Forms.RichTextBox
    val mutable cpda : Hocpda.hocpda
    val mutable codestartline : int
    val mutable cumul_linelength : int array

    new (title, newcpda) as this =
       { outerSplitContainer = null;
         treeviewheadLabel = null;
         rightContainer = null;
         confhistoryTextBox = null;
         outputLabel =null;
         runButton =null;
         stepButton =null;
         rightUpperSplitContainer =null;
         pathTextBox =null;
         pathLabel = null;
         codeLabel = null;
         valueTreeView = null;
         imageList = null;
         codeRichTextBox = null;
         components = null;
         cpda = newcpda;
         codestartline = 0;
         cumul_linelength = [||];
       }
       
       then 
        this.InitializeComponent();
        this.Text <- title;

        let rootNode = new TreeNode(title, Tag = (null : obj), ImageKey = "BookStack", SelectedImageKey = "BookStack")
        ignore(this.valueTreeView.Nodes.Add(rootNode));
        rootNode.Expand();
        

        let SNode = new TreeNode("0")
        SNode.ImageKey <- "Help";
        SNode.SelectedImageKey <- "Help";
        init_treeviewnode_history SNode (hocpda_initconf this.cpda);
        ignore(rootNode.Nodes.Add(SNode));
        this.valueTreeView.SelectedNode <- SNode;
        
  end
  