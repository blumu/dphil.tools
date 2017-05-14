namespace GUI
{
    partial class InputVar_PickChild
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
            this.textChildNodeIndex = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.playButton = new System.Windows.Forms.Button();
            this.label = new System.Windows.Forms.Label();
            this.labRange = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textChildNodeIndex
            // 
            this.textChildNodeIndex.Location = new System.Drawing.Point(15, 62);
            this.textChildNodeIndex.Name = "textChildNodeIndex";
            this.textChildNodeIndex.Size = new System.Drawing.Size(100, 20);
            this.textChildNodeIndex.TabIndex = 7;
            this.textChildNodeIndex.Text = "1";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(15, 88);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // playButton
            // 
            this.playButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.playButton.Location = new System.Drawing.Point(96, 88);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(80, 23);
            this.playButton.TabIndex = 5;
            this.playButton.Text = "&Play O-move";
            this.playButton.UseVisualStyleBackColor = true;
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.Location = new System.Drawing.Point(12, 19);
            this.label.MaximumSize = new System.Drawing.Size(350, 0);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(342, 26);
            this.label.TabIndex = 4;
            this.label.Text = "Enter the index of the child lambda node for the selected P-node in the O-view.  " +
    "(Child lambda-nodes are numbered from 1 onwards):";
            // 
            // label1
            // 
            this.labRange.AutoSize = true;
            this.labRange.Location = new System.Drawing.Point(121, 65);
            this.labRange.MaximumSize = new System.Drawing.Size(350, 0);
            this.labRange.Name = "label1";
            this.labRange.Size = new System.Drawing.Size(72, 13);
            this.labRange.TabIndex = 8;
            this.labRange.Text = "Range:  [1, ?]";
            // 
            // InputVar_PickChild
            // 
            this.AcceptButton = this.playButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(347, 119);
            this.Controls.Add(this.labRange);
            this.Controls.Add(this.textChildNodeIndex);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.playButton);
            this.Controls.Add(this.label);
            this.Name = "InputVar_PickChild";
            this.Text = "(InputVar) rule: pick a child";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cancelButton;
        public System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Label label;
        public System.Windows.Forms.TextBox textChildNodeIndex;
        public System.Windows.Forms.Label labRange;
    }
}