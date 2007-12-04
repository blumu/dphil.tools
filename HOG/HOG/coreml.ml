(** $Id$
	Description: Structures for encoding Core Ml terms
	Author:		William Blum
**)
open Common
open Type
open Lnf

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

type ml_context = (ident * typ) list
  
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


(* Infer a polymorphic type for a term-in-context *)
let infer_polytype (context,term) =  
    let freshvar = ref (-1) in
    let new_freshvar() = incr(freshvar); "'"^(string_of_char (char_of_int ((int_of_char 'a')+ !freshvar))) in
    let rec infer context (term:ml_expr) =
        match term with
          MlVar(x) -> List.assoc x context
        | MlAppl(f,e) -> let tauf, taue = (infer context f),(infer context e) in
                         (match unify_polytype tauf (PTypAr(taue,PTypVar(new_freshvar()))) with
                                | PTypAr(_,sigma) -> sigma
                                | _ -> raise TypecheckingError )
        | Fun(x,e) -> let tau = PTypVar(new_freshvar()) in
                      PTypAr(tau, (infer ((x,tau)::context) e))
        | _ -> failwith "unsupported Ml constructs!"
    in infer context term
;;

(*
let pt = infer_polytype ([],(Fun("x",MlVar("x"))))
let pt = infer_polytype (["y",PTypGr],(Fun("x",MlAppl(MlVar("x"),MlVar("y")))))
string_of_polytype pt;
flatten_polytype pt;
*)


(** Return an type-annotated term corresponding to a given term-in-context. **)
let annotate_term ((context,term):ml_termincontext) :ml_annotated_expr =  
    let freshvar = ref (-1) in
    let new_freshvar() = incr(freshvar); "'"^(string_of_char (char_of_int ((int_of_char 'a')+ !freshvar))) in
    let rec annotate context (term:ml_expr) =
        match term with
          MlVar(x) -> (List.assoc x context),(AnMlVar(x))
        | MlAppl(f,e) -> let (tauf,_) as f_annotated = annotate context f
                         and (taue,_) as e_annotated = annotate context e in
                            (match unify_polytype tauf (PTypAr(taue,PTypVar(new_freshvar()))) with
                                | PTypAr(_,sigma) -> sigma
                                | _ -> raise TypecheckingError),
                            (AnMlAppl(f_annotated,e_annotated))
                            
        | Fun(x,e) -> let tau = PTypVar(new_freshvar()) in
                      let e_type,e_subanotated = annotate ((x,tau)::context) e in
                      (PTypAr(tau, e_type)), (AnFun((tau,x),(e_type,e_subanotated)))
        | _ -> failwith "unsupported Ml constructs!"
    in annotate (List.map (function a,b -> a, (simple_to_polymorph b)) context) term
;;


(** Long normal form (lnf) **)


(*
    let add_subst substlst (x,t) =
        (x,t)::(List.remove_assoc x substlst)
    in

*)

(** Decompose an term 'op e1 .... ek' into its constituents parts. 
   @return a pair (op,operands) where op is the operator (a terminal, variable or non-terminal) and
    operands is the list of operands terms. **)
let rec annotml_applicative_decomposition t = match snd t with
    AnMlVar(_)
  | AnFun(_)
  | AnLet(_)
  | AnLetrec(_)
  | AnIf(_)
  | AnMlInt(_)
  | AnMlBool(_)
  | AnEqTest(_)
  | AnPred
  | AnSucc -> t,[]
  | AnMlAppl(a,b) -> let op,oper = annotml_applicative_decomposition a in op,oper@[b]
;;

(** [lmdterm_to_lnf termincontext tmincontext] converts a term-in-context to eta-long normal form.
   @param tmincontext the input term-in-context
   @return the LNF of [tmincontext].
**)
let lmdterm_to_lnf ((context,term):ml_termincontext) :lnfrule = 
    (* For the creation of fresh variables *)
    let freshvar = ref 0 in
    let new_freshvar() = incr(freshvar); "#"^(string_of_int !freshvar) in
 
    let rec lnf_of ((polytyp,term):ml_annotated_expr as annot_term) :lnf = 
        match term with
            (* In the abstraction case, we just accumulate the abstracted variable to the abstraction 
                part of the etanf of the subterm e. *)
            | AnFun(x,e) -> let abse,appe = lnf_of e in
                            snd x::abse, appe
            (* Otherwise *)
            | _ ->
                (* decompose the term in the form e_0 e_1 ... e_k *)
                let op,operands = annotml_applicative_decomposition annot_term in   
                (* Create a list of fresh variables Phi_1 ... Phi_n where n is the arity of appterm *)
                let absvars = Array.to_list (Array.init (polytypearity polytyp) (fun i -> new_freshvar()) ) in
                (* create an association list mapping Phi_1 ... Phi_n to their respective types *)
                let absvars_types = create_param_polytyp_list absvars polytyp
                (* compute the lnfs of the operands *)
                and lnfoperands = List.map lnf_of operands in
                (* add 'lnf(Phi_1) ... lnf(Phi_n)'  to the list of operands *)    
                let lnfoperands_ext = lnfoperands@(List.map (function (v,t) -> lnf_of (t,(AnMlVar(v)))) absvars_types) in
                match snd op with
                | AnMlVar(v) -> absvars, LnfAppVar(v, lnfoperands_ext)
                | AnFun(abs, operands) -> absvars, LnfAppAbs((lnf_of op), lnfoperands_ext)
                
                | AnMlAppl(_) -> (* this cannot be reached *)
                                 failwith "lmdterm_to_lnf: the operator calculated by ml_applicative_decomposition is inconsistent!"
                | _ ->  failwith "unsupported Ml constructs!"

    in
    "", (* => the lnf has no name *)
    lnf_of (annotate_term (context,term))
;;

