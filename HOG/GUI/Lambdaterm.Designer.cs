namespace GUI
{
    public partial class Lambdaterm
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
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.codeRichTextBox = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btCalculator = new System.Windows.Forms.Button();
            this.btGraph = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // outputTextBox
            // 
            this.outputTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.outputTextBox.Location = new System.Drawing.Point(0, 337);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.outputTextBox.Size = new System.Drawing.Size(771, 90);
            this.outputTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(14, 321);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Output";
            // 
            // codeRichTextBox
            // 
            this.codeRichTextBox.AcceptsTab = true;
            this.codeRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.codeRichTextBox.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.codeRichTextBox.Location = new System.Drawing.Point(0, 50);
            this.codeRichTextBox.Name = "codeRichTextBox";
            this.codeRichTextBox.Size = new System.Drawing.Size(771, 268);
            this.codeRichTextBox.TabIndex = 0;
            this.codeRichTextBox.Text = "";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Input lambda-term:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btCalculator);
            this.panel1.Controls.Add(this.btGraph);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(771, 31);
            this.panel1.TabIndex = 13;
            // 
            // btCalculator
            // 
            this.btCalculator.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btCalculator.Location = new System.Drawing.Point(164, 3);
            this.btCalculator.Name = "btCalculator";
            this.btCalculator.Size = new System.Drawing.Size(127, 24);
            this.btCalculator.TabIndex = 1;
            this.btCalculator.Text = "&Traversal Calculator";
            this.btCalculator.UseVisualStyleBackColor = true;
            // 
            // btGraph
            // 
            this.btGraph.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btGraph.Location = new System.Drawing.Point(12, 3);
            this.btGraph.Name = "btGraph";
            this.btGraph.Size = new System.Drawing.Size(146, 24);
            this.btGraph.TabIndex = 0;
            this.btGraph.Text = "&View computation graph";
            this.btGraph.UseVisualStyleBackColor = true;
            // 
            // Lambdaterm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 427);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.codeRichTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Lambdaterm";
            this.Text = "Lambdaterm";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox outputTextBox;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.RichTextBox codeRichTextBox;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Button btCalculator;
        public System.Windows.Forms.Button btGraph;

    }
}