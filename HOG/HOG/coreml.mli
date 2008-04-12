(** $Id$
	Description: Structures for encoding Core Ml terms
	Author:		William Blum
**)

open Type;;
open Lnf;;

type ident = string;;
 
type ml_expr =
    MlVar of ident
  | Fun of ident * ml_expr
  | MlAppl of ml_expr * ml_expr
  | Let of (ident * (ident list) * ml_expr) list  * ml_expr
  | Letrec of (ident * (ident list) * ml_expr) list  * ml_expr
  | If of ml_expr * ml_expr * ml_expr
  | MlInt of int 
  | MlBool of bool
  | EqTest of ml_expr * ml_expr
  | Pred
  | Succ

type ml_context = (ident * poltyp) list

type ml_termincontext = ml_context*ml_expr

type ml_annotated_expr = poltyp * ml_annotated_subexpr
and ml_annotated_ident = poltyp * ident
and ml_annotated_subexpr =
    AnMlVar of ident
  | AnFun of ml_annotated_ident * ml_annotated_expr
  | AnMlAppl of ml_annotated_expr * ml_annotated_expr
  | AnLet of (ml_annotated_ident * (ml_annotated_ident list) * ml_annotated_expr) list * ml_annotated_expr
  | AnLetrec of (ml_annotated_ident * (ml_annotated_ident list) * ml_annotated_expr) list * ml_annotated_expr
  | AnIf of ml_annotated_expr * ml_annotated_expr * ml_annotated_expr
  | AnMlInt of int 
  | AnMlBool of bool
  | AnEqTest of ml_annotated_expr * ml_annotated_expr
  | AnPred
  | AnSucc

type ml_annotated_termincontext = ml_context*ml_annotated_expr


val string_of_mlterm : ml_expr -> string
val string_of_mltermincontext : ml_termincontext -> string


val annotate_termincontext : ml_termincontext -> ml_annotated_termincontext
val annotatedterm_to_lnf : ml_annotated_termincontext -> lnf
val annotatedterm_to_lnfrule : ml_annotated_termincontext -> lnfrule

