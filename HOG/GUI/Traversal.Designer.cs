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
            this.gViewer = new Microsoft.Glee.GraphViewerGdi.GViewer();
            this.btSubtermProj = new System.Windows.Forms.Button();
            this.btHerProj = new System.Windows.Forms.Button();
            this.btExportTrav = new System.Windows.Forms.Button();
            this.btExportGraph = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btAdd = new System.Windows.Forms.Button();
            this.btBackspace = new System.Windows.Forms.Button();
            this.btEditLabel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btExt = new System.Windows.Forms.Button();
            this.btStar = new System.Windows.Forms.Button();
            this.btPrefix = new System.Windows.Forms.Button();
            this.btDuplicate = new System.Windows.Forms.Button();
            this.btNew = new System.Windows.Forms.Button();
            this.btDelete = new System.Windows.Forms.Button();
            this.btPview = new System.Windows.Forms.Button();
            this.btOview = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btOpen = new System.Windows.Forms.Button();
            this.btSave = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
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
            this.splitContainer1.Location = new System.Drawing.Point(0, 94);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.seqflowPanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.gViewer);
            this.splitContainer1.Size = new System.Drawing.Size(880, 432);
            this.splitContainer1.SplitterDistance = 588;
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
            this.seqflowPanel.Size = new System.Drawing.Size(584, 428);
            this.seqflowPanel.TabIndex = 0;
            this.seqflowPanel.TabStop = true;
            this.seqflowPanel.WrapContents = false;
            // 
            // gViewer
            // 
            this.gViewer.AsyncLayout = false;
            this.gViewer.AutoScroll = true;
            this.gViewer.BackwardEnabled = false;
            this.gViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gViewer.ForwardEnabled = false;
            this.gViewer.Graph = null;
            this.gViewer.Location = new System.Drawing.Point(0, 0);
            this.gViewer.MouseHitDistance = 0.05;
            this.gViewer.Name = "gViewer";
            this.gViewer.NavigationVisible = true;
            this.gViewer.PanButtonPressed = false;
            this.gViewer.SaveButtonVisible = true;
            this.gViewer.Size = new System.Drawing.Size(283, 428);
            this.gViewer.TabIndex = 0;
            this.gViewer.ZoomF = 1;
            this.gViewer.ZoomFraction = 0.5;
            this.gViewer.ZoomWindowThreshold = 0.05;
            // 
            // btSubtermProj
            // 
            this.btSubtermProj.Location = new System.Drawing.Point(254, 52);
            this.btSubtermProj.Margin = new System.Windows.Forms.Padding(2);
            this.btSubtermProj.Name = "btSubtermProj";
            this.btSubtermProj.Size = new System.Drawing.Size(125, 28);
            this.btSubtermProj.TabIndex = 9;
            this.btSubtermProj.Text = "&Subterm projection";
            this.btSubtermProj.UseVisualStyleBackColor = true;
            // 
            // btHerProj
            // 
            this.btHerProj.Location = new System.Drawing.Point(254, 18);
            this.btHerProj.Margin = new System.Windows.Forms.Padding(2);
            this.btHerProj.Name = "btHerProj";
            this.btHerProj.Size = new System.Drawing.Size(125, 28);
            this.btHerProj.TabIndex = 8;
            this.btHerProj.Text = "&Hereditary projection";
            this.btHerProj.UseVisualStyleBackColor = true;
            // 
            // btExportTrav
            // 
            this.btExportTrav.Location = new System.Drawing.Point(383, 32);
            this.btExportTrav.Margin = new System.Windows.Forms.Padding(2);
            this.btExportTrav.Name = "btExportTrav";
            this.btExportTrav.Size = new System.Drawing.Size(106, 28);
            this.btExportTrav.TabIndex = 10;
            this.btExportTrav.Text = "Export to &LaTeX...";
            this.btExportTrav.UseVisualStyleBackColor = true;
            // 
            // btExportGraph
            // 
            this.btExportGraph.Location = new System.Drawing.Point(5, 52);
            this.btExportGraph.Margin = new System.Windows.Forms.Padding(2);
            this.btExportGraph.Name = "btExportGraph";
            this.btExportGraph.Size = new System.Drawing.Size(138, 28);
            this.btExportGraph.TabIndex = 0;
            this.btExportGraph.Text = "Expor&t graph to LaTeX...";
            this.btExportGraph.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btAdd);
            this.groupBox1.Controls.Add(this.btBackspace);
            this.groupBox1.Controls.Add(this.btEditLabel);
            this.groupBox1.Location = new System.Drawing.Point(173, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(135, 84);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Node";
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(7, 18);
            this.btAdd.Margin = new System.Windows.Forms.Padding(2);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(47, 28);
            this.btAdd.TabIndex = 0;
            this.btAdd.Text = "&+";
            this.btAdd.UseVisualStyleBackColor = true;
            // 
            // btBackspace
            // 
            this.btBackspace.Location = new System.Drawing.Point(7, 52);
            this.btBackspace.Margin = new System.Windows.Forms.Padding(2);
            this.btBackspace.Name = "btBackspace";
            this.btBackspace.Size = new System.Drawing.Size(47, 28);
            this.btBackspace.TabIndex = 1;
            this.btBackspace.Text = "<&-";
            this.btBackspace.UseVisualStyleBackColor = true;
            // 
            // btEditLabel
            // 
            this.btEditLabel.Location = new System.Drawing.Point(59, 52);
            this.btEditLabel.Margin = new System.Windows.Forms.Padding(2);
            this.btEditLabel.Name = "btEditLabel";
            this.btEditLabel.Size = new System.Drawing.Size(66, 28);
            this.btEditLabel.TabIndex = 2;
            this.btEditLabel.Text = "&Edit label";
            this.btEditLabel.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btExt);
            this.groupBox2.Controls.Add(this.btStar);
            this.groupBox2.Controls.Add(this.btExportTrav);
            this.groupBox2.Controls.Add(this.btPrefix);
            this.groupBox2.Controls.Add(this.btDuplicate);
            this.groupBox2.Controls.Add(this.btNew);
            this.groupBox2.Controls.Add(this.btSubtermProj);
            this.groupBox2.Controls.Add(this.btDelete);
            this.groupBox2.Controls.Add(this.btHerProj);
            this.groupBox2.Controls.Add(this.btPview);
            this.groupBox2.Controls.Add(this.btOview);
            this.groupBox2.Location = new System.Drawing.Point(321, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(497, 85);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Sequence";
            // 
            // btExt
            // 
            this.btExt.Location = new System.Drawing.Point(185, 52);
            this.btExt.Name = "btExt";
            this.btExt.Size = new System.Drawing.Size(66, 28);
            this.btExt.TabIndex = 7;
            this.btExt.Text = "E&xtension";
            this.btExt.UseVisualStyleBackColor = true;
            // 
            // btStar
            // 
            this.btStar.Location = new System.Drawing.Point(185, 18);
            this.btStar.Name = "btStar";
            this.btStar.Size = new System.Drawing.Size(66, 28);
            this.btStar.TabIndex = 6;
            this.btStar.Text = "St&ar";
            this.btStar.UseVisualStyleBackColor = true;
            // 
            // btPrefix
            // 
            this.btPrefix.Location = new System.Drawing.Point(112, 52);
            this.btPrefix.Margin = new System.Windows.Forms.Padding(2);
            this.btPrefix.Name = "btPrefix";
            this.btPrefix.Size = new System.Drawing.Size(70, 28);
            this.btPrefix.TabIndex = 5;
            this.btPrefix.Text = "P&refix";
            this.btPrefix.UseVisualStyleBackColor = true;
            // 
            // btDuplicate
            // 
            this.btDuplicate.Location = new System.Drawing.Point(112, 18);
            this.btDuplicate.Margin = new System.Windows.Forms.Padding(2);
            this.btDuplicate.Name = "btDuplicate";
            this.btDuplicate.Size = new System.Drawing.Size(70, 28);
            this.btDuplicate.TabIndex = 4;
            this.btDuplicate.Text = "D&uplicate";
            this.btDuplicate.UseVisualStyleBackColor = true;
            // 
            // btNew
            // 
            this.btNew.Location = new System.Drawing.Point(8, 18);
            this.btNew.Margin = new System.Windows.Forms.Padding(2);
            this.btNew.Name = "btNew";
            this.btNew.Size = new System.Drawing.Size(46, 28);
            this.btNew.TabIndex = 0;
            this.btNew.Text = "&New";
            this.btNew.UseVisualStyleBackColor = true;
            // 
            // btDelete
            // 
            this.btDelete.Location = new System.Drawing.Point(8, 52);
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
            this.btOview.Location = new System.Drawing.Point(58, 18);
            this.btOview.Margin = new System.Windows.Forms.Padding(2);
            this.btOview.Name = "btOview";
            this.btOview.Size = new System.Drawing.Size(52, 28);
            this.btOview.TabIndex = 2;
            this.btOview.Text = "&O-View";
            this.btOview.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btSave);
            this.groupBox3.Controls.Add(this.btOpen);
            this.groupBox3.Controls.Add(this.btExportGraph);
            this.groupBox3.Location = new System.Drawing.Point(9, 5);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(151, 84);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Worksheet";
            // 
            // btOpen
            // 
            this.btOpen.Location = new System.Drawing.Point(5, 18);
            this.btOpen.Margin = new System.Windows.Forms.Padding(2);
            this.btOpen.Name = "btOpen";
            this.btOpen.Size = new System.Drawing.Size(67, 28);
            this.btOpen.TabIndex = 0;
            this.btOpen.Text = "Open...";
            this.btOpen.UseVisualStyleBackColor = true;
            // 
            // btSave
            // 
            this.btSave.Location = new System.Drawing.Point(76, 18);
            this.btSave.Margin = new System.Windows.Forms.Padding(2);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(67, 28);
            this.btSave.TabIndex = 0;
            this.btSave.Text = "Save...";
            this.btSave.UseVisualStyleBackColor = true;
            // 
            // Traversal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(880, 525);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.groupBox1);
            this.Name = "Traversal";
            this.Text = "Traversal Calculator";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
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
        public Microsoft.Glee.GraphViewerGdi.GViewer gViewer;
        public System.Windows.Forms.Button btExportGraph;
        public System.Windows.Forms.Button btHerProj;
        public System.Windows.Forms.Button btSubtermProj;
        public System.Windows.Forms.Button btExportTrav;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.Button btAdd;
        public System.Windows.Forms.Button btBackspace;
        public System.Windows.Forms.Button btEditLabel;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.Button btDuplicate;
        public System.Windows.Forms.Button btNew;
        public System.Windows.Forms.Button btDelete;
        public System.Windows.Forms.Button btPview;
        public System.Windows.Forms.Button btOview;
        public System.Windows.Forms.Button btPrefix;
        private System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.Button btExt;
        public System.Windows.Forms.Button btStar;
        public System.Windows.Forms.Button btSave;
        public System.Windows.Forms.Button btOpen;
    }
}

