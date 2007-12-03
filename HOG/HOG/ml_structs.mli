(** $Id$
	Description: Structures for encoding Core Ml terms
	Author:		William Blum
**)

open Type;;

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


val string_of_mltermincontext : ml_termincontext -> string