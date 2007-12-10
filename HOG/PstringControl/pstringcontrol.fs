#light

(** $Id: $
	Description: Pointer string Windows Form Control
	Author:		William Blum
**)

namespace Pstring

type NodeClickEventArgs =
    class
        val node : int
        new (_node:int) as this =
         {   node = _node; }
         then ()
    end
     
type pstring_node = string
type pstring = pstring_node list

type NodeClickHandler = IHandlerEvent<NodeClickEventArgs> 
//delegate 
type PstringControl = 
  class
    inherit System.Windows.Forms.UserControl as base

        
    val mutable public components : System.ComponentModel.Container;
        override this.Dispose(disposing) =
            if (disposing && (match this.components with null -> false | _ -> true)) then
              this.components.Dispose();
            base.Dispose(disposing)

    val mutable public nodeEditTextBox : System.Windows.Forms.TextBox
    val mutable public pichScroll : System.Windows.Forms.HScrollBar
    val mutable public picTrav : System.Windows.Forms.PictureBox

    // Events
    val mutable private nodeClickEventPair : (PstringControl * NodeClickEventArgs -> unit) * IHandlerEvent<NodeClickEventArgs>

    member x.nodeClick with get() = snd x.nodeClickEventPair

    member this.InitializeComponent() =
    
        this.nodeEditTextBox <- new System.Windows.Forms.TextBox()
        this.pichScroll <- new System.Windows.Forms.HScrollBar()
        this.picTrav <- new System.Windows.Forms.PictureBox()

        //((System.ComponentModel.ISupportInitialize)(this.picTrav)).BeginInit();
        this.SuspendLayout();
        // 
        // nodeEditTextBox
        // 
        this.nodeEditTextBox.AcceptsReturn <- true;
        this.nodeEditTextBox.CausesValidation <- false;
        this.nodeEditTextBox.Location <- new System.Drawing.Point(656, 17);
        this.nodeEditTextBox.Name <- "nodeEditTextBox";
        this.nodeEditTextBox.Size <- new System.Drawing.Size(66, 20);
        this.nodeEditTextBox.TabIndex <- 21;
        this.nodeEditTextBox.Visible <- false;
        // 
        // pichScroll
        // 
        this.pichScroll.Dock <- System.Windows.Forms.DockStyle.Bottom;
        this.pichScroll.Location <- new System.Drawing.Point(0, 216);
        this.pichScroll.Name <- "pichScroll";
        this.pichScroll.Size <- new System.Drawing.Size(820, 12);
        this.pichScroll.TabIndex <- 20;
        // 
        // picTrav
        // 
        this.picTrav.Anchor <- Enum.combine [ System.Windows.Forms.AnchorStyles.Top;
                                              System.Windows.Forms.AnchorStyles.Bottom;
                                              System.Windows.Forms.AnchorStyles.Left;
                                              System.Windows.Forms.AnchorStyles.Right]
        this.picTrav.Location <- new System.Drawing.Point(0, 0);
        this.picTrav.Margin <- new System.Windows.Forms.Padding(3, 3, 3, 0);
        this.picTrav.Name <- "picTrav";
        this.picTrav.Size <- new System.Drawing.Size(820, 216);
        this.picTrav.TabIndex <- 19;
        this.picTrav.TabStop <- false;
        this.picTrav.MouseClick.Add(fun _ -> (fst this.nodeClickEventPair)(this, NodeClickEventArgs(8))
                                    );
        // 
        // PstringUsercontrol
        // 
        this.AutoScaleDimensions <- new System.Drawing.SizeF(6.0f, 13.0f);
        this.AutoScaleMode <- System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.nodeEditTextBox);
        this.Controls.Add(this.pichScroll);
        this.Controls.Add(this.picTrav);
        this.Name <- "PstringUsercontrol";
        this.Size <- new System.Drawing.Size(820, 228);
        //((System.ComponentModel.ISupportInitialize)(this.picTrav)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();


    new (pstr:pstring) as this =
        {   components = null;
            nodeEditTextBox = null;
            pichScroll = null;
            picTrav = null;
            nodeClickEventPair = Microsoft.FSharp.Control.IEvent.create_HandlerEvent()
           }
        then
            // Create the events
            //this.nodeclick <- fst nodeClickEvent

            this.InitializeComponent(); 
        
  end