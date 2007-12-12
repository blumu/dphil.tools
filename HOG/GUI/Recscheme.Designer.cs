namespace GUI
{
    public partial class Recscheme
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
            this.components = new System.ComponentModel.Container();
            this.btRun = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.btGraph = new System.Windows.Forms.Button();
            this.btPda = new System.Windows.Forms.Button();
            this.btCpda = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.valueTreeView = new System.Windows.Forms.TreeView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btNp1pda = new System.Windows.Forms.Button();
            this.btCalculator = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btRun
            // 
            this.btRun.ImageKey = "(none)";
            this.btRun.Location = new System.Drawing.Point(6, 4);
            this.btRun.Name = "btRun";
            this.btRun.Size = new System.Drawing.Size(129, 30);
            this.btRun.TabIndex = 0;
            this.btRun.Text = "&Run";
            this.btRun.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btRun.UseVisualStyleBackColor = true;
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // btGraph
            // 
            this.btGraph.Location = new System.Drawing.Point(554, 4);
            this.btGraph.Name = "btGraph";
            this.btGraph.Size = new System.Drawing.Size(129, 30);
            this.btGraph.TabIndex = 4;
            this.btGraph.Text = "Computation &graph";
            this.btGraph.UseVisualStyleBackColor = true;
            // 
            // btPda
            // 
            this.btPda.Location = new System.Drawing.Point(417, 4);
            this.btPda.Name = "btPda";
            this.btPda.Size = new System.Drawing.Size(129, 30);
            this.btPda.TabIndex = 3;
            this.btPda.Text = "Build n-PDA (safe RS)";
            this.btPda.UseVisualStyleBackColor = true;
            // 
            // btCpda
            // 
            this.btCpda.Location = new System.Drawing.Point(280, 4);
            this.btCpda.Name = "btCpda";
            this.btCpda.Size = new System.Drawing.Size(129, 30);
            this.btCpda.TabIndex = 2;
            this.btCpda.Text = "Build n-CPDA";
            this.btCpda.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btCpda.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(6, 85);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.valueTreeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(823, 306);
            this.splitContainer1.SplitterDistance = 274;
            this.splitContainer1.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(-3, -1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "The lazy value-tree:";
            // 
            // valueTreeView
            // 
            this.valueTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.valueTreeView.HideSelection = false;
            this.valueTreeView.ImageIndex = 0;
            this.valueTreeView.ImageList = this.imageList;
            this.valueTreeView.Location = new System.Drawing.Point(0, 16);
            this.valueTreeView.Name = "valueTreeView";
            this.valueTreeView.SelectedImageIndex = 0;
            this.valueTreeView.ShowLines = false;
            this.valueTreeView.ShowRootLines = false;
            this.valueTreeView.Size = new System.Drawing.Size(275, 290);
            this.valueTreeView.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.txtCode);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.txtOutput);
            this.splitContainer2.Panel2.Controls.Add(this.label4);
            this.splitContainer2.Size = new System.Drawing.Size(545, 306);
            this.splitContainer2.SplitterDistance = 122;
            this.splitContainer2.TabIndex = 5;
            // 
            // txtCode
            // 
            this.txtCode.AcceptsReturn = true;
            this.txtCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCode.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtCode.ForeColor = System.Drawing.Color.Blue;
            this.txtCode.Location = new System.Drawing.Point(0, 16);
            this.txtCode.Multiline = true;
            this.txtCode.Name = "txtCode";
            this.txtCode.ReadOnly = true;
            this.txtCode.Size = new System.Drawing.Size(545, 103);
            this.txtCode.TabIndex = 0;
            this.txtCode.WordWrap = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(179, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Description of the recursion scheme:";
            // 
            // txtOutput
            // 
            this.txtOutput.AcceptsReturn = true;
            this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutput.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtOutput.Location = new System.Drawing.Point(0, 19);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOutput.Size = new System.Drawing.Size(542, 161);
            this.txtOutput.TabIndex = 0;
            this.txtOutput.WordWrap = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Output:";
            // 
            // txtPath
            // 
            this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPath.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtPath.Location = new System.Drawing.Point(6, 52);
            this.txtPath.Multiline = true;
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPath.Size = new System.Drawing.Size(823, 27);
            this.txtPath.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(192, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Result of the value tree path validation:";
            // 
            // btNp1pda
            // 
            this.btNp1pda.Location = new System.Drawing.Point(143, 4);
            this.btNp1pda.Name = "btNp1pda";
            this.btNp1pda.Size = new System.Drawing.Size(129, 30);
            this.btNp1pda.TabIndex = 1;
            this.btNp1pda.Text = "Build (n+1)-PDA";
            this.btNp1pda.UseVisualStyleBackColor = true;
            // 
            // btCalculator
            // 
            this.btCalculator.Location = new System.Drawing.Point(691, 4);
            this.btCalculator.Name = "btCalculator";
            this.btCalculator.Size = new System.Drawing.Size(129, 30);
            this.btCalculator.TabIndex = 5;
            this.btCalculator.Text = "&Traversal Calculator";
            this.btCalculator.UseVisualStyleBackColor = true;
            // 
            // Recscheme
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(841, 403);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btCpda);
            this.Controls.Add(this.btNp1pda);
            this.Controls.Add(this.btPda);
            this.Controls.Add(this.btCalculator);
            this.Controls.Add(this.btGraph);
            this.Controls.Add(this.btRun);
            this.Name = "Recscheme";
            this.Text = "Recscheme";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Button btRun;
        public System.Windows.Forms.Button btGraph;
        public System.Windows.Forms.Button btPda;
        public System.Windows.Forms.Button btCpda;
        public System.Windows.Forms.SplitContainer splitContainer1;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.TreeView valueTreeView;
        public System.Windows.Forms.TextBox txtPath;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.ImageList imageList;
        public System.Windows.Forms.Button btNp1pda;
        public System.Windows.Forms.Button btCalculator;
        private System.Windows.Forms.SplitContainer splitContainer2;
        public System.Windows.Forms.TextBox txtCode;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox txtOutput;
        public System.Windows.Forms.Label label4;
    }
}