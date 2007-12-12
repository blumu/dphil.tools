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
            this.btDuplicate = new System.Windows.Forms.Button();
            this.btNew = new System.Windows.Forms.Button();
            this.btDelete = new System.Windows.Forms.Button();
            this.btPview = new System.Windows.Forms.Button();
            this.btOview = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
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
            this.splitContainer1.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.splitContainer1.Size = new System.Drawing.Size(852, 432);
            this.splitContainer1.SplitterDistance = 570;
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
            this.seqflowPanel.Size = new System.Drawing.Size(566, 428);
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
            this.gViewer.Size = new System.Drawing.Size(273, 428);
            this.gViewer.TabIndex = 0;
            this.gViewer.ZoomF = 1;
            this.gViewer.ZoomFraction = 0.5;
            this.gViewer.ZoomWindowThreshold = 0.05;
            // 
            // btSubtermProj
            // 
            this.btSubtermProj.Location = new System.Drawing.Point(241, 50);
            this.btSubtermProj.Margin = new System.Windows.Forms.Padding(2);
            this.btSubtermProj.Name = "btSubtermProj";
            this.btSubtermProj.Size = new System.Drawing.Size(125, 28);
            this.btSubtermProj.TabIndex = 9;
            this.btSubtermProj.Text = "&Subterm projection";
            this.btSubtermProj.UseVisualStyleBackColor = true;
            // 
            // btHerProj
            // 
            this.btHerProj.Location = new System.Drawing.Point(241, 18);
            this.btHerProj.Margin = new System.Windows.Forms.Padding(2);
            this.btHerProj.Name = "btHerProj";
            this.btHerProj.Size = new System.Drawing.Size(125, 28);
            this.btHerProj.TabIndex = 8;
            this.btHerProj.Text = "&Hereditary projection";
            this.btHerProj.UseVisualStyleBackColor = true;
            // 
            // btExportTrav
            // 
            this.btExportTrav.Location = new System.Drawing.Point(370, 50);
            this.btExportTrav.Margin = new System.Windows.Forms.Padding(2);
            this.btExportTrav.Name = "btExportTrav";
            this.btExportTrav.Size = new System.Drawing.Size(133, 28);
            this.btExportTrav.TabIndex = 11;
            this.btExportTrav.Text = "&Sequence -> LaTeX...";
            this.btExportTrav.UseVisualStyleBackColor = true;
            // 
            // btExportGraph
            // 
            this.btExportGraph.Location = new System.Drawing.Point(370, 18);
            this.btExportGraph.Margin = new System.Windows.Forms.Padding(2);
            this.btExportGraph.Name = "btExportGraph";
            this.btExportGraph.Size = new System.Drawing.Size(133, 28);
            this.btExportGraph.TabIndex = 10;
            this.btExportGraph.Text = "&Graph -> LaTeX...";
            this.btExportGraph.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btAdd);
            this.groupBox1.Controls.Add(this.btBackspace);
            this.groupBox1.Controls.Add(this.btEditLabel);
            this.groupBox1.Location = new System.Drawing.Point(8, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(135, 84);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Node";
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(7, 18);
            this.btAdd.Margin = new System.Windows.Forms.Padding(2);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(47, 28);
            this.btAdd.TabIndex = 7;
            this.btAdd.Text = "&+";
            this.btAdd.UseVisualStyleBackColor = true;
            // 
            // btBackspace
            // 
            this.btBackspace.Location = new System.Drawing.Point(7, 50);
            this.btBackspace.Margin = new System.Windows.Forms.Padding(2);
            this.btBackspace.Name = "btBackspace";
            this.btBackspace.Size = new System.Drawing.Size(47, 28);
            this.btBackspace.TabIndex = 6;
            this.btBackspace.Text = "<&-";
            this.btBackspace.UseVisualStyleBackColor = true;
            // 
            // btEditLabel
            // 
            this.btEditLabel.Location = new System.Drawing.Point(59, 50);
            this.btEditLabel.Margin = new System.Windows.Forms.Padding(2);
            this.btEditLabel.Name = "btEditLabel";
            this.btEditLabel.Size = new System.Drawing.Size(66, 28);
            this.btEditLabel.TabIndex = 8;
            this.btEditLabel.Text = "&Edit label";
            this.btEditLabel.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btExportTrav);
            this.groupBox2.Controls.Add(this.btExportGraph);
            this.groupBox2.Controls.Add(this.btDuplicate);
            this.groupBox2.Controls.Add(this.btNew);
            this.groupBox2.Controls.Add(this.btSubtermProj);
            this.groupBox2.Controls.Add(this.btDelete);
            this.groupBox2.Controls.Add(this.btHerProj);
            this.groupBox2.Controls.Add(this.btPview);
            this.groupBox2.Controls.Add(this.btOview);
            this.groupBox2.Location = new System.Drawing.Point(149, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(512, 85);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Sequence";
            // 
            // btDuplicate
            // 
            this.btDuplicate.Location = new System.Drawing.Point(148, 18);
            this.btDuplicate.Margin = new System.Windows.Forms.Padding(2);
            this.btDuplicate.Name = "btDuplicate";
            this.btDuplicate.Size = new System.Drawing.Size(89, 28);
            this.btDuplicate.TabIndex = 12;
            this.btDuplicate.Text = "D&uplicate";
            this.btDuplicate.UseVisualStyleBackColor = true;
            // 
            // btNew
            // 
            this.btNew.Location = new System.Drawing.Point(8, 18);
            this.btNew.Margin = new System.Windows.Forms.Padding(2);
            this.btNew.Name = "btNew";
            this.btNew.Size = new System.Drawing.Size(66, 28);
            this.btNew.TabIndex = 8;
            this.btNew.Text = "&New";
            this.btNew.UseVisualStyleBackColor = true;
            // 
            // btDelete
            // 
            this.btDelete.Location = new System.Drawing.Point(8, 50);
            this.btDelete.Margin = new System.Windows.Forms.Padding(2);
            this.btDelete.Name = "btDelete";
            this.btDelete.Size = new System.Drawing.Size(66, 28);
            this.btDelete.TabIndex = 9;
            this.btDelete.Text = "&Delete";
            this.btDelete.UseVisualStyleBackColor = true;
            // 
            // btPview
            // 
            this.btPview.Location = new System.Drawing.Point(78, 50);
            this.btPview.Margin = new System.Windows.Forms.Padding(2);
            this.btPview.Name = "btPview";
            this.btPview.Size = new System.Drawing.Size(66, 28);
            this.btPview.TabIndex = 11;
            this.btPview.Text = "&P-View";
            this.btPview.UseVisualStyleBackColor = true;
            // 
            // btOview
            // 
            this.btOview.Location = new System.Drawing.Point(78, 18);
            this.btOview.Margin = new System.Windows.Forms.Padding(2);
            this.btOview.Name = "btOview";
            this.btOview.Size = new System.Drawing.Size(66, 28);
            this.btOview.TabIndex = 10;
            this.btOview.Text = "&O-View";
            this.btOview.UseVisualStyleBackColor = true;
            // 
            // Traversal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(852, 525);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.groupBox1);
            this.Name = "Traversal";
            this.Text = "Traversal Calculator";
            this.Load += new System.EventHandler(this.Traversal_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
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
    }
}

