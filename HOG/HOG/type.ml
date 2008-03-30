(** $Id$
	Description: Structures for encoding simple-types and polymorphic types
	Author:		 William Blum
**)

open Common

(*** ------------------------
     Simple types         ***)

type typ = Gr | Ar of typ * typ ;;

(* order and arity of a type *)
let rec typeorder = function  Gr ->  0 | Ar(a,b) -> max (1+ typeorder a) (typeorder b) ;;
let rec typearity = function  Gr ->  0 | Ar(_,b) -> 1+ (typearity b) ;;


(* Create an association list mapping parameter names from a list [p] to 
  the type of the parameters in the type [t].
  e.g.  create_param_typ_list ["x","y","z"] Ar(Gr,Ar(Gr,Gr),Ar(Gr,Gr),Ar(Gr,Ar(Gr,Gr)))
       = [("x",Gr); ("y",Ar(Gr,Gr)); ("z",Ar(Gr,Gr))]
   *)
let rec create_param_typ_list p t = match p,t with
    [],Gr -> []
  | x::q, Ar(l,r) -> (x,l)::(create_param_typ_list q r)
  | _ -> failwith ("create_param_typ_list type does not match with the number of specified parameters.")
;;



let rec string_of_type = function
    Gr -> "o"
  | Ar(Ar(_) as a, b) -> "("^(string_of_type a)^") -> "^(string_of_type b)
  | Ar(a,b) -> (string_of_type a)^" -> "^(string_of_type b)
;;

let string_of_alphabet_aux sep a =
	let string_of_letter (l,t) =   
	  l^":"^(string_of_type t) ;
	in
	(* List.fold_left (function acc -> function l -> acc^(string_of_letter l)) "" a *)
	String.concat sep (List.map string_of_letter a) 
;;

let string_of_alphabet a = (string_of_alphabet_aux eol a)^eol;;

let print_alphabet a = print_string (string_of_alphabet a);;



(*** ------------------------
     Polymorphic types    ***)


(** Structure used to encode polymorphic types **)
type typename = string
and poltyp =   PTypGr
             | PTypVar of typename
             | PTypAr of poltyp * poltyp;;

(** order and arity of a polymorphic types **)
let rec polytypeorder = function  PTypGr|PTypVar(_) ->  0 | PTypAr(a,b) -> max (1+ polytypeorder a) (polytypeorder b) ;;
let rec polytypearity = function  PTypGr|PTypVar(_) ->  0 | PTypAr(_,b) -> 1+ (polytypearity b) ;;


(** [type_substitute s t] performs the type substitution [s] on the type [t].
    The substitution is given by a a list of pair of the form (typevar,typ) **)
let rec type_substitute s = function
    PTypGr -> PTypGr
  | PTypVar(x) -> (try List.assoc x s
                  with Not_found -> PTypVar(x))
  | PTypAr(a, b) -> PTypAr((type_substitute s a), (type_substitute s b))
;;


exception NotUnifiable;;

(* Perfom the unification of two polymorphic types and return a tuple (sl,sr,sigma)
 where sigma is the unified type and s1 and s2 are type-variable substitutions (a list of pair of the form (typevar,type))
 such that t1[s1] = t2[s2] = sigma.
 *)
let rec unify_polytype t1 t2 = match(t1,t2) with
    PTypGr,PTypGr -> [],[],PTypGr
   |PTypVar(x),PTypVar(y) when x<y->  [],[y,PTypVar(x)],  PTypVar(x)
   |PTypVar(x),PTypVar(y) when y<x -> [x,PTypVar(y)],[],  PTypVar(y)
   |PTypVar(x),PTypVar(y) -> [],[],PTypVar(x)
   |PTypVar(x),_ -> [x,t2], [], t2
   |_,PTypVar(y) -> [],[y,t1],  t1
   |PTypAr(l1,r1), PTypAr(l2,r2) -> let sl1,sl2,tau1 = unify_polytype l1 l2 in 
                                    let sr1,sr2,tau2 = unify_polytype (type_substitute sl1 r1) (type_substitute sl2 r2)
                                    in sl1@sr1, sl2@sr2, PTypAr(tau1,tau2)
   | _ -> raise NotUnifiable;;
   
exception TypecheckingError;;



let rec string_of_polytype = function
    PTypGr -> "o"
  | PTypVar(n) -> n
  | PTypAr(PTypAr(_) as a, b) -> "("^(string_of_polytype a)^") -> "^(string_of_polytype b)
  | PTypAr(a,b) -> (string_of_polytype a)^" -> "^(string_of_polytype b)
;;


(** Flatten a polymorphic type into a simple type by replacing occurrences
    of type-variable by the ground type. **)
let rec flatten_polytype = function 
      PTypGr -> Gr
    | PTypVar(_) -> Gr
    | PTypAr(l,r) -> Ar((flatten_polytype l), (flatten_polytype r))   
;;

(** Injection mapping simple types to their polymorphic equivalent **)
let rec simple_to_polymorph = function
      Gr -> PTypGr
    | Ar(l,r) -> PTypAr((simple_to_polymorph l), (simple_to_polymorph r))
;;


(* Same as [create_param_typ_list] but for polymorphic types
   *)
let rec create_param_polytyp_list p t = match p,t with
    [],PTypGr
  | [], PTypVar(_) -> []
  | x::q, PTypAr(l,r) -> (x,l)::(create_param_polytyp_list q r)
  | _ -> failwith ("create_paramtyplist: type does not match with the number of specified parameters.")
;;

