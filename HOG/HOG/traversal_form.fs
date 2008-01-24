(** $Id: $
	Description: Traversals window
	Author:		William Blum
**)

#light
open Common
open System.Xml
open System.Drawing
open System.Windows.Forms
open Traversal
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
  if node = null then
    failwith "Bad worksheet xml file!"


(** Map a player to a node shape **)
let player_to_shape = function Proponent -> ShapeRectangle
                               | Opponent -> ShapeOval
(** Map a player to a node color **)
let player_to_color = function Proponent -> Color.Coral
                               | Opponent -> Color.CornflowerBlue
(***** Pstring functions *****)

(** Convert a colors of type System.Drawing.Color to a color of type Microsoft.Glee.Drawing.Color **)
let sysdrawingcolor_2_gleecolor (color:System.Drawing.Color) :Microsoft.Glee.Drawing.Color = 
    Microsoft.Glee.Drawing.Color(color.R,color.G,color.B)

(** Convert a shape of type Pstring.Shapes to a shape of type Microsoft.Glee.Drawing.Shape **)
let pstringshape_2_gleeshape =
    function  Pstring.Shapes.ShapeRectangle -> Microsoft.Glee.Drawing.Shape.Box
             | Pstring.Shapes.ShapeOval -> Microsoft.Glee.Drawing.Shape.Diamond //Microsoft.Glee.Drawing.Shape.Circle

(** Create a pstring occurrence from a gennode **)
let occ_from_gennode (compgraph:computation_graph) gennode lnk =
    let player = compgraph.gennode_player gennode
    let lab =
        match gennode with
          Custom -> "?"
        | ValueLeaf(nodeindex,v) -> (string_of_int nodeindex)^"_{"^(compgraph.node_label_with_idsuffix nodeindex)^"}"
        | InternalNode(nodeindex) -> compgraph.node_label_with_idsuffix nodeindex
    { label= lab;
      tag = box gennode; // Use the tag field to store the generalized graph node
      link = lnk;
      shape=player_to_shape player;
      color=player_to_color player
    } 
                                        

(** Return the generalized graph node of a given occurrence in a Pstringcontrol.pstring sequence **)
let pstr_occ_getnode (nd:pstring_occ) =
    if nd.tag = null then
        failwith "This is not a valid justified sequence. Some node-occurrence does not belong to the computation graph/tree."
    else 
        (unbox nd.tag:gen_node)


/////////////////////// Sequence transformations

(**** Sequence transformations specialized for sequences of type Pstringcontrol.pstring  ****)
     
(** P-View **) 
let pstrseq_pview_at gr seq i = seq_Xview
                                     gr
                                     Proponent 
                                     pstr_occ_getnode        // getnode function
                                     pstr_occ_getlink        // getlink function
                                     pstr_occ_updatelink     // update link function
                                     seq
                                     i // compute the P-view at the occurrence i
                                     
let pstrseq_pview gr seq = pstrseq_pview_at gr seq ((Array.length seq)-1)
                                    

(** O-View **) 
let pstrseq_oview_at gr seq i = seq_Xview
                                     gr
                                     Opponent
                                     pstr_occ_getnode        // getnode function
                                     pstr_occ_getlink        // getlink function
                                     pstr_occ_updatelink     // update link function
                                     seq
                                     i // compute the O-view at the occurrence i

let pstrseq_oview gr seq = pstrseq_oview_at gr seq ((Array.length seq)-1)

(** Subterm projection with respect to a reference root node **) 
let pstrseq_subtermproj gr = subtermproj gr
                                         pstr_occ_getnode
                                         pstr_occ_getlink
                                         pstr_occ_updatelink

(** Hereditary projection **) 
let pstrseq_herproj = heredproj pstr_occ_getlink
                                pstr_occ_updatelink
                                
(** Prefixing **)
let pstrseq_prefix seq at = if Array.length seq = 0 then [||] else Array.sub seq 0 (at+1)


(** Traversal star **)
let pstrseq_star gr = star gr
                           pstr_occ_getnode
                           pstr_occ_getlink
                           pstr_occ_updatelink


(** Traversal extension **)
let pstrseq_ext gr = extension gr
                               pstr_occ_getnode
                               pstr_occ_getlink
                               pstr_occ_updatelink
                               Pstring.create_dummy_occ

/////////////////////// GLEE graph generation

(** Set the attributes for a node of the graph
    @param node_2_color a mapping from compgraph nodes to colors
    @param node_2_shape a mapping from compgraph nodes to shapes
    @param gr is the computation graph
    @param i is the node number in the computation graph
    @param node is the GLEE graph node
    **)
let gleegraphnode_set_attributes node_2_color node_2_shape (gr:computation_graph) i (node:Microsoft.Glee.Drawing.Node) =
    node.Attr.Id <- string_of_int i
    node.Attr.Shape <- pstringshape_2_gleeshape (node_2_shape gr.nodes.(i))
    node.Attr.Fillcolor <- sysdrawingcolor_2_gleecolor (node_2_color gr.nodes.(i))
    node.Attr.Label <- gr.node_label_with_idsuffix i
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


(** [compgraph_to_graphview node_2_color node_2_shape gr] creates the GLEE graph view of a computation graph.
    @param node_2_color a mapping from compgraph nodes to colors
    @param node_2_shape a mapping from compgraph nodes to shapes
    @param gr the computation graph    
    @return the GLEE graph object representing the computation graph
**)
let compgraph_to_graphview node_2_color node_2_shape (gr:computation_graph) =
    (* create a graph object *)
    let gleegraph = new Microsoft.Glee.Drawing.Graph("graph") in
    
    (* add the nodes to the graph *)
    for k = 0 to (Array.length gr.nodes)-1 do
        gleegraphnode_set_attributes grnode_2_color
                                     grnode_2_shape
                                     gr
                                     k
                                     (gleegraph.AddNode (string_of_int k))
    done;

    (* add the edges to the graph *)
    let addtargets source targets =
        let source_id = string_of_int source in
        let aux i target = 
            let target_id = string_of_int target in
            let edge = gleegraph.AddEdge(source_id,target_id) in
            (match gr.nodes.(source) with
                NCntApp -> edge.EdgeAttr.Label <- string_of_int i;
                               (* Highlight the first edge if it is an @-node (going to the operator) *)
                               if i = 0 then edge.EdgeAttr.Color <- Microsoft.Glee.Drawing.Color.Green;
               | NCntAbs(_)  -> ();
               | _ -> edge.EdgeAttr.Label <- string_of_int (i+1);
            )

        in 
        List.iteri aux targets
    in
    Array.iteri addtargets gr.edges;
    gleegraph
;;


(** Loads a window showing a computation graph and permitting the user to export it to latex. **)
let ShowCompGraphWindow mdiparent filename compgraph lnfrules =
    // create a form
    let form = new System.Windows.Forms.Form()
    let viewer = new Microsoft.Glee.GraphViewerGdi.GViewer()
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
    buttonLatex.Click.Add(fun e -> Texexportform.LoadExportGraphToLatexWindow mdiparent lnfrules )


    // create a viewer object
    panel1.SuspendLayout();
    panel1.Anchor <- Enum.combine [ System.Windows.Forms.AnchorStyles.Top ; 
                                    System.Windows.Forms.AnchorStyles.Bottom ; 
                                    System.Windows.Forms.AnchorStyles.Left ; 
                                    System.Windows.Forms.AnchorStyles.Right ];
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

// Worksheet parameters
type WorksheetParam = { graphsource_filename:string; 
                        compgraph:computation_graph; 
                        lnfrules: lnfrule list;
                        gleeviewer: Microsoft.Glee.GraphViewerGdi.GViewer
                        seqflowpanel: System.Windows.Forms.FlowLayoutPanel
                        labinfo: System.Windows.Forms.Label
                      }


/////////////////////// Worksheet objects
type WorksheetObject =
  class
    val ws : WorksheetParam

    new(nws) as this = {ws = nws}
                                                  
    abstract Control : System.Windows.Forms.Control
    abstract Clone : unit -> WorksheetObject
    abstract Selection : unit -> unit
    abstract Deselection : unit -> unit
    abstract Refocus : unit -> unit
    abstract OnCompGraphNodeMouseDown : MouseEventArgs -> Microsoft.Glee.Drawing.Node  -> unit 

    // [ToXmlElement xmldoc] converts the object into an XML element.
    // @param xmldoc is the xmldocument
    // @return the XML element created
    abstract ToXmlElement : System.Xml.XmlDocument -> System.Xml.XmlElement
    
  end 

type PstringObject = 
  class
    inherit WorksheetObject as base
    val pstrcontrol : Pstring.PstringControl
    new (nws,pstr:pstring) as this = {inherit WorksheetObject(nws);
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
        x.pstrcontrol.Selection() // tell the control that it's about to be selected using PstringControl.Selection()
        x.pstrcontrol.Select()    // select it with System.Windows.Forms.Form.Select()
        
    override x.Deselection() =
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
        
        let xmlColor = xmlOcc.SelectSingleNode("color")
        and xmlLabel = xmlOcc.SelectSingleNode("label")
        and xmlLink = xmlOcc.SelectSingleNode("link")
        and xmlShape = xmlOcc.SelectSingleNode("shape")
        and xmlTreenode = xmlOcc.SelectSingleNode("graphnode")
        in
            { tag= box (
                        match xmlTreenode.Attributes.GetNamedItem("type").Value with
                            "node" -> InternalNode(int_of_string (xmlTreenode.Attributes.GetNamedItem("index").Value))                           
                          | "value" -> ValueLeaf(int_of_string (xmlTreenode.Attributes.GetNamedItem("index").Value),
                                                 int_of_string (xmlTreenode.Attributes.GetNamedItem("value").Value))
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
        if occ.tag = null then
          xmlTreenode.SetAttribute("type","custom");
        else
          match (occ.tag :?> gen_node) with
          | Custom -> xmlTreenode.SetAttribute("type","custom");
          | InternalNode(i) -> xmlTreenode.SetAttribute("type","node");
                               xmlTreenode.SetAttribute("index",(string_of_int i));
          | ValueLeaf(i,v) -> xmlTreenode.SetAttribute("type","value");
                              xmlTreenode.SetAttribute("index",(string_of_int i));
                              xmlTreenode.SetAttribute("value",(string_of_int v));
          
            
        xmlOcc.AppendChild(xmlTreenode) |> ignore

        rootelement.AppendChild(xmlOcc) |> ignore

      in
       Array.iter occ_to_xml x.pstrcontrol.Sequence
        
  end
  
             
(** Editable pstring object **)
type EditablePstringObject = 
  class
    inherit PstringObject as base
    
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
    override x.OnCompGraphNodeMouseDown e gleenode =
        // add the selected graph node at the end of the sequence
        let gr_nodeindex = int_of_string gleenode.Attr.Id
        
        // add an internal node to the traversal
        if e.Button = MouseButtons.Left then
            x.pstrcontrol.add_node (occ_from_gennode x.ws.compgraph (InternalNode(gr_nodeindex)) 0 )
        // add a value-leaf node to the traversal
        else
            x.pstrcontrol.add_node (occ_from_gennode x.ws.compgraph (ValueLeaf(gr_nodeindex,1)) 0 )

        x.Refocus()
        
    override x.pview() = 
        let seq = pstrseq_pview_at x.ws.compgraph base.pstrcontrol.Sequence x.pstrcontrol.SelectedNodeIndex
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.oview() = 
        let seq = pstrseq_oview_at x.ws.compgraph base.pstrcontrol.Sequence x.pstrcontrol.SelectedNodeIndex
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


(** Traversal object: type used for sequence respecting the traversal rules **)
type TraversalObject = 
  class
    inherit PstringObject as base
    
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
      x.pstrcontrol.nodeClick.Add(fun e -> 
        if x.wait_for_ojustifier <> [] then
            if List.mem e.node x.wait_for_ojustifier then
               // update the link
               x.pstrcontrol.updatejustifier (x.pstrcontrol.Length-1) e.node
               x.wait_for_ojustifier <- []
               x.highlight_potential_justifiers()
               // play for the computer
               x.play_for_p()
            else
              ignore( MessageBox.Show("This justifier is not valid for the selected move!","This is not a valid justifier!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation))
              
          )
    

    // Constructor
    new (ws,pstr:pstring) as x = {inherit PstringObject(ws,pstr)
                                  valid_omoves=Map.empty
                                  wait_for_ojustifier=[]
                                 }
                                 then 
                                    TraversalObject.init x

    override x.Clone() = new TraversalObject(x.ws,base.pstrcontrol.Sequence):>WorksheetObject

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
                                            base.pstrcontrol.remove_last_occ() // delete the trailing dummy node
                                          TraversalObject.init x

    member x.RefreshLabelInfo() =
        x.ws.labinfo.Text <- if Map.is_empty x.valid_omoves then "Traversal completed!"
                             else if x.wait_for_ojustifier = [] then  "Pick a node in the graph!"
                             else "Pick a justifier in the sequence!"
        
    override x.Selection()=
        x.RefreshCompGraphViewer()
        x.RefreshLabelInfo()
        //let hscr = x.ws.seqflowpanel.HorizontalScroll
        //hscr.Value <- hscr.Maximum
        //base.pstrcontrol.Width
        
        
        base.Selection()

    override x.Deselection()=
        x.RestoreCompGraphViewer()
        base.Deselection()
        
    (* a graph-node has been clicked while this object was selected *)
    override x.OnCompGraphNodeMouseDown e gleenode = 
        let gr_nodeindex = int_of_string gleenode.Attr.Id
        let l = x.pstrcontrol.Length
        
        // valid O-move?
        try
            let valid_justifiers = Map.find gr_nodeindex x.valid_omoves in
        
           
            match valid_justifiers with
                [] -> // no justifier
                      // create a new occurrence for the corresponding graph node
                      let newocc = occ_from_gennode x.ws.compgraph (InternalNode(gr_nodeindex)) 0
                      x.pstrcontrol.replace_last_node newocc // add the occurrence at the end of the sequence
                      x.wait_for_ojustifier <- []
                      x.play_for_p() // play for the Propoment
                      
              | [j] -> // only one justifier, the Opponent has no choice
                      let newocc = occ_from_gennode x.ws.compgraph (InternalNode(gr_nodeindex)) ((l-1)-j)
                      x.pstrcontrol.replace_last_node newocc
                      x.wait_for_ojustifier <- []
                      x.play_for_p()
                      
              | j::q -> // more than one choice: wait for the Opponent to choose one before playing for P
                      let newocc = occ_from_gennode x.ws.compgraph (InternalNode(gr_nodeindex)) 0
                      x.pstrcontrol.replace_last_node newocc
                      x.wait_for_ojustifier <- valid_justifiers
                      x.highlight_potential_justifiers()
                      x.RefreshLabelInfo()
                          
           
        // Invalid move 
        with Not_found -> ()
        
        x.Refocus()
    
    (* Add an occurrence of a gennode to the end of the traversal *)
    member private x.add_gennode gennode link =
        x.pstrcontrol.add_node (occ_from_gennode x.ws.compgraph gennode link)

    (* Update the graph node colors to indicate to the user which moves are allowed *)
    member x.RefreshCompGraphViewer() =
        let gleegraph = x.ws.gleeviewer.Graph
        for k = 0 to gleegraph.NodeCount-1 do
           let gleenode = gleegraph.FindNode(string_of_int k)
           gleegraphnode_set_attributes (fun _ -> if Map.mem k x.valid_omoves then player_to_color Opponent 
                                                  else Color.LightGray )
                                        grnode_2_shape
                                        x.ws.compgraph
                                        k
                                        gleenode
        done;
        x.ws.gleeviewer.Invalidate()

    (* Restore the original node colors in the graph-view *)
    member x.RestoreCompGraphViewer() =
        let gleegraph = x.ws.gleeviewer.Graph
        for k = 0 to gleegraph.NodeCount-1 do
           let gleenode = gleegraph.FindNode(string_of_int k)
           gleegraphnode_set_attributes grnode_2_color
                                        grnode_2_shape
                                        x.ws.compgraph
                                        k
                                        gleenode
        done;
        x.ws.gleeviewer.Invalidate()
    
    (* Compute the list of valid O-moves *)    
    member private x.recompute_valid_omoves() =
        let l = x.pstrcontrol.Length
        // Rule (Root)
        if  l = 0 then // seq is the empty traversal ?
            x.valid_omoves <- Map.add 0 [] Map.empty
        else
            let last = x.pstrcontrol.Occurrence(l-1)
            // What is the type of the last occurrence?
            match pstr_occ_getnode last with
            
                  Custom -> failwith "Bad traversal! There is an occurence of a node that does not belong to the computation graph!"
                
                // it is an intenal-node
                | InternalNode(i) -> match x.ws.compgraph.nodes.(i) with
                                      // Traversal rule (Lmd):
                                      | NCntAbs(_,_) ->
                                          failwith "O has no valid move since it's P's turn!"
                                      
                                      // Rule (App): O can only play the 0th child of the node i
                                      |NCntApp ->
                                          // find the child node of the lambda node
                                          let firstchild = List.hd x.ws.compgraph.edges.(i) 
                                          x.valid_omoves <- Map.add firstchild [l-1] Map.empty
                                      
                                      // Rule (Var) or (InputVar): O can play any P-move whose parent occur in the O-view
                                      | NCntVar(_) | NCntTm(_) ->
                                          // function mapping an occurrence to the index of the corresponding node in the graph
                                          let get_grnodeindex_from_occ occ =
                                                match pstr_occ_getnode (x.pstrcontrol.Occurrence(occ)) with 
                                                  | Custom | ValueLeaf(_,_) -> failwith "Bad traversal!" // the O-view cannot contain value leaf since the traversal ends with a variable node
                                                  | InternalNode(i) -> i
                                          
                                          // Is it an input variable?
                                          if x.ws.compgraph.he_by_root.(i) then
                                              // Rule (InputVar): O can play any P-move whose parent occur in the O-view
                                             
                                              // get the list of occurrence from the O-view
                                              let oview_occs = seq_occs_in_Xview
                                                                  x.ws.compgraph
                                                                  Opponent 
                                                                  pstr_occ_getnode        // getnode function
                                                                  pstr_occ_getlink        // getlink function
                                                                  pstr_occ_updatelink     // update link function
                                                                  x.pstrcontrol.Sequence
                                                                  (l-1)
                                              
                                              // filter the list to keep only occurrences of Opponent nodes
                                              let parents_occs = List.filter (fun o -> (x.ws.compgraph.gennode_player (pstr_occ_getnode (x.pstrcontrol.Occurrence(o)))) = Proponent) oview_occs
                                              
                                              // given an occurrence occ of a graph node,
                                              // map all its children in the graph to itself
                                              let map_children_to_occ occ m =
                                                List.fold_right (fun j s ->
                                                                    Map.add j (occ::(try Map.find j s with Not_found -> [])) s)
                                                                x.ws.compgraph.edges.(get_grnodeindex_from_occ occ)
                                                                m
                                                
                                              // finally compute a Map which associate for each node of the tree 
                                              // whose parent occurs in the O-view, the set of occurrences
                                              // of its parent in the O-view.
                                              let children_to_parentoccs = List.fold_left (fun s o -> map_children_to_occ o s)
                                                                                          Map.empty
                                                                                          parents_occs
                                              
                                              
                                              // children_to_parentoccs is precisely the list of valid O-moves:
                                              // a map from valid O-moves to the list of all valid justifiers in the traversal!
                                              x.valid_omoves <- children_to_parentoccs

                                           
                                           else // it is not an input variable
                                             
                                             // Rule (Var): O can only copy-cat the last P-move
                                             let justifier = l - 1 - last.link - 1
                                             let copycat_node =
                                                x.ws.compgraph.nth_child (get_grnodeindex_from_occ justifier)
                                                                         x.ws.compgraph.parameterindex.(i)
                                             in
                                             x.valid_omoves <- Map.add copycat_node [justifier] Map.empty

                                      
                // it is a value leaf
                | ValueLeaf(i,v) -> // TODO: play using the copycat rule (Value) or the (InputValue) rule
                                    x.valid_omoves <- Map.empty

        if not (Map.is_empty x.valid_omoves) then
          x.pstrcontrol.add_node (create_blank_occ()) // add a dummy node for the forthcoming initial O-move
        
        x.RefreshLabelInfo()
                                    
    
    (* Play the next move for P according to the strategy given by the term *)
    member x.play_for_p() =
        let l = x.pstrcontrol.Length
        if l = 0 then failwith "The game has not started yet! You (the Opponent) must start!"
            
        let last = x.pstrcontrol.Occurrence(l-1)
        // What is the type of the last occurrence?
        match pstr_occ_getnode last with
            | Custom -> failwith "Bad traversal! There is an occurence of a node that does not belong to the computation graph!"
            
            // it is an intenal-node
            | InternalNode(i) -> match x.ws.compgraph.nodes.(i) with
                                    NCntApp | NCntVar(_) | NCntTm(_) 
                                        -> failwith "It is your turn. You are playing the Opponent!"

                                  // Traversal rule (Lmd):
                                  | NCntAbs(_,_) ->
                                      // find the child node of the lambda node
                                      let firstchild = List.hd x.ws.compgraph.edges.(i) 
                                      
                                      // Get the span of the node
                                      let p = x.ws.compgraph.span.(firstchild) in
                                      
                                      // compute the link
                                      let link =
                                          // if no span 
                                          if p = -1 then
                                            0 // then it's an @-node so it has no justifier
                                          else
                                            l-
                                              // get the occurrence of the binder in the P-view
                                              // the binder b can be reached in the traversal by going
                                              // (p-1)-steps backward in the P-view of the traversal
                                              (seq_Xview_ilast x.ws.compgraph
                                                               Proponent 
                                                               pstr_occ_getnode        // getnode function
                                                               pstr_occ_getlink        // getlink function
                                                               pstr_occ_updatelink     // update link function
                                                               x.pstrcontrol.Sequence
                                                               (l-1)
                                                               (p-1))
                                      
                                      x.add_gennode (InternalNode(firstchild)) link 
                                      x.recompute_valid_omoves()
                                      x.RefreshCompGraphViewer()
                                  
            // it is a value leaf
            | ValueLeaf(i,v) -> // TODO: play using the copycat rule (Value)
                                ()


    (** [adjust_to_valid_occurrence i] adujsts the occurrence index i to a valid occurrence position by making sure
        that it does not refer to the trailling dummy node (labelled "...") a the end of the sequence.
        
        @return the index of the first valid occurrence preceding occurrence number i,
        and -1 if there is no such occurrence (if the traversal is empty) **)
    member private x.adjust_to_valid_occurrence i =
        i - (let occ = x.pstrcontrol.Occurrence(i)
             if occ.tag = null || pstr_occ_getnode occ = Custom  then 1 else 0)

    override x.pview() = 
        let seq = pstrseq_pview_at x.ws.compgraph base.pstrcontrol.Sequence (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new TraversalObject(x.ws,seq)):>WorksheetObject
    override x.oview() = 
        let seq = pstrseq_oview_at x.ws.compgraph base.pstrcontrol.Sequence (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.herproj() =
        let seq_with_no_trail = Array.sub x.pstrcontrol.Sequence 0 (1+(x.adjust_to_valid_occurrence (x.pstrcontrol.Length -1)))
        let seq = pstrseq_herproj seq_with_no_trail (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.subtermproj() =
        let seq_with_no_trail = Array.sub x.pstrcontrol.Sequence 0 (1+(x.adjust_to_valid_occurrence (x.pstrcontrol.Length -1)))
        let seq = pstrseq_subtermproj x.ws.compgraph seq_with_no_trail (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.prefix()=
        // make sure that the selected node does not refer to the trailling dummy occurrence
        let p = x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex
        let seq = if p = -1 then [||]
                  else pstrseq_prefix
                        x.pstrcontrol.Sequence 
                        // make sure p refers to a Proponent node
                        (match pstr_occ_getnode (x.pstrcontrol.Occurrence(x.pstrcontrol.SelectedNodeIndex)) with
                        | _ as node when x.ws.compgraph.gennode_player node = Opponent -> p-1
                        | _ -> p)
        (new TraversalObject(x.ws,seq)):>WorksheetObject
    override x.ext()=
        let seq = pstrseq_ext x.ws.compgraph x.pstrcontrol.Sequence (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
    override x.star()=
        let seq = pstrseq_star x.ws.compgraph x.pstrcontrol.Sequence (x.adjust_to_valid_occurrence x.pstrcontrol.SelectedNodeIndex)
        (new EditablePstringObject(x.ws,seq)):>WorksheetObject
 
  end



// execute a function on the current selection if it is of a given type
// the parameter (_:'a->unit) is a trick used to pass the type 'a as a parameter to the function
let do_onsomeobject_oftype (_:'a->unit) (f:'a->unit)  = function 
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

    let form  = new Traversal.Traversal()

    // create the GLEE graph
    let gleegraph = compgraph_to_graphview grnode_2_color grnode_2_shape compgraph            

    // this holds the WorksheetObject that is currently selected.
    let selection : WorksheetObject option ref = ref None

    // Traversal game buttons
    let wsparam = { graphsource_filename=graphsource_filename;
                    compgraph=compgraph;
                    lnfrules=lnfrules;
                    gleeviewer=form.gViewer
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
    
    form.seqflowPanel.Enter.Add( fun _ -> apply_to_selection (fun cursel -> cursel.Control.Invalidate() )
                                    )
    form.seqflowPanel.Leave.Add( fun _ -> apply_to_selection (fun cursel -> cursel.Control.Invalidate() )
                                    )
    
    



    map_button_to_transform form.btDuplicate (fun (pstrobj:WorksheetObject) -> pstrobj.Clone())    
    map_button_to_transform form.btPview (fun (pstrobj) -> (pstrobj:PstringObject).pview())
    map_button_to_transform form.btOview (fun (pstrobj:PstringObject) -> pstrobj.oview())
    map_button_to_transform form.btHerProj (fun (pstrobj:PstringObject) -> pstrobj.herproj())
    map_button_to_transform form.btSubtermProj (fun (pstrobj:PstringObject) -> pstrobj.subtermproj())
    map_button_to_transform form.btPrefix (fun (pstrobj:PstringObject) -> pstrobj.prefix())
    map_button_to_transform form.btExt (fun (pstrobj:PstringObject) -> pstrobj.ext())
    map_button_to_transform form.btStar (fun (pstrobj:PstringObject) -> pstrobj.star())
    
    
    form.btDelete.Click.Add( fun _ -> apply_to_selection
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
                                              )
                                       );

    form.btNew.Click.Add(fun _ -> change_selection_object (AddObject (new EditablePstringObject(wsparam,[||]):>WorksheetObject))
                );
                
    map_button_to_transform_inplace form.btBackspace (fun (editobj:EditablePstringObject) -> editobj.remove_last_occ())
    map_button_to_transform_inplace form.btAdd (fun (editobj:EditablePstringObject) -> editobj.add_occ())
    map_button_to_transform_inplace form.btEditLabel (fun (editobj:EditablePstringObject) -> editobj.edit_occ_label())
                                              

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
                           
    form.btNewGame.Click.Add(fun _ -> change_selection_object (AddObject ((new TraversalObject(wsparam,[||])):>WorksheetObject))
                                );

    //map_button_to_transform_inplace form.btPlay (fun (trav:TraversalObject) -> trav.play_for_p())


    // bind the graph to the viewer
    form.gViewer.Graph <- gleegraph
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

    let (gleegraph_last_hovered_node: Microsoft.Glee.Drawing.Node option ref) = ref None
    form.gViewer.SelectionChanged.Add(fun _ -> if form.gViewer.SelectedObject = null then
                                                      gleegraph_last_hovered_node := None
                                                    else if (form.gViewer.SelectedObject :? Microsoft.Glee.Drawing.Node) then
                                                      gleegraph_last_hovered_node := Some(form.gViewer.SelectedObject :?> Microsoft.Glee.Drawing.Node)
                                                    else
                                                      gleegraph_last_hovered_node := None
                                          );
    
    form.gViewer.MouseDown.Add(fun e -> 
            match !selection, !gleegraph_last_hovered_node  with 
                Some(selobj), Some(nd) -> selobj.OnCompGraphNodeMouseDown e nd
              | _ -> ()
        );
        
    
    //////////
    // convert an node-occurrence to latex code
    let pstrocc_to_latex (pstrnode:pstring_occ) =
        if pstrnode.tag = null then
          pstrnode.label
        else
          compgraph.gennode_to_latex ((unbox pstrnode.tag) : gen_node)
    
    form.btExportGraph.Click.Add(fun _ -> Texexportform.LoadExportGraphToLatexWindow mdiparent lnfrules)
    
    map_button_to_transform_inplace form.btExportSeq
                                    (fun (trav:TraversalObject) -> 
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
"
                                        and latex_post = "
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
    let filename = xmlSource.Attributes.GetNamedItem("file").InnerText

    let lnfrules =
        match xmlTyp.InnerText with
        "lambdaterm" ->
            // parse the lambda term
            match Parsing.parse_file Ml_parser.term_in_context Ml_lexer.token filename with
              None -> failwith "The lambda term file associated to this worksheet could not be opened!"
            | Some(lmdterm) -> 
                // convert the term to LNF
                [Coreml.lmdterm_to_lnfrule lmdterm]

        |"recursionscheme" -> 
            // parse the recursion scheme file
            match Parsing.parse_file Hog_parser.hog_specification Hog_lexer.token filename with
               None -> failwith "The recursion scheme file associated to this worksheet could not be opened!"
             | Some(hors) -> // convert the rules to LNF
                             fst (Hog.rs_to_lnf hors)
        
        | _ -> failwith "Unknown computation graph source!"


    // create the computation graph from the lnfrules
    let compgraph = Compgraph.rs_lnfrules_to_graph lnfrules
    ShowTraversalCalculatorWindow
                        mdiparent
                        filename     // graph source file name
                        compgraph    // computation graph
                        lnfrules     // lnf rules
                        (import_worksheet ws_filename) // Initialization function: import a default worksheet
    
    ;;