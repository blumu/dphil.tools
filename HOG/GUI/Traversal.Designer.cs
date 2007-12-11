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
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btAdd = new System.Windows.Forms.Button();
            this.btBackspace = new System.Windows.Forms.Button();
            this.btExportGraph = new System.Windows.Forms.Button();
            this.btExportTrav = new System.Windows.Forms.Button();
            this.btEditLabel = new System.Windows.Forms.Button();
            this.btHerProj = new System.Windows.Forms.Button();
            this.btOview = new System.Windows.Forms.Button();
            this.btSubtermProj = new System.Windows.Forms.Button();
            this.btPview = new System.Windows.Forms.Button();
            this.btNew = new System.Windows.Forms.Button();
            this.btDuplicate = new System.Windows.Forms.Button();
            this.btDelete = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.seqflowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.gViewer = new Microsoft.Glee.GraphViewerGdi.GViewer();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
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
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(890, 66);
            this.panel1.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 7;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 51F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 138F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 138F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 353F));
            this.tableLayoutPanel1.Controls.Add(this.btAdd, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btBackspace, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btExportGraph, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.btExportTrav, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.btEditLabel, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.btHerProj, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.btOview, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.btSubtermProj, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.btPview, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.btNew, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btDuplicate, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btDelete, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(500, 65);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(890, 65);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btAdd
            // 
            this.btAdd.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btAdd.Location = new System.Drawing.Point(9, 36);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(33, 24);
            this.btAdd.TabIndex = 1;
            this.btAdd.Text = "&+";
            this.btAdd.UseVisualStyleBackColor = true;
            // 
            // btBackspace
            // 
            this.btBackspace.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btBackspace.Location = new System.Drawing.Point(9, 4);
            this.btBackspace.Name = "btBackspace";
            this.btBackspace.Size = new System.Drawing.Size(33, 24);
            this.btBackspace.TabIndex = 0;
            this.btBackspace.Text = "<&-";
            this.btBackspace.UseVisualStyleBackColor = true;
            // 
            // btExportGraph
            // 
            this.btExportGraph.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btExportGraph.Location = new System.Drawing.Point(404, 4);
            this.btExportGraph.Name = "btExportGraph";
            this.btExportGraph.Size = new System.Drawing.Size(127, 24);
            this.btExportGraph.TabIndex = 10;
            this.btExportGraph.Text = "&Graph -> LaTeX...";
            this.btExportGraph.UseVisualStyleBackColor = true;
            // 
            // btExportTrav
            // 
            this.btExportTrav.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btExportTrav.Location = new System.Drawing.Point(404, 36);
            this.btExportTrav.Name = "btExportTrav";
            this.btExportTrav.Size = new System.Drawing.Size(127, 24);
            this.btExportTrav.TabIndex = 11;
            this.btExportTrav.Text = "&Sequence -> LaTeX...";
            this.btExportTrav.UseVisualStyleBackColor = true;
            // 
            // btEditLabel
            // 
            this.btEditLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btEditLabel.Location = new System.Drawing.Point(124, 36);
            this.btEditLabel.Name = "btEditLabel";
            this.btEditLabel.Size = new System.Drawing.Size(63, 24);
            this.btEditLabel.TabIndex = 5;
            this.btEditLabel.Text = "&Edit label";
            this.btEditLabel.UseVisualStyleBackColor = true;
            // 
            // btHerProj
            // 
            this.btHerProj.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btHerProj.Location = new System.Drawing.Point(269, 4);
            this.btHerProj.Margin = new System.Windows.Forms.Padding(0);
            this.btHerProj.Name = "btHerProj";
            this.btHerProj.Size = new System.Drawing.Size(121, 23);
            this.btHerProj.TabIndex = 8;
            this.btHerProj.Text = "&Hereditary projection";
            this.btHerProj.UseVisualStyleBackColor = true;
            // 
            // btOview
            // 
            this.btOview.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btOview.Location = new System.Drawing.Point(194, 4);
            this.btOview.Name = "btOview";
            this.btOview.Size = new System.Drawing.Size(64, 24);
            this.btOview.TabIndex = 6;
            this.btOview.Text = "&O-View";
            this.btOview.UseVisualStyleBackColor = true;
            // 
            // btSubtermProj
            // 
            this.btSubtermProj.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btSubtermProj.Location = new System.Drawing.Point(269, 37);
            this.btSubtermProj.Margin = new System.Windows.Forms.Padding(0);
            this.btSubtermProj.Name = "btSubtermProj";
            this.btSubtermProj.Size = new System.Drawing.Size(121, 23);
            this.btSubtermProj.TabIndex = 9;
            this.btSubtermProj.Text = "&Subterm projection";
            this.btSubtermProj.UseVisualStyleBackColor = true;
            // 
            // btPview
            // 
            this.btPview.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btPview.Location = new System.Drawing.Point(194, 36);
            this.btPview.Name = "btPview";
            this.btPview.Size = new System.Drawing.Size(64, 24);
            this.btPview.TabIndex = 7;
            this.btPview.Text = "&P-View";
            this.btPview.UseVisualStyleBackColor = true;
            // 
            // btNew
            // 
            this.btNew.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btNew.Location = new System.Drawing.Point(54, 4);
            this.btNew.Name = "btNew";
            this.btNew.Size = new System.Drawing.Size(63, 24);
            this.btNew.TabIndex = 2;
            this.btNew.Text = "&New";
            this.btNew.UseVisualStyleBackColor = true;
            // 
            // btDuplicate
            // 
            this.btDuplicate.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btDuplicate.Location = new System.Drawing.Point(124, 4);
            this.btDuplicate.Name = "btDuplicate";
            this.btDuplicate.Size = new System.Drawing.Size(63, 24);
            this.btDuplicate.TabIndex = 4;
            this.btDuplicate.Text = "D&uplicate";
            this.btDuplicate.UseVisualStyleBackColor = true;
            // 
            // btDelete
            // 
            this.btDelete.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btDelete.Location = new System.Drawing.Point(54, 36);
            this.btDelete.Name = "btDelete";
            this.btDelete.Size = new System.Drawing.Size(63, 24);
            this.btDelete.TabIndex = 3;
            this.btDelete.Text = "&Delete";
            this.btDelete.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 66);
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
            this.splitContainer1.Size = new System.Drawing.Size(890, 367);
            this.splitContainer1.SplitterDistance = 589;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 13;
            // 
            // seqflowPanel
            // 
            this.seqflowPanel.AutoScroll = true;
            this.seqflowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.seqflowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.seqflowPanel.Location = new System.Drawing.Point(0, 0);
            this.seqflowPanel.Name = "seqflowPanel";
            this.seqflowPanel.Size = new System.Drawing.Size(585, 363);
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
            this.gViewer.Size = new System.Drawing.Size(292, 363);
            this.gViewer.TabIndex = 0;
            this.gViewer.ZoomF = 1;
            this.gViewer.ZoomFraction = 0.5;
            this.gViewer.ZoomWindowThreshold = 0.05;
            // 
            // Traversal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(890, 433);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Name = "Traversal";
            this.Text = "Traversal Calculator";
            this.Load += new System.EventHandler(this.Traversal_Load);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        public System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        public System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        public System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        public System.Windows.Forms.ToolStripContentPanel ContentPanel;
        public System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Button btExportTrav;
        public System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.Button btAdd;
        public System.Windows.Forms.Button btBackspace;
        public System.Windows.Forms.Button btOview;
        public System.Windows.Forms.Button btPview;
        public System.Windows.Forms.Button btHerProj;
        public System.Windows.Forms.Button btSubtermProj;
        public System.Windows.Forms.Button btExportGraph;
        public System.Windows.Forms.SplitContainer splitContainer1;
        public System.Windows.Forms.FlowLayoutPanel seqflowPanel;
        public Microsoft.Glee.GraphViewerGdi.GViewer gViewer;
        public System.Windows.Forms.Button btDelete;
        public System.Windows.Forms.Button btDuplicate;
        public System.Windows.Forms.Button btEditLabel;
        public System.Windows.Forms.Button btNew;
    }
}

