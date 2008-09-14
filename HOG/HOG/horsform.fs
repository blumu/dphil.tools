(** $Id$
	Description: Higher-order recursion scheme window
	Author:		William Blum
**)
#light

open System
open System.ComponentModel
open System.Data
open System.Drawing
open System.Text
open System.Windows.Forms
open System.IO
open Common
open Printf
open Type
open Lnf
open Compgraph
open Hog
open Hocpda


let appterm_of_treeviewnode (node:TreeNode) =
  node.Tag:?>appterm
;;

let is_terminal_treeviewnode (node:TreeNode) =
  node.Tag <> null &&  (match applicative_decomposition (appterm_of_treeviewnode node) with Tm(_),_ -> true  | _ -> false)
;;

let is_expandable_treeviewnode (node:TreeNode) =
  node.Tag <> null && (match applicative_decomposition (appterm_of_treeviewnode node) with Tm(_),_ -> false  | _ -> true)
;;


(* Expand the node of the treeview by performing one step of the CPDA.
   Return true if the CPDA has emitted a terminal, false otherwise. *)
let expand_term_in_treeview hors (treeview_node:TreeNode) =
    let rec expand_term_in_treeview_aux (rootnode:TreeNode) t = 
      let op,operands = applicative_decomposition t in
      match op with
        Tm(f) -> rootnode.Text <- f;
                 rootnode.ImageKey <- "BookClosed";
                 rootnode.SelectedImageKey <- "BookClosed";
                 rootnode.Tag <- t;
                 List.iter (function operand -> let newNode = new TreeNode((string_of_appterm operand), ImageKey = "Help", SelectedImageKey = "Help") in
                                                  newNode.Tag <- operand;
                                                  ignore(rootnode.Nodes.Add(newNode));
                                                  ignore(expand_term_in_treeview_aux newNode operand);
                                                ) operands;
                 rootnode.Expand();                                                             
                 true;
            (* The leftmost operator is not a terminal: we just replace the Treeview node label
               by the reduced term. *)
       | Nt(nt) -> rootnode.Text <- string_of_appterm t;
                   rootnode.Tag <- t;
                   false;
       | _ -> failwith "bug in applicative_decomposition!";
    in
    let _,redterm = step_reduce hors (appterm_of_treeviewnode treeview_node) in
    expand_term_in_treeview_aux treeview_node redterm;
;;


#light

type HorsForm = 
  class
    inherit GUI.Recscheme

    member this.validate_valuetree_path (node:TreeNode) =
      if node.Tag <> null then
          let nodepath = Cpdaform.TreeNode_get_path (if is_terminal_treeviewnode node then node else node.Parent)
          let path = List.map (function (n:TreeNode) -> n.Text) nodepath
          let v,c = this.hors.rs_path_validator path
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

    member this.InitializeComponent() =                

        // Treeview.ShowNodeToolTips is not implemented in  Mono
        if not Common.IsRunningOnMono then
            (); //this.valueTreeView.ShowNodeToolTips <- true;
            
        this.valueTreeView.add_NodeMouseDoubleClick(fun  _ e -> 
                if is_expandable_treeviewnode e.Node then
                  begin
                    ignore(expand_term_in_treeview this.hors e.Node);
                    this.validate_valuetree_path e.Node;
                  end
              );
                        
        this.valueTreeView.add_BeforeCollapse(fun _ e -> 
              // e.Node.Level is incompatible with Mono
              if e.Node.Parent = null then
                e.Cancel <- true;
            );
            
        this.valueTreeView.add_AfterSelect(fun _ e -> 
            let currentNode = this.valueTreeView.SelectedNode  
            this.btRun.Enabled <- is_expandable_treeviewnode currentNode;
            this.validate_valuetree_path currentNode;
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

        this.txtCode.Text <- (string_of_rs this.hors) ;

        
        // 
        // runButton
        //
        this.btRun.ImageList <- this.imageList
        this.btRun.ImageAlign <- System.Drawing.ContentAlignment.MiddleRight;
        this.btRun.ImageKey <- "Run"
        this.btRun.Click.Add(fun e -> let node = this.valueTreeView.SelectedNode
                                      if is_expandable_treeviewnode node then
                                        begin
                                         while not (expand_term_in_treeview this.hors node) do () done;
                                         this.validate_valuetree_path node;
                                        end
        );
        
        
        // 
        // graphButton
        // 
        this.btGraph.Click.Add(fun e -> Traversal_form.ShowCompGraphWindow this.MdiParent this.filename this.compgraph this.lnfrules );

        // 
        // calculator
        // 
        this.btCalculator.Click.Add(fun e -> Traversal_form.ShowTraversalCalculatorWindow this.MdiParent this.filename this.compgraph this.lnfrules (fun _ _ -> ()));
        
        //
        // cpdaButton
        //
        this.btCpda.Click.Add( fun e -> //create the cpda form
                                        let form = new Cpdaform.CpdaForm("CPDA built from the recursion scheme "^this.filename,
                                                                         Hocpda.hors_to_cpda Ncpda this.hors this.compgraph this.vartmtypes)
                                        form.MdiParent <- this.MdiParent;
                                        ignore(form.Show());
                             );

        //
        // pdaButton
        //
        this.btPda.Click.Add( fun e -> //create the pda
                                            let form = new Cpdaform.CpdaForm("PDA built from the recursion scheme "^this.filename,
                                                                             Hocpda.hors_to_cpda Npda this.hors this.compgraph this.vartmtypes)
                                            form.MdiParent <- this.MdiParent;
                                            ignore(form.Show());
                                 );
            

        //
        // np1pdaButton
        //
        this.btNp1pda.Click.Add( fun e -> //create the pda
                                            let form = new Cpdaform.CpdaForm("n+1-PDA built from the recursion scheme "^this.filename,
                                                                             Hocpda.hors_to_cpda Np1pda this.hors this.compgraph this.vartmtypes)
                                            form.MdiParent <- this.MdiParent;
                                            ignore(form.Show());
                                 );        

        // 
        // DisplayForm
        // 
        this.AcceptButton <- this.btRun;        


    val mutable hors : recscheme;
    val mutable lnfrules : lnfrule list;
    val mutable vartmtypes : (ident*typ) list;
    val mutable compgraph : computation_graph;
    val mutable filename : string;

    new (filename,newhors) as this =
       { hors = newhors;
         lnfrules = [];
         vartmtypes = [];
         compgraph = create_empty_graph();
         filename = "";
         }
       
       then 
        this.InitializeComponent();

        this.Text <- ("Higher-order recursion scheme - "^filename);
        this.filename <- filename;

        let rootNode = new TreeNode("Value tree", Tag = (null : obj), ImageKey = "BookStack", SelectedImageKey = "BookStack")
        ignore(this.valueTreeView.Nodes.Add(rootNode));
        rootNode.Expand();
              
        // check that the hors is well-defined
        let errors = rs_check this.hors in
        if errors <> [] then
          begin
            let msg = "Inconsistent HORS definition. Please check types and definitions of terminals, non-terminals and variables."^eol^"List of errors:"^eol^"  "^(String.concat (eol^"  ") errors) in
            //Mainform.Debug_print msg;
            failwith msg
          end
          
        // convert the rules to LNF
        let r,v = rs_to_lnf this.hors in

        this.lnfrules <- r;
        this.vartmtypes <- v;
        
        // create the computation graph from the rules of the recursion scheme in in LNF
        this.compgraph <- rs_lnfrules_to_graph this.lnfrules;
    
        this.txtOutput.Text <- "Rules in eta-long normal form:"^Environment.NewLine
                                ^(String.concat Environment.NewLine (List.map lnfrule_to_string this.lnfrules));

        let SNode = new TreeNode("S")  
        SNode.ImageKey <- "Help";
        SNode.SelectedImageKey <- "Help";
        SNode.Tag <- Nt("S");
        ignore(rootNode.Nodes.Add(SNode)); 
        this.valueTreeView.SelectedNode <- SNode;      
  end
