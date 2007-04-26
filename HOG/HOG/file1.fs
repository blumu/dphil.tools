
type typ = Gr | Ar of typ * typ ;;

let rec typeorder = function  Gr ->  0 | Ar(a,b) -> max (1+ typeorder a) (typeorder b) ;;

type ident = string;;
type alphabet = (ident * typ) list;;
type terminal = ident;;
type nonterminal = ident;;

(* applicative term *)
type appterm = Nt of nonterminal | Tm of terminal | Var of ident | App of appterm * appterm;;

type equation = nonterminal * ident list * appterm;;
type recscheme = { nonterminals : alphabet;  sigma : alphabet;
				   eqs : equation list } ;;

let rcs = { nonterminals = [ "S", Gr;
                             "F", Ar(Gr,Ar(Gr,Gr)) ;
                             "G", Ar(Ar(Gr,Gr),Ar(Gr,Gr)) ];
  sigma = [ "f", Ar(Gr,Ar(Gr,Gr));
            "g", Ar(Gr,Gr);
            "t", Ar(Gr,Ar(Gr,Gr)) ;
            "e", Gr ];
  eqs = [ "S",[],App(App(Tm("f"), Tm("e")),Tm("e"));
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
  eqs = [ "S",[],App(Tm("["),
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

let rec print_appterm = function
  Tm(f) -> print_string f
 | Nt(nt) -> print_string nt
 | Var(x) -> print_string x
 | App(a,b) -> print_string "("; print_appterm a; print_string " ";
	print_appterm b; print_string ")"; 
;;

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
    print_string "Equations:"; print_newline();
    List.iter (print_eq rcs) rcs.eqs;
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
              print_eq rcs eq;
            end;
            a=p) rcs.sigma)) para in
    
    try if (typecheck_term appterm) != Gr then
        begin
          print_string ("RHS is not of ground type in: ");
          print_eq rcs eq;
        end
    with 
        Wrong_variable_name(x) ->
            print_string ("Undefined variable '"^x^"' in RHS of: ") ;
            print_eq rcs eq;
        | Wrong_terminal_name(x) ->
            print_string ("Undefined terminal '"^x^"' in RHS of: ") ;
            print_eq rcs eq;
        | Wrong_nonterminal_name(x) ->
            print_string ("Undefined non-terminal '"^x^"' in RHS of: ") ;
            print_eq rcs eq;
        | Type_check_error ->
            print_string ("Type-checking error in RHS of: ") ;
            print_eq rcs eq;
in
    List.iter check_eq rcs.eqs
;;

rcs_check rcs;;

print_newline();;
print_rcs urz;;
rcs_check urz;;

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


(* We define the type of the right-hand side of an equation in lnf that we call RHS for short. It is given by:
- the top abstraction: a list of abstracted variables,
- the (leftmost) operator : either @, a variable or a nonterminal,
- the list of operands that are themselves of the type RHS. *)
type lnfrhs = LnfRhs of (lnfabstraction  * lnfoperator * lnfoperand list)
and lnfabstraction = ident list
and lnfoperator = LnfOpAt | LnfOpVar of ident | LnfOpNt of nonterminal 
and lnfoperand = lnfrhs ;;

(* a recursion scheme in lnf *)
type lnfrecscheme = { sigma : alphabet  ; eqs: nonterminal * lnfrhs list };;

let freshvar = ref 0;;
(*
let eq_to_lnf (nt,param,appterm) = 
  let rec aux = function
     Tm(t) -> LnfOpAt
   | App(a,b)  ->
  in
  let _,op,operand = aux appterm in   (* the first element of the triple (list of abstracted variables) 
     must be empty since the rhs of the equation is of order 0 *)
  nt, LnfRhs(param, op, operand)
;;

let rs_to_lnf s = List.map eq_to_lnf s;;

*)


class widget =
  object (self)
    val mutable state = 0 ;
    method poke n = state <- state + n
    method peek = state 
    method hasBeenPoked = (state <> 0)
end;;
