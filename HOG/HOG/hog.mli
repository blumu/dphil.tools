(* Interface File *)
(* namespace Comlab *)

type typ = Gr | Ar of typ * typ ;;

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

val string_of_appterm : appterm -> string
val string_of_rs : recscheme -> string
val appterm_operator_operands  : appterm -> appterm * appterm list
val step_reduce : recscheme -> appterm -> bool * appterm

val rs_check : recscheme -> bool


type lnfrhs = lnfabstractpart * lnfapplicativepart
and lnfabstractpart = ident list
and lnfapplicativepart = 
      LnfAppVar of ident * lnfrhs list
    | LnfAppNt of nonterminal * lnfrhs list
    | LnfAppTm of terminal * lnfrhs list
;;
type lnfrule = nonterminal * lnfrhs ;;

val lnf_to_string : recscheme -> lnfrule -> string
val rule_to_lnf : recscheme -> rule -> lnfrule
val rs_to_lnf : recscheme -> lnfrule list
