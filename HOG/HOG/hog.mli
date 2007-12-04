(* Interface File *)
(* namespace Comlab *)
open Type
open Lnf

type alphabet = (ident * typ) list;;


(* applicative term *)
type appterm = Nt of nonterminal | Tm of terminal | Var of ident | App of appterm * appterm;;

type rule = nonterminal * ident list * appterm;;
type recscheme = { nonterminals : alphabet;
				   sigma : alphabet;
				   rules : rule list;
				   rs_path_validator : terminal list -> bool * string } ;;


val terminal_type : recscheme -> terminal -> typ
val get_parameter_type : recscheme -> ident -> typ

val string_of_appterm : appterm -> string
val string_of_rs : recscheme -> string
val applicative_decomposition  : appterm -> appterm * appterm list
val step_reduce : recscheme -> appterm -> bool * appterm

val rs_check : recscheme -> string list
val rule_to_lnf : recscheme -> rule -> lnfrule * (ident*typ) list
val rs_to_lnf : recscheme -> (lnfrule list) * (ident*typ) list


(***** Validators *****)
val default_validator : terminal list -> bool * string
val demiranda_validator :  terminal list -> bool * string
val reverse_demiranda_validator :  terminal list -> bool * string

