#light

(** $Id: $
	Description: Pointer string Windows Form Control
	Author:		William Blum
**)

module Pstring

open System.Drawing
open System.Windows.Forms

type NodeClickEventArgs =
    class
        val node : int
        new (_node:int) as this =
         {   node = _node; }
         then () 
    end
     
type pstring_node = { tag: obj; label: string; link:int }
type pstring = pstring_node array


type NodeClickHandler = IHandlerEvent<NodeClickEventArgs> 

type VerticalAlignement = Top | Bottom | Middle


let prefix = "► "
let f2 = float32 2.0

//  Constants measures
let leftmargin = 5 // margin on the left of the sequence
let node_padding = 3   // space arround the node
let internodesep = 10  // distance between two consecutive nodes
let link_vertical_increment = 5

let penWidth = float32 1 
let pen = new Pen(Color.Black, penWidth)

let selectcolor = Color.Blue

let mutable selection_pen = new Pen(selectcolor, float32 2.0)
selection_pen.DashStyle <-  System.Drawing.Drawing2D.DashStyle.Dash
selection_pen.DashOffset <- float32 2.0


let seq_selection_pensize = float32 2
let seq_unselection_pen =  new Pen(System.Drawing.SystemColors.Control, seq_selection_pensize)
let mutable seq_selection_pen = new Pen(selectcolor, seq_selection_pensize)
seq_selection_pen.DashStyle <-  System.Drawing.Drawing2D.DashStyle.Dash
seq_selection_pen.DashOffset <- float32 2.0

let arrow_pen = new Pen(Color.Black, penWidth)
arrow_pen.EndCap <- System.Drawing.Drawing2D.LineCap.ArrowAnchor

let selection_arrow_pen = new Pen(selectcolor, float32 2.0)
selection_arrow_pen.EndCap <- System.Drawing.Drawing2D.LineCap.ArrowAnchor


let fontHeight:float32 = float32 10.0
let font = new Font("Arial", fontHeight)

let backgroundColor = Color.Azure
let brush = new SolidBrush(backgroundColor)
let selection_brush = new SolidBrush(backgroundColor)
let textBrush = new SolidBrush(Color.Black);
let height_of_link link = link*link_vertical_increment

type PstringControl = 
  class
    inherit System.Windows.Forms.UserControl as base

        
    val mutable public components : System.ComponentModel.Container;
        override this.Dispose(disposing) =
            if (disposing && (match this.components with null -> false | _ -> true)) then
              this.components.Dispose();
            base.Dispose(disposing)

    val mutable public nodeEditTextBox : System.Windows.Forms.TextBox
    val mutable public hScroll : System.Windows.Forms.HScrollBar
    
    // Events
    val mutable private nodeClickEventPair : (PstringControl * NodeClickEventArgs -> unit) * IHandlerEvent<NodeClickEventArgs>
    member x.nodeClick with get() = snd x.nodeClickEventPair

    val mutable private sequence : pstring
    member x.Sequence with get() = Array.copy x.sequence

    val mutable autosize : bool
    override x.AutoSize with get() = x.autosize
                         and set b = x.autosize <- b
                                     x.hScroll.Visible <- not b
                                     x.recompute_bbox()

    val mutable nodesvalign : VerticalAlignement
    member x.NodesVerticalAlignment with get() = x.nodesvalign
                                      and set a = x.nodesvalign <- a;

    val mutable bboxes : Rectangle array    // bounding boxes of the nodes
    val mutable prefix_bbox : Rectangle     // bounding box of the prefix string
    val mutable link_anchors : Point array  // link anchor positions
    val mutable edited_node : int           // index of the node currently edited
    val mutable selected_node : int         // index of the selected node

    member private this.recompute_bbox() =
        let graphics = this.CreateGraphics()

        this.bboxes <- Array.create (Array.length this.sequence) (System.Drawing.Rectangle(0,0,0,0))
        this.link_anchors <- Array.create (Array.length this.sequence) (Point(0,0))


        let highest_bbox = ref 0
        
        // Compute the bounding boxes and the links anchors positions
        let x = ref (int seq_selection_pensize + leftmargin)
        let longest_link = ref 0

        // place some text horizontally and return a pair containing
        // its bounding box and the text dimensions
        let place_in_hbox txt =
            let txt_dim = graphics.MeasureString(txt, font);
            let bbox = Rectangle(!x, 0, int txt_dim.Width + 2*node_padding, int txt_dim.Height + 2*node_padding)
            x:= !x + int txt_dim.Width + internodesep
            highest_bbox := max !highest_bbox bbox.Height
            bbox,txt_dim

        
        // place the prefix string
        this.prefix_bbox <- fst (place_in_hbox prefix);
        
        for i = 0 to (Array.length this.sequence)-1 do 
          let label = this.sequence.(i).label

          longest_link := max !longest_link (this.sequence.(i).link)
          
          let bbox,textdim = place_in_hbox label
          this.bboxes.(i) <- bbox
          this.link_anchors.(i) <- Point(bbox.Left+(int (textdim.Width/f2)), 0)
        done;

        //////////////////////
        // Vertically realignment of the nodes bounding boxes,
        
        // top of the part containing the nodes (just underneath the links)
        let nodespart_top = int seq_selection_pensize + height_of_link !longest_link
        
        // compute the overall bounding box
        let overall_bbox = ref (Rectangle(0,0,1,nodespart_top)) // account for the space used by the links

        // compute the vertical offset for the nodes to be aligned as the user requested
        let bbox_vertalign_offset (bbox:Rectangle) = nodespart_top + 
                                                     match this.nodesvalign with
                                                          Middle -> (!highest_bbox-bbox.Height)/2
                                                        | Bottom -> !highest_bbox-bbox.Height
                                                        | Top -> 0

        // First, treat the bbox of the prefix string
        this.prefix_bbox <- Rectangle(this.prefix_bbox.X, (bbox_vertalign_offset this.prefix_bbox)+this.prefix_bbox.Y, this.prefix_bbox.Width, this.prefix_bbox.Height)
        overall_bbox := Rectangle.Union(!overall_bbox,this.prefix_bbox)
        
        // Treat all the nodes of the sequence
        for i = 0 to (Array.length this.sequence)-1 do 
            let org_bbox = this.bboxes.(i)
            let top_y = bbox_vertalign_offset org_bbox
            this.bboxes.(i) <- Rectangle(org_bbox.X, top_y+org_bbox.Y, org_bbox.Width, org_bbox.Height)
            let org_anch = this.link_anchors.(i)
            this.link_anchors.(i) <- Point(org_anch.X, top_y+org_anch.Y)
            
            overall_bbox := Rectangle.Union(!overall_bbox,this.bboxes.(i))
        done
    
        // Resize the control
        if this.autosize then
            this.ClientSize <- Size((!overall_bbox).Width
                                     + int seq_selection_pensize,
                                     (!overall_bbox).Height + (if this.hScroll.Visible then this.hScroll.Height else 0) 
                                     +  int seq_selection_pensize)

        // Center vertically the entity {links+nodes}
        //let top_y = (clientHeight-overall_bbox.Height)/2
        // ...
        
        // Adjust the horizontal scrollbar
        this.hScroll.Minimum <- 0
        this.hScroll.Maximum <- max 0  (!overall_bbox).Right
        this.hScroll.LargeChange <- this.ClientRectangle.Width
        this.hScroll.SmallChange <- if Array.length this.bboxes > 0 then this.bboxes.(0).Width else 0
                
    member this.add_node node = 
        this.sequence <- Array.concat [this.sequence; [|node|]];
        this.recompute_bbox() // recompute the bounding boxes
        this.hScroll.Value <- max 0 (this.hScroll.Maximum-this.hScroll.LargeChange)
        this.Invalidate()

    member this.remove_last_node() =
        if Array.length this.sequence > 0 then
           begin 
            this.sequence  <- Array.sub this.sequence 0 ((Array.length this.sequence)-1)
            this.recompute_bbox()
            this.Invalidate()
           end
    
    
    // is the control selected?
    val mutable private selection_active : bool
    
    // called by the list container when this pstring control is selected by the user
    member public this.Selection() =
        this.selection_active <- true
        this.Invalidate()
        
    // called by the list container when this pstring control is deselected by the user
    member public this.Deselection() =
        this.selection_active <- false
        this.Invalidate()
    

    member this.InitializeComponent() =
    
        this.nodeEditTextBox <- new System.Windows.Forms.TextBox()
        this.hScroll <- new System.Windows.Forms.HScrollBar()

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
        // hScroll
        // 
        this.hScroll.Dock <- System.Windows.Forms.DockStyle.Bottom;
        this.hScroll.Location <- new System.Drawing.Point(0, 216);
        this.hScroll.Name <- "hScroll";
        this.hScroll.Size <- new System.Drawing.Size(820, 12);
        this.hScroll.TabIndex <- 20;
        this.hScroll.Value <- 0
        this.AutoSize <-  this.autosize; // setting this property will produce a call to this.recompute_bbox()

        // 
        // PstringUsercontrol
        // 
        this.AutoScaleDimensions <- new System.Drawing.SizeF(6.0f, 13.0f);
        this.AutoScaleMode <- System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.nodeEditTextBox);
        this.Controls.Add(this.hScroll);
        this.Name <- "PstringUsercontrol";
        this.Size <- new System.Drawing.Size(820, 228);
        this.ResumeLayout(false);
        this.PerformLayout();


        let NodeFromClientPosition (pos:Point) =
            Array.find_index (function (a:Rectangle) -> a.Contains(pos+Size(this.hScroll.Value,0))) this.bboxes
        
        let ClientPositionFromNode inode =
            let rc = this.bboxes.(inode) in
            Point(rc.X, rc.Y)+Size(-this.hScroll.Value,0)


        let ConfirmLabelEdit() = 
            if this.nodeEditTextBox.Visible then
                this.nodeEditTextBox.Visible <- false
                if this.edited_node< Array.length this.sequence then
                    this.sequence.(this.edited_node) <- { tag=this.sequence.(this.edited_node).tag;
                                                          label=this.nodeEditTextBox.Text;
                                                          link=this.sequence.(this.edited_node).link }
                    this.recompute_bbox()
                    this.Invalidate()
  
        this.MouseClick.Add ( fun e -> 
                    this.nodeEditTextBox.Visible <- false
                    if e.Button = MouseButtons.Left then                   
                        try
                            this.selected_node <- NodeFromClientPosition e.Location;
                            (fst this.nodeClickEventPair)(this, NodeClickEventArgs(this.selected_node))
                            this.Invalidate();
                        with Not_found -> ();
                        
                    else if  e.Button = MouseButtons.Right 
                        && (this.selected_node < Array.length this.sequence) then
                        begin
                            try
                                let sel = NodeFromClientPosition e.Location
                                if sel < this.selected_node then
                                  begin
                                    let node = this.sequence.(this.selected_node)
                                    this.sequence.(this.selected_node) <- {link =this.selected_node-sel;
                                                                           tag = node.tag;
                                                                           label=node.label}
                                  end
                            with Not_found ->
                                    let node = this.sequence.(this.selected_node)
                                    this.sequence.(this.selected_node) <- {link =0;
                                                                                tag = node.tag;
                                                                                label=node.label}
                            this.recompute_bbox()
                            this.Invalidate()
                        end
                    );
                    
        this.MouseDoubleClick.Add(fun e -> 
                        try                        
                            let i = NodeFromClientPosition e.Location
                            ConfirmLabelEdit()
                            this.edited_node <- i
                            this.nodeEditTextBox.Width <- this.bboxes.(i).Width
                            this.nodeEditTextBox.Location <- ClientPositionFromNode i
                            this.nodeEditTextBox.Visible <- true 
                            this.nodeEditTextBox.Text <- this.sequence.(i).label
                            this.nodeEditTextBox.Select()
                        with Not_found -> ()
                        );

        this.nodeEditTextBox.KeyUp.Add( fun e -> match e.KeyCode with
                                                        Keys.Return -> ConfirmLabelEdit()
                                                       | Keys.Escape -> this.nodeEditTextBox.Visible <- false
                                                       | _ -> ()
                )
        this.nodeEditTextBox.Leave.Add(fun _ -> this.nodeEditTextBox.Visible <- false);

        this.Resize.Add(fun _ -> if not this.autosize then this.recompute_bbox()
                                );
    
        this.Leave.Add(fun _ -> this.Invalidate());
        
        this.hScroll.ValueChanged.Add( fun _ -> this.Invalidate());
                    
        this.Paint.Add( fun e -> 
          let graphics = e.Graphics      
          graphics.SmoothingMode <- System.Drawing.Drawing2D.SmoothingMode.AntiAlias

          let mutable rc = this.ClientRectangle
          rc.Width <- rc.Width -  int (seq_selection_pensize/float32 2.0)
          rc.Height <- rc.Height -  int (seq_selection_pensize/float32 2.0)
          if this.selection_active then
            graphics.DrawRectangle(seq_selection_pen,rc)
          else
            graphics.DrawRectangle(seq_unselection_pen,rc)
          
          TextRenderer.DrawText(e.Graphics, prefix, font, this.prefix_bbox, 
                                (if this.selection_active then selectcolor else SystemColors.ControlText), (Enum.combine [TextFormatFlags.VerticalCenter;TextFormatFlags.HorizontalCenter]));
          
          let DrawNode i brush pen arrow_pen = 
              let delta = -this.hScroll.Value
              let label = this.sequence.(i).label 
              and link = this.sequence.(i).link
              let mutable bbox = this.bboxes.(i)
              bbox.Offset(delta, 0);          
              graphics.FillEllipse(brush, bbox )
              graphics.DrawEllipse(pen, bbox )
              TextRenderer.DrawText(e.Graphics, label, font, bbox, 
                                     SystemColors.ControlText,
                                     (Enum.combine [TextFormatFlags.VerticalCenter;TextFormatFlags.HorizontalCenter]));

              if link<>0 then
                begin
                    let src = this.link_anchors.(i)+Size(delta,0)
                    let dst = this.link_anchors.(i-link)+Size(delta,0)
                    let tmp = src + Size(dst)
                    let mid = Point(tmp.X/2,tmp.Y/2- (height_of_link link))
                    graphics.DrawCurve(arrow_pen, [|src;mid;dst|])
                end
          
          for i = 0 to (Array.length this.sequence)-1 do 
            if not this.selection_active || i <> this.selected_node then DrawNode i brush pen arrow_pen
          done
          // Draw the selected node after the other nodes have been drawn
          if this.selection_active && this.selected_node < Array.length this.sequence then
            DrawNode this.selected_node selection_brush selection_pen selection_arrow_pen
        );

    new (pstr:pstring) as this =
        {   components = null;
            nodeEditTextBox = null;
            hScroll = null;
            sequence=pstr;
            bboxes=[||];
            prefix_bbox=Rectangle(0,0,0,0)
            link_anchors=[||];
            edited_node=0;
            selected_node=0;
            autosize=true;
            selection_active=false;
            nodesvalign= Middle;
            // Create the events
            nodeClickEventPair = Microsoft.FSharp.Control.IEvent.create_HandlerEvent()
           }
        then
            this.InitializeComponent(); 
        
  end