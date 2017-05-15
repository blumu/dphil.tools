namespace GUI
{
    partial class PointerSequenceFlowPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

        public void ScrollToEndOfSequence(System.Windows.Forms.Control c)
        {
            int i = base.Controls.IndexOf(c);
            if(i>= 0)
            {
                var scroll = base.ScrollToControl(c);
                base.SetDisplayRectLocation(scroll.X - c.ClientSize.Width, scroll.Y);
                base.AdjustFormScrollbars(true);
            }
        }

        #endregion
    }
}
