(** Description: Untyped lambda terms definition and operations
    Author: William Blum
**)
open FSharp.Compatibility.OCaml
open Common

/// Variable name
type identifier = string;;

/// An untyped lambda term
type ulc_term =
  | Var of identifier
  | Lam of identifier * ulc_term
  | App of ulc_term * ulc_term

/// Expression defining an untyped lambda term.
/// Such expression can contain let-bindings to define and reuse closed terms as part of a larger expression
type ulc_expression =
  | Var of identifier
  | Lam of identifier * ulc_expression
  | App of ulc_expression * ulc_expression
  | Let of identifier * ulc_expression * ulc_expression

///////////// Pretty-printing

(** Pretty-print an untyped lambda expression *)
let rec string_of_ulc_expression = function
  | Var(x) -> x
  | App(App(e1,e2),e3) -> (string_of_ulc_expression (App(e1,e2)))^" "^(string_in_bracket e3)
  | App(e1,e2) -> (string_in_bracket e1)^" "^(string_in_bracket e2)
  | Lam(x,e) -> "Lam "^x^" . "^(string_of_ulc_expression e)
  | Let(x,e,e1) -> "let "^x^" = "^(string_of_ulc_expression e)^" in \r\n"^(string_of_ulc_expression e1)
   
and string_in_bracket = function
  | Var (x) -> x;
  | e -> "("^(string_of_ulc_expression e)^")";
;;

let lookup_binding name context = List.try_assoc name context

/// Convert an ULC expression to ULC term without any let-binding
let rec ulcexpression_to_ulcterm bindings = function 
    | ulc_expression.Var x ->
        (match lookup_binding x bindings with
        | Some v -> v
        | None -> ulc_term.Var(x))
    | ulc_expression.App (x,y) ->
        let x_term = ulcexpression_to_ulcterm bindings x in
        let y_term = ulcexpression_to_ulcterm bindings y in
        ulc_term.App(x_term, y_term)
    | ulc_expression.Lam (x, e) -> 
        let newbindings = List.remove_assoc x bindings in
        ulc_term.Lam (x, ulcexpression_to_ulcterm newbindings e)
    | ulc_expression.Let(x, e, e1) ->
        let x_term = ulcexpression_to_ulcterm bindings e in
        let newbindings = (x,x_term)::bindings in
        ulcexpression_to_ulcterm newbindings e1

(** Interpret a term as an applicative term 'operator operand1 .... operandk' and return the result 
    as a pair (op,operands) where op is the operator (that is either a lambda abstraction or a variable),
    and `operands` is the list of operands in the term application. **)
let rec applicative_decomposition = function
  | ulc_term.Var(_)
  | ulc_term.Lam(_, _) as t ->
        t,[]
  | ulc_term.App(a,b) -> let op,oper = applicative_decomposition a in op,oper@[b]
;;


(** [ulcterm_to_lnfrule term] converts an untyped lambda term as LNF rules.
    
    Convert an ULC expression in LNF format. The result is not technically an eta-long normal form
    since there is no such thing for untyped lambda terms. In particular, the application are kept as is 
    without eta-expansion. The point of this function is just to encode the term in the appropriate format 
    to be used to generate computation graphs and traversals. The returned LNF expression can be thought of
    as a representation of the infinite eta-expansion of the untyped lamdba-term.

    @return the (name,lnf) where lnf is the long normal form of [tmincontext] and name is a dummy rule name.
**)
let ulcterm_to_lnfrule (term:ulc_term) : Lnf.lnfrule =
    (* For the creation of fresh variables *)
    let freshvar = ref 0 in
    let new_freshvar() = incr(freshvar); "#"^(string_of_int !freshvar) in

    let rec lnf_of (term:ulc_term) :Lnf.lnf =
        match term with
        (* In the abstraction case, we aggregated all the consecutive lambda abstractions in 
            the parameter part of the LNF rule. *)
        | ulc_term.Lam(x,e) ->
            let consecutiveAbstractedVariables, applicativeTerm = lnf_of e in
            x::consecutiveAbstractedVariables, applicativeTerm

        | _ ->
            (* decompose the term in the form `operator operand_1 ... operand_k` *)
            let operator, operands = applicative_decomposition term in
            (* compute the lnfs of the operands *)
            let lnfoperands = List.map lnf_of operands in
            match operator with
            | ulc_term.Var(v) -> [], Lnf.LnfAppVar(v, lnfoperands)
            | ulc_term.Lam(_, _) -> [], Lnf.LnfAppAbs((lnf_of operator), lnfoperands)
            | ulc_term.App(_, _) ->
                failwith "ulcterm_to_lnfrule: the operator returned by applicative_decomposition is incorrect!"

    in
    // give a dummy name to the lnf (different from the empty string in order for the rule to be treated as a valid non terminal of a grammar)
    "Root", (lnf_of term)
;;

(** Return just the lnf of an untyped term **)
let ulcterm_to_lnf term = snd (ulcterm_to_lnfrule term)
