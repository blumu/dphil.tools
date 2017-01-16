(** $Id$
	Description: Window for Higher-order collapsible pushdown automata
	Author:		 William Blum
**)
#light
module Cpdaform

open FSharp.Compatibility.OCaml
open System
open System.ComponentModel
open System.Data
open System.Drawing
open System.Text
open System.Windows.Forms
open System.IO
open Printf
open Common
open Hog
open Hocpda



let RichTextbox_SelectLine cum (rtb:RichTextBox) line  = 
  if line < Array.length cum then
    begin
      // selection of negative length are not compatible with Mono
      //ignore(rtb.Select( cum.(line) + rtb.Lines.(line).Length , -rtb.Lines.(line).Length ));
      ignore(rtb.Select( cum.(line) , rtb.Lines.(line).Length+1 ));
//      rtb.ScrollToCaret();
    end
;;

let RichTextBox_CumulLinesLength (rtb:RichTextBox) = 
    let cum = Array.create rtb.Lines.Length 0 in
    for i = 0 to rtb.Lines.Length-2 do
        cum .(i+1) <- cum .(i) + rtb.Lines.(i).Length + 1;
    done;
    cum
;;
(** The Tag field of the Treeview nodes are of the form [(alive,hist)] where
    [alive] tells whether the CPDA is still running at that node (i.e. has not halted nor has emitted any constant)
    and [hist] contains the history of all visited configurations.
    (stored as a list of gen_configuration, the head of the list being the current configuration). **)
let cpda_confhistory_from_treeviewnode (node:TreeNode) = snd (node.Tag:?>(bool * gen_configuration list)) ;;

let cpda_conf_from_treeviewnode (node:TreeNode) = 
    try List.hd (snd (node.Tag:?>bool * gen_configuration list))
    with Failure("hd") -> failwith "wrong treeviewnode tag"
    ;;

let is_cpda_alive_at_treeviewnode (node:TreeNode) = 
    (node.Tag<>null) && (fst (node.Tag:?>bool * gen_configuration list)) ;;

let is_cpda_emitting_at_treeviewnode (node:TreeNode) = 
    (node.Tag<>null) && ( match (node.Tag:?>bool * gen_configuration list) with 
                                  _,(TmState(_),_)::q -> true
                                | _ -> false
                        ) ;;


let is_configuration_treeviewnode (node:TreeNode) = node<> null && node.Tag<>null;;

(* Add a configuration to the history.
   When consecutive configurations share the same stack, only the last one is kept in the history.
   Also terminal configurastion are not added to the history. *)
let add_conf_to_treeviewnode_history (node:TreeNode) (state,stk as newconf :gen_configuration)  = 
  let _,history = node.Tag:?>bool * gen_configuration list
  node.Tag <- match state with
                TmState(_) -> false, history
                | _ ->  true,(match history with
                               (_,prevstk)::q when prevstk = stk -> newconf::q
                                 | _ ->  newconf::history
                             )
;;

let init_treeviewnode_history (node:TreeNode) (newconf:gen_configuration) = 
   let tag = true,[newconf]
   node.Tag <- tag;;


let TreeNode_get_path (node:TreeNode) =
   let rec aux (node:TreeNode) = 
     // incompatible with Mono
     //if node.Level = 0 then
     if node.Parent = null then
       []
     else 
       (aux node.Parent)@[node]
   in
     aux node
;;
         
type CpdaForm = 
  class
    inherit GUI.Pda

    member this.selectTreeViewNodeSourceline() =
      let node = this.valueTreeView.SelectedNode
      if is_configuration_treeviewnode node then
          match cpda_conf_from_treeviewnode node with 
              TmState(_),_ -> ();
            | State(ip),_ -> RichTextbox_SelectLine this.cumul_linelength this.txtCode (ip+this.codestartline);
    
    member this.validate_valuetree_path (node:TreeNode) =
      if is_configuration_treeviewnode node then
          let nodepath = TreeNode_get_path (if is_cpda_alive_at_treeviewnode node then node.Parent else node)
          let path = List.map (function (n:TreeNode) -> n.Text) nodepath
          let v,c = this.cpda.cpda_path_validator path
          if v then
            begin
              this.txtPath.Text <- c;
              this.txtPath.ForeColor <- System.Drawing.Color.Green
            end
          else
            begin
              this.txtPath.Text <- c;
              this.txtPath.ForeColor <- System.Drawing.Color.Red
            end
      else
        this.txtPath.Clear();

    member this.expand_selected_treeviewconfiguration (node:TreeNode) =
        if is_cpda_alive_at_treeviewnode node then 
            let conf = cpda_conf_from_treeviewnode node
            try 
                  let newconf = hocpda_step this.cpda conf
                  add_conf_to_treeviewnode_history node newconf;
                  this.selectTreeViewNodeSourceline();
                  this.txtConfHistory.Text <- String.concat (eol^eol) (List.map string_of_genconfiguration (cpda_confhistory_from_treeviewnode node));                  
                  match newconf with 
                    State(ip),stk ->  node.Text <- string_of_int ip;
                                      this.validate_valuetree_path node;
                                      true;
                  | TmState(f,ql),stk ->  node.Text <- f;
                                          node.ImageKey <- "BookClosed";
                                          node.SelectedImageKey <- "BookClosed";
                                          List.iter (function ip_param -> let newNode = new TreeNode((string_of_int ip_param), ImageKey = "Help", SelectedImageKey = "Help")
                                                                          let conf = State(ip_param),stk
                                                                          init_treeviewnode_history newNode conf;
                                                                          ignore(node.Nodes.Add(newNode));
                                                    ) ql;
                                          node.Expand();
                                          this.validate_valuetree_path node;
                                          this.btRun.Enabled <- is_cpda_alive_at_treeviewnode node;
                                          this.btStep.Enabled <- is_cpda_alive_at_treeviewnode node;                                          
                                          false;
                  
            with CpdaHalt -> node.Tag <- null;
                             node.Text <- "halted";
                             node.ImageKey <- "BookClosed";
                             node.SelectedImageKey <- "BookClosed";
                             this.validate_valuetree_path node;
                             this.selectTreeViewNodeSourceline();
                             false;
        else
          false;
        

    member this.InitializeComponent() =
        // 
        // valueTreeView
        // 
        this.valueTreeView.ImageKey <- "";
        this.valueTreeView.SelectedImageKey <- "";
        this.valueTreeView.PathSeparator <- " ";
        // Treeview..ShowNodeToolTips Not implemented in  Mono
        if not Common.IsRunningOnMono then
            (); // this.valueTreeView.ShowNodeToolTips <- true;
        
        this.valueTreeView.ShowRootLines <- false;
        this.valueTreeView.add_KeyPress(fun  _ k ->  if k.KeyChar = ' ' then 
                                                        ignore(this.expand_selected_treeviewconfiguration this.valueTreeView.SelectedNode)
                                       );
        this.valueTreeView.add_NodeMouseDoubleClick(fun  _ e ->
                ignore(this.expand_selected_treeviewconfiguration e.Node);
              );
        this.valueTreeView.add_GotFocus(fun  _ _ -> 
               this.selectTreeViewNodeSourceline();
              );
        this.valueTreeView.add_BeforeCollapse(fun _ e -> 
              // e.Node.Level is incompatible with Mono
              if e.Node.Parent = null then
                e.Cancel <- true;
              );

        this.valueTreeView.add_AfterSelect(fun _ e -> 
            let currentNode = this.valueTreeView.SelectedNode  
            this.btRun.Enabled <- is_cpda_alive_at_treeviewnode currentNode;
            this.btStep.Enabled <- is_cpda_alive_at_treeviewnode currentNode;
            this.validate_valuetree_path currentNode;
            this.selectTreeViewNodeSourceline();
            if is_configuration_treeviewnode currentNode then
                this.txtConfHistory.Text <- String.concat (eol^eol) (List.map string_of_genconfiguration (cpda_confhistory_from_treeviewnode e.Node));
            else
                this.txtConfHistory.Clear();
            );
              
        // 
        // imageList
        // 
        
        this.imageList.Images.Add((GUI.Properties.Resources.roundquestionmark:>System.Drawing.Image));
        this.imageList.Images.Add((GUI.Properties.Resources.books:>System.Drawing.Image));
        this.imageList.Images.Add((GUI.Properties.Resources.closedbook:>System.Drawing.Image));
        this.imageList.Images.Add((GUI.Properties.Resources.openbook:>System.Drawing.Image));
        this.imageList.Images.Add((GUI.Properties.Resources.questionmark:>System.Drawing.Image));
        this.imageList.Images.Add((GUI.Properties.Resources.run:>System.Drawing.Image));
        this.imageList.Images.SetKeyName(0, "Help");
        this.imageList.Images.SetKeyName(1, "BookStack");
        this.imageList.Images.SetKeyName(2, "BookClosed");
        this.imageList.Images.SetKeyName(3, "BookOpen");
        this.imageList.Images.SetKeyName(4, "Item");
        this.imageList.Images.SetKeyName(5, "Run");

        // 
        // pathTextBox
        // 
        //this.pathTextBox.Text <- "Double-click on a node of the treeview to execute the corresponding transition of the CPDA.";

        // 
        // txtCode RichTextBox
        // 
        this.txtCode.Text <- string_of_hocpda this.cpda ;
        // compute lines length and the position where the code dump start
        this.cumul_linelength <- (RichTextBox_CumulLinesLength this.txtCode);
        this.codestartline <- 2+this.txtCode.GetLineFromCharIndex(this.txtCode.Find("Code:"))
        
        // 
        // btRun
        // 
        this.btRun.ImageAlign <- System.Drawing.ContentAlignment.MiddleRight;
        this.btRun.ImageKey <- "Run";
        this.btRun.ImageList <- this.imageList;
        this.btRun.TextImageRelation <- System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.btRun.Click.Add(fun e -> let node = this.valueTreeView.SelectedNode
                                      while this.expand_selected_treeviewconfiguration node do () done;
        );        
         

        // 
        // btStep
        // 
        this.btStep.ImageAlign <- System.Drawing.ContentAlignment.MiddleRight;
        this.btStep.ImageKey <- "Run";
        this.btStep.ImageList <- this.imageList;
        this.btStep.TextImageRelation <- System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.btStep.Click.Add(fun e -> let node = this.valueTreeView.SelectedNode
                                       ignore(this.expand_selected_treeviewconfiguration node);
        );        

        // 
        // DisplayForm
        // 
        this.AcceptButton <- this.btRun;
        //this.Icon <- (GUI.Properties.Resources.app:>System.Drawing.Icon)


    val mutable cpda : Hocpda.hocpda
    val mutable codestartline : int
    val mutable cumul_linelength : int array

    new (title, newcpda) as this =
       { cpda = newcpda;
         codestartline = 0;
         cumul_linelength = [||];
       }
       
       then 
        this.InitializeComponent();
        this.Text <- title;

        let rootNode = new TreeNode(title, Tag = (null : obj), ImageKey = "BookStack", SelectedImageKey = "BookStack")
        ignore(this.valueTreeView.Nodes.Add(rootNode));
        rootNode.Expand();
        

        let SNode = new TreeNode("0")
        SNode.ImageKey <- "Help";
        SNode.SelectedImageKey <- "Help";
        init_treeviewnode_history SNode (hocpda_initconf this.cpda);
        ignore(rootNode.Nodes.Add(SNode));
        this.valueTreeView.SelectedNode <- SNode;
        
  end
  