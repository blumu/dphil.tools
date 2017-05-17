#light

(** $Id: $
	Description: Pointer string Windows Form Control
	Author:		William Blum
**)

module Pstring

open System.Drawing
open System.Windows.Forms

type NodeClickEventArgs(node:int) =
    inherit System.EventArgs()
    member __.Node = node

/// NodeClickEventArgsDelegate: sender, argument -> unit
/// See https://en.wikibooks.org/wiki/F_Sharp_Programming/Events
type NodeClickEventArgsDelegate = delegate of obj * NodeClickEventArgs -> unit

type Shapes = ShapeRectangle | ShapeOval

type AutoSizeType = Height | Width | Both

let shape_to_string = function ShapeRectangle -> "ShapeRectangle" | ShapeOval -> "ShapeOval"
let shape_of_string = function "ShapeRectangle" -> ShapeRectangle | "ShapeOval" -> ShapeOval | _ -> failwith "Unknown shape!"

/// Length of a justifier link. Defined as the distance between a node and its justifier in a traversal
type LinkLength = int

// Type for nodes occurrences
type pstring_occ = {
    /// Value associated with the occurrence
    tag : obj;
    /// Label attached to the occurrence
    label : string;
    /// Link length: the difference between the index of the occurrence and the index of its justifing node in the justified sequence
    link : LinkLength;
    /// Shape used to represent the occurrence
    shape : Shapes;
    /// Color used to represent the occurrence
    color : Color
}

// getlink function for sequences of type Pstringcontrol.pstring
let pstr_occ_getlink (nd:pstring_occ) = nd.link

// updatelink function for sequences of type Pstringcontrol.pstring
let pstr_occ_updatelink (nd:pstring_occ) newlink =
    {tag=nd.tag;color=nd.color;shape=nd.shape;label=nd.label;link=newlink}

// create a dummy pstring occurence (which does not correspond to any node of the computation graph)
let create_dummy_occ newlabel newlink =
    {tag=null;color=Color.Beige;shape=ShapeRectangle;label=newlabel;link=newlink}

// create a blank pstring occurrence (which does not correspond to any node of the computation graph)
let create_blank_occ () =
     {label="..."; link=0; tag = null; shape = ShapeRectangle; color = Color.White}

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


type NodeClickHandler = obj // IHandlerEvent<NodeClickEventArgs>

type VerticalAlignement = Top | Bottom | Middle


/// Prompt message printed between the label and the pointer string itself
let Prompt = "► "
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

let selection_pen =
    new Pen(selectcolor, float32 2.0,
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
            DashOffset = float32 2.0)

let inactive_selection_pen =
    new Pen(inactive_selectcolor,
            float32 2.0,
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
            DashOffset = float32 2.0)


let seq_selection_pensize = float32 2
let seq_selection_pen =
    new Pen(selectcolor,
            seq_selection_pensize,
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
            DashOffset = float32 2.0)

let arrow_pen = new Pen(Color.Black, penWidth, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor)

let selection_arrow_pen = new Pen(selectcolor, float32 2.0, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor)

let inactive_selection_arrow_pen = new Pen(inactive_selectcolor, float32 2.0, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor)


let fontHeight:float32 = float32 10.0
let baseFont = new Font("Arial", fontHeight)
let get_scaled_font (g:Graphics) (scale:float32) =
    new Font(baseFont.FontFamily,
            baseFont.SizeInPoints * scale,
            baseFont.Style,
            GraphicsUnit.Point,
            baseFont.GdiCharSet,
            baseFont.GdiVerticalFont)
 
let get_scaled_rect (scale:float32) (r:Rectangle) =
    let s = float scale
    new Rectangle(int <| System.Math.Ceiling(float r.X * s),
                  int <| System.Math.Ceiling(float r.Y * s),
                  int <| System.Math.Ceiling(float r.Width * s),
                  int <| System.Math.Ceiling(float r.Height * s))

let get_scaled_size (scale:float32) (size:Size) =
    let s = float scale
    new Size(int <| System.Math.Ceiling(float size.Width * s),
             int <| System.Math.Ceiling(float size.Height * s))

let selection_brush = new SolidBrush(selectcolor)
let textBrush = new SolidBrush(Color.Black)

let height_of_link link = link*link_vertical_increment

/// A Windows Form user control used to render pointer-strings
/// The constructor (pstr, dummy) is reserved for internal use, it should not be used directly to create instances of this object.
type PstringControl (pstr:pstring, dummy : unit) =
  class
    inherit System.Windows.Forms.UserControl()

    let mutable autosize_maxwidth : int = 0
    let mutable autosizemode : AutoSizeType = AutoSizeType.Both
    let mutable editable : bool = true
    let mutable autosize : bool = true
    // is the control selected?
    let mutable control_selected : bool = false

    /// the sequence of nodes with links to be represented
    let mutable sequence : pstring = pstr
    
    /// bounding boxes of the nodes
    let mutable bboxes : Rectangle array = [||]

    /// bounding box of the prefix string
    let mutable prefix_bbox : Rectangle = Rectangle(0,0,0,0)
    
    /// link anchor positions
    let mutable link_anchors : Point array = [||]

    /// index of the node currently edited
    let mutable edited_node : int = 0
    
    /// index of the selected node
    let mutable selected_node : int = (Array.length pstr)-1

    /// Pens and brushes
    /// pen of the color of the background
    let mutable seq_unselection_pen : System.Drawing.Pen = null

    /// Controls
    let nodeEditTextBox : System.Windows.Forms.TextBox = new System.Windows.Forms.TextBox ()
    let hScroll : System.Windows.Forms.HScrollBar = new System.Windows.Forms.HScrollBar()

    /// Events
    let nodeClickEvent : Event<PstringControl * NodeClickEventArgs> = new Event<PstringControl * NodeClickEventArgs>()

    /// Default constructor. This constructor must be used
    /// when creating new instances of this object instead of "new (pstr:pstring, ?dummy : unit)".
    new (pstr:pstring, label:string) as this =
        // The following trick is need to be able to define a "then" clause to call InitializeComponent() 
        // only after the object is constructed.
        // If using a "do" statement instead, this would result in the following exception at initialization time:
        // "InvalidOperationException: the initialization of an object or value resulted in an object or value being accessed recursively before it was fully initialized"
        new PstringControl (pstr, ())
        then
            this.Label <- label
            this.BackColor <- System.Drawing.SystemColors.Control
            this.InitializeComponent()

    member public x.components : System.ComponentModel.Container = null
    
    override this.Dispose(disposing) =
        if disposing && (match this.components with null -> false | _ -> true) then
            this.components.Dispose()
        base.Dispose(disposing)

    [<CLIEvent>]
    member x.nodeClick with get() = nodeClickEvent.Publish

    //// Properties
    member public x.Sequence with get() = Array.copy sequence
                             and  set s = sequence <- Array.copy s
                                          selected_node <- min ((Array.length s)-1) selected_node
                                          x.recompute_bbox()
                                          x.Invalidate()

    member public x.Occurrence with get i = sequence.[i]
                               and  set i o = sequence.[i] <- o
                                              x.recompute_bbox()
                                              x.Invalidate()

    member public x.Bbox with get i = bboxes.[i]

    member public x.Length with get () = Array.length sequence

    member public x.SelectedNodeIndex with get() = selected_node

    override x.AutoSize with get() = autosize
                         and set b = autosize <- b
                                     x.recompute_bbox()

    // This property sets the width above which
    // the scrollbar should appear.
    member x.AutoSizeMaxWidth with get() = autosize_maxwidth
                               and set m = autosize_maxwidth <- m
                                           x.recompute_bbox()

    member x.AutoSizeMode with get() = autosizemode
                          and set m = autosizemode <- m
                                      x.recompute_bbox()


    member x.Editable with get() = editable
                      and set b = editable <- b
                                  if not b then x.CancelLabelEdit()

    member val NodesVerticalAlignment = Middle with get, set

    member val Label = "Test" with get, set

    // Property to change the color of the background
    override x.BackColor with set c = 
                            seq_unselection_pen <- new Pen(c, seq_selection_pensize)
                            base.BackColor <- c
                         and get() = base.BackColor

    /// Scaling factor used for rendering
    member val ScaleFactor = 1.0f with get, set

    //// Methods

    member private this.recompute_bbox() =
        let graphics = this.CreateGraphics()

        bboxes <- Array.create (Array.length sequence) (System.Drawing.Rectangle(0,0,0,0))
        link_anchors <- Array.create (Array.length sequence) (Point(0,0))


        let highest_bbox = ref 0

        // Compute the bounding boxes and the links anchors positions
        let x = ref (int seq_selection_pensize + leftmargin)
        let longest_link = ref 0

        // place some text horizontally and return a pair containing
        // its bounding box and the text dimensions
        let place_in_hbox txt =
            use font = get_scaled_font graphics this.ScaleFactor
            let txt_dim = graphics.MeasureString(txt, font);
            let bbox = Rectangle(!x, 0, int txt_dim.Width + 2*node_padding, int txt_dim.Height + 2*node_padding)
            x:= !x + int txt_dim.Width + internodesep
            highest_bbox := max !highest_bbox bbox.Height
            bbox,txt_dim


        // place the prefix string
        let prefix_string = this.Label + Prompt
        prefix_bbox <- fst (place_in_hbox prefix_string);

        for i = 0 to (Array.length sequence)-1 do
          let label = sequence.[i].label

          longest_link := max !longest_link (sequence.[i].link)

          let bbox,textdim = place_in_hbox label
          bboxes.[i] <- bbox
          link_anchors.[i] <- Point(bbox.Left+(int (textdim.Width/f2)), 0)
        done;

        //////////////////////
        // Vertically realignment of the nodes bounding boxes,

        // top of the part containing the nodes (just underneath the links)
        let nodespart_top = int seq_selection_pensize + height_of_link !longest_link

        // compute the overall bounding box
        let overall_bbox = ref (Rectangle(0,0,1,nodespart_top)) // account for the space used by the links

        // compute the vertical offset for the nodes to be aligned as the user requested
        let bbox_vertalign_offset (bbox:Rectangle) = nodespart_top +
                                                     match this.NodesVerticalAlignment with
                                                          Middle -> (!highest_bbox-bbox.Height)/2
                                                        | Bottom -> !highest_bbox-bbox.Height
                                                        | Top -> 0

        // First, treat the bbox of the prefix string
        prefix_bbox <- Rectangle(prefix_bbox.X, (bbox_vertalign_offset prefix_bbox)+prefix_bbox.Y, prefix_bbox.Width, prefix_bbox.Height)
        overall_bbox := Rectangle.Union(!overall_bbox,prefix_bbox)

        // Treat all the nodes of the sequence
        for i = 0 to (Array.length sequence)-1 do
            let org_bbox = bboxes.[i]
            let top_y = bbox_vertalign_offset org_bbox
            bboxes.[i] <- Rectangle(org_bbox.X, top_y+org_bbox.Y, org_bbox.Width, org_bbox.Height)
            let org_anch = link_anchors.[i]
            link_anchors.[i] <- Point(org_anch.X, top_y+org_anch.Y)

            overall_bbox := Rectangle.Union(!overall_bbox,bboxes.[i])
        done

        // Adjust the horizontal scrollbar bounds
        let overall_box_scaled = get_scaled_rect this.ScaleFactor !overall_bbox
        hScroll.Minimum <- 0
        hScroll.Maximum <- max 0  overall_box_scaled.Right

        // Resize the control
        if this.AutoSize then
            let width =
                match this.AutoSizeMode with
                | AutoSizeType.Both | AutoSizeType.Width ->
                    let w = (!overall_bbox).Width + int seq_selection_pensize
                    if this.AutoSizeMaxWidth <> 0 && w > this.AutoSizeMaxWidth then
                        hScroll.Visible <- true
                        this.AutoSizeMaxWidth
                    else
                        hScroll.Visible <- false
                        w
                | _ ->
                    this.ClientSize.Width

            let height =
                if this.AutoSizeMode = AutoSizeType.Both || this.AutoSizeMode = AutoSizeType.Height then
                    (!overall_bbox).Height + (if hScroll.Visible then hScroll.Height else 0) +  int seq_selection_pensize
                else
                    this.ClientSize.Height

            this.ClientSize <- get_scaled_size this.ScaleFactor (Size(width, height))

            // Adjust the horizontal scrollbar increments
            hScroll.LargeChange <- this.ClientRectangle.Width
            hScroll.SmallChange <- if not <| Seq.isEmpty bboxes then 
                                            let incrementBox = get_scaled_rect this.ScaleFactor bboxes.[0]
                                            incrementBox.Width
                                        else
                                            0

        else
            hScroll.Visible <- false


    // scroll so that postion x is aligned against the right edge of the client area
    member private this.ScrollAlignRight x =
        hScroll.Value <- max 0 (x-hScroll.LargeChange)

    // make sure the current view of the sequence contain the last node
    member public this.ScrollToEnd() =
        this.ScrollAlignRight hScroll.Maximum

    // make sure the current view of the sequence contain the node inode
    member private this.ScrollToMakeNodeVisible inode =
        let rc = bboxes.[inode] in
        if hScroll.Value <= rc.Left then
            if rc.Right <= hScroll.Value + hScroll.LargeChange then
              () // the node is visible: do nothing
            else
               // scroll_alignright_node
               this.ScrollAlignRight rc.Right
        else
            hScroll.Value <- rc.Left


    member public this.add_node node =
        sequence <- Array.concat [sequence; [|node|]];
        selected_node <- (Array.length sequence)-1
        this.recompute_bbox() // recompute the bounding boxes
        // make sure the current view of the sequence contain the newly created node
        this.ScrollToEnd()
        this.Invalidate()

    member public this.replace_last_node node =
        let n = Array.length sequence
        if n > 0 then
            sequence.[n-1] <- node
            selected_node <- n-1
            this.recompute_bbox() // recompute the bounding boxes
            this.ScrollToEnd()
            this.Invalidate()

    member public this.remove_last_occ() =
        let n = Array.length sequence
        if n > 0 then
           begin
            selected_node <- min (n-2) selected_node
            sequence  <- Array.sub sequence 0 ((Array.length sequence)-1)
            this.recompute_bbox()
            this.Invalidate()
           end

    (** [updatejustifier o j] make the occurrence o points to occurrence j **)
    member public this.updatejustifier o j =
      let occ = sequence.[o]
      sequence.[o] <- { tag=occ.tag;
                             label=occ.label;
                             link=o-j;
                             shape=occ.shape;
                             color=occ.color}



    member private this.ScrollShift = if hScroll.Visible then hScroll.Value else 0

    member private this.tryGetNodeFromClientPosition (pos:Point) =
        Array.tryFindIndex (function (a:Rectangle) -> (get_scaled_rect this.ScaleFactor a).Contains(pos+Size(this.ScrollShift,0))) bboxes

    member private this.ClientPositionFromNode inode =
        let rc = bboxes.[inode] in
        Point(rc.X, rc.Y)+Size(-this.ScrollShift,0)

    // Cancel the renaming of the node label
    member public this.CancelLabelEdit() =
        nodeEditTextBox.Visible <- false
        this.Select()

    // called to start editing the selected node
    // @param i is the node index
    member public this.EditLabel() =
        let i = selected_node
        if i >= 0 && i < Array.length sequence then
            edited_node <- i
            nodeEditTextBox.Width <- bboxes.[i].Width
            nodeEditTextBox.Location <- this.ClientPositionFromNode i
            nodeEditTextBox.Visible <- true
            nodeEditTextBox.Text <- sequence.[i].label
            nodeEditTextBox.Select()

    // called by the list container when this pstring control is selected by the user
    member public this.Selection() =
        control_selected <- true
        this.Invalidate()

    // called by the list container when this pstring control is deselected by the user
    member public this.Deselection() =
        control_selected <- false
        this.CancelLabelEdit()
        this.Invalidate()


    // prevents the arrow keys from being intercepted
    override this.IsInputKey (k:Keys) = match k with
                                          | Keys.Left | Keys.Right -> true
                                          | Keys.Up | Keys.Down -> true
                                          | _ -> base.IsInputKey k
    // form initialization
    member this.InitializeComponent() =
        base.SuspendLayout();
        //
        // nodeEditTextBox
        //
        nodeEditTextBox.AcceptsReturn <- true
        nodeEditTextBox.CausesValidation <- false
        nodeEditTextBox.Location <- new System.Drawing.Point(656, 17)
        nodeEditTextBox.Name <- "nodeEditTextBox"
        nodeEditTextBox.Size <- new System.Drawing.Size(66, 20)
        nodeEditTextBox.TabIndex <- 21
        nodeEditTextBox.Visible <- false
        // remove the beeps when the user press Escape or return
        nodeEditTextBox.KeyPress.Add (fun e ->  match System.Convert.ToInt32 e.KeyChar with
                                                13 | 27 -> e.Handled <- true
                                                | _ -> ())

        //
        // hScroll
        //
        hScroll.Dock <- System.Windows.Forms.DockStyle.Bottom
        hScroll.Location <- new System.Drawing.Point(0, 216)
        hScroll.Name <- "hScroll"
        hScroll.Size <- new System.Drawing.Size(820, 12)
        hScroll.TabIndex <- 20
        hScroll.Value <- 0
        this.recompute_bbox()

        //
        // PstringUsercontrol
        //
        this.AutoScaleDimensions <- new System.Drawing.SizeF(6.0f, 13.0f);
        this.AutoScaleMode <- System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(nodeEditTextBox);
        this.Controls.Add(hScroll);
        this.Name <- "PstringUsercontrol";
        this.Size <- new System.Drawing.Size(820, 228);
        this.ResumeLayout(false);
        this.PerformLayout();


        // Rename the selected node label with the content of the edit textbox
        let ConfirmLabelEdit() =
            if nodeEditTextBox.Visible then
                nodeEditTextBox.Visible <- false
                if edited_node< Array.length sequence then
                    let cur = sequence.[edited_node]
                    sequence.[edited_node] <- { tag=cur.tag;
                                                label=nodeEditTextBox.Text;
                                                link=cur.link;
                                                shape=cur.shape;
                                                color=cur.color }

                    this.Select()
                    this.recompute_bbox()
                    this.Invalidate()

        /// Zoom using mouse wheel. 
        /// Could not find a way to avoid interferance with parent control scrolling. Switched to keypress handler instead.
        //this.MouseWheel.Add(fun e ->
        //                        this.scaleFactor <- max 0.3f (this.scaleFactor + (float32 e.Delta)/1000.0f)
        //                        this.recompute_bbox()
        //                        this.Invalidate())

        this.MouseDown.Add (fun e ->
                    this.CancelLabelEdit()
                    match this.tryGetNodeFromClientPosition e.Location with
                    | Some clickedNodeIndex when e.Button = MouseButtons.Left ->
                        selected_node <- clickedNodeIndex
                        this.ScrollToMakeNodeVisible selected_node
                        // Trigger the node_click event
                        nodeClickEvent.Trigger(this, NodeClickEventArgs(selected_node))
                        this.Invalidate();

                    | Some clickedNodeIndex when e.Button = MouseButtons.Right
                            && editable
                            && selected_node >= 0 && selected_node < Array.length sequence ->
                            if clickedNodeIndex < selected_node then
                                begin
                                    let node = sequence.[selected_node]
                                    sequence.[selected_node] <- {link=selected_node-clickedNodeIndex;
                                                                 tag=node.tag;
                                                                 label=node.label;
                                                                 shape=node.shape;
                                                                 color=node.color }
                                end
                            this.recompute_bbox()
                            this.Invalidate()
                    | _ -> ()

                    );

        this.MouseDoubleClick.Add(fun e -> if editable then this.EditLabel() );

        this.KeyDown.Add( fun e -> match e.KeyCode with
                                        (* Selection navigation *)
                                        | Keys.Left when selected_node > 0 ->   selected_node <- selected_node-1
                                                                                this.ScrollToMakeNodeVisible selected_node
                                                                                this.Invalidate()
                                        | Keys.Right when selected_node < (Array.length sequence)-1 -> selected_node <- selected_node+1
                                                                                                       this.ScrollToMakeNodeVisible selected_node
                                                                                                       this.Invalidate()

                                        (* justifier selection *)
                                        | Keys.PageUp when editable && selected_node >= 0 && selected_node < Array.length sequence ->
                                            let node = sequence.[selected_node]
                                            if selected_node-node.link-1 >= 0 then
                                              sequence.[selected_node] <- {link = node.link+1; tag = node.tag; label=node.label;shape=node.shape;color=node.color}
                                              this.recompute_bbox()
                                              this.Invalidate()
                                        | Keys.PageDown when editable && selected_node >= 0 && selected_node < Array.length sequence ->
                                            let node = sequence.[selected_node]
                                            if node.link > 0 then
                                              sequence.[selected_node] <- {link = node.link-1; tag = node.tag; label=node.label;shape=node.shape;color=node.color}
                                              this.recompute_bbox()
                                              this.Invalidate()

                                        (* edition of the sequence *)
                                        | Keys.Enter when editable && not nodeEditTextBox.Visible -> this.EditLabel()
                                        | Keys.Back when editable -> this.remove_last_occ()

                                        (* zoom out *)
                                        | Keys.Subtract ->
                                            this.ScaleFactor <- max 0.3f (this.ScaleFactor - 0.01f)
                                            this.recompute_bbox()
                                            this.Invalidate()
                                        
                                        (* zoom in *)
                                        | Keys.Add ->
                                            this.ScaleFactor <- max 0.3f (this.ScaleFactor + 0.01f)
                                            this.recompute_bbox()
                                            this.Invalidate()

                                        | _ -> ()
                           )

        nodeEditTextBox.KeyDown.Add( fun e -> match e.KeyCode with
                                                      Keys.Return -> ConfirmLabelEdit()
                                                    | Keys.Escape -> this.CancelLabelEdit()
                                                    | _ -> ()
                                    )

        nodeEditTextBox.Leave.Add(fun _ -> this.CancelLabelEdit());

        this.Resize.Add(fun _ -> if not autosize then this.recompute_bbox()
                                );


        hScroll.ValueChanged.Add( fun _ -> this.Invalidate());

        this.Paint.Add( fun e ->
          let graphics = e.Graphics
          graphics.SmoothingMode <- System.Drawing.Drawing2D.SmoothingMode.AntiAlias
         

          let active_selection = this.Parent.ContainsFocus

          do
              let rc = new Rectangle(this.ClientRectangle.Location, 
                                    new Size(this.ClientRectangle.Size.Width - int (seq_selection_pensize/float32 2.0),
                                             this.ClientRectangle.Size.Height - int (seq_selection_pensize/float32 2.0)))
              if control_selected then
                if active_selection then
                    graphics.DrawRectangle(selection_pen,rc)
                else
                    graphics.DrawRectangle(inactive_selection_pen,rc)
              else
                graphics.DrawRectangle(seq_unselection_pen,rc)

          graphics.ScaleTransform(this.ScaleFactor, this.ScaleFactor)
        
          use font = get_scaled_font graphics this.ScaleFactor
          let prefix_string = this.Label + Prompt
          TextRenderer.DrawText(e.Graphics, prefix_string, font, get_scaled_rect this.ScaleFactor prefix_bbox,
                                    (if control_selected then
                                        (if active_selection then selectcolor  else inactive_selectcolor)
                                     else
                                        SystemColors.ControlText),
                                    (TextFormatFlags.VerticalCenter ||| TextFormatFlags.HorizontalCenter));

          let DrawNode i brush pen arrow_pen =
              let delta = -this.ScrollShift
              let node = sequence.[i]
              let mutable bbox = bboxes.[i]
              bbox.Offset(delta, 0);
              match node.shape with
                 ShapeRectangle -> graphics.FillRectangle(brush, bbox )
                                   graphics.DrawRectangle(pen, bbox )
                | ShapeOval -> graphics.FillEllipse(brush, bbox )
                               graphics.DrawEllipse(pen, bbox )

              TextRenderer.DrawText(e.Graphics, node.label, font, get_scaled_rect this.ScaleFactor bbox,
                                     SystemColors.ControlText,
                                     ( TextFormatFlags.VerticalCenter ||| TextFormatFlags.HorizontalCenter));

              if node.link<>0 then
                begin
                    let src = link_anchors.[i]+Size(delta,0)
                    let dst = link_anchors.[i-node.link]+Size(delta,0)
                    let tmp = src + Size(dst)
                    let mid = Point(tmp.X/2,tmp.Y/2- (height_of_link node.link))
                    graphics.DrawCurve(arrow_pen, [|src;mid;dst|])
                end

          for i = 0 to (Array.length sequence)-1 do
            if not control_selected || i <> selected_node then DrawNode i (new SolidBrush(sequence.[i].color)) pen arrow_pen
          done
          // Draw the selected node after the other nodes have been drawn
          if control_selected && selected_node >= 0 && selected_node < Array.length sequence then
            DrawNode selected_node (new SolidBrush(sequence.[selected_node].color))
                                   (if active_selection then selection_pen else inactive_selection_pen )
                                   (if active_selection then selection_arrow_pen else inactive_selection_arrow_pen)
        );


  end