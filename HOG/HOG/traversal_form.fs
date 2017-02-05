(** Description: Traversal view.
    This module contains the view used to represent and manipulate traversals, as well as the logic 
    to let the user "play the traversal game" on a given copmutation graph.

    Author: William Blum
**)
module Traversal_form

open FSharp.Compatibility.OCaml
open Common
open System.Xml
open System.Drawing
open System.Windows.Forms
open Lnf
open Compgraph
open GUI
open Pstring


/////////////////////// Some usefull functions

(** assertions used for XML parsing **)
let assert_xmlname nameexpected (node:XmlNode) =
  if node.Name <> nameexpected then
    failwith "Bad worksheet xml file!"

let assert_xmlnotnull (node:XmlNode) =
  if isNull node then
    failwith "Bad worksheet xml file!"


(** Map a player to a node shape **)
let player_to_shape = function Proponent -> ShapeRectangle
                               | Opponent -> ShapeOval
(** Map a player to a node color **)
let player_to_color = function Proponent -> Color.Coral
                               | Opponent -> Color.CornflowerBlue
(***** Pstring functions *****)

(** Convert a colors of type System.Drawing.Color to a color of type Microsoft.Msagl.Drawing.Color **)
let sysdrawingcolor_2_msaglcolor (color:System.Drawing.Color) :Microsoft.Msagl.Drawing.Color =
    Microsoft.Msagl.Drawing.Color(color.R,color.G,color.B)

(** Convert a shape of type Pstring.Shapes to a shape of type Microsoft.Msagl.Drawing.Shape **)
let pstringshape_2_msaglshape =
    function  Pstring.Shapes.ShapeRectangle -> Microsoft.Msagl.Drawing.Shape.Box
             | Pstring.Shapes.ShapeOval -> Microsoft.Msagl.Drawing.Shape.Circle // Microsoft.Msagl.Drawing.Shape.Diamond

(** Create a pstring occurrence from a gennode **)
let occ_from_gennode (compgraph:computation_graph) gennode lnk =
    let player = compgraph.gennode_player gennode
    let label =
        match gennode with
        | GhostLambda i -> string_of_int i
        | GhostVariable i -> string_of_int i
        | Custom -> "?"
        | ValueLeaf(nodeindex,v) -> (string_of_int nodeindex)^"_{"^(compgraph.node_label_with_idsuffix nodeindex)^"}"
        | InternalNode(nodeindex) -> compgraph.node_label_with_idsuffix nodeindex
    { label = label;
      tag = box gennode; // Use the tag field to store the generalized graph node
      link = lnk;
      shape=player_to_shape player;
      color=player_to_color player
    }


(** Return the generalized graph node of a given occurrence in a Pstringcontrol.pstring sequence **)
let pstr_occ_getnode (nd:pstring_occ) =
    if isNull nd.tag then
        failwith "This is not a valid justified sequence. Some node-occurrence does not belong to the computation graph/tree."
    else
        (unbox nd.tag:gen_node)


/////////////////////// Sequence transformations

(**** Sequence transformations specialized for sequences of type Pstringcontrol.pstring  ****)

(** P-View **)
let pstrseq_pview_at gr seq i = Traversal.seq_Xview
                                     gr
                                     Proponent
                                     pstr_occ_getnode        // getnode function
                                     pstr_occ_getlink        // getlink function
                                     pstr_occ_updatelink     // update link function
                                     seq
                                     i // compute the P-view at the occurrence i

let pstrseq_pview gr seq = pstrseq_pview_at gr seq ((Array.length seq)-1)


(** O-View **)
let pstrseq_oview_at gr seq i = Traversal.seq_Xview
                                     gr
                                     Opponent
                                     pstr_occ_getnode        // getnode function
                                     pstr_occ_getlink        // getlink function
                                     pstr_occ_updatelink     // update link function
                                     seq
                                     i // compute the O-view at the occurrence i

let pstrseq_oview gr seq = pstrseq_oview_at gr seq ((Array.length seq)-1)

(** Subterm projection with respect to a reference root node **)
let pstrseq_subtermproj gr = Traversal.subtermproj gr
                                         pstr_occ_getnode
                                         pstr_occ_getlink
                                         pstr_occ_updatelink

(** Hereditary projection **)
let pstrseq_herproj = Traversal.heredproj pstr_occ_getlink
                                pstr_occ_updatelink

(** Prefixing **)
let pstrseq_prefix seq at = let l = Array.length seq in if at <0 || at >= l then [||] else Array.sub seq 0 (at+1)


(** Traversal star **)
let pstrseq_star gr = Traversal.star gr
                           pstr_occ_getnode
                           pstr_occ_getlink
                           pstr_occ_updatelink


(** Traversal extension **)
let pstrseq_ext gr = Traversal.extension gr
                               pstr_occ_getnode
                               pstr_occ_getlink
                               pstr_occ_updatelink
                               Pstring.create_dummy_occ

/////////////////////// MSAGL graph generation

(** Set the style attributes for a node of the graph
    @param node_2_color a mapping from compgraph nodes to colors
    @param node_2_shape a mapping from compgraph nodes to shapes
    @param gr is the computation graph
    @param nodecontent is the node of the computation graph
    @param node is the MSAGL graph node
    **)
let msaglgraphnode_set_attributes_styles node_2_color node_2_shape (gr:computation_graph) (cgnode:nodecontent) (node:Microsoft.Msagl.Drawing.Node) =
    node.Attr.Shape <- pstringshape_2_msaglshape (node_2_shape cgnode)
    node.Attr.FillColor <- sysdrawingcolor_2_msaglcolor (node_2_color cgnode)
;;

(** Set the attributes for a node of the graph
    @param node_2_color a mapping from compgraph nodes to colors
    @param node_2_shape a mapping from compgraph nodes to shapes
    @param gr is the computation graph
    @param i is the node number in the computation graph
    @param node is the MSAGL graph node
    **)
let msaglgraphnode_set_attributes node_2_color node_2_shape (gr:computation_graph) i (node:Microsoft.Msagl.Drawing.Node) =
    node.Attr.Id <- string_of_int i
    node.Attr.Label <- gr.node_label_with_idsuffix i
    msaglgraphnode_set_attributes_styles node_2_color node_2_shape gr gr.nodes.(i) node
    match gr.nodes.(i) with
      | NCntTm(tm) -> node.Attr.LabelMargin <- 10;
      | NCntApp
      | NCntVar(_)
      | NCntAbs(_,_) -> ()
;;

(** Example of coloring function for computation graph nodes **)
let grnode_2_color nd = player_to_color (graphnode_player nd)

(** Example of shaping functions for computation graph nodes **)
let grnode_2_shape nd = player_to_shape (graphnode_player nd)

(* Id used for the single placeholder MSAGL node representing all the ghost lambda-nodes
 involved during a traversal game *)
let MsaglGraphGhostButtonId = "-1"

(** [compgraph_to_graphview node_2_color node_2_shape gr] creates the MSAGL graph view of a computation graph.
    @param node_2_color a mapping from compgraph nodes to colors
    @param node_2_shape a mapping from compgraph nodes to shapes
    @param gr the computation graph
    @return the MSAGL graph object representing the computation graph
**)
let compgraph_to_graphview node_2_color node_2_shape (gr:computation_graph) =
    (* create a graph object *)
    let msaglgraph = new Microsoft.Msagl.Drawing.Graph("graph") in

    (* add the nodes to the graph *)
    gr.nodes |> Array.iteri 
        (fun k _ -> msaglgraph.AddNode (string_of_int k)
                    |> msaglgraphnode_set_attributes
                        grnode_2_color
                        grnode_2_shape
                        gr
                        k)
    
    (* Add one extra "ghost" node to be used to play ghost moves in a traversal game. *)
    let ghostNode = msaglgraph.AddNode MsaglGraphGhostButtonId
    ghostNode.Attr.Id <- MsaglGraphGhostButtonId
    ghostNode.Attr.Shape <- pstringshape_2_msaglshape Shapes.ShapeRectangle
    ghostNode.Attr.FillColor <- sysdrawingcolor_2_msaglcolor (Color.Azure)
    ghostNode.Attr.Label <- "Ghost"


    (* add the edges to the graph *)
    let addtargets source targets =
        let source_id = string_of_int source in
        let aux i target =
            let target_id = string_of_int target in
            let edge = msaglgraph.AddEdge(source_id,target_id) in
            (match gr.nodes.(source) with
                NCntApp -> edge.Attr.Label <- string_of_int i;
                               (* Highlight the first edge if it is an @-node (going to the operator) *)
                               if i = 0 then edge.Attr.Color <- Microsoft.Msagl.Drawing.Color.Green;
               | NCntAbs(_)  -> ();
               | _ -> edge.Attr.Label <- string_of_int (i+1);
            )

        in
        List.iteri aux targets
    in
    Array.iteri addtargets gr.edges;
    msaglgraph
;;


(** Loads a window showing a computation graph and permitting the user to export it to latex. **)
let ShowCompGraphWindow mdiparent filename compgraph (lnfrules:lnfrule list) =
    // create a form
    let form = new System.Windows.Forms.Form()
    let viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer()
    let panel1 = new System.Windows.Forms.Panel();
    let buttonLatex = new System.Windows.Forms.Button()

    form.SuspendLayout();
    form.MdiParent <- mdiparent;
    form.Text <- "Computation graph of "^filename;
    form.Size <- Size(700,800);
    
    buttonLatex.Location <- new System.Drawing.Point(1, 1)
    buttonLatex.Name <- "button1"
    buttonLatex.Size <- new System.Drawing.Size(267, 23)
    buttonLatex.TabIndex <- 2
    buttonLatex.Text <- "Export to Latex"
    buttonLatex.UseVisualStyleBackColor <- true
    buttonLatex.Click.Add(fun _ -> Texexportform.LoadExportGraphToLatexWindow mdiparent lnfrules)

    // create a viewer object
    panel1.SuspendLayout();
    panel1.Anchor <- System.Windows.Forms.AnchorStyles.Top |||
                     System.Windows.Forms.AnchorStyles.Bottom |||
                     System.Windows.Forms.AnchorStyles.Left |||
                     System.Windows.Forms.AnchorStyles.Right;
    panel1.Controls.Add(viewer);
    panel1.Location <- new System.Drawing.Point(1, 29);
    panel1.Margin <- new System.Windows.Forms.Padding(2, 2, 2, 2);
    panel1.Name <- "panel1";
    panel1.Size <- new System.Drawing.Size(972, 505);
    panel1.TabIndex <- 4;

    // bind the graph to the viewer
    let nd_2_col = function  NCntApp -> System.Drawing.Color.White
                            | NCntTm(tm) -> System.Drawing.Color.Salmon;
                            | NCntVar(x) -> System.Drawing.Color.Green;
                            | NCntAbs("",vars) -> System.Drawing.Color.White
                            | NCntAbs(nt,vars) -> System.Drawing.Color.Yellow;


    let nd_2_shape = function NCntVar(_) | NCntAbs(_,_) | NCntApp -> Pstring.Shapes.ShapeOval
                                | NCntTm(_) -> Pstring.Shapes.ShapeRectangle

    viewer.Graph <- compgraph_to_graphview nd_2_col nd_2_shape compgraph;
    viewer.AsyncLayout <- false;
    viewer.AutoScroll <- true;
    viewer.BackwardEnabled <- false;
    viewer.Dock <- System.Windows.Forms.DockStyle.Fill;
    viewer.ForwardEnabled <- false;
    viewer.Location <- new System.Drawing.Point(0, 0);
    viewer.MouseHitDistance <- 0.05;
    viewer.Name <- "gViewer";
    viewer.NavigationVisible <- true;
    viewer.PanButtonPressed <- false;
    viewer.SaveButtonVisible <- true;
    viewer.Size <- new System.Drawing.Size(674, 505);
    viewer.TabIndex <- 3;
    viewer.ZoomF <- 1.0;
    viewer.ZoomFraction <- 0.5;
    viewer.ZoomWindowThreshold <- 0.05;

    //associate the viewer with the form
    form.ClientSize <- new System.Drawing.Size(970, 532);

    form.Controls.Add(buttonLatex);
    form.Controls.Add(panel1);
    panel1.ResumeLayout(false);
    form.ResumeLayout(false);

    //show the form
    ignore(form.Show())
;;

/// Worksheet parameters that are serialized to XML
type WorksheetParam = 
    {
        graphsource_filename:string;
        compgraph:computation_graph;
        lnfrules: lnfrule list;
        msaglviewer: Microsoft.Msagl.GraphViewerGdi.GViewer
        seqflowpanel: System.Windows.Forms.FlowLayoutPanel
        labinfo: System.Windows.Forms.Label
    }


/// Represent an object from the Worksheet
[<AbstractClass>]
type WorksheetObject =
  class
    val ws : WorksheetParam

    new(nws) = {ws = nws}

    abstract Control : System.Windows.Forms.Control
    abstract Clone : unit -> WorksheetObject
    abstract Selection : unit -> unit
    abstract Deselection : unit -> unit
    abstract Refocus : unit -> unit
    abstract OnCompGraphNodeMouseDown : MouseEventArgs -> Microsoft.Msagl.Drawing.Node  -> unit

    // [ToXmlElement xmldoc] converts the object into an XML element.
    // @param xmldoc is the xmldocument
    // @return the XML element created
    abstract ToXmlElement : System.Xml.XmlDocument -> System.Xml.XmlElement

    // adujust the scrolling of the flow container so that the end of the sequence is visible
    member x.flush_flowcontainer_to_right() =
        ()
        //let viewwidth = x.ws.seqflowpanel.ClientSize.Width // HorizontalScroll.LargeChange
        //form.seqflowPanel.HorizontalScroll.Maximum -
        //base.ws.seqflowpanel.HorizontalScroll.Value <- max 0 (base.pstrcontrol.Width - viewwidth)
        //let p = base.ws.seqflowpanel.AutoScrollPosition in
        //base.ws.seqflowpanel.AutoScrollPosition <- Point((max 0 (base.pstrcontrol.Width - viewwidth)), p.Y)
          //x.ws.seqflowpanel.AutoScrollPosition <- //Point(min 0 (viewwidth-x.pstrcontrol.Width), x.ws.seqflowpanel.AutoScrollPosition.Y)
                                                    //Point(x.ws.seqflowpanel.AutoScrollPosition.Y+10, x.ws.seqflowpanel.AutoScrollPosition.Y)
        //hscr.Value <- obj.Control.Width

  end

/// Represent a justification sequence object
type PstringObject =
  class
    inherit WorksheetObject
    val pstrcontrol : Pstring.PstringControl
    new (nws,pstr:pstring) = {inherit WorksheetObject(nws);
                              pstrcontrol = new Pstring.PstringControl(pstr) }

    // specific to objects supporting game-semantic transformations (like editable pstring and traversal)
    abstract pview : unit -> WorksheetObject
    default x.pview() = PstringObject(x.ws,[||]):>WorksheetObject
    abstract oview : unit -> WorksheetObject
    default x.oview() = PstringObject(x.ws,[||]):>WorksheetObject
    abstract herproj : unit -> WorksheetObject
    default x.herproj() = PstringObject(x.ws,[||]):>WorksheetObject
    abstract subtermproj : unit -> WorksheetObject
    default x.subtermproj() = PstringObject(x.ws,[||]):>WorksheetObject
    abstract ext : unit -> WorksheetObject
    default x.ext() = PstringObject(x.ws,[||]):>WorksheetObject
    abstract star : unit -> WorksheetObject
    default x.star() = PstringObject(x.ws,[||]):>WorksheetObject
    abstract prefix : unit -> WorksheetObject
    default x.prefix() = PstringObject(x.ws,[||]):>WorksheetObject

    override x.Control = x.pstrcontrol:>System.Windows.Forms.Control

    override x.Clone() = new PstringObject(x.ws,x.pstrcontrol.Sequence):>WorksheetObject

    override x.Selection() =
        x.pstrcontrol.AutoSizeMaxWidth <- x.ws.seqflowpanel.ClientSize.Width
        x.pstrcontrol.Selection() // tell the control that it's about to be selected using PstringControl.Selection()
        x.pstrcontrol.Select()    // select it with System.Windows.Forms.Form.Select()

    override x.Deselection() =
        x.pstrcontrol.AutoSizeMaxWidth <- 0
        x.pstrcontrol.Deselection()

    // Called to give the focus back to the traversal control
    override x.Refocus() =
        x.pstrcontrol.Select()

    override x.OnCompGraphNodeMouseDown _ _ = ()

    // create a pstring sequence from an XML description
    new (nws,xmlPstr:XmlNode,_) as x =
      {inherit WorksheetObject(nws);
       pstrcontrol = new Pstring.PstringControl([||]) }
      then
        assert_xmlname "pstring" xmlPstr;
        x.LoadSequenceFromXmlNode xmlPstr


    // convert the pstring sequence to an XML element.
    override x.ToXmlElement xmldoc =
      let xmlPstr = xmldoc.CreateElement("pstring")
      x.SequenceToXml xmldoc xmlPstr
      xmlPstr

    // load the pstring sequence from an XML description
    member x.LoadSequenceFromXmlNode (xmlPstr:XmlNode) =
      let xml_to_occ (xmlOcc:XmlNode) =
        assert_xmlname "occ" xmlOcc;

        let xmlColor = xmlOcc.SelectSingleNode("color") in
        let xmlLabel = xmlOcc.SelectSingleNode("label") in
        let xmlLink = xmlOcc.SelectSingleNode("link") in
        let xmlShape = xmlOcc.SelectSingleNode("shape") in
        let xmlTreenode = xmlOcc.SelectSingleNode("graphnode")
        in
            { tag= box (
                        match xmlTreenode.Attributes.GetNamedItem("type").Value with
                            "node" -> InternalNode(int_of_string (xmlTreenode.Attributes.GetNamedItem("index").Value))
                          | "value" -> ValueLeaf(int_of_string (xmlTreenode.Attributes.GetNamedItem("index").Value),
                                                 int_of_string (xmlTreenode.Attributes.GetNamedItem("value").Value))
                          | "ghostVariable" -> GhostVariable(int_of_string (xmlTreenode.Attributes.GetNamedItem("label").Value))
                          | "ghostLambda" -> GhostLambda(int_of_string (xmlTreenode.Attributes.GetNamedItem("label").Value))
                          | "custom" -> Custom;
                          | _ -> failwith "Incorrect occurrence type attribute.");
              color=Color.FromName(xmlColor.InnerText);
              label=xmlLabel.InnerText;
              link=int_of_string xmlLink.InnerText;
              shape=shape_of_string xmlShape.InnerText; }
      in
        let p = xmlPstr.ChildNodes.Count in
        x.pstrcontrol.Sequence <- Array.init p (fun i -> xml_to_occ (xmlPstr.ChildNodes.Item(i)))


    // Convert the occurrences of the pstring sequence into XML nodes and attach them to a given root XML element.
    // @param xmldoc is the xmldocument
    // @param root the root element to which the XML nodes will be attached
    member x.SequenceToXml xmldoc rootelement =
      let occ_to_xml occ =
        let xmlOcc = xmldoc.CreateElement("occ")

        let xmlColor = xmldoc.CreateElement("color")
        xmlColor.InnerText <- occ.color.Name;
        xmlOcc.AppendChild(xmlColor) |> ignore

        let xmlLabel = xmldoc.CreateElement("label")
        xmlLabel.InnerText <- occ.label;
        xmlOcc.AppendChild(xmlLabel) |> ignore

        let xmlLink = xmldoc.CreateElement("link")
        xmlLink.InnerText <- string_of_int occ.link;
        xmlOcc.AppendChild(xmlLink) |> ignore

        let xmlShape = xmldoc.CreateElement("shape")
        xmlShape.InnerText <- shape_to_string occ.shape;
        xmlOcc.AppendChild(xmlShape) |> ignore

        let xmlTreenode = xmldoc.CreateElement("graphnode")
        if isNull occ.tag then
          xmlTreenode.SetAttribute("type","custom");
        else
          match (occ.tag :?> gen_node) with
          | GhostLambda l -> 
                xmlTreenode.SetAttribute("type","ghostLambda");
                xmlTreenode.SetAttribute("label",(string_of_int l));
          | GhostVariable l ->
                xmlTreenode.SetAttribute("type","ghostVariable");
                xmlTreenode.SetAttribute("label",(string_of_int l));
          | Custom ->
                xmlTreenode.SetAttribute("type","custom");
          | InternalNode(i) ->
                xmlTreenode.SetAttribute("type","node");
                xmlTreenode.SetAttribute("index",(string_of_int i));
          | ValueLeaf(i,v) ->
                xmlTreenode.SetAttribute("type","value");
                xmlTreenode.SetAttribute("index",(string_of_int i));
                xmlTreenode.SetAttribute("value",(string_of_int v));


        xmlOcc.AppendChild(xmlTreenode) |> ignore

        rootelement.AppendChild(xmlOcc) |> ignore

      in
       Array.iter occ_to_xml x.pstrcontrol.Sequence

  end


/// Represents an editable justification sequence object
type EditablePstringObject =
  class
    inherit PstringObject

    // Constructor
    new (nws,pstr:pstring) as x = {inherit PstringObject(nws,pstr)} then x.pstrcontrol.Editable <- true

    override x.Clone() = new EditablePstringObject(x.ws,x.pstrcontrol.Sequence):>WorksheetObject

    // Convert the pstring sequence to an XML element.
    override x.ToXmlElement xmldoc =
      let xmlPstr = xmldoc.CreateElement("editablepstring")
      x.SequenceToXml xmldoc xmlPstr
      xmlPstr

    // create a pstring sequence from an XML description
    new (ws,xmlPstr:XmlNode,_) as x =
        {inherit PstringObject(ws,[||])}
        then
          x.pstrcontrol.Editable <- true
          assert_xmlname "editablepstring" xmlPstr;
          base.LoadSequenceFromXmlNode xmlPstr

    // a graph-node has been clicked while this object was selected
    override x.OnCompGraphNodeMouseDown e msaglnode =
        // add the selected graph node at the end of the sequence
        let gr_nodeindex = int_of_string msaglnode.Attr.Id

        // add an internal node to the traversal
        if e.Button = MouseButtons.Left then
            x.pstrcontrol.add_node (occ_from_gennode x.ws.compgraph (InternalNode(gr_nodeindex)) 0 )
        // add a value-leaf node to the traversal
        else
            x.pstrcontrol.add_node (occ_from_gennode x.ws.compgraph (ValueLeaf(gr_nodeindex,1)) 0 )

        x.Refocus()

    override x.pview() =
        let seq = pstrseq_pview_at x.ws.compgraph x.pstrcontrol.Sequence x.pstrcontrol.SelectedNodeIndex
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.oview() =
        let seq = pstrseq_oview_at x.ws.compgraph x.pstrcontrol.Sequence x.pstrcontrol.SelectedNodeIndex
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.herproj() =
        let seq = pstrseq_herproj x.pstrcontrol.Sequence x.pstrcontrol.SelectedNodeIndex
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.subtermproj() =
        let seq = pstrseq_subtermproj x.ws.compgraph x.pstrcontrol.Sequence x.pstrcontrol.SelectedNodeIndex
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.prefix()=
        let seq = pstrseq_prefix x.pstrcontrol.Sequence x.pstrcontrol.SelectedNodeIndex
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.ext()=
        let seq = pstrseq_ext x.ws.compgraph x.pstrcontrol.Sequence x.pstrcontrol.SelectedNodeIndex
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.star()=
        let seq = pstrseq_star x.ws.compgraph x.pstrcontrol.Sequence x.pstrcontrol.SelectedNodeIndex
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject


    member x.remove_last_occ() = x.pstrcontrol.remove_last_occ()
    member x.add_occ() = x.pstrcontrol.add_node (create_blank_occ())
    member x.edit_occ_label() = x.pstrcontrol.EditLabel()
  end

(** Traversal object: type used for sequence constructed with the traversal rules **)
type TraversalObject =
  class
    inherit PstringObject

    // [valid_omoves] is a map describing the valid o-moves:
    //   - the list of map keys gives the set of valid o-moves (graph node indices)
    //   - each key i is mapped to the list of occurrences of its enabler in the traversal that are valid justifiers
    //     for the corresponding O-move.
    val mutable valid_omoves : Map<int,int list>

    // [wait_for_ojustifier] is a list which is not empty iff O has selected a move but has not chosen his justifier yet.
    // It contains the list of valid justifier for the given move.
    val mutable wait_for_ojustifier : int list

    // highlight the occurrence in the sequence that are valid justifiers for the O-move
    // that has been selected by the user.
    member x.highlight_potential_justifiers() =
        let n = x.pstrcontrol.Length
        // are we waiting for O to select a justifier?
        if x.wait_for_ojustifier = [] then
          // no: so remove the highlighting by restoring the original color and shape
          x.pstrcontrol.Sequence <-
            Array.init n (fun i -> let o = x.pstrcontrol.Occurrence(i) in
                                    { label= o.label;
                                      tag = o.tag;
                                      link = o.link;
                                      shape= o.shape;
                                      color=player_to_color (x.ws.compgraph.gennode_player (pstr_occ_getnode o))
                                    } )
          else
              // yes: we then put first all the occurrences into shade
              let seq = Array.init n (fun i -> let o = x.pstrcontrol.Occurrence(i) in
                                                { label= o.label;
                                                  tag = o.tag;
                                                  link = o.link;
                                                  shape= o.shape;
                                                  color= Color.LightGray
                                                } )

              // ... and then highlight the justifiers
              List.iter (fun i -> let o = x.pstrcontrol.Occurrence(i)
                                  seq.(i) <-
                                        { label= o.label;
                                          tag = o.tag;
                                          link = o.link;
                                          shape= o.shape;
                                          color = player_to_color Proponent
                                        } ) x.wait_for_ojustifier

              x.pstrcontrol.Sequence <- seq

    // a *static* function used to initialize the traversal
    static member init (x:TraversalObject) =
      // it is assumed that pstr is an odd-length sequence (finishing with an O-move)
      x.pstrcontrol.Editable <- false
      x.recompute_valid_omoves()

      // add a handler for click on the nodes of the sequence
      x.pstrcontrol.nodeClick.AddHandler(new Handler<PstringControl * NodeClickEventArgs>(fun _ (_,e) ->
        if x.wait_for_ojustifier <> [] then
            if List.mem e.Node x.wait_for_ojustifier then
               // update the link
               x.pstrcontrol.updatejustifier (x.pstrcontrol.Length-1) e.Node
               x.wait_for_ojustifier <- []
               x.highlight_potential_justifiers()
               // play for the computer
               x.play_for_p()
            else
              ignore( MessageBox.Show("This justifier is not valid for the selected move!","This is not a valid justifier!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation))

          ))


    // Constructor
    new (ws,pstr:pstring) as x = {inherit PstringObject(ws,pstr)
                                  valid_omoves=Map.empty
                                  wait_for_ojustifier=[]
                                 }
                                 then
                                    TraversalObject.init x

    override x.Clone() =
        let l = Array.length x.pstrcontrol.Sequence
        let nseq =
            if l = 0 then
                [||]
            else if x.pstrcontrol.Occurrence(l-1).tag = null then
                Array.sub x.pstrcontrol.Sequence 0 (l-1)
            else
                x.pstrcontrol.Sequence
        in
        new TraversalObject(x.ws,nseq):>WorksheetObject



    override x.ToXmlElement xmldoc =
      let xmlPstr = xmldoc.CreateElement("traversal")
      x.SequenceToXml xmldoc xmlPstr
      xmlPstr

    // create a pstring sequence from an XML description
    new (ws,xmlPstr:XmlNode,_) as x = {inherit PstringObject(ws,[||])
                                       valid_omoves=Map.empty
                                       wait_for_ojustifier=[]
                                      }
                                      then
                                          assert_xmlname "traversal" xmlPstr;
                                          base.LoadSequenceFromXmlNode xmlPstr
                                          let occ = x.pstrcontrol.Occurrence(x.pstrcontrol.Length-1)
                                          if occ.tag = null || pstr_occ_getnode occ = Custom  then
                                            x.pstrcontrol.remove_last_occ() // delete the trailing dummy node
                                          TraversalObject.init x

    member x.RefreshLabelInfo() =
        x.ws.labinfo.Text <- if Map.isEmpty x.valid_omoves then "Traversal completed!"
                             else if x.wait_for_ojustifier = [] then  "Pick a node in the graph!"
                             else "Pick a justifier in the sequence!"


    // adujust the scrolling of the flow container so that the node occurrence i is visible
    member x.scroll_flowcontainer_to_occ i =
        ()
        //let bb = base.pstrcontrol.Bbox(i) in
        //let viewwidth = x.ws.seqflowpanel.ClientSize.Width // HorizontalScroll.LargeChange
        //base.ws.seqflowpanel.HorizontalScroll.Visible <- true
        //let p = base.ws.seqflowpanel.AutoScrollPosition in
        //base.ws.seqflowpanel.AutoScrollPosition <- Point((max 0 (bb.Right - viewwidth)), -p.Y)
        //base.ws.seqflowpanel.HorizontalScroll.Value <- max 0 (bb.Right - viewwidth)

    override x.Selection()=
        x.HighlightAllowedMovesOnCompGraph()
        x.RefreshLabelInfo()
        base.Selection()
        base.flush_flowcontainer_to_right()


    override x.Deselection()=
        x.RestoreCompGraphViewer()
        base.Deselection()

    (* a graph-node has been clicked while this object was selected *)
    override x.OnCompGraphNodeMouseDown e msaglnode =
        let l = x.pstrcontrol.Length

        // valid O-move?
        let gr_nodeindex = int_of_string msaglnode.Attr.Id
        let valid_justifiers = Map.tryFind gr_nodeindex x.valid_omoves in

        match valid_justifiers with
        | None -> // Invalid move: traversal is complete: no valid move wiht valid justifier
            ()

        | Some [] -> // valid initial move with no justifier
            // create a new occurrence for the corresponding graph node
            let newocc = occ_from_gennode x.ws.compgraph (InternalNode(gr_nodeindex)) 0
            x.pstrcontrol.replace_last_node newocc // add the occurrence at the end of the sequence
            x.wait_for_ojustifier <- []
            x.play_for_p() // play for the Propoment

        | Some [j] -> // only one justifier, the Opponent has no choice
    
            let generalizedNode =
                // Did the user click on the ghost node place-holder button?
                if msaglnode.Attr.Id = MsaglGraphGhostButtonId then
                    // read the label of the ghost node from the custom data field of the ghost button
                    GhostLambda(msaglnode.UserData :?> int)
                else
                    // Generate a new traversal occurrence of the computation graph node that was clicked
                    InternalNode(gr_nodeindex)

            let newocc = occ_from_gennode x.ws.compgraph generalizedNode ((l-1)-j)
            x.pstrcontrol.replace_last_node newocc
            x.wait_for_ojustifier <- []
            x.play_for_p()

        | Some valid_justifiers -> // more than one choice: wait for the Opponent to choose one before playing for P
            let newocc = occ_from_gennode x.ws.compgraph (InternalNode(gr_nodeindex)) 0
            x.pstrcontrol.replace_last_node newocc
            x.wait_for_ojustifier <- valid_justifiers
            x.highlight_potential_justifiers()
            x.RefreshLabelInfo()

        x.Refocus()

    (* Add an occurrence of a gennode to the end of the traversal *)
    member private x.add_gennode gennode link =
        x.pstrcontrol.add_node (occ_from_gennode x.ws.compgraph gennode link)

    (* Update the graph node colors according to the specified coloring function *)
    member x.RefreshCompGraphViewer (color:int -> nodecontent -> Color) =
        let msaglgraph = x.ws.msaglviewer.Graph

        let compgraph_node =
            function 
            | -1 -> NCntAbs("Lam", [])
            | i -> x.ws.compgraph.nodes.(i)

        msaglgraph.NodeMap.Keys
        |> Seq.cast<string>
        |> Seq.iter(fun id ->
           let i = int_of_string id
           msaglgraph.FindNode(id)
           |> msaglgraphnode_set_attributes_styles (color i)
                                         grnode_2_shape
                                         x.ws.compgraph
                                         (compgraph_node i)
        )
        x.ws.msaglviewer.Invalidate()


    (* Update the graph node colors to indicate to the user which moves are allowed *)
    member x.HighlightAllowedMovesOnCompGraph() =
        x.RefreshCompGraphViewer (fun id _ -> if Map.containsKey id x.valid_omoves then
                                                 player_to_color Opponent
                                              else
                                                 Color.LightGray)

    (* Restore the original node colors in the graph-view *)
    member x.RestoreCompGraphViewer() =
        x.RefreshCompGraphViewer (fun _ -> grnode_2_color)

    (* get the arity of a traversal occurrence *)
    member x.occurrence_arity occ =
        match pstr_occ_getnode (x.pstrcontrol.Occurrence(occ)) with
        | Custom -> failwith "Bad traversal! Custom nodes are not supported!"
        | ValueLeaf(_,_) ->  0
        | GhostVariable _ -> 0
        | GhostLambda _ -> 0
        | InternalNode(i) -> x.ws.compgraph.arity i

    (* Get the n^th child a traversal occurrence
       If the n^th child does not exist then return a ghost node (etiher lambda or variable) *)
    member x.occurrence_child occ n =
        // the arity of the node tells us how many chidren it has         
        let justifier_arity = x.occurrence_arity occ

        match pstr_occ_getnode (x.pstrcontrol.Occurrence(occ)) with
        | Custom -> failwith "Bad traversal! Custom nodes are not supported!"
        | ValueLeaf(_,_) ->  failwith "A leaf node does not have children!"
        | GhostVariable _ -> GhostLambda n 
        | GhostLambda _ -> GhostVariable n
        | InternalNode i when n <= justifier_arity -> InternalNode (x.ws.compgraph.nth_child i n)
        | InternalNode i when graphnode_player x.ws.compgraph.nodes.(i) = Opponent -> GhostVariable n
        | InternalNode _ -> GhostLambda n
        
    (* Calculate the list of valid O-moves at this point. Returns a Map whose keys are the possible nodes to play and
    values are the list of possible justification pointers for each possible node to play. *)
    member private x.recompute_valid_omoves() =
        let l = x.pstrcontrol.Length

        // Rule (Var): Only possible move for O is the copy-cat of the last P-move
        // Pre-condition: the last node occurrence in the traversal is a (possibly ghost) variable node
        // that is hereditarily justified by an @ node.
        let rule_var variable_bindingindex =
            // The last occurrence in the traversal (i.e. the variable node occurrence)
            let last = x.pstrcontrol.Occurrence(l-1)
            // index to the justifier of the variable occurrence
            let var_justifier = (l - 1) - last.link

            // [justifier] is the index in the traversal of the occurrence of the variable or @ node
            // that will justify the next O-move.
            // By the definition of the (Var) rule it is the immediate predecessor of the justifier of the variable node:
            let justifier = var_justifier - 1
                                             
            match x.occurrence_child justifier variable_bindingindex with
            | Custom | ValueLeaf(_,_) | GhostVariable _ ->
                failwith "Not possible. The next move can only be a lambda node!"
                                
            // the justifier's child exist in the computation graph
            | InternalNode i ->  
                Map.ofList [i, [justifier]]

            // if the last node is a ghost variable node justified by the root then no further move is allowed
            | GhostLambda k when var_justifier = 0 ->
                Map.empty

            // the justifier does not have enough children..
            | GhostLambda k -> 
                // __On-the-fly eta-expansion__
                // Conceptually we eta-expand the sub-term rooted at [justifier] 
                // sufficiently enough so that the the justifier node has at least 
                // variable_bindingindex children (ghost) lambda nodes.
                // This trick allows us to proceed using the (Var) rules on ghost nodes
                // as if there were genuine nodes.

                // The ghost lambda node that we introduce is the variable_bindingindex^th
                // child of [justifier].
                //
                // A ghost lambda node is labelled by its child index relatively to its parent
                // [justifier]. We temporarily store this label in the ghost button of the MSAGL graph
                // so that its value is readily available when the user clicks on the Ghost button to play the move.
                let msaglGhostButton = x.ws.msaglviewer.Graph.FindNode(MsaglGraphGhostButtonId)
                msaglGhostButton.UserData <- k
                msaglGhostButton.Attr.Label <- sprintf "Ghost %d" k

                let ghost_copycat_node_index = int_of_string MsaglGraphGhostButtonId                            
                Map.ofList [ghost_copycat_node_index, [justifier]]

        // Rule (InputVar): O can play any P-move whose parent occur in the O-view
        // Pre-condition: the last node occurrence in the traversal is a (possibly ghost) variable node
        // that is hereditarily justified by the initial root node.
        let rule_inputvar () =
            // get the list of occurrence from the O-view
            let oview_occs = Traversal.seq_occs_in_Xview
                                x.ws.compgraph
                                Opponent
                                pstr_occ_getnode        // getnode function
                                pstr_occ_getlink        // getlink function
                                pstr_occ_updatelink     // update link function
                                x.pstrcontrol.Sequence
                                (l-1)

            // filter the list to keep only occurrences of Opponent nodes
            let parents_occs = List.filter (fun o -> (x.ws.compgraph.gennode_player (pstr_occ_getnode (x.pstrcontrol.Occurrence(o)))) = Proponent) oview_occs

            // map a traversal occurrence index to the corresponding node index in the graph
            let get_grnodeindex_from_occ occ =
                match pstr_occ_getnode (x.pstrcontrol.Occurrence(occ)) with
                | Custom -> failwith "Bad traversal! Custom nodes are not supported!" 
                | ValueLeaf(_,_) -> failwith "Bad traversal! The O-view cannot contain value leaf since the traversal ends with a variable node"
                | GhostVariable _ | GhostLambda _ -> failwith "Bad traversal! The O-view only contains node hereditarily justified by the root since the last node in the traversal is. Hence there cannot be any ghost node in the O-view!"
                | InternalNode(i) -> i

            // given an occurrence occ of a graph node,
            // map all its children in the graph to itself
            let map_children_to_occ m occ =
                List.fold_right (fun j s ->
                                    Map.add j (occ::(match Map.tryFind j s with Some v -> v | None -> [])) s)
                            x.ws.compgraph.edges.(get_grnodeindex_from_occ occ)
                            m

            // finally compute a Map which associate for each node of the tree
            // whose parent occurs in the O-view, the set of occurrences
            // of its parent in the O-view.
            let children_to_parentoccs = List.fold_left map_children_to_occ
                                                        Map.empty
                                                        parents_occs


            // children_to_parentoccs is precisely the list of valid O-moves:
            // a map from valid O-moves to the list of all valid justifiers in the traversal!
            children_to_parentoccs

        // Rule (Root)
        x.valid_omoves <-
            if l = 0 then
                // Rule (Root): only possible initial move is the root of the computation graph
                Map.add 0 [] Map.empty
            else
                // What is the node type of the last occurrence?
                let last = x.pstrcontrol.Occurrence(l-1)
                match pstr_occ_getnode last with
                | Custom -> failwith "Bad traversal! There is an occurence of a node that does not belong to the computation graph!"
                | GhostLambda l -> failwith "O has no valid move since it's P's turn!"
                | ValueLeaf(i,v) -> Map.empty // TODO: ValueLeaf: play value using the copycat rule (Value) or the (InputValue) rule
                
                // TODO
                //| GhostVariable variable_bindingindex when heredirearily justified by initial move ->
                //  rule_inputvar ()

                | GhostVariable variable_bindingindex ->
                    rule_var variable_bindingindex

                | InternalNode(i) ->
                    match x.ws.compgraph.nodes.(i) with
                    | NCntAbs(_,_) ->
                        failwith "O has no valid move since it's P's turn!"

                    // Rule (App): O can only play the 0th child of the node i
                    | NCntApp ->
                        // find the child node of the lambda node
                        let firstchild = List.hd x.ws.compgraph.edges.(i)
                        Map.ofList [firstchild,[l-1]]

                    // Rule (InputVar): O can play any P-move whose parent occur in the O-view
                    | NCntVar(_) | NCntTm(_) when x.ws.compgraph.he_by_root.(i) ->
                        rule_inputvar ()
                    
                    // Rule (Var): variable node hereditarily justified by an @ node
                    | NCntVar(_) | NCntTm(_) ->
                        // The variable name index is given by the link label
                        rule_var x.ws.compgraph.parameterindex.(i)

        if not (Map.isEmpty x.valid_omoves) then
          x.pstrcontrol.add_node (create_blank_occ()) // add a dummy node for the forthcoming initial O-move
          x.flush_flowcontainer_to_right()

        x.RefreshLabelInfo()


    (* Play the next move for P according to the strategy given by the term *)
    member x.play_for_p() =
        let l = x.pstrcontrol.Length
        if l = 0 then failwith "The game has not started yet! You (the Opponent) must start!"

        let last = x.pstrcontrol.Occurrence(l-1)
        // What is the type of the last occurrence?
        match pstr_occ_getnode last with
        | Custom ->
            failwith "Bad traversal! There is an occurence of a node that does not belong to the computation graph!"

        // it is a value leaf
        // TODO: play using the copycat rule (Value)
        | ValueLeaf(i,v) -> ()

        | GhostVariable _ ->
            failwith "It is your turn. You are playing the Opponent!"
        
        | GhostLambda i ->
            // compute the link
            let ghostlambda_justifier = l-1 - last.link
            // The justifier of the next P-move is ip(jp(t))
            let justifier = ghostlambda_justifier - 1
            let link = l-justifier // = last.link + 2
            let alpha = (x.occurrence_arity justifier) + i - (x.occurrence_arity ghostlambda_justifier)
            x.add_gennode (GhostVariable(alpha)) link

        // it is an internal-node
        | InternalNode(i) ->
            match x.ws.compgraph.nodes.(i) with
            | NCntApp | NCntVar(_) | NCntTm(_)
                -> failwith "It is your turn. You are playing the Opponent!"

            // Traversal rule (Lmd):
            | NCntAbs(_,_) ->
                // find the child node of the lambda node
                let firstchild = List.hd x.ws.compgraph.edges.(i)

                // Get the enabler of the node
                let enabler = x.ws.compgraph.enabler.(firstchild) in

                // compute the link
                let link =
                    // no enabler?
                    if enabler = -1 then
                        0 // then it's an @-node so it has no justifier
                    else
                        l-
                        // get the occurrence of the binder in the P-view
                        (Traversal.seq_find_lastocc_in_Xview x.ws.compgraph
                                        Proponent
                                        pstr_occ_getnode        // getnode function
                                        pstr_occ_getlink        // getlink function
                                        pstr_occ_updatelink     // update link function
                                        x.pstrcontrol.Sequence
                                        (l-1)
                                        (InternalNode(enabler)))

                x.add_gennode (InternalNode(firstchild)) link

        x.recompute_valid_omoves()
        x.HighlightAllowedMovesOnCompGraph()


    (** [adjust_to_valid_occurrence i] adjusts the occurrence index i to a valid occurrence position by making sure
        that it does not refer to the trailling dummy node (labelled "...") a the end of the sequence.

        @return the index of the first valid occurrence preceding occurrence number i,
        and -1 if there is no such occurrence (if the traversal is empty) **)
    member private x.adjust_to_valid_occurrence i =
        let j = max 0 (min (x.pstrcontrol.Length-1) i)
        let occ = x.pstrcontrol.Occurrence(j)
        if isNull occ.tag || pstr_occ_getnode occ = Custom  then j-1
        else j

    (** undo all the moves played after the selected node **)
    member x.undo()=
        // If there is no selection then by default we take the last occurrence
        let s = if x.pstrcontrol.SelectedNodeIndex >= 0 then x.pstrcontrol.SelectedNodeIndex else x.pstrcontrol.Length-1
        let p = // if the traversal is empty then take the empty prefix
                if s = -1 then
                    -1
                else
                    // if the selection is on the dummy trailing node then we only undo the last O-move
                    let occ = x.pstrcontrol.Occurrence(s)
                    if isNull occ.tag || pstr_occ_getnode occ = Custom then
                        s-3 // we remove the last three nodes: the dummy node + last P-move + last O-move
                    // othwerise undo up to the last Proponent move
                    else if x.ws.compgraph.gennode_player (pstr_occ_getnode (x.pstrcontrol.Occurrence(s))) = Opponent then
                        s-1
                    else
                        s
        (new TraversalObject(x.ws,(pstrseq_prefix x.pstrcontrol.Sequence p))):>WorksheetObject

    override x.pview() =
        let seq = pstrseq_pview_at x.ws.compgraph x.pstrcontrol.Sequence (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.oview() =
        let seq = pstrseq_oview_at x.ws.compgraph x.pstrcontrol.Sequence (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.herproj() =
        let seq_with_no_trail = Array.sub x.pstrcontrol.Sequence 0 (1+(x.adjust_to_valid_occurrence (x.pstrcontrol.Length-1)))
        let seq = pstrseq_herproj seq_with_no_trail (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.subtermproj() =
        let seq_with_no_trail = Array.sub x.pstrcontrol.Sequence 0 (1+(x.adjust_to_valid_occurrence (x.pstrcontrol.Length-1)))
        let seq = pstrseq_subtermproj x.ws.compgraph seq_with_no_trail (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.prefix()=
        let seq = pstrseq_prefix x.pstrcontrol.Sequence (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.ext()=
        let seq = pstrseq_ext x.ws.compgraph x.pstrcontrol.Sequence (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.star()=
        let seq = pstrseq_star x.ws.compgraph x.pstrcontrol.Sequence (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
   end



// execute a function on the current selection if it is of a given type
// the parameter (_:'a->unit) is a trick used to pass the type 'a as a parameter to the function
let do_onsomeobject_oftype (u:'a->unit) (f:'a->unit) =
     function
        | Some(c:WorksheetObject) when (c:?'a) -> f (c:?>'a)
        | _ -> ()


// Return the object corresponding to the ith control of a seqflowPanel
let object_from_controlindex (ws:WorksheetParam) (i:int) = ws.seqflowpanel.Controls.Item(i).Tag:?>WorksheetObject


(************* WORKSHEET FILE OPERATIONS  *************)


(** Save the worksheet to an XML file **)
let save_worksheet (filename:string) (ws:WorksheetParam)  =
    let xmldoc = new XmlDocument()

    //Create Parent Node
    let xmlWorksheet = xmldoc.CreateElement("worksheet")
    xmldoc.AppendChild(xmlWorksheet) |> ignore

    // source of the computation graph
    let xmlCompgraph = xmldoc.CreateElement("compgraph")
    let xmlSource = xmldoc.CreateElement("source")
    xmlSource.SetAttribute("type",match Parsing.get_file_extension ws.graphsource_filename with
                                  | "rs" -> "recursionscheme"
                                  | "lmd" -> "lambdaterm"
                                  | _ -> "unknown");
    xmlSource.SetAttribute("file",ws.graphsource_filename);
    xmlCompgraph.AppendChild(xmlSource) |> ignore
    xmlWorksheet.AppendChild(xmlCompgraph) |> ignore

    for i = 0 to ws.seqflowpanel.Controls.Count-1 do
      xmlWorksheet.AppendChild((object_from_controlindex ws i).ToXmlElement xmldoc) |> ignore
    done;

    // save the document
    xmldoc.Save(filename);;


(** import a worksheet XML file into the current worksheet **)
let import_worksheet (filename:string) (ws:WorksheetParam) addObjectFunc =
    let xmldoc = new XmlDocument()
    xmldoc.Load(filename);

    let xmlWorksheet = xmldoc.SelectSingleNode("worksheet") in
    assert_xmlnotnull xmlWorksheet;

    let n = xmlWorksheet.ChildNodes.Count in
    for i = 1 to n-1 do
      let xmlnode = xmlWorksheet.ChildNodes.Item(i)
      let obj = match xmlnode.Name with
                  | "pstring" -> new PstringObject(ws,xmlnode,0):>WorksheetObject
                  | "editablepstring" -> new EditablePstringObject(ws,xmlnode,0):>WorksheetObject
                  | "traversal" -> new TraversalObject(ws,xmlnode,0):>WorksheetObject
                  | _ -> failwith "Unknown object in the XML file your are trying to open!"
      ignore(addObjectFunc obj)
    done;
;;


(**** Traversal calculator window ***)

(** Loads the traversal calculator window for a given computation graph.
    @param initialize_ws is a function that is executed to initialized the worksheet. It take as a parameter a function
    that create an object and add it to the worksheet.
**)
let ShowTraversalCalculatorWindow mdiparent graphsource_filename (compgraph:computation_graph) lnfrules initialize_ws =

    let form  = new GUI.Traversal()

    // create the MSAGL graph
    let msaglgraph = compgraph_to_graphview grnode_2_color grnode_2_shape compgraph

    // this holds the WorksheetObject that is currently selected.
    let selection : WorksheetObject option ref = ref None

    // Traversal game buttons
    let wsparam = { graphsource_filename=graphsource_filename;
                    compgraph=compgraph;
                    lnfrules=lnfrules;
                    msaglviewer=form.gViewer
                    seqflowpanel=form.seqflowPanel
                    labinfo = form.labGameInfo
                  }

    // enable/disable the calculator buttons
    // bSel true if a control is selected
    // bTrav true if the selected control is a traversal
    let enable_buttons bSel bTrav =
        form.grpNode.Enabled <- not bTrav
        form.btUndo.Enabled <- bTrav
        form.btPlay.Enabled <- bTrav
        form.labGameInfo.Enabled <- bTrav

        form.btDelete.Enabled <- bSel
        form.btDuplicate.Enabled <- bSel
        form.btOview.Enabled <- bSel
        form.btPview.Enabled <- bSel
        form.btBackspace.Enabled <- bSel
        form.btEditLabel.Enabled <- bSel
        form.btExportSeq.Enabled <- bSel
        form.btHerProj.Enabled <- bSel
        form.btAdd.Enabled <- bSel
        form.btSubtermProj.Enabled <- bSel
        form.btPrefix.Enabled <- bSel
        form.btStar.Enabled <- bSel
        form.btExt.Enabled <- bSel



    // execute a function on the currently selected worksheet object if there is one
    let apply_to_selection f =
        do_onsome f !selection

    // execute a function on the current selection if it is of a given type
    // the parameter (_:'a->unit) is a trick used to pass the type 'a as a parameter to the function
    let apply_to_selection_ifoftype subtype (f:'a->unit) =
        do_onsomeobject_oftype subtype f !selection

    // unselect the currently selected object
    let unselect_object() =
        apply_to_selection
          (fun obj ->
            enable_buttons false false
            obj.Deselection();
            selection := None)

    // select a worksheet object
    let select_object (obj:WorksheetObject) =
        enable_buttons true (obj:?TraversalObject)
        obj.Selection()

        selection := Some(obj)

    // change the current selection
    let change_selection_object (wsobj:WorksheetObject) =
        apply_to_selection (fun curobj -> curobj.Deselection() ) // if an object is already selected then deselect it
        select_object wsobj

    // change the current selection
    let change_selection_object (wsobj:WorksheetObject) =
        apply_to_selection (fun curobj -> curobj.Deselection() ) // if an object is already selected then deselect it
        select_object wsobj

    // Add an object to the worksheet
    let AddObject (new_obj:WorksheetObject) =
        let ctrl = new_obj.Control
        ctrl.AutoSize <- true
        ctrl.TabStop <- false
        ctrl.Tag <- (new_obj:>obj) // link back to the worksheet object (it's not really clean since there may be objects that want to use the Tag property for themselves, but this avoids to create an extra array to store the mapping...)
        ctrl.BackColor <- form.seqflowPanel.BackColor
        // add an event handler to the a given pstring control in order to detect selection
        // of the control by the user
        ctrl.MouseDown.Add(fun _ -> change_selection_object new_obj );
        ctrl.KeyDown.Add( fun e -> match e.KeyCode with
                                              Keys.Up -> let i = match !selection with
                                                                          None -> -1
                                                                        | Some(selobj) -> form.seqflowPanel.Controls.GetChildIndex(selobj.Control)
                                                         // if there is a predecessor then select it
                                                         if i-1 >=0 then
                                                             change_selection_object (object_from_controlindex wsparam (i-1))
                                            | Keys.Down -> let i = match !selection with
                                                                          None -> -1
                                                                        | Some(selobj) -> form.seqflowPanel.Controls.GetChildIndex(selobj.Control)
                                                           // if there is a successor then select it
                                                           if i+1 < form.seqflowPanel.Controls.Count then
                                                             change_selection_object (object_from_controlindex wsparam (i+1))
                                            | _ -> ()
                               );
        // we need to add the control of that object to the seqflowPanel control
        form.seqflowPanel.Controls.Add ctrl
        new_obj

    (* map a button click event to an object transformation *)
    let map_button_to_transform (bt:System.Windows.Forms.Button) (transform:'a->WorksheetObject) =
        bt.Click.Add(fun _ -> apply_to_selection_ifoftype
                                    (fun (_:'a) -> ()) // trick used to pass the type 'a as a parameter
                                    (fun cursel -> change_selection_object (AddObject (transform cursel))))


    (* map a button click event to an in-place transformation *)
    let map_button_to_transform_inplace (bt:System.Windows.Forms.Button) (inplacetransform:'a->unit) =
        bt.Click.Add(fun _ -> apply_to_selection_ifoftype
                                    (fun (_:'a) -> ())
                                    (fun cursel -> inplacetransform cursel))

    // Return the object corresponding to the ith control of the seqflowPanel
    let object_from_controlindex (i:int) = form.seqflowPanel.Controls.Item(i).Tag:?>WorksheetObject

    // Add an object to the worksheet
    let AddObject (new_obj:WorksheetObject) =
        let ctrl = new_obj.Control
        ctrl.AutoSize <- true
        ctrl.TabStop <- false
        ctrl.Tag <- (new_obj:>obj) // link back to the worksheet object (it's not really clean since there may be objects that want to use the Tag property for themselves, but this avoids to create an extra array to store the mapping...)
        ctrl.BackColor <- form.seqflowPanel.BackColor
        // add an event handler to the a given pstring control in order to detect selection
        // of the control by the user
        ctrl.MouseDown.Add(fun _ -> change_selection_object new_obj );
        ctrl.KeyDown.Add( fun e -> match e.KeyCode with
                                              Keys.Up -> let i = match !selection with
                                                                          None -> -1
                                                                        | Some(selobj) -> form.seqflowPanel.Controls.GetChildIndex(selobj.Control)
                                                         // if there is a predecessor then select it
                                                         if i-1 >=0 then
                                                             change_selection_object (object_from_controlindex (i-1))
                                            | Keys.Down -> let i = match !selection with
                                                                          None -> -1
                                                                        | Some(selobj) -> form.seqflowPanel.Controls.GetChildIndex(selobj.Control)
                                                           // if there is a successor then select it
                                                           if i+1 < form.seqflowPanel.Controls.Count then
                                                             change_selection_object (object_from_controlindex (i+1))
                                            | _ -> ()
                               );
        // we need to add the control of that object to the seqflowPanel control
        form.seqflowPanel.Controls.Add ctrl
        new_obj


    // Give the focus back to the currently selected line
    let refocus_object() =
        apply_to_selection (fun curobj -> curobj.Refocus() )


    // The user has clicked in the void.
    form.seqflowPanel.MouseDown.Add(fun _ -> // if the control (or one of the controls it contains) already has the focus
                                                  if form.seqflowPanel.ContainsFocus then
                                                     unselect_object() // then unselect the currently selected object
                                                  else
                                                     // otherwise gives the focus back to the currently selected line
                                                     refocus_object()
                                              )

    form.seqflowPanel.Enter.Add(fun _ -> apply_to_selection (fun cursel -> cursel.Control.Invalidate() ) )
    form.seqflowPanel.Leave.Add(fun _ -> apply_to_selection (fun cursel -> cursel.Control.Invalidate() ) )
    form.seqflowPanel.SizeChanged.Add(fun _ -> apply_to_selection (fun cursel -> cursel.Selection() ) )

    ////// experimental:
    //form.seqflowPanel.AutoScroll <- false
    //form.seqflowPanel.HScroll
    ////


    // Traversal buttons
    form.btNewGame.Click.Add(fun _ -> change_selection_object (AddObject ((new TraversalObject(wsparam,[||])):>WorksheetObject)) );
    //map_button_to_transform_inplace form.btPlay (fun (trav:TraversalObject) -> trav.play_for_p())
    map_button_to_transform form.btUndo (fun (trav:TraversalObject) -> trav.undo())

    // Sequence buttons
    map_button_to_transform form.btDuplicate (fun (pstrobj:WorksheetObject) -> pstrobj.Clone())
    map_button_to_transform form.btPview (fun (pstrobj) -> (pstrobj:PstringObject).pview())
    map_button_to_transform form.btOview (fun (pstrobj:PstringObject) -> pstrobj.oview())
    map_button_to_transform form.btHerProj (fun (pstrobj:PstringObject) -> pstrobj.herproj())
    map_button_to_transform form.btSubtermProj (fun (pstrobj:PstringObject) -> pstrobj.subtermproj())
    map_button_to_transform form.btPrefix (fun (pstrobj:PstringObject) -> pstrobj.prefix())
    map_button_to_transform form.btExt (fun (pstrobj:PstringObject) -> pstrobj.ext())
    map_button_to_transform form.btStar (fun (pstrobj:PstringObject) -> pstrobj.star())

    form.btNew.Click.Add(fun _ -> change_selection_object (AddObject (new EditablePstringObject(wsparam,[||]):>WorksheetObject)))
    map_button_to_transform_inplace  form.btDelete
                                     (fun selbobj ->
                                        let i = form.seqflowPanel.Controls.GetChildIndex(selbobj.Control)
                                        // last line removed?
                                        if i = form.seqflowPanel.Controls.Count-1 then
                                          if i > 0 then
                                            // select the previous line if there is one
                                            change_selection_object (object_from_controlindex (i-1))
                                          else
                                            unselect_object() // we are removing the only remaining line, so we just unselect it
                                        else // it's not the last line that is removed
                                            // select the next line
                                            change_selection_object (object_from_controlindex (i+1))

                                        // remove the control of the object from the flow panel
                                        form.seqflowPanel.Controls.Remove(selbobj.Control)
                                      );

    map_button_to_transform_inplace form.btBackspace (fun (editobj:EditablePstringObject) -> editobj.remove_last_occ())
    map_button_to_transform_inplace form.btAdd (fun (editobj:EditablePstringObject) -> editobj.add_occ())
    map_button_to_transform_inplace form.btEditLabel (fun (editobj:EditablePstringObject) -> editobj.edit_occ_label())


    // Worksheet buttons
    let filter = "Traversal worksheet *.xml|*.xml|All files *.*|*.*" in
    form.btSave.Click.Add(fun _ -> // savefile dialog box
                                    let d = new SaveFileDialog() in
                                    d.Filter <- filter;
                                    d.FilterIndex <- 1;
                                    if d.ShowDialog() = DialogResult.OK then
                                        save_worksheet d.FileName wsparam )

    form.btImport.Click.Add(fun _ ->  // openfile dialog box
                                      let d = new OpenFileDialog() in
                                      d.Filter <- filter
                                      d.FilterIndex <- 1
                                      d.Title <- "Import a worksheet..."
                                      if d.ShowDialog() = DialogResult.OK then
                                        import_worksheet d.FileName wsparam AddObject )



    // bind the graph to the viewer
    form.gViewer.Graph <- msaglgraph
    form.gViewer.AsyncLayout <- false;
    form.gViewer.AutoScroll <- true;
    form.gViewer.BackwardEnabled <- true;
    form.gViewer.Dock <- System.Windows.Forms.DockStyle.Fill;
    form.gViewer.ForwardEnabled <- true;
    form.gViewer.Location <- new System.Drawing.Point(0, 0);
    form.gViewer.MouseHitDistance <- 0.05;
    form.gViewer.Name <- "gViewer";
    form.gViewer.NavigationVisible <- true;
    form.gViewer.PanButtonPressed <- false;
    form.gViewer.SaveButtonVisible <- true;
    form.gViewer.Size <- new System.Drawing.Size(674, 505);
    form.gViewer.TabIndex <- 3;
    form.gViewer.ZoomF <- 1.0;
    form.gViewer.ZoomFraction <- 0.5;
    form.gViewer.ZoomWindowThreshold <- 0.10;

    let (msaglgraph_last_hovered_node: Microsoft.Msagl.Drawing.Node option ref) = ref None
    form.gViewer.SelectionChanged.Add(fun _ -> if isNull form.gViewer.SelectedObject then
                                                 msaglgraph_last_hovered_node := None
                                               else if (form.gViewer.SelectedObject :? Microsoft.Msagl.Drawing.Node) then
                                                 msaglgraph_last_hovered_node := Some(form.gViewer.SelectedObject :?> Microsoft.Msagl.Drawing.Node)
                                               else
                                                 msaglgraph_last_hovered_node := None
                                          );

    form.gViewer.MouseDown.Add(fun e ->
            match !selection, !msaglgraph_last_hovered_node  with
                Some(selobj), Some(nd) -> selobj.OnCompGraphNodeMouseDown e nd
              | _ -> ()
        );


    //////////
    // convert an node-occurrence to latex code
    let pstrocc_to_latex (pstrnode:pstring_occ) =
        if isNull pstrnode.tag then
          pstrnode.label
        else
          compgraph.gennode_to_latex ((unbox pstrnode.tag) : gen_node)

    form.btExportGraph.Click.Add(fun _ -> Texexportform.LoadExportGraphToLatexWindow mdiparent lnfrules)

    map_button_to_transform_inplace form.btExportSeq
                                    (fun (trav:PstringObject) ->
                                        Texexportform.LoadExportPstringToLatexWindow mdiparent pstrocc_to_latex trav.pstrcontrol.Sequence)

    form.btExportWS.Click.Add(fun _ ->  let p = form.seqflowPanel.Controls.Count
                                        let exp = ref ""
                                        for i = 0 to p-1 do
                                            exp := !exp^eol^("\\item $"^(Texexportform.traversal_to_latex pstrocc_to_latex (form.seqflowPanel.Controls.Item(i):?>PstringControl).Sequence)^"$")
                                        done;
                                        let latex_preamb = "% Generated automatically by HOG
% -*- TeX -*- -*- Soft -*-
\\documentclass{article}
\\usepackage{pstring}

\\begin{document}
\\begin{itemize}
"                                       in
                                        let latex_post = "
\\end{itemize}
\\end{document}
"
                                        Texexportform.LoadExportToLatexWindow mdiparent latex_preamb !exp latex_post)

    // execute the worksheet initialization function
    initialize_ws wsparam AddObject

    // select the last line
    let p = form.seqflowPanel.Controls.Count
    if p > 0 then
        select_object (object_from_controlindex (p-1))

    form.MdiParent <- mdiparent;
    form.WindowState<-FormWindowState.Maximized;
    ignore(form.Show())




(** Open a worksheet file **)
exception FileError of string;;
let open_worksheet mdiparent (ws_filename:string) =

    let xmldoc = new XmlDocument()
    xmldoc.Load(ws_filename);

    let xmlWorksheet = xmldoc.SelectSingleNode("worksheet") in
    assert_xmlnotnull xmlWorksheet;

    // get the source of the computation graph
    let xmlCompgraph = xmlWorksheet.SelectSingleNode("compgraph")
    assert_xmlnotnull xmlCompgraph
    let xmlSource = xmlCompgraph.SelectSingleNode("source")
    assert_xmlnotnull xmlSource
    let xmlTyp = xmlSource.Attributes.GetNamedItem("type");
    let filename = xmlSource.Attributes.GetNamedItem("file").InnerText // relative filename

    // set the current directory to the directory containing the worksheet file
    System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(ws_filename)))
    try
        let lnfrules =
            match xmlTyp.InnerText with
            "lambdaterm" ->
                // parse the lambda term
                match Parsing.parse_file Ml_parser.term_in_context Ml_lexer.token filename with
                  None -> raise (FileError("The lambda term file associated to this worksheet could not be opened!"));
                | Some(lmdterm) ->
                    // convert the term to LNF
                    [Coreml.annotatedterm_to_lnfrule (Coreml.annotate_termincontext lmdterm)]

            |"recursionscheme" ->
                // parse the recursion scheme file
                match Parsing.parse_file Hog_parser.hog_specification Hog_lexer.token filename with
                   None -> raise (FileError("The recursion scheme file associated to this worksheet could not be opened!"));
                 | Some(hors) -> // convert the rules to LNF
                                fst (Hog.rs_to_lnf hors)
            | _ -> raise (FileError("The type of the source computation graph for this worksheet is not valid."));


        // create the computation graph from the lnfrules
        let compgraph = Compgraph.rs_lnfrules_to_graph lnfrules
        ShowTraversalCalculatorWindow
                            mdiparent
                            filename     // graph source file name
                            compgraph    // computation graph
                            lnfrules     // lnf rules
                            (import_worksheet ws_filename) // Initialization function: import a default worksheet

    with FileError(msg) -> ignore(MessageBox.Show(msg, "File error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation))
         | :?System.NotSupportedException -> ignore(MessageBox.Show("Cannot open the source of the computation graph associated to this worksheet.", "File error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation))

    ;;