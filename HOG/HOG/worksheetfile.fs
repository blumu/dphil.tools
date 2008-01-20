(** $Id$
    Description: Open/Close worksheet files
    Author:      William Blum
**)

#light

open System.Drawing
open System.Xml
open System //For EventHandler
//open System.Xml.Serialization

open Pstring
open Compgraph
open Traversal

(** Save the worksheet to an XML file **)
let save_worksheet (filename:string) lambdatermfile lnfrules (seqflowPanel:System.Windows.Forms.FlowLayoutPanel) =
    let xmldoc = new XmlDocument()
    
    //Create Parent Node
    let xmlWorksheet = xmldoc.CreateElement("worksheet")
    xmldoc.AppendChild(xmlWorksheet) |> ignore
    
    // source of the computation graph
    let xmlCompgraph = xmldoc.CreateElement("compgraph")
    let xmlSource = xmldoc.CreateElement("source")
    xmlSource.SetAttribute("type","lambdaterm");
    xmlSource.SetAttribute("file",lambdatermfile);    
    xmlCompgraph.AppendChild(xmlSource) |> ignore    
    xmlWorksheet.AppendChild(xmlCompgraph) |> ignore

    // save a pstring sequence (= a line of the worksheet)
    let pstring_to_xml (pstr:Pstring.PstringControl) = 
      let xmlPstr = xmldoc.CreateElement("pstring")
      xmlWorksheet.AppendChild(xmlPstr) |> ignore
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

        xmlPstr.AppendChild(xmlOcc) |> ignore

      in
       Array.iter occ_to_xml pstr.Sequence
      
    in
     for i = 0 to seqflowPanel.Controls.Count-1 do
       pstring_to_xml (seqflowPanel.Controls.Item(i):?> Pstring.PstringControl)
     done;

    // save the document
    xmldoc.Save(filename);;

(** Assertions **)

let assertname nameexpected (node:XmlNode) =
  if node.Name <> nameexpected then
    failwith "Bad worksheet xml file!"

let assertnotnull (node:XmlNode) =
  if node = null then
    failwith "Bad worksheet xml file!"

(** import a worksheet XML file into the current worksheet **)
let import_worksheet (filename:string) lnfrules createAndAddPstringCtrl =
    let xmldoc = new XmlDocument()
    xmldoc.Load(filename);
    
    let xmlWorksheet = xmldoc.SelectSingleNode("worksheet") in
    assertnotnull xmlWorksheet;
    
    // load a pstring sequence (= a line of the worksheet)
    let pstring_from_xml (xmlPstr:XmlNode) = 
      assertname "pstring" xmlPstr;
    
      let xml_to_occ (xmlOcc:XmlNode) =
        assertname "occ" xmlOcc;
        
        let xmlColor = xmlOcc.SelectSingleNode("color")
        and xmlLabel = xmlOcc.SelectSingleNode("label")
        and xmlLink = xmlOcc.SelectSingleNode("link")
        and xmlShape = xmlOcc.SelectSingleNode("shape")
        and xmlTreenode = xmlOcc.SelectSingleNode("graphnode")
        in
            { tag= box (let i = int_of_string (xmlTreenode.Attributes.GetNamedItem("index").Value ) in
                        match xmlTreenode.Attributes.GetNamedItem("type").Value with
                            "node" -> InternalNode(i)                           
                          | "value" -> let v = int_of_string (xmlTreenode.Attributes.GetNamedItem("value").Value)
                                       ValueLeaf(i,v)
                          | "custom" -> Custom;
                          | _ -> failwith "Incorrect occurrence type attribute.");
              color=Color.FromName(xmlColor.InnerText);
              label=xmlLabel.InnerText;
              link=int_of_string xmlLink.InnerText;
              shape=shape_of_string xmlShape.InnerText; }
      in
        let p = xmlPstr.ChildNodes.Count in
        let seq = Array.create p (create_blank_occ()) in
        for i = 0 to p-1 do
             seq.(i) <- xml_to_occ (xmlPstr.ChildNodes.Item(i));
        done;
        ignore(createAndAddPstringCtrl seq);
    in
     let n = xmlWorksheet.ChildNodes.Count in
     for i = 1 to n-1 do
       pstring_from_xml (xmlWorksheet.ChildNodes.Item(i))
     done;
    ;;


(** Open a worksheet file **)
let open_worksheet (ws_filename:string) showworksheet_func = 

    let xmldoc = new XmlDocument()
    xmldoc.Load(ws_filename);
    
    let xmlWorksheet = xmldoc.SelectSingleNode("worksheet") in
    assertnotnull xmlWorksheet;

    // get the source of the computation graph
    let xmlCompgraph = xmlWorksheet.SelectSingleNode("compgraph")
    assertnotnull xmlCompgraph
    let xmlSource = xmlCompgraph.SelectSingleNode("source")
    assertnotnull xmlSource
    let xmlTyp = xmlSource.Attributes.GetNamedItem("type");
    let lmd_filename = xmlSource.Attributes.GetNamedItem("file").InnerText

    match xmlTyp.InnerText with
    "lambdaterm" ->
        // parse the lambda term
        match Parsing.parse_file Ml_parser.term_in_context Ml_lexer.token lmd_filename with
          None -> ()
        | Some(lmdterm) -> 
            // convert the term to LNF
            let lnfterm = Coreml.lmdterm_to_lnf lmdterm
            // create the computation graph from the LNF of the term
            let compgraph = Compgraph.lnfrs_to_graph [("",lnfterm)]

            showworksheet_func  lmd_filename // graph source file name
                                compgraph    // computation graph
                                ["",lnfterm] // lnf
                                // Initialization function
                                (function createAndAddPstringCtrl -> // import a default worksheet if requested
                                                                     import_worksheet ws_filename lnfterm createAndAddPstringCtrl
                                )
    |"hors" -> ()
    
    | _ -> failwith "Unknown computation graph source!"
    ;;