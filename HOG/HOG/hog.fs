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
 };;

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


let print_alphabet a =
	let print_letter (l,t) =
	  print_string (l^":"^(string_of_type t)) ;
	  print_newline();
	in
	List.iter print_letter a
;;

(* 
print_alphabet rcs.sigma;; *)

let rec print_appterm_old = function
  Tm(f) -> print_string f
 | Nt(nt) -> print_string nt
 | Var(x) -> print_string x
 | App(a,b) -> print_string "("; print_appterm_old a; print_string " ";
	print_appterm_old b; print_string ")"; 
;;


let string_of_appterm term = 
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


let print_eq rcs (nt,para,appterm) = 
    print_string nt;
    print_string " ";
    List.iter (function s -> print_string (s^" ")) para;
    print_string "= ";
    print_appterm appterm;
    print_newline()
;;

let print_rcs rcs =
    print_string "Terminals:"; print_newline();
    print_alphabet rcs.sigma;
    print_string "Non-terminals:"; print_newline();
    print_alphabet rcs.nonterminals;
    print_string "Rules:"; print_newline();
    List.iter (print_eq rcs) rcs.rules;
;;

print_rcs rcs;;


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
              print_eq rcs eq; valid := false;
            end;
            a=p) rcs.sigma)) para in
    
    try if (typecheck_term appterm) <> Gr then
        begin
          print_string ("RHS is not of ground type in: ");
          print_eq rcs eq; valid := false;
        end
    with 
        Wrong_variable_name(x) ->
            print_string ("Undefined variable '"^x^"' in RHS of: ") ;
            print_eq rcs eq; valid := false;
        | Wrong_terminal_name(x) ->
            print_string ("Undefined terminal '"^x^"' in RHS of: ") ;
            print_eq rcs eq; valid := false;
        | Wrong_nonterminal_name(x) ->
            print_string ("Undefined non-terminal '"^x^"' in RHS of: ") ;
            print_eq rcs eq; valid := false;
        | Type_check_error ->
            print_string ("Type-checking error in RHS of: ") ;
            print_eq rcs eq; valid := false;
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

print_newline();;
print_rcs urz;;
rcs_check urz;;

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
				let f,o,t = findredcontext_and_substitute t0_tqminusone (operands@[tq]) in
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


// #light

open System;;
open System.Collections.Generic;;
open System.Windows.Forms;;
open System.IO;;

/// <summary>
/// The main entry point for the application.
/// </summary>
[<STAThread>]
let main() = 
    Application.EnableVisualStyles();
    let form = new Display.SampleForm("HOG derviation tree" ) in
    ignore(form.ShowDialog());;
//(urz:Hogrammar.recscheme)
main();;


// Create a form and set some properties
(*
let form = new Form() in

form.Text <- "My First F# Form";
form.Visible <- true;

let menu = form.Menu <- new MainMenu()
let mnuFile = form.Menu.MenuItems.Add("&File")

let mnuiOpen = 
  new MenuItem("&Open...", 
               new EventHandler(fun _ _ -> 
                   let d = new OpenFileDialog() in 
                   d.InitialDirectory <- "c:\\";
                   d.Filter <- "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                   d.FilterIndex <- 2;
                   d.RestoreDirectory <- true;
                   if d.ShowDialog() = DialogResult.OK then 
                       match d.OpenFile() with 
                       | null -> printf "Ooops... Could not read the file...\n"
                       | s -> 
                           let r = new StreamReader(s) in 
                           printf "The first line of the file is: %s!\n" (r.ReadLine());
                           s.Close();
               ), 
               Shortcut.CtrlO)

mnuFile.MenuItems.Add(mnuiOpen)

#if COMPILED
// Run the main code. The attribute marks the startup application thread as "Single 
// Thread Apartment" mode, which is necessary for GUI applications. 
[<STAThread>]    
do Application.Run(form)

#endif
*)


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

(*
class widget =
  object (self)
    val mutable state = 0 ;
    method poke n = state <- state + n
    method peek = state 
    method hasBeenPoked = (state <> 0)
end;;
*)