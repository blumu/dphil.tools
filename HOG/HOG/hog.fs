//namespace Comlab
module Hogrammar



type typ = Gr | Ar of typ * typ ;;

let rec typeorder = function  Gr ->  0 | Ar(a,b) -> max (1+ typeorder a) (typeorder b) ;;
let rec typearity = function  Gr ->  0 | Ar(_,b) -> 1+ (typearity b) ;;


type ident = string;;
type alphabet = (ident * typ) list;;
type terminal = ident;;
type nonterminal = ident;;

(* applicative term *)
type appterm = Nt of nonterminal | Tm of terminal | Var of ident | App of appterm * appterm;;

type rule = nonterminal * ident list * appterm;;
type recscheme = { nonterminals : alphabet;
				   sigma : alphabet;
				   rules : rule list } ;;


let rcs = { nonterminals = [ "S", Gr;
                             "F", Ar(Gr,Ar(Gr,Gr)) ;
                             "G", Ar(Ar(Gr,Gr),Ar(Gr,Gr)) ];
  sigma = [ "f", Ar(Gr,Ar(Gr,Gr));
            "g", Ar(Gr,Gr);
            "t", Ar(Gr,Ar(Gr,Gr)) ;
            "e", Gr ];
  rules = [ "S",[],App(App(Tm("f"), Tm("e")),Tm("e"));
          "F",["x";"y"],App(App(Tm("f"), Var("x")), Var("y"));
          "G",["phi";"x"],App(Tm("g"), App(Tm("g"), Var("x")));
        ]
 } ;;


(* HORS producing Urzyczyn's tree *)
let urz = { nonterminals = [ "S", Gr;
                             "D", Ar(Ar(Gr,Ar(Gr,Gr)), Ar(Gr,Ar(Gr,Ar(Gr,Gr)))) ;
                             "F", Ar(Gr,Gr) ;
                             "E", Gr ;
                             "G", Ar(Gr,Ar(Gr,Gr)) ;
							 ];
  sigma = [ "[", Ar(Gr,Gr);
            "]", Ar(Gr,Gr);
            "*", Ar(Gr,Gr);
            "3", Ar(Gr,Ar(Gr,Ar(Gr,Gr)));
            "e", Gr;
            "r", Gr;
			];
  rules = [ "S",[],App(Tm("["),
					 App(App(App(App(Nt("D"), Nt("G")),Nt("E")),Nt("E")),Nt("E"))
					); 
          "D",["phi";"x";"y";"z"],
			App(App(App(Tm("3"),
				App(Tm("["), 
				App(App(App(App(Nt("D"), App(App(Nt("D"), Var("phi")),Var("x"))),
				Var("z")
				),
				App(Nt("F"), Var("y"))
				),App(Nt("F"), Var("y")))
				)),
				App(Tm("]"), App(App(Var("phi"),Var("y")),Var("x")))),
				App(Tm("*"), Var("z")));
		  "F",["x"],App(Tm("*"),Var("x"));
		  "E",[],Tm("e");
		  "G",["u";"v"],Tm("r");
        ]
 };;

 
let rec string_of_type = function
    Gr -> "o"
  | Ar(a,b) -> "("^(string_of_type a)^" -> "^(string_of_type b)^")"
;;

string_of_type (Ar(Gr,Ar(Gr,Gr)));;


let string_of_alphabet a =
	let string_of_letter (l,t) =
	  l^":"^(string_of_type t)^"\n" ;
	in
	List.fold_left (function acc -> function l -> acc^(string_of_letter l)) "" a
;;
let print_alphabet a = print_string (string_of_alphabet a);;

(* print_alphabet rcs.sigma;; *)

let string_of_appterm term :string= 
  let rec aux term = 
     match term with
  	  Tm(f) -> f
	| Nt(nt) -> nt
	| Var(x) -> x
	| App(l,(App(_,_) as r)) -> (aux l)^" ("^(aux  r)^")"
	| App(l,r) -> (aux l)^" "^(aux  r)
  in aux term
;;

let print_appterm term = print_string (string_of_appterm term);;


let string_of_rule rcs ((nt,para,appterm):rule) = 
    nt^" "^(List.fold_left (function acc -> function p -> acc^p^" ") "" para)^"= "^(string_of_appterm appterm)^"\n"
;;

let print_rule rcs r = print_string (string_of_rule rcs r);;

let string_of_rcs rcs =
    "Terminals:\n"^(string_of_alphabet rcs.sigma)^
    "Non-terminals:\n"^(string_of_alphabet rcs.nonterminals)^
    "Rules:\n"^(List.fold_left (function acc -> function r -> acc^(string_of_rule rcs r)) "" rcs.rules)
;;

let print_rcs rcs = print_string (string_of_rcs rcs);;

(* print_rcs rcs;; *)


exception Type_check_error;;
exception Wrong_variable_name of ident;;
exception Wrong_terminal_name of ident;;
exception Wrong_nonterminal_name of ident;;

let terminal_type rcs f =
    try 
        List.assoc f rcs.sigma
    with Not_found -> raise (Wrong_terminal_name f)
;;

let nonterminal_type rcs nt =
  try 
    List.assoc nt rcs.nonterminals
  with Not_found -> raise (Wrong_nonterminal_name nt)
;;

(* Create an association list mapping parameters name in p to 
  the type of the correspding parameter in type t *)
let rec create_paramtyplist nt p t = match p,t with
  | [],Gr -> []
  | x::q, Ar(l,r) -> (x,l)::(create_paramtyplist nt q r)
  | _ -> failwith ("Type of non-terminal "^nt^" does not match with the number of specified parameters.")
;;



(* Check that rcs is a well-defined recursion scheme *)
let rcs_check rcs =
  let valid = ref true in
  
  (* - parameters' names must be disjoint from terminals' names
     - appterm must be well-typed and para must be a superset of fv(appterm)
     - appterm must be of ground type
  *)
  let check_eq ((nt,para,appterm) as eq) =
    let partypelst = create_paramtyplist nt para (nonterminal_type rcs nt) in
    let var_type x =  List.assoc x partypelst in
    let rec typecheck_term = function
          Tm(f) -> terminal_type rcs f
        | Nt(nt) -> nonterminal_type rcs nt
        | Var(x) -> if List.exists (function m -> m=x) para then
                      var_type x
                    else
                      raise (Wrong_variable_name x);
        | App(a,b) -> match (typecheck_term a), (typecheck_term b) with
                       Ar(tl,tr), tb when tl=tb ->  tr
                      |  _ -> raise Type_check_error;
    in
    (* ensures that the non-terminal name is defined *)
    let _ = nonterminal_type rcs nt in

    (* check that the parameters names do not clash with terminals names *)
    let _ = List.exists (function p-> 
    (List.exists (function (a,t)-> 
            if a=p then
            begin
              print_string ("Parameter name "^p^" conflicts with a terminal name in ");
              print_rule rcs eq; valid := false;
            end;
            a=p) rcs.sigma)) para in
    
    try if (typecheck_term appterm) <> Gr then
        begin
          print_string ("RHS is not of ground type in: ");
          print_rule rcs eq; valid := false;
        end
    with 
        Wrong_variable_name(x) ->
            print_string ("Undefined variable '"^x^"' in RHS of: ") ;
            print_rule rcs eq; valid := false;
        | Wrong_terminal_name(x) ->
            print_string ("Undefined terminal '"^x^"' in RHS of: ") ;
            print_rule rcs eq; valid := false;
        | Wrong_nonterminal_name(x) ->
            print_string ("Undefined non-terminal '"^x^"' in RHS of: ") ;
            print_rule rcs eq; valid := false;
        | Type_check_error ->
            print_string ("Type-checking error in RHS of: ") ;
            print_rule rcs eq; valid := false;
in
	(* Check all the rules *)
    List.iter check_eq rcs.rules;
    
	(* Check that the name (i.e. the non-terminal) of the first rule is of ground type *)
	if (List.length rcs.rules) > 0 then
	begin
		match (List.hd rcs.rules) with
			_,[],_ -> ()
		|	_ -> print_string ("The LHS of the first rule must be of ground type (i.e. no parameter)!");
				 print_newline(); valid := false;
	end;
				 
	!valid    
;;

rcs_check rcs;;

(* print_newline();;
print_rcs urz;;
rcs_check urz;;
*)

(** Tree structure used for performing reduction of the grammar rules 
type gramredtree = GrtNt of nonterminal | GrtTm of terminal | GrtApp of gramredtree * gramredtree;;
*)


(* Retrieve the operator and the operands from an applicative term.
   @return a pair (op,operands) where op is the operator (a terminal, variable or non-terminal) and
   operands is the list of operands terms. *)
let rec appterm_operator_operands t = match t with
 Tm(_) | Var(_) | Nt(_) -> t,[]
 | App(a,b) -> let op,oper = appterm_operator_operands a in op,oper@[b]
;;

(* Perform a reduction on the applicative term appterm (which must not contain any free variable) *)
let step_reduce rcs appterm = 
	let substitute nt operands = 
		let nttyp = nonterminal_type rcs nt in
		let _,parms,rhs = List.find (function rname,_,_ -> rname=nt) rcs.rules in
		(* Check that we have the right number of operands (we only reduce full applications to non-terminals) *)
		if List.length parms = List.length operands then	
			let substlst = List.combine parms operands in
			let rec subst term = match term with
			     Tm(_) | Nt(_) -> term
			   | App(l,r) -> App((subst l), (subst r))
			   | Var(x)  -> try List.assoc x substlst
					        with Not_found -> term
			in
			subst rhs
		else
			failwith ("Error: partial application to the terminal "^nt^".")
	in
	
	(* Look for the outermost reduction context in appterm, and if there is one perform the reduction.
		A reduction context is of the form C[_] where the hole contains a term of the form X t1 ... tn for some nonterminal X.
		
	 The function takes two parameters:
		-appterm: the term in which we look for a context. 
		 Let us assume that it is of the form T0 T1 T2 ... Tq where T0 is not an application.
		-operands: a list of operands terms that are applied to appterm (this parameter is used to 
		collect the list of parameters as we approach the operator node in the AST so that the parameters list
		is available when we reach the operator node and so we can perform the substitution)
		
	 The function returns a triple: (found,outer,term) where
        If found=true then a reduction context has been found in appterm
	  and 'term' contains the reducted term. 'outer' is set to true iff 
	  the outermost redex-context is exactly the outermost application
	  i.e. the context is C[_] = _ (and therefore C[appterm] = appterm)
	  and Op is a non-terminal.
	  
	    If found=false then 'term' contains appterm and 'outer' has no meaning.
	  *)
	let rec findredcontext_and_substitute appterm operands =
			match appterm with
			  Tm(_) -> false,false,appterm
			| Var(_) -> failwith "Trying to reduce an open term!";
			| Nt(nt) -> true,true,(substitute nt operands)
			| App(t0_tqminusone,tq) -> 			
				let f,o,t = findredcontext_and_substitute t0_tqminusone (tq::operands) in
					if f then (* a context was found (and reduced) *)
					  true,o,
						( (* the outermost redex is exaclty the outermost application appterm = T0 T1 ... Tq *)
                          if o then t 
						  (* the outermost context lies somewhere inside T0 or T1 or ... T_(q-1) *)
						  else App(t,tq)
						)
					else (* no context found: *)
					    (* then look for a context in the operand T_q *)
						let f,o,t = findredcontext_and_substitute tq [] in
							f,false,App(t0_tqminusone, t)
	in
	let f,_,red = findredcontext_and_substitute appterm [] in
	f,red
 ;;

(* Perform an OI-derivation of the recursion scheme rcs *)
let oi_derivation rcs =
	
	let t = ref (Nt("S")) in 
    print_newline(); print_appterm !t;  print_newline();
	while (input_line stdin) <> "q" do
	  let red,tred = step_reduce urz !t in
	  t := tred;	

	  print_appterm !t; print_newline(); print_newline();
	done;
;;

(* oi_derivation urz; *)


(*
		  let appterm_type rcs param = function
	  Tm(f) -> terminal_type f
	| Var(x) -> 
	let var_type x =   List.assoc f rcs.sigma
;;
	var_type f
	| App(a,b) ->
;;


let appterm_order rcs param t = typeorder (appterm_type rcs param t);;
*)


  
(** Grammars in long normal form (lnf) *)


(* We define the type of the right-hand side of a rule in lnf that we call RHS for short. It is given by:
- the top abstraction: a list of abstracted variables,
- the (leftmost) operator : either @, a variable or a nonterminal,
- the list of operands that are themselves of the type RHS. *)
type lnfrhs = LnfRhs of (lnfabstraction  * lnfoperator * lnfoperand list)
and lnfabstraction = ident list
and lnfoperator = LnfOpAt | LnfOpVar of ident | LnfOpNt of nonterminal 
and lnfoperand = lnfrhs ;;

(* a recursion scheme in lnf *)
type lnfrecscheme = { sigma : alphabet  ; rules: nonterminal * lnfrhs list };;

let freshvar = ref 0;;
(*
let eq_to_lnf (nt,param,appterm) = 
  let rec aux = function
     Tm(t) -> LnfOpAt
   | App(a,b)  ->
  in
  let _,op,operand = aux appterm in   (* the first element of the triple (list of abstracted variables) 
     must be empty since the rhs of the rule is of order 0 *)
  nt, LnfRhs(param, op, operand)
;;

let rs_to_lnf s = List.map eq_to_lnf s;;

*)




#light

open System
//open System.Collections.Generic
open System.ComponentModel
open System.Data
open System.Drawing
open System.Text
open System.Windows.Forms
open System.IO
open Printf
//open Comlab

let keywords = 
   [  "abstract";"and";"as";"assert"; "asr";
      "begin"; "class"; "constructor"; "default";
      "delegate"; "do";"done";
      "downcast";"downto";
      "elif";"else";"end";"exception";
      "false";"finally";"for";"fun";"function";
      "if";"in"; "inherit"; "inline";
      "interface"; "land"; "lazy"; "let";
      "lor"; "lsl";
      "lsr"; "lxor";
      "match"; "member";"method";"mod";"module";
      "mutable";"namespace"; "new";"null"; "of";"object";"open";
      "or"; "override";
      "property";"rec";"static";"struct"; "sig";
      "then";"to";"true";"try";
      "type";"upcast"; "val";"virtual";"when";
      "while";"with";"="; ":?"; "->"; "|"; "#light"  ]
let colorizeCode(rtb: # RichTextBox) = 
    let text = rtb.Text 
            
    rtb.SelectAll()
    rtb.SelectionColor <- rtb.ForeColor

    keywords |> List.iter (fun keyword -> 
        let mutable keywordPos = rtb.Find(keyword, Enum.combine[RichTextBoxFinds.MatchCase; RichTextBoxFinds.WholeWord])
        while (keywordPos <> -1) do 
            let underscorePos = text.IndexOf("_", keywordPos)
            let commentPos = text.LastIndexOf("//", keywordPos)
            let newLinePos = text.LastIndexOf('\n', keywordPos)
            let mutable quoteCount = 0
            let mutable quotePos = text.IndexOf("\"", newLinePos + 1, keywordPos - newLinePos)
            while (quotePos <> -1) do
                quoteCount <- quoteCount + 1
                quotePos <- text.IndexOf("\"", quotePos + 1, keywordPos - (quotePos + 1))
            
            if (newLinePos >= commentPos && 
                underscorePos <> keywordPos + rtb.SelectionLength  && 
                quoteCount % 2 = 0) 
             then
                rtb.SelectionColor <- Color.Blue;

            keywordPos <- rtb.Find(keyword, keywordPos + rtb.SelectionLength, Enum.combine[RichTextBoxFinds.MatchCase; RichTextBoxFinds.WholeWord])
    );
    rtb.Select(0, 0)

type MyForm = 
  class
    inherit Form as base

    val mutable components: System.ComponentModel.Container;
    override this.Dispose(disposing) =
        if (disposing && (match this.components with null -> false | _ -> true)) then
          this.components.Dispose();
        base.Dispose(disposing)

    member this.InitializeComponent() =
        this.components <- new System.ComponentModel.Container();
        let resources = new System.ComponentModel.ComponentResourceManager((type MyForm)) 
        this.outerSplitContainer <- new System.Windows.Forms.SplitContainer();
        this.samplesTreeView <- new System.Windows.Forms.TreeView();
        this.imageList <- new System.Windows.Forms.ImageList(this.components);
        this.samplesLabel <- new System.Windows.Forms.Label();
        this.rightContainer <- new System.Windows.Forms.SplitContainer();
        this.rightUpperSplitContainer <- new System.Windows.Forms.SplitContainer();
        this.descriptionTextBox <- new System.Windows.Forms.TextBox();
        this.descriptionLabel <- new System.Windows.Forms.Label();
        this.codeRichTextBox <- new System.Windows.Forms.RichTextBox();
        this.codeLabel <- new System.Windows.Forms.Label();
        this.runButton <- new System.Windows.Forms.Button();
        this.outputTextBox <- new System.Windows.Forms.RichTextBox();
        this.outputLabel <- new System.Windows.Forms.Label();
        this.outerSplitContainer.Panel1.SuspendLayout();
        this.outerSplitContainer.Panel2.SuspendLayout();
        this.outerSplitContainer.SuspendLayout();
        this.rightContainer.Panel1.SuspendLayout();
        this.rightContainer.Panel2.SuspendLayout();
        this.rightContainer.SuspendLayout();
        this.rightUpperSplitContainer.Panel1.SuspendLayout();
        this.rightUpperSplitContainer.Panel2.SuspendLayout();
        this.rightUpperSplitContainer.SuspendLayout();
        this.SuspendLayout();
        // 
        // outerSplitContainer
        // 
        this.outerSplitContainer.Dock <- System.Windows.Forms.DockStyle.Fill;
        this.outerSplitContainer.FixedPanel <- System.Windows.Forms.FixedPanel.Panel1;
        this.outerSplitContainer.Location <- new System.Drawing.Point(0, 0);
        this.outerSplitContainer.Name <- "outerSplitContainer";
        // 
        // outerSplitContainer.Panel1
        // 
        this.outerSplitContainer.Panel1.Controls.Add(this.samplesTreeView);
        this.outerSplitContainer.Panel1.Controls.Add(this.samplesLabel);
        // 
        // outerSplitContainer.Panel2
        // 
        this.outerSplitContainer.Panel2.Controls.Add(this.rightContainer);
        this.outerSplitContainer.Size <- new System.Drawing.Size(952, 682);
        this.outerSplitContainer.SplitterDistance <- 268;
        this.outerSplitContainer.TabIndex <- 0;
        // 
        // samplesTreeView
        // 
        this.samplesTreeView.Anchor <- 
          Enum.combine [ System.Windows.Forms.AnchorStyles.Top; 
                        System.Windows.Forms.AnchorStyles.Bottom;
                        System.Windows.Forms.AnchorStyles.Left;
                        System.Windows.Forms.AnchorStyles.Right];
        this.samplesTreeView.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.samplesTreeView.HideSelection <- false;
        this.samplesTreeView.ImageKey <- "Item";
        this.samplesTreeView.ImageList <- this.imageList;
        this.samplesTreeView.Location <- new System.Drawing.Point(0, 28);
        this.samplesTreeView.Name <- "samplesTreeView";
        this.samplesTreeView.SelectedImageKey <- "Item";
        this.samplesTreeView.ShowNodeToolTips <- true;
        this.samplesTreeView.ShowRootLines <- false;
        this.samplesTreeView.Size <- new System.Drawing.Size(266, 654);
        this.samplesTreeView.TabIndex <- 1;
        //this.samplesTreeView.add_AfterExpand(fun _ e -> 
        this.samplesTreeView.add_NodeMouseDoubleClick(fun  _ e -> 
              match e.Node.Level with 
              | 0  -> 
                e.Node.ImageKey <- "BookOpen";
                e.Node.SelectedImageKey <- "BookOpen";
              | _ when (e.Node.Tag<> null) -> let _,redterm = step_reduce urz (e.Node.Tag:?>appterm)
                                              let op,operands = appterm_operator_operands redterm
                                              (match op with
                                                Tm(f) -> e.Node.Text <- f;
                                                         e.Node.ImageKey <- "Item";
                                                         e.Node.SelectedImageKey <- "SelectedImageKey";
                                                         e.Node.Tag <- null;
                                                         List.iter (function operand -> let newNode = new TreeNode((string_of_appterm operand), ImageKey = "Help", SelectedImageKey = "Help")
                                                                                        newNode.Tag <- operand;
                                                                                        ignore(e.Node.Nodes.Add(newNode));) operands;
                                                         e.Node.Expand();
                                                    (* The leftmost operator is not a terminal: we just replace the Treeview node label
                                                       by the reduced term. *)
                                               | Nt(nt) -> e.Node.Text <- string_of_appterm redterm;
                                                           e.Node.Tag <- redterm;
                                               | _ -> failwith "bug in appterm_operator_operands!");
              | _ -> ();
              );
                        
          this.samplesTreeView.add_BeforeCollapse(fun _ e -> 
              match e.Node.Level with 
              | 0 -> 
                e.Cancel <- true;
              | _ -> ());
            
        this.samplesTreeView.add_AfterSelect(fun _ e -> 
            let currentNode = this.samplesTreeView.SelectedNode  
            match currentNode.Tag with 
            | null -> 
                this.runButton.Enabled <- false;
                this.descriptionTextBox.Text <- "Select a query from the tree to the left.";
                //this.codeRichTextBox.Clear();
                this.outputTextBox.Clear();
                if (e.Action <> TreeViewAction.Collapse && e.Action <> TreeViewAction.Unknown) then
                    e.Node.Expand();
            | _ -> ());
              
     (*   this.samplesTreeView.add_AfterCollapse(fun _ e -> 
          match e.Node.Level with 
          | 1 -> 
            e.Node.ImageKey <- "BookStack";
            e.Node.SelectedImageKey <- "BookStack";
          |  2 ->
            e.Node.ImageKey <- "BookClosed";
            e.Node.SelectedImageKey <- "BookClosed"
          | _ -> ());
*)
        // 
        // imageList
        // 
        
        this.imageList.ImageStream <- (resources.GetObject("imageList.ImageStream") :?> System.Windows.Forms.ImageListStreamer);
        this.imageList.Images.SetKeyName(0, "Help");
        this.imageList.Images.SetKeyName(1, "BookStack");
        this.imageList.Images.SetKeyName(2, "BookClosed");
        this.imageList.Images.SetKeyName(3, "BookOpen");
        this.imageList.Images.SetKeyName(4, "Item");
        this.imageList.Images.SetKeyName(5, "Run");
        
        // 
        // samplesLabel
        // 
        this.samplesLabel.AutoSize <- true;
        this.samplesLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.samplesLabel.Location <- new System.Drawing.Point(3, 9);
        this.samplesLabel.Name <- "samplesLabel";
        this.samplesLabel.Size <- new System.Drawing.Size(58, 16);
        this.samplesLabel.TabIndex <- 0;
        this.samplesLabel.Text <- "Samples:";
        // 
        // rightContainer
        // 
        this.rightContainer.Dock <- System.Windows.Forms.DockStyle.Fill;
        this.rightContainer.Location <- new System.Drawing.Point(0, 0);
        this.rightContainer.Name <- "rightContainer";
        this.rightContainer.Orientation <- System.Windows.Forms.Orientation.Horizontal;
        // 
        // rightContainer.Panel1
        // 
        this.rightContainer.Panel1.Controls.Add(this.rightUpperSplitContainer);
        // 
        // rightContainer.Panel2
        // 
        this.rightContainer.Panel2.Controls.Add(this.runButton);
        this.rightContainer.Panel2.Controls.Add(this.outputTextBox);
        this.rightContainer.Panel2.Controls.Add(this.outputLabel);
        this.rightContainer.Size <- new System.Drawing.Size(680, 682);
        this.rightContainer.SplitterDistance <- 357;
        this.rightContainer.TabIndex <- 0;
        // 
        // rightUpperSplitContainer
        // 
        this.rightUpperSplitContainer.Dock <- System.Windows.Forms.DockStyle.Fill;
        this.rightUpperSplitContainer.FixedPanel <- System.Windows.Forms.FixedPanel.Panel1;
        this.rightUpperSplitContainer.Location <- new System.Drawing.Point(0, 0);
        this.rightUpperSplitContainer.Name <- "rightUpperSplitContainer";
        this.rightUpperSplitContainer.Orientation <- System.Windows.Forms.Orientation.Horizontal;
        // 
        // rightUpperSplitContainer.Panel1
        // 
        this.rightUpperSplitContainer.Panel1.Controls.Add(this.descriptionTextBox);
        this.rightUpperSplitContainer.Panel1.Controls.Add(this.descriptionLabel);
        // 
        // rightUpperSplitContainer.Panel2
        // 
        this.rightUpperSplitContainer.Panel2.Controls.Add(this.codeRichTextBox);
        this.rightUpperSplitContainer.Panel2.Controls.Add(this.codeLabel);
        this.rightUpperSplitContainer.Size <- new System.Drawing.Size(680, 357);
        this.rightUpperSplitContainer.SplitterDistance <- 95;
        this.rightUpperSplitContainer.TabIndex <- 0;
        // 
        // descriptionTextBox
        // 
        this.descriptionTextBox.Anchor<- 
          Enum.combine [ System.Windows.Forms.AnchorStyles.Top;
                    System.Windows.Forms.AnchorStyles.Bottom;
                    System.Windows.Forms.AnchorStyles.Left;
                    System.Windows.Forms.AnchorStyles.Right ];
        this.descriptionTextBox.BackColor <- System.Drawing.SystemColors.ControlLight;
        this.descriptionTextBox.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.descriptionTextBox.Location <- new System.Drawing.Point(0, 28);
        this.descriptionTextBox.Multiline <- true;
        this.descriptionTextBox.Name <- "descriptionTextBox";
        this.descriptionTextBox.ReadOnly <- true;
        this.descriptionTextBox.ScrollBars <- System.Windows.Forms.ScrollBars.Vertical;
        this.descriptionTextBox.Size <- new System.Drawing.Size(680, 67);
        this.descriptionTextBox.TabIndex <- 1;
        // 
        // descriptionLabel
        // 
        this.descriptionLabel.AutoSize <- true;
        this.descriptionLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.descriptionLabel.Location <- new System.Drawing.Point(3, 9);
        this.descriptionLabel.Name <- "descriptionLabel";
        this.descriptionLabel.Size <- new System.Drawing.Size(72, 16);
        this.descriptionLabel.TabIndex <- 0;
        this.descriptionLabel.Text <- "Description:";
        // 
        // codeRichTextBox
        // 
        this.codeRichTextBox.Anchor <- 
          Enum.combine [ System.Windows.Forms.AnchorStyles.Top;
                    System.Windows.Forms.AnchorStyles.Bottom;
                    System.Windows.Forms.AnchorStyles.Left;
                    System.Windows.Forms.AnchorStyles.Right ];
        this.codeRichTextBox.BackColor <- System.Drawing.SystemColors.ControlLight;
        //this.codeRichTextBox.ForeColor <- System.Drawing.Color.Blue;
        this.codeRichTextBox.BorderStyle <- System.Windows.Forms.BorderStyle.FixedSingle;
        this.codeRichTextBox.Font <- new System.Drawing.Font("Lucida Console", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.codeRichTextBox.Location <- new System.Drawing.Point(0, 18);
        this.codeRichTextBox.Name <- "codeRichTextBox";
        this.codeRichTextBox.ReadOnly <- true;
        this.codeRichTextBox.Size <- new System.Drawing.Size(680, 240);
        this.codeRichTextBox.TabIndex <- 1;
        this.codeRichTextBox.Text <- (string_of_rcs urz) ;
        //this.codeRichTextBox.Dock <- DockStyle.Fill;
        this.codeRichTextBox.WordWrap <- false;
        // 
        // codeLabel
        // 
        this.codeLabel.AutoSize <- true;
        this.codeLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.codeLabel.Location <- new System.Drawing.Point(3, -1);
        this.codeLabel.Name <- "codeLabel";
        this.codeLabel.Size <- new System.Drawing.Size(38, 16);
        this.codeLabel.TabIndex <- 0;
        this.codeLabel.Text <- "Code:";
        // 
        // runButton
        // 
        this.runButton.Enabled <- false;
        this.runButton.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.runButton.ImageAlign <- System.Drawing.ContentAlignment.MiddleRight;
        this.runButton.ImageKey <- "Run";
        this.runButton.ImageList <- this.imageList;
        this.runButton.Location <- new System.Drawing.Point(0, -1);
        this.runButton.Name <- "runButton";
        this.runButton.Size <- new System.Drawing.Size(119, 27);
        this.runButton.TabIndex <- 0;
        this.runButton.Text <- " Run Sample!";
        this.runButton.TextImageRelation <- System.Windows.Forms.TextImageRelation.ImageBeforeText;
        this.runButton.Click.Add(fun e -> 
         
              this.UseWaitCursor <- true;
              try 
                this.outputTextBox.Text <- "";
                let stream = new MemoryStream()  
                let writer = new StreamWriter(stream)  
                stream.SetLength(0L);
                writer.Flush();
                this.outputTextBox.Text <- this.outputTextBox.Text + writer.Encoding.GetString(stream.ToArray());
              finally
                this.UseWaitCursor <- false);
            // 
        // outputTextBox
        // 
        this.outputTextBox.Anchor <- 
          Enum.combine [ System.Windows.Forms.AnchorStyles.Top;
                    System.Windows.Forms.AnchorStyles.Bottom;
                    System.Windows.Forms.AnchorStyles.Left;
                    System.Windows.Forms.AnchorStyles.Right ];
        this.outputTextBox.BackColor <- System.Drawing.SystemColors.ControlLight;
        this.outputTextBox.Font <- new System.Drawing.Font("Lucida Console", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.outputTextBox.Location <- new System.Drawing.Point(0, 48);
        this.outputTextBox.Multiline <- true;
        this.outputTextBox.Name <- "outputTextBox";
        this.outputTextBox.ReadOnly <- true;
        this.outputTextBox.ScrollBars <- System.Windows.Forms.RichTextBoxScrollBars.Both;
        this.outputTextBox.Size <- new System.Drawing.Size(680, 273);
        this.outputTextBox.TabIndex <- 2;
        this.outputTextBox.WordWrap <- false;
        // 
        // outputLabel
        // 
        this.outputLabel.AutoSize <- true;
        this.outputLabel.Font <- new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0uy);
        this.outputLabel.Location <- new System.Drawing.Point(3, 29);
        this.outputLabel.Name <- "outputLabel";
        this.outputLabel.Size <- new System.Drawing.Size(47, 16);
        this.outputLabel.TabIndex <- 1;
        this.outputLabel.Text <- "Output:";
        // 
        // DisplayForm
        // 
        this.AcceptButton <- this.runButton;
        this.AutoScaleDimensions <- new System.Drawing.SizeF(6.0F, 13.0F);
        this.AutoScaleMode <- System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize <- new System.Drawing.Size(952, 682);
        this.Controls.Add(this.outerSplitContainer);
        this.Font <- new System.Drawing.Font("Tahoma", 8.25F);
        this.Icon <- (resources.GetObject("$this.Icon") :?> System.Drawing.Icon);
        this.Name <- "DisplayForm";
        this.Text <- "Samples";
        this.outerSplitContainer.Panel1.ResumeLayout(false);
        this.outerSplitContainer.Panel1.PerformLayout();
        this.outerSplitContainer.Panel2.ResumeLayout(false);
        this.outerSplitContainer.ResumeLayout(false);
        this.rightContainer.Panel1.ResumeLayout(false);
        this.rightContainer.Panel2.ResumeLayout(false);
        this.rightContainer.Panel2.PerformLayout();
        this.rightContainer.ResumeLayout(false);
        this.rightUpperSplitContainer.Panel1.ResumeLayout(false);
        this.rightUpperSplitContainer.Panel1.PerformLayout();
        this.rightUpperSplitContainer.Panel2.ResumeLayout(false);
        this.rightUpperSplitContainer.Panel2.PerformLayout();
        this.rightUpperSplitContainer.ResumeLayout(false);
        this.ResumeLayout(false)


    val mutable outerSplitContainer : System.Windows.Forms.SplitContainer;
    val mutable samplesLabel : System.Windows.Forms.Label;
    val mutable rightContainer : System.Windows.Forms.SplitContainer;
    val mutable outputTextBox : System.Windows.Forms.RichTextBox;
    val mutable outputLabel : System.Windows.Forms.Label;
    val mutable runButton : System.Windows.Forms.Button;
    val mutable rightUpperSplitContainer : System.Windows.Forms.SplitContainer;
    val mutable descriptionTextBox : System.Windows.Forms.TextBox;
    val mutable descriptionLabel : System.Windows.Forms.Label;
    val mutable codeLabel : System.Windows.Forms.Label;
    val mutable samplesTreeView : System.Windows.Forms.TreeView;
    val mutable imageList : System.Windows.Forms.ImageList;
    val mutable codeRichTextBox : System.Windows.Forms.RichTextBox

    new (title) as this =
       { outerSplitContainer = null;
         samplesLabel = null;
         rightContainer = null;
         outputTextBox = null;
         outputLabel =null;
         runButton =null;
         rightUpperSplitContainer =null;
         descriptionTextBox =null;
         descriptionLabel = null;
         codeLabel = null;
         samplesTreeView = null;
         imageList = null;
         codeRichTextBox = null;
         components = null }
       
       then 
        this.InitializeComponent();

        this.Text <- title;

        let rootNode = new TreeNode(title, Tag = (null : obj), ImageKey = "Help", SelectedImageKey = "Help")
        ignore(this.samplesTreeView.Nodes.Add(rootNode));
        rootNode.Expand();
      

        let harnessNode = new TreeNode("S")  
        harnessNode.Tag <- (null : obj);
        harnessNode.ImageKey <- "BookStack";
        harnessNode.SelectedImageKey <- "BookStack";
        harnessNode.Tag <- Nt("S");
        ignore(rootNode.Nodes.Add(harnessNode));
        (*
        let category = ref ""  
        let categoryNode = ref (null: TreeNode)          
            let n = new TreeNode("Truc")
            n.Tag <- (null : obj);
            n.ImageKey <- "BookClosed";
            n.SelectedImageKey <- "BookClosed";
            ignore(harnessNode.Nodes.Add(n));
            category := "chose";
            categoryNode := n;          
           
            let node = new TreeNode("Machin")  
            node.Tag <- sample;
            node.ImageKey <- "Item";
            node.SelectedImageKey <- "Item";
            ignore((!categoryNode).Nodes.Add(node))); *)
  end
  
  



/// <summary>
/// The main entry point for the application.
/// </summary>
[<STAThread>]
let main() = 
    Application.EnableVisualStyles();
    let form = new MyForm("HOG value tree" ) in
    ignore(form.ShowDialog());;
//(urz:Hogrammar.recscheme)
main();;
