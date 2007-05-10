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
type CpdaForm = 
  class
    inherit Form as base

    val mutable components: System.ComponentModel.Container;
    override this.Dispose(disposing) =
        if (disposing && (match this.components with null -> false | _ -> true)) then
          this.components.Dispose();
        base.Dispose(disposing)
        
    member this.expand_selected_treeviewconfiguration (node:TreeNode) =
        let conf = (node.Tag:?>gen_configuration)
        try 
              let newconf = hocpda_step this.cpda conf
              this.outputTextBox.Text <- string_of_genconfiguration newconf;
              match newconf with 
                State(ip),stk ->  node.Tag <- newconf;
                                  node.Text <- string_of_int ip;
                                  false;
              | TmState(f,ql),stk ->  node.Text <- f;
                                      node.ImageKey <- "BookClosed";
                                      node.SelectedImageKey <- "BookClosed";
                                      node.Tag <- null;
                                      List.iter (function ip -> let newNode = new TreeNode((string_of_int ip), ImageKey = "Help", SelectedImageKey = "Help")
                                                                let conf = State(ip),stk
                                                                newNode.Tag <- conf;
                                                                ignore(node.Nodes.Add(newNode));
                                                                ) ql;
                                      node.Expand();
                                      true;
              
        with CpdaHalt -> node.Tag <- null;
                         node.Text <- "halted";
                         node.ImageKey <- "BookClosed";
                         node.SelectedImageKey <- "BookClosed";
                         false;


    member this.InitializeComponent() =
        this.components <- new System.ComponentModel.Container();
        let resources = new System.ComponentModel.ComponentResourceManager((type CpdaForm)) 
        this.outerSplitContainer <- new System.Windows.Forms.SplitContainer();
        this.samplesTreeView <- new System.Windows.Forms.TreeView();
        this.imageList <- new System.Windows.Forms.ImageList(this.components);
        this.samplesLabel <- new System.Windows.Forms.Label();
        this.rightContainer <- new System.Windows.Forms.SplitContainer();
        this.rightUpperSplitContainer <- new System.Windows.Forms.SplitContainer();
        this.descriptionTextBox <- new System.Windows.Forms.TextBox();
        this.descriptionLabel <- new System.Windows.Forms.Label();
        this.codeRichTextBox <- new System.Windows.Forms.RichTextBox();
        this.codeLabel <- new System.Windows.Forms.Label();
        this.runButton <- new System.Windows.Forms.Button();
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
        this.outerSplitContainer.Panel1.Controls.Add(this.samplesTreeView);
        this.outerSplitContainer.Panel1.Controls.Add(this.samplesLabel);
        // 
        // outerSplitContainer.Panel2
        // 
        this.outerSplitContainer.Panel2.Controls.Add(this.rightContainer);
        this.outerSplitContainer.Size <- new System.Drawing.Size(952, 682);
        this.outerSplitContainer.SplitterDistance <- 268;
        this.outerSplitContainer.TabIndex <- 0;
        // 
        // samplesTreeView
        // 
        this.samplesTreeView.Anchor <- 
          Enum.combine [ System.Windows.Forms.AnchorStyles.Top; 
                        System.Windows.Forms.AnchorStyles.Bottom;
                        System.Windows.Forms.AnchorStyles.Left;
                        System.Windows.Forms.AnchorStyles.Right];
        this.samplesTreeView.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.samplesTreeView.HideSelection <- false;
        this.samplesTreeView.ImageKey <- "";
        this.samplesTreeView.SelectedImageKey <- "";
        this.samplesTreeView.ImageList <- this.imageList;
        this.samplesTreeView.Location <- new System.Drawing.Point(0, 28);
        this.samplesTreeView.Name <- "samplesTreeView";
        this.samplesTreeView.ShowNodeToolTips <- true;
        this.samplesTreeView.ShowRootLines <- false;
        this.samplesTreeView.Size <- new System.Drawing.Size(266, 654);
        this.samplesTreeView.TabIndex <- 1;
        //this.samplesTreeView.add_AfterExpand(fun _ e -> 
        this.samplesTreeView.add_NodeMouseDoubleClick(fun  _ e -> 
              match e.Node.Level with 
              | 0  -> ()
              | _ when (e.Node.Tag<> null) -> ignore(this.expand_selected_treeviewconfiguration e.Node)
              | _ -> ();
              );
              
                        
        this.samplesTreeView.add_BeforeCollapse(fun _ e -> 
              match e.Node.Level with 
              | 0 -> 
                e.Cancel <- true;
              | _ -> ());
            
        this.samplesTreeView.add_AfterSelect(fun _ e -> 
            let currentNode = this.samplesTreeView.SelectedNode  
            this.runButton.Enabled <- (currentNode.Tag<>null);
            match currentNode.Tag with 
            | null ->                                 
                this.descriptionTextBox.Text <- "Double-click on a node of the treeview to execute the corresponding transition of the CPDA.";
                this.outputTextBox.Clear();
                if (e.Action <> TreeViewAction.Collapse && e.Action <> TreeViewAction.Unknown) then
                    e.Node.Expand();
            | _ ->  this.outputTextBox.Text <- (string_of_genconfiguration (e.Node.Tag:?>gen_configuration));
            );
              
     (*   this.samplesTreeView.add_AfterCollapse(fun _ e -> 
          match e.Node.Level with 
          | 1 -> 
            e.Node.ImageKey <- "BookStack";
            e.Node.SelectedImageKey <- "BookStack";
          |  2 ->
            e.Node.ImageKey <- "BookClosed";
            e.Node.SelectedImageKey <- "BookClosed"
          | _ -> ());
*)
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
        // samplesLabel
        // 
        this.samplesLabel.AutoSize <- true;
        this.samplesLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.samplesLabel.Location <- new System.Drawing.Point(3, 9);
        this.samplesLabel.Name <- "samplesLabel";
        this.samplesLabel.Size <- new System.Drawing.Size(58, 16);
        this.samplesLabel.TabIndex <- 0;
        this.samplesLabel.Text <- "Value tree:";
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
        this.rightUpperSplitContainer.Panel1.Controls.Add(this.descriptionTextBox);
        this.rightUpperSplitContainer.Panel1.Controls.Add(this.descriptionLabel);
        // 
        // rightUpperSplitContainer.Panel2
        // 
        this.rightUpperSplitContainer.Panel2.Controls.Add(this.codeRichTextBox);
        this.rightUpperSplitContainer.Panel2.Controls.Add(this.codeLabel);
        this.rightUpperSplitContainer.Size <- new System.Drawing.Size(680, 357);
        this.rightUpperSplitContainer.SplitterDistance <- 95;
        this.rightUpperSplitContainer.TabIndex <- 0;
        // 
        // descriptionTextBox
        // 
        this.descriptionTextBox.Anchor<- 
          Enum.combine [ System.Windows.Forms.AnchorStyles.Top;
                    System.Windows.Forms.AnchorStyles.Bottom;
                    System.Windows.Forms.AnchorStyles.Left;
                    System.Windows.Forms.AnchorStyles.Right ];
        this.descriptionTextBox.BackColor <- System.Drawing.SystemColors.ControlLight;
        this.descriptionTextBox.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.descriptionTextBox.Location <- new System.Drawing.Point(0, 28);
        this.descriptionTextBox.Multiline <- true;
        this.descriptionTextBox.Name <- "descriptionTextBox";
        this.descriptionTextBox.ReadOnly <- true;
        this.descriptionTextBox.ScrollBars <- System.Windows.Forms.ScrollBars.Vertical;
        this.descriptionTextBox.Size <- new System.Drawing.Size(680, 67);
        this.descriptionTextBox.TabIndex <- 1;
        // 
        // descriptionLabel
        // 
        this.descriptionLabel.AutoSize <- true;
        this.descriptionLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.descriptionLabel.Location <- new System.Drawing.Point(3, 9);
        this.descriptionLabel.Name <- "descriptionLabel";
        this.descriptionLabel.Size <- new System.Drawing.Size(72, 16);
        this.descriptionLabel.TabIndex <- 0;
        this.descriptionLabel.Text <- "Tip:";
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
        this.codeRichTextBox.Text <- string_of_hocpda this.cpda ;
        //this.codeRichTextBox.Dock <- DockStyle.Fill;
        this.codeRichTextBox.WordWrap <- false;
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
        this.runButton.Click.Add(fun e -> let node = this.samplesTreeView.SelectedNode
                                          if node.Tag<> null then
                                             while not (this.expand_selected_treeviewconfiguration node) do () done;
        );        
         (*
              this.UseWaitCursor <- true;
              try 
                this.outputTextBox.Text <- "";
                let stream = new MemoryStream()  
                let writer = new StreamWriter(stream)  
                stream.SetLength(0L);
                writer.Flush();
                this.outputTextBox.Text <- this.outputTextBox.Text + writer.Encoding.GetString(stream.ToArray());
              finally
                this.UseWaitCursor <- false);  *)
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
        this.Icon <- (resources.GetObject("$this.Icon") :?> System.Drawing.Icon);
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
    val mutable samplesLabel : System.Windows.Forms.Label;
    val mutable rightContainer : System.Windows.Forms.SplitContainer;
    val mutable outputTextBox : System.Windows.Forms.RichTextBox;
    val mutable outputLabel : System.Windows.Forms.Label;
    val mutable runButton : System.Windows.Forms.Button;
    val mutable rightUpperSplitContainer : System.Windows.Forms.SplitContainer;
    val mutable descriptionTextBox : System.Windows.Forms.TextBox;
    val mutable descriptionLabel : System.Windows.Forms.Label;
    val mutable codeLabel : System.Windows.Forms.Label;
    val mutable samplesTreeView : System.Windows.Forms.TreeView;
    val mutable imageList : System.Windows.Forms.ImageList;
    val mutable codeRichTextBox : System.Windows.Forms.RichTextBox
    val mutable cpda : Hocpda.hocpda

    new (title, newcpda, initconf:gen_configuration) as this =
       { outerSplitContainer = null;
         samplesLabel = null;
         rightContainer = null;
         outputTextBox = null;
         outputLabel =null;
         runButton =null;
         rightUpperSplitContainer =null;
         descriptionTextBox =null;
         descriptionLabel = null;
         codeLabel = null;
         samplesTreeView = null;
         imageList = null;
         codeRichTextBox = null;
         components = null;
         cpda = newcpda;
       }
       
       then 
        this.InitializeComponent();
        this.Text <- title;

        let rootNode = new TreeNode(title, Tag = (null : obj), ImageKey = "BookStack", SelectedImageKey = "BookStack")
        ignore(this.samplesTreeView.Nodes.Add(rootNode));
        rootNode.Expand();
      

        let SNode = new TreeNode("0")
        SNode.ImageKey <- "Help";
        SNode.SelectedImageKey <- "Help";
        SNode.Tag <- initconf;
        ignore(rootNode.Nodes.Add(SNode));
  end
  