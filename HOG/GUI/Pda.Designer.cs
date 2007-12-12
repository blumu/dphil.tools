namespace GUI
{
    public partial class Pda
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
            this.txtPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btStep = new System.Windows.Forms.Button();
            this.txtConfHistory = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.txtCode = new System.Windows.Forms.RichTextBox();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.btRun = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.valueTreeView = new System.Windows.Forms.TreeView();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtPath
            // 
            this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPath.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtPath.Location = new System.Drawing.Point(8, 60);
            this.txtPath.Multiline = true;
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPath.Size = new System.Drawing.Size(772, 27);
            this.txtPath.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(192, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Result of the value tree path validation:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "CPDA description:";
            // 
            // btStep
            // 
            this.btStep.Location = new System.Drawing.Point(145, 12);
            this.btStep.Name = "btStep";
            this.btStep.Size = new System.Drawing.Size(129, 30);
            this.btStep.TabIndex = 1;
            this.btStep.Text = "Step";
            this.btStep.UseVisualStyleBackColor = true;
            // 
            // txtConfHistory
            // 
            this.txtConfHistory.AcceptsReturn = true;
            this.txtConfHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConfHistory.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtConfHistory.Location = new System.Drawing.Point(0, 19);
            this.txtConfHistory.MinimumSize = new System.Drawing.Size(4, 50);
            this.txtConfHistory.Multiline = true;
            this.txtConfHistory.Name = "txtConfHistory";
            this.txtConfHistory.ReadOnly = true;
            this.txtConfHistory.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtConfHistory.Size = new System.Drawing.Size(508, 213);
            this.txtConfHistory.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(172, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Configuration at the selected node:";
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
            this.splitContainer2.Panel2.Controls.Add(this.txtConfHistory);
            this.splitContainer2.Panel2.Controls.Add(this.label4);
            this.splitContainer2.Size = new System.Drawing.Size(511, 423);
            this.splitContainer2.SplitterDistance = 187;
            this.splitContainer2.TabIndex = 5;
            // 
            // txtCode
            // 
            this.txtCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCode.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCode.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCode.HideSelection = false;
            this.txtCode.Location = new System.Drawing.Point(0, 16);
            this.txtCode.MinimumSize = new System.Drawing.Size(4, 50);
            this.txtCode.Name = "txtCode";
            this.txtCode.ReadOnly = true;
            this.txtCode.Size = new System.Drawing.Size(508, 168);
            this.txtCode.TabIndex = 0;
            this.txtCode.Text = "";
            this.txtCode.WordWrap = false;
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // btRun
            // 
            this.btRun.ImageKey = "(none)";
            this.btRun.Location = new System.Drawing.Point(8, 12);
            this.btRun.Name = "btRun";
            this.btRun.Size = new System.Drawing.Size(129, 30);
            this.btRun.TabIndex = 0;
            this.btRun.Text = "&Run";
            this.btRun.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btRun.UseVisualStyleBackColor = true;
            this.btRun.Click += new System.EventHandler(this.btRun_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(-3, -1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Value-tree:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(8, 93);
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
            this.splitContainer1.Size = new System.Drawing.Size(772, 423);
            this.splitContainer1.SplitterDistance = 257;
            this.splitContainer1.TabIndex = 13;
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
            this.valueTreeView.Size = new System.Drawing.Size(258, 407);
            this.valueTreeView.TabIndex = 0;
            // 
            // Pda
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 528);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btStep);
            this.Controls.Add(this.btRun);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Pda";
            this.Text = "Pda";
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox txtPath;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Button btStep;
        public System.Windows.Forms.TextBox txtConfHistory;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.SplitContainer splitContainer2;
        public System.Windows.Forms.ImageList imageList;
        public System.Windows.Forms.Button btRun;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.SplitContainer splitContainer1;
        public System.Windows.Forms.TreeView valueTreeView;
        public System.Windows.Forms.RichTextBox txtCode;

    }
}