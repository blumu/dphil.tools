namespace GUI
{
    public partial class Traversal
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.seqflowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.gViewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            this.btSubtermProj = new System.Windows.Forms.Button();
            this.btHerProj = new System.Windows.Forms.Button();
            this.btExportSeq = new System.Windows.Forms.Button();
            this.btExportWS = new System.Windows.Forms.Button();
            this.grpNode = new System.Windows.Forms.GroupBox();
            this.btAdd = new System.Windows.Forms.Button();
            this.btBackspace = new System.Windows.Forms.Button();
            this.btEditLabel = new System.Windows.Forms.Button();
            this.grpSeq = new System.Windows.Forms.GroupBox();
            this.btExt = new System.Windows.Forms.Button();
            this.btStar = new System.Windows.Forms.Button();
            this.btPrefix = new System.Windows.Forms.Button();
            this.btDuplicate = new System.Windows.Forms.Button();
            this.btNew = new System.Windows.Forms.Button();
            this.btDelete = new System.Windows.Forms.Button();
            this.btPview = new System.Windows.Forms.Button();
            this.btOview = new System.Windows.Forms.Button();
            this.grpWS = new System.Windows.Forms.GroupBox();
            this.btSave = new System.Windows.Forms.Button();
            this.btImport = new System.Windows.Forms.Button();
            this.btExportGraph = new System.Windows.Forms.Button();
            this.grpLatex = new System.Windows.Forms.GroupBox();
            this.btPlay = new System.Windows.Forms.Button();
            this.grpTrav = new System.Windows.Forms.GroupBox();
            this.btBetaReduce = new System.Windows.Forms.Button();
            this.labGameInfo = new System.Windows.Forms.Label();
            this.btNewGame = new System.Windows.Forms.Button();
            this.btUndo = new System.Windows.Forms.Button();
            this.btPlayAllOMoves = new System.Windows.Forms.Button();
            this.chkInplace = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grpNode.SuspendLayout();
            this.grpSeq.SuspendLayout();
            this.grpWS.SuspendLayout();
            this.grpLatex.SuspendLayout();
            this.grpTrav.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Size = new System.Drawing.Size(178, 125);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Location = new System.Drawing.Point(0, 90);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.seqflowPanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.gViewer);
            this.splitContainer1.Size = new System.Drawing.Size(1010, 393);
            this.splitContainer1.SplitterDistance = 580;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 13;
            // 
            // seqflowPanel
            // 
            this.seqflowPanel.AutoScroll = true;
            this.seqflowPanel.BackColor = System.Drawing.SystemColors.Window;
            this.seqflowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.seqflowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.seqflowPanel.Location = new System.Drawing.Point(0, 0);
            this.seqflowPanel.Name = "seqflowPanel";
            this.seqflowPanel.Size = new System.Drawing.Size(639, 389);
            this.seqflowPanel.TabIndex = 0;
            this.seqflowPanel.TabStop = true;
            this.seqflowPanel.WrapContents = false;
            // 
            // gViewer
            // 
            this.gViewer.AsyncLayout = false;
            this.gViewer.AutoScroll = true;
            this.gViewer.BackwardEnabled = false;
            this.gViewer.BuildHitTree = true;
            this.gViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gViewer.ForwardEnabled = false;
            this.gViewer.Graph = null;
            this.gViewer.Location = new System.Drawing.Point(0, 0);
            this.gViewer.MouseHitDistance = 0.05;
            this.gViewer.Name = "gViewer";
            this.gViewer.NavigationVisible = true;
            this.gViewer.NeedToCalculateLayout = true;
            this.gViewer.PanButtonPressed = false;
            this.gViewer.SaveButtonVisible = true;
            this.gViewer.Size = new System.Drawing.Size(200, 200);
            this.gViewer.TabIndex = 0;
            this.gViewer.ZoomF = 1;
            this.gViewer.ZoomWindowThreshold = 0.05;
            // 
            // btSubtermProj
            // 
            this.btSubtermProj.Location = new System.Drawing.Point(246, 52);
            this.btSubtermProj.Margin = new System.Windows.Forms.Padding(2);
            this.btSubtermProj.Name = "btSubtermProj";
            this.btSubtermProj.Size = new System.Drawing.Size(118, 28);
            this.btSubtermProj.TabIndex = 9;
            this.btSubtermProj.Text = "&Subterm projection";
            this.btSubtermProj.UseVisualStyleBackColor = true;
            // 
            // btHerProj
            // 
            this.btHerProj.Location = new System.Drawing.Point(246, 18);
            this.btHerProj.Margin = new System.Windows.Forms.Padding(2);
            this.btHerProj.Name = "btHerProj";
            this.btHerProj.Size = new System.Drawing.Size(118, 28);
            this.btHerProj.TabIndex = 8;
            this.btHerProj.Text = "&Hereditary projection";
            this.btHerProj.UseVisualStyleBackColor = true;
            // 
            // btExportSeq
            // 
            this.btExportSeq.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btExportSeq.FlatAppearance.BorderSize = 5;
            this.btExportSeq.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.btExportSeq.Location = new System.Drawing.Point(5, 18);
            this.btExportSeq.Margin = new System.Windows.Forms.Padding(2);
            this.btExportSeq.Name = "btExportSeq";
            this.btExportSeq.Size = new System.Drawing.Size(74, 28);
            this.btExportSeq.TabIndex = 0;
            this.btExportSeq.Text = "Se&quence...";
            this.btExportSeq.UseVisualStyleBackColor = true;
            // 
            // btExportWS
            // 
            this.btExportWS.Location = new System.Drawing.Point(83, 18);
            this.btExportWS.Margin = new System.Windows.Forms.Padding(2);
            this.btExportWS.Name = "btExportWS";
            this.btExportWS.Size = new System.Drawing.Size(77, 28);
            this.btExportWS.TabIndex = 2;
            this.btExportWS.Text = "&Worksheet...";
            this.btExportWS.UseVisualStyleBackColor = true;
            // 
            // grpNode
            // 
            this.grpNode.Controls.Add(this.btAdd);
            this.grpNode.Controls.Add(this.btBackspace);
            this.grpNode.Controls.Add(this.btEditLabel);
            this.grpNode.Location = new System.Drawing.Point(97, 0);
            this.grpNode.Name = "grpNode";
            this.grpNode.Size = new System.Drawing.Size(108, 84);
            this.grpNode.TabIndex = 1;
            this.grpNode.TabStop = false;
            this.grpNode.Text = "Node operations";
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(7, 18);
            this.btAdd.Margin = new System.Windows.Forms.Padding(2);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(31, 28);
            this.btAdd.TabIndex = 0;
            this.btAdd.Text = "&+";
            this.btAdd.UseVisualStyleBackColor = true;
            // 
            // btBackspace
            // 
            this.btBackspace.Location = new System.Drawing.Point(7, 51);
            this.btBackspace.Margin = new System.Windows.Forms.Padding(2);
            this.btBackspace.Name = "btBackspace";
            this.btBackspace.Size = new System.Drawing.Size(31, 28);
            this.btBackspace.TabIndex = 1;
            this.btBackspace.Text = "<&-";
            this.btBackspace.UseVisualStyleBackColor = true;
            // 
            // btEditLabel
            // 
            this.btEditLabel.Location = new System.Drawing.Point(42, 52);
            this.btEditLabel.Margin = new System.Windows.Forms.Padding(2);
            this.btEditLabel.Name = "btEditLabel";
            this.btEditLabel.Size = new System.Drawing.Size(59, 28);
            this.btEditLabel.TabIndex = 2;
            this.btEditLabel.Text = "Edit &label";
            this.btEditLabel.UseVisualStyleBackColor = true;
            // 
            // grpSeq
            // 
            this.grpSeq.Controls.Add(this.chkInplace);
            this.grpSeq.Controls.Add(this.btExt);
            this.grpSeq.Controls.Add(this.btStar);
            this.grpSeq.Controls.Add(this.btPrefix);
            this.grpSeq.Controls.Add(this.btDuplicate);
            this.grpSeq.Controls.Add(this.btNew);
            this.grpSeq.Controls.Add(this.btSubtermProj);
            this.grpSeq.Controls.Add(this.btDelete);
            this.grpSeq.Controls.Add(this.btHerProj);
            this.grpSeq.Controls.Add(this.btPview);
            this.grpSeq.Controls.Add(this.btOview);
            this.grpSeq.Location = new System.Drawing.Point(211, 1);
            this.grpSeq.Name = "grpSeq";
            this.grpSeq.Size = new System.Drawing.Size(445, 85);
            this.grpSeq.TabIndex = 2;
            this.grpSeq.TabStop = false;
            this.grpSeq.Text = "Sequence operations";
            // 
            // btExt
            // 
            this.btExt.Location = new System.Drawing.Point(178, 52);
            this.btExt.Name = "btExt";
            this.btExt.Size = new System.Drawing.Size(64, 28);
            this.btExt.TabIndex = 7;
            this.btExt.Text = "E&xtension";
            this.btExt.UseVisualStyleBackColor = true;
            // 
            // btStar
            // 
            this.btStar.Location = new System.Drawing.Point(178, 18);
            this.btStar.Name = "btStar";
            this.btStar.Size = new System.Drawing.Size(64, 28);
            this.btStar.TabIndex = 6;
            this.btStar.Text = "St&ar";
            this.btStar.UseVisualStyleBackColor = true;
            // 
            // btPrefix
            // 
            this.btPrefix.Location = new System.Drawing.Point(113, 52);
            this.btPrefix.Margin = new System.Windows.Forms.Padding(2);
            this.btPrefix.Name = "btPrefix";
            this.btPrefix.Size = new System.Drawing.Size(61, 28);
            this.btPrefix.TabIndex = 5;
            this.btPrefix.Text = "P&refix";
            this.btPrefix.UseVisualStyleBackColor = true;
            // 
            // btDuplicate
            // 
            this.btDuplicate.Location = new System.Drawing.Point(113, 18);
            this.btDuplicate.Margin = new System.Windows.Forms.Padding(2);
            this.btDuplicate.Name = "btDuplicate";
            this.btDuplicate.Size = new System.Drawing.Size(61, 28);
            this.btDuplicate.TabIndex = 4;
            this.btDuplicate.Text = "D&uplicate";
            this.btDuplicate.UseVisualStyleBackColor = true;
            // 
            // btNew
            // 
            this.btNew.Location = new System.Drawing.Point(7, 18);
            this.btNew.Margin = new System.Windows.Forms.Padding(2);
            this.btNew.Name = "btNew";
            this.btNew.Size = new System.Drawing.Size(46, 28);
            this.btNew.TabIndex = 0;
            this.btNew.Text = "&New";
            this.btNew.UseVisualStyleBackColor = true;
            // 
            // btDelete
            // 
            this.btDelete.Location = new System.Drawing.Point(7, 52);
            this.btDelete.Margin = new System.Windows.Forms.Padding(2);
            this.btDelete.Name = "btDelete";
            this.btDelete.Size = new System.Drawing.Size(46, 28);
            this.btDelete.TabIndex = 1;
            this.btDelete.Text = "&Delete";
            this.btDelete.UseVisualStyleBackColor = true;
            // 
            // btPview
            // 
            this.btPview.Location = new System.Drawing.Point(57, 52);
            this.btPview.Margin = new System.Windows.Forms.Padding(2);
            this.btPview.Name = "btPview";
            this.btPview.Size = new System.Drawing.Size(52, 28);
            this.btPview.TabIndex = 3;
            this.btPview.Text = "&P-View";
            this.btPview.UseVisualStyleBackColor = true;
            // 
            // btOview
            // 
            this.btOview.Location = new System.Drawing.Point(57, 18);
            this.btOview.Margin = new System.Windows.Forms.Padding(2);
            this.btOview.Name = "btOview";
            this.btOview.Size = new System.Drawing.Size(52, 28);
            this.btOview.TabIndex = 2;
            this.btOview.Text = "&O-View";
            this.btOview.UseVisualStyleBackColor = true;
            // 
            // grpWS
            // 
            this.grpWS.Controls.Add(this.btSave);
            this.grpWS.Controls.Add(this.btImport);
            this.grpWS.Location = new System.Drawing.Point(9, 0);
            this.grpWS.Name = "grpWS";
            this.grpWS.Size = new System.Drawing.Size(82, 84);
            this.grpWS.TabIndex = 0;
            this.grpWS.TabStop = false;
            this.grpWS.Text = "Worksheet";
            // 
            // btSave
            // 
            this.btSave.Location = new System.Drawing.Point(14, 51);
            this.btSave.Margin = new System.Windows.Forms.Padding(2);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(55, 28);
            this.btSave.TabIndex = 1;
            this.btSave.Text = "Sa&ve...";
            this.btSave.UseVisualStyleBackColor = true;
            // 
            // btImport
            // 
            this.btImport.Location = new System.Drawing.Point(14, 18);
            this.btImport.Margin = new System.Windows.Forms.Padding(2);
            this.btImport.Name = "btImport";
            this.btImport.Size = new System.Drawing.Size(55, 28);
            this.btImport.TabIndex = 0;
            this.btImport.Text = "&Import...";
            this.btImport.UseVisualStyleBackColor = true;
            // 
            // btExportGraph
            // 
            this.btExportGraph.Location = new System.Drawing.Point(5, 49);
            this.btExportGraph.Margin = new System.Windows.Forms.Padding(2);
            this.btExportGraph.Name = "btExportGraph";
            this.btExportGraph.Size = new System.Drawing.Size(74, 28);
            this.btExportGraph.TabIndex = 1;
            this.btExportGraph.Text = "&Graph...";
            this.btExportGraph.UseVisualStyleBackColor = true;
            // 
            // grpLatex
            // 
            this.grpLatex.Controls.Add(this.btExportSeq);
            this.grpLatex.Controls.Add(this.btExportGraph);
            this.grpLatex.Controls.Add(this.btExportWS);
            this.grpLatex.Location = new System.Drawing.Point(930, 3);
            this.grpLatex.Name = "grpLatex";
            this.grpLatex.Size = new System.Drawing.Size(164, 84);
            this.grpLatex.TabIndex = 3;
            this.grpLatex.TabStop = false;
            this.grpLatex.Text = "Latex export";
            // 
            // btPlay
            // 
            this.btPlay.Location = new System.Drawing.Point(103, 19);
            this.btPlay.Name = "btPlay";
            this.btPlay.Size = new System.Drawing.Size(45, 28);
            this.btPlay.TabIndex = 14;
            this.btPlay.Text = "Pla&y!";
            this.btPlay.UseVisualStyleBackColor = true;
            this.btPlay.Visible = false;
            // 
            // grpTrav
            // 
            this.grpTrav.Controls.Add(this.btPlayAllOMoves);
            this.grpTrav.Controls.Add(this.btBetaReduce);
            this.grpTrav.Controls.Add(this.labGameInfo);
            this.grpTrav.Controls.Add(this.btNewGame);
            this.grpTrav.Controls.Add(this.btUndo);
            this.grpTrav.Controls.Add(this.btPlay);
            this.grpTrav.Location = new System.Drawing.Point(662, 3);
            this.grpTrav.Name = "grpTrav";
            this.grpTrav.Size = new System.Drawing.Size(262, 84);
            this.grpTrav.TabIndex = 15;
            this.grpTrav.TabStop = false;
            this.grpTrav.Text = "Traversal game";
            // 
            // btBetaReduce
            // 
            this.btBetaReduce.Location = new System.Drawing.Point(154, 47);
            this.btBetaReduce.Name = "btBetaReduce";
            this.btBetaReduce.Size = new System.Drawing.Size(98, 28);
            this.btBetaReduce.TabIndex = 16;
            this.btBetaReduce.Text = "&Beta-reduce";
            this.btBetaReduce.UseVisualStyleBackColor = true;
            // 
            // labGameInfo
            // 
            this.labGameInfo.BackColor = System.Drawing.SystemColors.Info;
            this.labGameInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labGameInfo.Location = new System.Drawing.Point(6, 51);
            this.labGameInfo.Name = "labGameInfo";
            this.labGameInfo.Size = new System.Drawing.Size(142, 26);
            this.labGameInfo.TabIndex = 15;
            this.labGameInfo.Text = "Press \'New\' to start a game";
            this.labGameInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btNewGame
            // 
            this.btNewGame.Location = new System.Drawing.Point(6, 19);
            this.btNewGame.Name = "btNewGame";
            this.btNewGame.Size = new System.Drawing.Size(43, 28);
            this.btNewGame.TabIndex = 14;
            this.btNewGame.Text = "N&ew";
            this.btNewGame.UseVisualStyleBackColor = true;
            // 
            // btUndo
            // 
            this.btUndo.Location = new System.Drawing.Point(55, 19);
            this.btUndo.Name = "btUndo";
            this.btUndo.Size = new System.Drawing.Size(43, 28);
            this.btUndo.TabIndex = 14;
            this.btUndo.Text = "&Undo";
            this.btUndo.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.btPlayAllOMoves.Location = new System.Drawing.Point(154, 17);
            this.btPlayAllOMoves.Name = "button1";
            this.btPlayAllOMoves.Size = new System.Drawing.Size(98, 28);
            this.btPlayAllOMoves.TabIndex = 16;
            this.btPlayAllOMoves.Text = "Play all &O-moves";
            this.btPlayAllOMoves.UseVisualStyleBackColor = true;
            // 
            // chkInplace
            // 
            this.chkInplace.AutoSize = true;
            this.chkInplace.Location = new System.Drawing.Point(373, 23);
            this.chkInplace.Name = "chkInplace";
            this.chkInplace.Size = new System.Drawing.Size(64, 17);
            this.chkInplace.TabIndex = 17;
            this.chkInplace.Text = "In pla&ce";
            this.chkInplace.UseVisualStyleBackColor = true;
            // 
            // Traversal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1018, 482);
            this.Controls.Add(this.grpTrav);
            this.Controls.Add(this.grpWS);
            this.Controls.Add(this.grpSeq);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.grpLatex);
            this.Controls.Add(this.grpNode);
            this.Name = "Traversal";
            this.Text = "Traversal calculator";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.grpNode.ResumeLayout(false);
            this.grpSeq.ResumeLayout(false);
            this.grpWS.ResumeLayout(false);
            this.grpLatex.ResumeLayout(false);
            this.grpTrav.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        public System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        public System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        public System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        public System.Windows.Forms.ToolStripContentPanel ContentPanel;
        public System.Windows.Forms.SplitContainer splitContainer1;
        public System.Windows.Forms.FlowLayoutPanel seqflowPanel;
        public Microsoft.Msagl.GraphViewerGdi.GViewer gViewer;
        public System.Windows.Forms.Button btExportWS;
        public System.Windows.Forms.Button btHerProj;
        public System.Windows.Forms.Button btSubtermProj;
        public System.Windows.Forms.Button btExportSeq;
        public System.Windows.Forms.Button btAdd;
        public System.Windows.Forms.Button btBackspace;
        public System.Windows.Forms.Button btEditLabel;
        public System.Windows.Forms.Button btDuplicate;
        public System.Windows.Forms.Button btNew;
        public System.Windows.Forms.Button btDelete;
        public System.Windows.Forms.Button btPview;
        public System.Windows.Forms.Button btOview;
        public System.Windows.Forms.Button btPrefix;
        public System.Windows.Forms.Button btExt;
        public System.Windows.Forms.Button btStar;
        public System.Windows.Forms.Button btSave;
        public System.Windows.Forms.Button btImport;
        public System.Windows.Forms.Button btExportGraph;
        public System.Windows.Forms.Button btPlay;
        public System.Windows.Forms.Button btNewGame;
        public System.Windows.Forms.Button btUndo;
        public System.Windows.Forms.Label labGameInfo;
        public System.Windows.Forms.GroupBox grpNode;
        public System.Windows.Forms.GroupBox grpSeq;
        public System.Windows.Forms.GroupBox grpWS;
        public System.Windows.Forms.GroupBox grpLatex;
        public System.Windows.Forms.GroupBox grpTrav;
        public System.Windows.Forms.Button btBetaReduce;
        public System.Windows.Forms.Button btPlayAllOMoves;
        public System.Windows.Forms.CheckBox chkInplace;
    }
}

