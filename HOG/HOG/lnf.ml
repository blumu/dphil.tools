(** $Id$
	Description: Eta-long normal form
	Author:		William Blum
**)

open Common
open Type


type ident = string;;
type terminal = ident;;
type nonterminal = ident;;

(** Structures for storing terms in eta-normal form **)

(* A LNF expression is an alternation of abstraction and applicative term of ground type *)
type lnf = lnfabstraction
(* an abstraction lnf is a list of identifiers (the abstracted variables) and 
 an applicative term in lnf *)
and lnfabstraction = ident list  * lnfapplicativepart
(* an applicative expression of ground type is given by
 (leftmost) operator (either @, a variable or a nonterminal)
  and the list of operands that are abstraction lnf. *)
and lnfapplicativepart = 
    LnfAppVar of ident * lnfabstraction list
  | LnfAppTm of terminal * lnfabstraction list
  | LnfAppNt of nonterminal * lnfabstraction list
  | LnfAppAbs of lnfabstraction * lnfabstraction list
                 (* this constructor is only use for lnf of simply-typed terms-in-context,
                    not for LNF of HORS *)
;;

(* A rule in LNF is given by its name (the nonterminal) and the right-hand side expression in eta-long nf *)
type lnfrule = nonterminal * lnf;;



let rec lnf_to_string (abs_part,app_part) =
    let bracketize_lnf t = "("^(lnf_to_string t)^")" in
    LAMBDA_SYMBOL^(String.concat " " abs_part)^"."
        ^(match app_part with
             LnfAppTm(x,[])
            |LnfAppVar(x,[])
            |LnfAppNt(x,[]) -> x
            |LnfAppTm(x,operands)
            |LnfAppVar(x,operands)
            |LnfAppNt(x,operands) ->
                    x^" "^(String.concat " " (List.map bracketize_lnf operands))
            |LnfAppAbs(_,[]) -> failwith "Ill-formed eta-long nf!"
            |LnfAppAbs(abs,operands) ->
                    (bracketize_lnf abs)^" "^(String.concat " " (List.map bracketize_lnf operands))
        )
;;

let lnfrule_to_string ((nt,lnfrhs):lnfrule) =
    nt^" = "^(lnf_to_string lnfrhs)
;;



