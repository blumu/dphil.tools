(** $Id$
	Description: Structures for encoding Core Ml terms
	Author:		William Blum
**)
open Type

type ident = string;;

type ml_expr =
    MlVar of ident
  | Fun of ident * ml_expr
  | MlAppl of ml_expr * ml_expr
  | Let of (ident * (ident list) * ml_expr) list  * ml_expr
  | Letrec of (ident * (ident list) * ml_expr) list  * ml_expr
  | If of ml_expr * ml_expr * ml_expr
  | MlInt of int 
  | AnyInt
  | MlBool of bool
  | EqTest of ml_expr * ml_expr
  | Pred
  | Succ


type ml_context = (ident * typ) list
  
type ml_termincontext = ml_context*ml_expr


(* ************************************************************** *)
(* Pretty-printing functions                                      *)

(** Print a lambda expression *)
let rec string_of_mlterm = function
    MlVar(x) -> x;
  |MlAppl(MlAppl(e1,e2),e3) -> (string_of_mlterm (MlAppl(e1,e2)))^" "^(string_in_bracket e3);
  |MlAppl(e1,e2) -> (string_in_bracket e1)^" "^(string_in_bracket e2);
  |Fun(x,e) -> "fun "^x^" -> "^(string_of_mlterm e)
  | _ -> failwith "unsupported Ml constructs!"
      
and string_in_bracket = function
    MlVar(x) -> x;
  | e -> "("^(string_of_mlterm e)^")";
;;

let string_of_mltermincontext (c,t) =
     (string_of_alphabet_aux " " c)^"|-"^(string_of_mlterm t);;
