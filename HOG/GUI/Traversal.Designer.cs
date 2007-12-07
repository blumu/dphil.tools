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
            this.btExportGraph = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btExportTrav = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.gViewer = new Microsoft.Glee.GraphViewerGdi.GViewer();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.panelTravLeft = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btAdd = new System.Windows.Forms.Button();
            this.btBackspace = new System.Windows.Forms.Button();
            this.nodeEditTextBox = new System.Windows.Forms.TextBox();
            this.pichScroll = new System.Windows.Forms.HScrollBar();
            this.picTrav = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelTravLeft.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTrav)).BeginInit();
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
            // btExportGraph
            // 
            this.btExportGraph.Location = new System.Drawing.Point(12, 12);
            this.btExportGraph.Name = "btExportGraph";
            this.btExportGraph.Size = new System.Drawing.Size(154, 25);
            this.btExportGraph.TabIndex = 5;
            this.btExportGraph.Text = "Export graph to LaTeX...";
            this.btExportGraph.UseVisualStyleBackColor = true;
            this.btExportGraph.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btExportTrav);
            this.panel1.Controls.Add(this.btExportGraph);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(755, 57);
            this.panel1.TabIndex = 5;
            // 
            // btExportTrav
            // 
            this.btExportTrav.Location = new System.Drawing.Point(226, 13);
            this.btExportTrav.Name = "btExportTrav";
            this.btExportTrav.Size = new System.Drawing.Size(157, 24);
            this.btExportTrav.TabIndex = 6;
            this.btExportTrav.Text = "Export traversal to LaTeX...";
            this.btExportTrav.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 57);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panelTravLeft);
            this.splitContainer2.Size = new System.Drawing.Size(755, 490);
            this.splitContainer2.SplitterDistance = 381;
            this.splitContainer2.TabIndex = 15;
            this.splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer2_SplitterMoved);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.gViewer);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.propertyGrid1);
            this.splitContainer1.Size = new System.Drawing.Size(755, 381);
            this.splitContainer1.SplitterDistance = 513;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 12;
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
            this.gViewer.Size = new System.Drawing.Size(509, 377);
            this.gViewer.TabIndex = 9;
            this.gViewer.ZoomF = 1;
            this.gViewer.ZoomFraction = 0.5;
            this.gViewer.ZoomWindowThreshold = 0.05;
            this.gViewer.Load += new System.EventHandler(this.gViewer_Load);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(233, 377);
            this.propertyGrid1.TabIndex = 1;
            // 
            // panelTravLeft
            // 
            this.panelTravLeft.Controls.Add(this.tableLayoutPanel1);
            this.panelTravLeft.Controls.Add(this.nodeEditTextBox);
            this.panelTravLeft.Controls.Add(this.pichScroll);
            this.panelTravLeft.Controls.Add(this.picTrav);
            this.panelTravLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTravLeft.Location = new System.Drawing.Point(0, 0);
            this.panelTravLeft.Name = "panelTravLeft";
            this.panelTravLeft.Size = new System.Drawing.Size(755, 105);
            this.panelTravLeft.TabIndex = 18;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.btAdd, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btBackspace, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(721, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(34, 105);
            this.tableLayoutPanel1.TabIndex = 20;
            // 
            // btAdd
            // 
            this.btAdd.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btAdd.Location = new System.Drawing.Point(7, 63);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(20, 30);
            this.btAdd.TabIndex = 19;
            this.btAdd.Text = "+";
            this.btAdd.UseVisualStyleBackColor = true;
            // 
            // btBackspace
            // 
            this.btBackspace.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btBackspace.Location = new System.Drawing.Point(7, 11);
            this.btBackspace.Name = "btBackspace";
            this.btBackspace.Size = new System.Drawing.Size(20, 30);
            this.btBackspace.TabIndex = 18;
            this.btBackspace.Text = "<";
            this.btBackspace.UseVisualStyleBackColor = true;
            // 
            // nodeEditTextBox
            // 
            this.nodeEditTextBox.Location = new System.Drawing.Point(548, 30);
            this.nodeEditTextBox.Name = "nodeEditTextBox";
            this.nodeEditTextBox.Size = new System.Drawing.Size(66, 20);
            this.nodeEditTextBox.TabIndex = 18;
            this.nodeEditTextBox.Visible = false;
            // 
            // pichScroll
            // 
            this.pichScroll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pichScroll.Location = new System.Drawing.Point(0, 93);
            this.pichScroll.Name = "pichScroll";
            this.pichScroll.Size = new System.Drawing.Size(721, 12);
            this.pichScroll.TabIndex = 17;
            // 
            // picTrav
            // 
            this.picTrav.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.picTrav.Location = new System.Drawing.Point(0, 0);
            this.picTrav.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.picTrav.Name = "picTrav";
            this.picTrav.Size = new System.Drawing.Size(718, 93);
            this.picTrav.TabIndex = 16;
            this.picTrav.TabStop = false;
            // 
            // Traversal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(755, 547);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.panel1);
            this.Name = "Traversal";
            this.Text = "Traversals";
            this.Load += new System.EventHandler(this.Traversal_Load);
            this.panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.panelTravLeft.ResumeLayout(false);
            this.panelTravLeft.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picTrav)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        public System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        public System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        public System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        public System.Windows.Forms.ToolStripContentPanel ContentPanel;
        public System.Windows.Forms.Button btExportGraph;
        public System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.SplitContainer splitContainer2;
        public System.Windows.Forms.SplitContainer splitContainer1;
        public Microsoft.Glee.GraphViewerGdi.GViewer gViewer;
        public System.Windows.Forms.PropertyGrid propertyGrid1;
        public System.Windows.Forms.Button btExportTrav;
        public System.Windows.Forms.Panel panelTravLeft;
        public System.Windows.Forms.HScrollBar pichScroll;
        public System.Windows.Forms.PictureBox picTrav;
        public System.Windows.Forms.TextBox nodeEditTextBox;
        public System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.Button btAdd;
        public System.Windows.Forms.Button btBackspace;
    }
}

