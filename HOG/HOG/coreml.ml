(** $Id$
	Description: Structures for encoding Core Ml terms
	Author:		William Blum
**)
open FSharp.Compatibility.OCaml
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
     (string_of_polyalphabet_aux " " c)^"|-"^(string_of_mlterm t);;

exception MissingVariableInContext;;

let lookup_var x context =
    match List.try_assoc x context with
    | Some v -> v
    | None -> raise MissingVariableInContext;;



(********************** Type inference **************************)



(** [namesubst_annotatedterm s context] performs the type substitution [s] for all the
    types occurring in the context [context] **)
let rec namesubst_context s =
    List.map (function (v,t) -> v, (type_substitute s t))
;;

(** [namesubst_annotatedterm s aterm] performs the type substitution [s] in all the types occurring in the annotated term [aterm] **)
let rec namesubst_annotatedterm s (typ,term) =
    let ts = type_substitute s typ in
    ts,
    (match term with
     | AnFun(x,a) -> AnFun(x, (namesubst_annotatedterm s a))
     | AnLet(u,a) -> AnLet((List.map (fun (a,b,c) -> a,b,namesubst_annotatedterm s c) u),namesubst_annotatedterm s a)
     | AnLetrec(u,a) -> AnLetrec((List.map (fun (a,b,c) -> a,b,namesubst_annotatedterm s c) u),namesubst_annotatedterm s a)
     | AnIf(a,b,c)   -> AnIf(namesubst_annotatedterm s a, namesubst_annotatedterm s b, namesubst_annotatedterm s c)
     | AnEqTest(a,b) -> AnEqTest(namesubst_annotatedterm s a, namesubst_annotatedterm s b)
     | AnMlAppl(a,b) -> AnMlAppl(namesubst_annotatedterm s a, namesubst_annotatedterm s b)
     | AnMlVar(_)
     | AnMlInt(_)
     | AnMlBool(_)
     | AnPred
     | AnSucc -> term)
;;

(* Infer a polymorphic type for a term-in-context *)
let infer_polytype (context,term) =
    let freshvar = ref (-1) in
    let new_freshvar() = incr(freshvar); "'"^(string_of_char (char_of_int ((int_of_char 'a')+ !freshvar))) in
    let rec infer context (term:ml_expr) =
        match term with
          MlVar(x) -> lookup_var x context
        | MlAppl(f,e) -> let tauf, taue = (infer context f),(infer context e) in
                         let _,_,sigma = unify_polytype tauf (PTypAr(taue,PTypVar(new_freshvar()))) in
                         (match sigma with
                          | PTypAr(_,sigma) -> sigma
                          | _ -> raise TypecheckingError)
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


(** Calculate the principal type of each subterms of some term-in-context and return an
    annotated version of the term-in-context.
 **)
let annotate_termincontext ((context,term):ml_termincontext) :ml_annotated_termincontext =
    let freshvar = ref (-1) in
    let new_freshvar() = incr(freshvar); "'"^(string_of_char (char_of_int ((int_of_char 'a')+ !freshvar))) in
    (* annotated the term [term] in the typed-context [context].
       return (subst_context,annoted term) where subst_context is the same as context but
       with some type variables substituted.
     *)
    let rec annotate context (term:ml_expr) =
        match term with
          MlVar(x) ->  context, ((lookup_var x context),(AnMlVar(x)))
        | MlAppl(f,e) -> let ncontext, ((tauf,_) as f_annotated) = annotate context f in
                         let nncontext, ((taue,_) as e_annotated) = annotate ncontext e in
                         let subst_f, subst_e, unif = unify_polytype tauf (PTypAr(taue,PTypVar(new_freshvar()))) in
                         (namesubst_context (subst_f@subst_e) nncontext),
                         (match unif with
                         | PTypAr(_,ret_type) -> ret_type, (AnMlAppl((namesubst_annotatedterm subst_f f_annotated),
                                                                     (namesubst_annotatedterm subst_e e_annotated)))
                         | _ -> raise TypecheckingError)

        | Fun(x,e) -> let ncontext, (e_type,e_subanotated) = annotate ((x,PTypVar(new_freshvar()))::context) e in
                      let xtype = List.assoc x ncontext in
                      (List.tl ncontext), ((PTypAr(xtype, e_type)),  (AnFun((xtype,x),(e_type,e_subanotated))))

        | _ -> failwith "unsupported Ml constructs!"

    in annotate context term
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


(** [annotatedlmdterm_to_lnfrule termincontext tmincontext] converts an annotated term-in-context
    to eta-long normal form.
    @param tmincontext the input term-in-context
    @return the (name,lnf) where lnf is the long normal form of [tmincontext] and name is a dummy rule name.
**)
let annotatedterm_to_lnfrule ((_,annotterm):ml_annotated_termincontext) :lnfrule =
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
                let absvars_types = create_param_polytyp_list absvars polytyp in
                (* compute the lnfs of the operands *)
                let lnfoperands = List.map lnf_of operands in
                (* add 'lnf(Phi_1) ... lnf(Phi_n)'  to the list of operands *)
                let lnfoperands_ext = lnfoperands@(List.map (function (v,t) -> lnf_of (t,(AnMlVar(v)))) absvars_types) in
                match snd op with
                | AnMlVar(v) -> absvars, LnfAppVar(v, lnfoperands_ext)
                | AnFun(abs, operands) -> absvars, LnfAppAbs((lnf_of op), lnfoperands_ext)

                | AnMlAppl(_) -> (* this cannot be reached *)
                                 failwith "annotatedlmdterm_to_lnfrule: the operator calculated by ml_applicative_decomposition is inconsistent!"
                | _ ->  failwith "unsupported Ml constructs!"

    in
    // give a dummy name to the lnf (different from the empty string in order for the rule to be treated as a valid non terminal of a grammar)
    "Root",(lnf_of annotterm)
;;

(** Return just the lnf of an annotated term in context [annot] **)
let annotatedterm_to_lnf annotterm = snd (annotatedterm_to_lnfrule annotterm)
