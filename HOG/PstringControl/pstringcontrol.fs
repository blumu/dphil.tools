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


type Shapes = ShapeRectangle | ShapeOval

type AutoSizeType = Height | Width | Both

let shape_to_string = function ShapeRectangle -> "ShapeRectangle" | ShapeOval -> "ShapeOval" 
and shape_of_string = function "ShapeRectangle" -> ShapeRectangle | "ShapeOval" -> ShapeOval | _ -> failwith "Unknown shape!"

// Type for nodes occurrences
type pstring_occ = { 
    tag: obj;
    label: string;
    link:int;
    shape:Shapes;
    color:Color
}


// getlink function for sequences of type Pstringcontrol.pstring
let pstr_occ_getlink (nd:pstring_occ) = nd.link

// updatelink function for sequences of type Pstringcontrol.pstring
let pstr_occ_updatelink (nd:pstring_occ) newlink =
    {tag=nd.tag;color=nd.color;shape=nd.shape;label=nd.label;link=newlink}
    //new pstring_occ(nd.tag,nd.label,newlink,nd.shape,nd.color)
     
// create a dummy pstring occurence (which does not correspond to any node of the computation graph)
let create_dummy_occ newlabel newlink = 
    {tag=null;color=Color.Beige;shape=ShapeRectangle;label=newlabel;link=newlink}
    //new pstring_occ(null,newlabel,newlink,ShapeRectangle,Color.Beige)

// create a blank pstring occurrence (which does not correspond to any node of the computation graph)
let create_blank_occ () = 
     {label="..."; link=0; tag = null; shape = ShapeRectangle; color = Color.White}
     //new pstring_occ(null,"...",0,ShapeRectangle,Color.White)
     

// [<field: XmlAttribute("Tag")>]
(*
type pstring_occ = class
                        // these must be immutable otherwise the world collapses...
                        val public tag:obj;
                        val public label:string;
                        val public link:int;
                        val public shape:Shapes;
                        val public color:Color;
                          
                        new () as this = 
                            { tag=null; label="";link=0;shape=ShapeOval;color=Color.Blue}
                            then ()

                        new (ntag,nlabel,nlink,nshape,ncolor) as this = 
                            { tag=ntag; label=nlabel;link=nlink;shape=nshape;color=ncolor}
                            then ()
                       end
*)
type pstring = pstring_occ array


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
let inactive_selectcolor = Color.Gray

let mutable selection_pen = new Pen(selectcolor, float32 2.0)
selection_pen.DashStyle <-  System.Drawing.Drawing2D.DashStyle.Dash
selection_pen.DashOffset <- float32 2.0


let mutable inactive_selection_pen = new Pen(inactive_selectcolor, float32 2.0)
inactive_selection_pen.DashStyle <-  System.Drawing.Drawing2D.DashStyle.Dash
inactive_selection_pen.DashOffset <- float32 2.0


let seq_selection_pensize = float32 2
let mutable seq_selection_pen = new Pen(selectcolor, seq_selection_pensize)
seq_selection_pen.DashStyle <-  System.Drawing.Drawing2D.DashStyle.Dash
seq_selection_pen.DashOffset <- float32 2.0

let arrow_pen = new Pen(Color.Black, penWidth)
arrow_pen.EndCap <- System.Drawing.Drawing2D.LineCap.ArrowAnchor

let selection_arrow_pen = new Pen(selectcolor, float32 2.0)
selection_arrow_pen.EndCap <- System.Drawing.Drawing2D.LineCap.ArrowAnchor

let inactive_selection_arrow_pen = new Pen(inactive_selectcolor, float32 2.0)
inactive_selection_arrow_pen.EndCap <- System.Drawing.Drawing2D.LineCap.ArrowAnchor


let fontHeight:float32 = float32 10.0
let font = new Font("Arial", fontHeight)


let selection_brush = new SolidBrush(selectcolor)
let textBrush = new SolidBrush(Color.Black);
let height_of_link link = link*link_vertical_increment

// The Pointer-string control
type PstringControl = 
  class
    inherit System.Windows.Forms.UserControl as base

    val mutable public components : System.ComponentModel.Container;
        override this.Dispose(disposing) =
            if (disposing && (match this.components with null -> false | _ -> true)) then
              this.components.Dispose();
            base.Dispose(disposing)
            
            
    //// Controls
    val mutable public nodeEditTextBox : System.Windows.Forms.TextBox
    val mutable public hScroll : System.Windows.Forms.HScrollBar
    
    //// Events
    val mutable private nodeClickEventPair : (PstringControl * NodeClickEventArgs -> unit) * IHandlerEvent<NodeClickEventArgs>
    member x.nodeClick with get() = snd x.nodeClickEventPair

    //// Properties
    member public x.Sequence with get() = Array.copy x.sequence
                             and  set s = x.sequence <- Array.copy s
                                          x.selected_node <- min ((Array.length s)-1) x.selected_node
                                          x.recompute_bbox()
                                          x.Invalidate()

    member public x.Occurrence with get i = x.sequence.(i)
                               and  set i o = x.sequence.(i) <- o
                                              x.recompute_bbox()
                                              x.Invalidate()
    
    member public x.Bbox with get i = x.bboxes.(i)

    member public x.Length with get = Array.length x.sequence
                                            
    member public x.SelectedNodeIndex with get() = x.selected_node

    val mutable private autosize : bool
    override x.AutoSize with get() = x.autosize
                         and set b = x.autosize <- b
                                     x.AutoSizeMode <- x.autosizemode

    // This property sets the width above which
    // the scrollbar should appear.
    val mutable private autosize_maxwidth : int
    member x.AutoSizeMaxWidth with get() = x.autosize_maxwidth
                               and set m = x.autosize_maxwidth <- m
                                           x.AutoSizeMode <- x.autosizemode

    val mutable private autosizemode : AutoSizeType
    member x.AutoSizeMode with get() = x.autosizemode
                          and set m = x.autosizemode <- m
                                      x.recompute_bbox()


    val mutable private editable : bool
    member x.Editable with get() = x.editable
                      and set b = x.editable <- b
                                  if not b then x.CancelLabelEdit()
                                     
    val mutable private nodesvalign : VerticalAlignement
    member x.NodesVerticalAlignment with get() = x.nodesvalign
                                      and set a = x.nodesvalign <- a;

    // Property to change the color of the background
    override x.BackColor with set c = x.seq_unselection_pen <- new Pen(c, seq_selection_pensize)
                         and get() = base.BackColor

    //// Private variables
    val mutable private sequence : pstring                  // the sequence of nodes with links to be represented
    val mutable private bboxes : Rectangle array    // bounding boxes of the nodes
    val mutable private prefix_bbox : Rectangle     // bounding box of the prefix string
    val mutable private link_anchors : Point array          // link anchor positions
    val mutable private edited_node : int           // index of the node currently edited
    val mutable private selected_node : int         // index of the selected node

    // Pens and brushes
    val mutable seq_unselection_pen : System.Drawing.Pen // pen of the color of the background    


    //// Constructor
    new (pstr:pstring) as this =
        {   components = null;
            nodeEditTextBox = new System.Windows.Forms.TextBox ();
            hScroll = new System.Windows.Forms.HScrollBar();
            sequence=pstr;
            bboxes=[||];
            prefix_bbox=Rectangle(0,0,0,0)
            link_anchors=[||];
            edited_node=0;
            selected_node= -1;
            autosize=true;
            autosizemode=AutoSizeType.Both;
            autosize_maxwidth=0;
            editable=true;
            control_selected=false;
            seq_unselection_pen=null
            nodesvalign= Middle;
            // Create the events
            nodeClickEventPair = Microsoft.FSharp.Control.IEvent.create_HandlerEvent()
           }
        then
            this.BackColor <- System.Drawing.SystemColors.Control
            this.InitializeComponent(); 
            
    //// Members
    
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
    
        // Adjust the horizontal scrollbar bounds
        this.hScroll.Minimum <- 0
        this.hScroll.Maximum <- max 0  (!overall_bbox).Right

        // Resize the control
        if this.autosize then
            let mutable sizeClient = this.ClientSize
            if this.autosizemode = AutoSizeType.Both || this.autosizemode = AutoSizeType.Width then
                sizeClient.Width <- (!overall_bbox).Width + int seq_selection_pensize
                if this.autosize_maxwidth<>0 && sizeClient.Width > this.autosize_maxwidth then
                    sizeClient.Width <- this.autosize_maxwidth
                    this.hScroll.Visible <- true
                else
                    this.hScroll.Visible <- false
                
            if this.autosizemode = AutoSizeType.Both || this.autosizemode = AutoSizeType.Height then
                sizeClient.Height <- (!overall_bbox).Height + (if this.hScroll.Visible then this.hScroll.Height else 0) 
                                +  int seq_selection_pensize
            this.ClientSize <- sizeClient
            
            // Adjust the horizontal scrollbar increments
            this.hScroll.LargeChange <- this.ClientRectangle.Width
            this.hScroll.SmallChange <- if Array.length this.bboxes > 0 then this.bboxes.(0).Width else 0

        else            
            this.hScroll.Visible <- false
                

    // scroll so that postion x is aligned against the right edge of the client area
    member private this.ScrollAlignRight x =
        this.hScroll.Value <- max 0 (x-this.hScroll.LargeChange)
    
    // make sure the current view of the sequence contain the last node
    member public this.ScrollToEnd() =
        this.ScrollAlignRight this.hScroll.Maximum

    // make sure the current view of the sequence contain the node inode
    member private this.ScrollToMakeNodeVisible inode =
        let rc = this.bboxes.(inode) in
        if this.hScroll.Value <= rc.Left then
            if rc.Right <= this.hScroll.Value + this.hScroll.LargeChange then
              () // the node is visible: do nothing
            else
               // scroll_alignright_node
               this.ScrollAlignRight rc.Right
        else
            this.hScroll.Value <- rc.Left
            

    member public this.add_node node = 
        this.sequence <- Array.concat [this.sequence; [|node|]];
        this.selected_node <- (Array.length this.sequence)-1
        this.recompute_bbox() // recompute the bounding boxes
        // make sure the current view of the sequence contain the newly created node
        this.ScrollToEnd()
        this.Invalidate()
    
    member public this.replace_last_node node =
        let n = Array.length this.sequence 
        if n > 0 then
            this.sequence.(n-1) <- node
            this.selected_node <- n-1
            this.recompute_bbox() // recompute the bounding boxes
            this.ScrollToEnd()
            this.Invalidate()

    member public this.remove_last_occ() =
        let n = Array.length this.sequence 
        if n > 0 then
           begin
            this.selected_node <- min (n-2) this.selected_node
            this.sequence  <- Array.sub this.sequence 0 ((Array.length this.sequence)-1)
            this.recompute_bbox()
            this.Invalidate()
           end

    (** [updatejustifier o j] make the occurrence o points to occurrence j **)
    member public this.updatejustifier o j =
      let occ = this.sequence.(o)
      this.sequence.(o) <- { tag=occ.tag;
                             label=occ.label;
                             link=o-j;
                             shape=occ.shape;
                             color=occ.color}
    
    
    
    member private this.ScrollShift = if this.hScroll.Visible then this.hScroll.Value else 0
    
    member private this.NodeFromClientPosition (pos:Point) =
        Array.find_index (function (a:Rectangle) -> a.Contains(pos+Size(this.ScrollShift,0))) this.bboxes
    
    member private this.ClientPositionFromNode inode =
        let rc = this.bboxes.(inode) in
        Point(rc.X, rc.Y)+Size(-this.ScrollShift,0)
    
    // Cancel the renaming of the node label
    member public this.CancelLabelEdit() = 
        this.nodeEditTextBox.Visible <- false
        this.Select()

    // called to start editing the selected node
    // @param i is the node index
    member public this.EditLabel() =
        let i = this.selected_node
        if i >= 0 && i < Array.length this.sequence then
            this.edited_node <- i
            this.nodeEditTextBox.Width <- this.bboxes.(i).Width
            this.nodeEditTextBox.Location <- this.ClientPositionFromNode i
            this.nodeEditTextBox.Visible <- true 
            this.nodeEditTextBox.Text <- this.sequence.(i).label
            this.nodeEditTextBox.Select()

    // is the control selected?
    val mutable private control_selected : bool
    
    // called by the list container when this pstring control is selected by the user
    member public this.Selection() =
        this.control_selected <- true
        this.Invalidate()
        
    // called by the list container when this pstring control is deselected by the user
    member public this.Deselection() =
        this.control_selected <- false
        this.CancelLabelEdit()
        this.Invalidate()
        
    
    // prevents the arrow keys from being intercepted
    override this.IsInputKey (k:Keys) = match k with
                                          | Keys.Left | Keys.Right -> true
                                          | Keys.Up | Keys.Down -> true
                                          | _ -> base.IsInputKey k
    // form initialization
    member this.InitializeComponent() =
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
        // remove the beeps when the user press Escape or return
        this.nodeEditTextBox.KeyPress.Add (fun e ->  match int_of_char e.KeyChar with
                                                      13 | 27 -> e.Handled <- true;
                                                     | _ -> ()
                                            );
        
        // 
        // hScroll
        // 
        this.hScroll.Dock <- System.Windows.Forms.DockStyle.Bottom;
        this.hScroll.Location <- new System.Drawing.Point(0, 216);
        this.hScroll.Name <- "hScroll";
        this.hScroll.Size <- new System.Drawing.Size(820, 12);
        this.hScroll.TabIndex <- 20;
        this.hScroll.Value <- 0
        this.AutoSize <- this.autosize; // setting this property will produce a call to this.recompute_bbox()

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

            
        // Rename the selected node label with the content of the edit textbox
        let ConfirmLabelEdit() = 
            if this.nodeEditTextBox.Visible then
                this.nodeEditTextBox.Visible <- false
                if this.edited_node< Array.length this.sequence then
                    let cur = this.sequence.(this.edited_node)
                    this.sequence.(this.edited_node) <- { tag=cur.tag;
                                                          label=this.nodeEditTextBox.Text;
                                                          link=cur.link;
                                                          shape=cur.shape;
                                                          color=cur.color }
                    //this.sequence.(this.edited_node).label <- this.nodeEditTextBox.Text;
                    
                    this.Select()
                    this.recompute_bbox()
                    this.Invalidate()
  
        this.MouseDown.Add ( fun e -> 
                    this.CancelLabelEdit()
                    if e.Button = MouseButtons.Left then                   
                        try
                            this.selected_node <- this.NodeFromClientPosition e.Location;
                            this.ScrollToMakeNodeVisible this.selected_node
                            // Triger the node_click event
                            (fst this.nodeClickEventPair)(this, NodeClickEventArgs(this.selected_node))
                            this.Invalidate();
                        with Not_found -> ();
                        
                    else if e.Button = MouseButtons.Right 
                        && this.editable
                        && this.selected_node >= 0 && this.selected_node < Array.length this.sequence then
                        begin
                            try
                                let sel = this.NodeFromClientPosition e.Location
                                if sel < this.selected_node then
                                  begin
                                    let node = this.sequence.(this.selected_node)
                                    //this.sequence.(this.edited_node).link <- this.selected_node-sel;
                                    this.sequence.(this.selected_node) <- {link=this.selected_node-sel;
                                                                           tag=node.tag;
                                                                           label=node.label;
                                                                           shape=node.shape;
                                                                           color=node.color }
                                  end
                            with Not_found ->
                                    let node = this.sequence.(this.selected_node)
                                    //this.sequence.(this.edited_node).link <- 0;
                                    this.sequence.(this.selected_node) <- {link=0;
                                                                           tag=node.tag;
                                                                           label=node.label
                                                                           shape=node.shape;
                                                                           color=node.color}
                            this.recompute_bbox()
                            this.Invalidate()
                        end
                    );
                    
        this.MouseDoubleClick.Add(fun e -> if this.editable then this.EditLabel() );

        this.KeyDown.Add( fun e -> match e.KeyCode with 
                                        (* Selection navigation *)
                                        | Keys.Left when this.selected_node > 0 ->   this.selected_node <- this.selected_node-1
                                                                                     this.ScrollToMakeNodeVisible this.selected_node
                                                                                     this.Invalidate()
                                        | Keys.Right when this.selected_node < (Array.length this.sequence)-1 -> this.selected_node <- this.selected_node+1                                        
                                                                                                                 this.ScrollToMakeNodeVisible this.selected_node
                                                                                                                 this.Invalidate()

                                        (* justifier selection *)
                                        | Keys.PageUp when this.editable && this.selected_node >= 0 && this.selected_node < Array.length this.sequence ->
                                            let node = this.sequence.(this.selected_node)
                                            if this.selected_node -node.link-1 >= 0 then
                                              this.sequence.(this.selected_node) <- {link = node.link+1; tag = node.tag; label=node.label;shape=node.shape;color=node.color}
                                              //this.sequence.(this.selected_node).link <- node.link+1;
                                              this.recompute_bbox()
                                              this.Invalidate()
                                        | Keys.PageDown when this.editable && this.selected_node >= 0 && this.selected_node < Array.length this.sequence ->
                                            let node = this.sequence.(this.selected_node)
                                            if node.link > 0 then
                                              this.sequence.(this.selected_node) <- {link = node.link-1; tag = node.tag; label=node.label;shape=node.shape;color=node.color}
                                              //this.sequence.(this.selected_node).link <- node.link-1;
                                              this.recompute_bbox()
                                              this.Invalidate()
                                                                                                                 
                                        (* edition of the sequence *)
                                        | Keys.Enter when this.editable && not this.nodeEditTextBox.Visible -> this.EditLabel() 
                                        | Keys.Back when this.editable -> this.remove_last_occ()
                                        | _ -> ()
                           )
                                      
        this.nodeEditTextBox.KeyDown.Add( fun e -> match e.KeyCode with
                                                      Keys.Return -> ConfirmLabelEdit()
                                                    | Keys.Escape -> this.CancelLabelEdit()
                                                    | _ -> ()
                                    )

        this.nodeEditTextBox.Leave.Add(fun _ -> this.CancelLabelEdit());

        this.Resize.Add(fun _ -> if not this.autosize then this.recompute_bbox()
                                );
    
        
        this.hScroll.ValueChanged.Add( fun _ -> this.Invalidate());
                    
        this.Paint.Add( fun e -> 
          let graphics = e.Graphics      
          graphics.SmoothingMode <- System.Drawing.Drawing2D.SmoothingMode.AntiAlias

          let active_selection = this.Parent.ContainsFocus
          
          let mutable rc = this.ClientRectangle
          rc.Width <- rc.Width -  int (seq_selection_pensize/float32 2.0)
          rc.Height <- rc.Height -  int (seq_selection_pensize/float32 2.0)
          if this.control_selected then
            if active_selection then
                graphics.DrawRectangle(selection_pen,rc)
            else
                graphics.DrawRectangle(inactive_selection_pen,rc)
          else
            graphics.DrawRectangle(this.seq_unselection_pen,rc)
          
          TextRenderer.DrawText(e.Graphics, prefix, font, this.prefix_bbox, 
                                    (if this.control_selected then 
                                        (if active_selection then selectcolor  else inactive_selectcolor)
                                     else
                                        SystemColors.ControlText),
                                    (Enum.combine [TextFormatFlags.VerticalCenter;TextFormatFlags.HorizontalCenter]));
          
          let DrawNode i brush pen arrow_pen = 
              let delta = -this.ScrollShift
              let node = this.sequence.(i)
              let mutable bbox = this.bboxes.(i)              
              bbox.Offset(delta, 0);
              match node.shape with
                 ShapeRectangle -> graphics.FillRectangle(brush, bbox )
                                   graphics.DrawRectangle(pen, bbox )
                | ShapeOval -> graphics.FillEllipse(brush, bbox )
                               graphics.DrawEllipse(pen, bbox )

              TextRenderer.DrawText(e.Graphics, node.label, font, bbox, 
                                     SystemColors.ControlText,
                                     (Enum.combine [TextFormatFlags.VerticalCenter;TextFormatFlags.HorizontalCenter]));

              if node.link<>0 then
                begin
                    let src = this.link_anchors.(i)+Size(delta,0)
                    let dst = this.link_anchors.(i-node.link)+Size(delta,0)
                    let tmp = src + Size(dst)
                    let mid = Point(tmp.X/2,tmp.Y/2- (height_of_link node.link))
                    graphics.DrawCurve(arrow_pen, [|src;mid;dst|])
                end
          
          for i = 0 to (Array.length this.sequence)-1 do 
            if not this.control_selected || i <> this.selected_node then DrawNode i (new SolidBrush(this.sequence.(i).color)) pen arrow_pen
          done
          // Draw the selected node after the other nodes have been drawn
          if this.control_selected && this.selected_node >= 0 && this.selected_node < Array.length this.sequence then
            DrawNode this.selected_node (new SolidBrush(this.sequence.(this.selected_node).color))
                                        (if active_selection then selection_pen else inactive_selection_pen )
                                        (if active_selection then selection_arrow_pen else inactive_selection_arrow_pen)
        );

        
  end