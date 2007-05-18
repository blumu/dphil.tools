(* namespace Comlab.Hogrammar *)


(****** Data-types ******)

type typ = Gr | Ar of typ * typ ;;
type ident = string;;
type alphabet = (ident * typ) list;;
type terminal = ident;;
type nonterminal = ident;;

(* applicative term *)
type appterm = Nt of nonterminal | Tm of terminal | Var of ident | App of appterm * appterm;;

(* Recursion scheme *)
type rule = nonterminal * ident list * appterm;;
type recscheme = { nonterminals : alphabet;
				   sigma : alphabet;
				   rules : rule list;
				   rs_path_validator : terminal list -> bool * string } ;;
 (** The path validator is a function that checks whether a given path (list of terminals)
    generated by the recscheme satisfies the desired specification. It returns a pair [(v,c)] where [v] tells 
    whether the validation is successful and [c] is a certificate of the validation (a string message) **)
 
(* order and arity of a type *)
let rec typeorder = function  Gr ->  0 | Ar(a,b) -> max (1+ typeorder a) (typeorder b) ;;
let rec typearity = function  Gr ->  0 | Ar(_,b) -> 1+ (typearity b) ;;


let rec string_of_type = function
    Gr -> "o"
  | Ar(Ar(_) as a, b) -> "("^(string_of_type a)^") -> "^(string_of_type b)
  | Ar(a,b) -> (string_of_type a)^" -> "^(string_of_type b)
;;

let string_of_alphabet a =
	let string_of_letter (l,t) =
	  l^":"^(string_of_type t)^"\n" ;
	in
	List.fold_left (function acc -> function l -> acc^(string_of_letter l)) "" a
;;
let print_alphabet a = print_string (string_of_alphabet a);;


let string_of_appterm term :string= 
  let rec aux term = 
     match term with
  	  Tm(f) -> f
	| Nt(nt) -> nt
	| Var(x) -> x
	| App(l,(App(_) as r)) -> (aux l)^" ("^(aux  r)^")"
	| App(l,r) -> (aux l)^" "^(aux  r)
  in aux term
;;
let print_appterm term = print_string (string_of_appterm term);;


let string_of_rule rs ((nt,para,appterm):rule) = 
    nt^" "^(List.fold_left (function acc -> function p -> acc^p^" ") "" para)^"= "^(string_of_appterm appterm)^"\n"
;;
let print_rule rs r = print_string (string_of_rule rs r);;

let string_of_rs rs =
    "Terminals:\n"^(string_of_alphabet rs.sigma)^
    "Non-terminals:\n"^(string_of_alphabet rs.nonterminals)^
    "Rules:\n"^(List.fold_left (function acc -> function r -> acc^(string_of_rule rs r)) "" rs.rules)
;;
let print_rs rs = print_string (string_of_rs rs);;


exception Type_check_error;;
exception Wrong_variable_name of ident;;
exception Wrong_terminal_name of ident;;
exception Wrong_nonterminal_name of ident;;

(* return the type of a terminal *)
let terminal_type rs f =
    try 
        List.assoc f rs.sigma
    with Not_found -> raise (Wrong_terminal_name f)
;;

(* return the type of a non-terminal *)
let nonterminal_type rs nt =
  try 
    List.assoc nt rs.nonterminals
  with Not_found -> raise (Wrong_nonterminal_name nt)
;;

(* Create an association list mapping parameters name in p to 
  the type of the correspding parameter in type t *)
let rec create_paramtyplist nt p t = match p,t with
  | [],Gr -> []
  | x::q, Ar(l,r) -> (x,l)::(create_paramtyplist nt q r)
  | _ -> failwith ("Type of non-terminal "^nt^" does not match with the number of specified parameters.")
;;


exception RscheckError of string;;

(* Check that rs is a well-defined recursion scheme *)
let rs_check rs =
  let valid = ref true in
  
  (* - parameters' names must be disjoint from terminals' names
     - appterm must be well-typed and para must be a superset of fv(appterm)
     - appterm must be of ground type
  *)
  let check_eq ((nt,para,appterm) as eq) =
    let partypelst = create_paramtyplist nt para (nonterminal_type rs nt) in
    let var_type x =  List.assoc x partypelst in
    let rec typecheck_term = function
          Tm(f) -> terminal_type rs f
        | Nt(nt) -> nonterminal_type rs nt
        | Var(x) -> if List.exists (function m -> m=x) para then
                      var_type x
                    else
                      raise (Wrong_variable_name x);
        | App(a,b) -> match (typecheck_term a), (typecheck_term b) with
                       Ar(tl,tr), tb when tl=tb ->  tr
                      |  _ -> raise Type_check_error;
    in
    (* ensures that the non-terminal name is defined *)
    let _ = nonterminal_type rs nt in

    (* check that the parameters names do not clash with terminals names *)
    let _ = List.exists (function p-> 
    (List.exists (function (a,t)-> 
            if a=p then
            begin
              raise (RscheckError ("Parameter name "^p^" conflicts with a terminal name in "^(string_of_rule rs eq)));
               valid := false;
            end;
            a=p) rs.sigma)) para in
    
    try if (typecheck_term appterm) <> Gr then
        begin
          raise (RscheckError "RHS is not of ground type in: ");
          print_rule rs eq; valid := false;
        end
    with 
        Wrong_variable_name(x) ->
            raise (RscheckError ("Undefined variable '"^x^"' in RHS of: "^(string_of_rule rs eq))) ;
            valid := false;
        | Wrong_terminal_name(x) ->
            raise (RscheckError ("Undefined terminal '"^x^"' in RHS of: "^(string_of_rule rs eq))) ;
            valid := false;
        | Wrong_nonterminal_name(x) ->
            raise (RscheckError ("Undefined non-terminal '"^x^"' in RHS of: "^(string_of_rule rs eq))) ;
            valid := false;
        | Type_check_error ->
            raise (RscheckError "Type-checking error in RHS of: ") ;
            valid := false;
in
	(* Check all the rules *)
    List.iter check_eq rs.rules;
    
	(* Check that the name (i.e. the non-terminal) of the first rule is of ground type *)
	if (List.length rs.rules) > 0 then
	begin
		match (List.hd rs.rules) with
			_,[],_ -> ()
		|	_ -> raise (RscheckError "The LHS of the first rule must be of ground type (i.e. no parameter)!");
				 valid := false;
	end;
				 
	!valid    
;;


(** Retrieve the operator and the operands from an applicative term.
   @return a pair (op,operands) where op is the operator (a terminal, variable or non-terminal) and
   operands is the list of operands terms. **)
let rec appterm_operator_operands t = match t with
 Tm(_) | Var(_) | Nt(_) -> t,[]
 | App(a,b) -> let op,oper = appterm_operator_operands a in op,oper@[b]
;;

(* Perform a reduction on the applicative term appterm (which must not contain any free variable) *)
let step_reduce rs appterm = 
  let substitute nt operands = 
    let _,parms,rhs = List.find (function rname,_,_ -> rname=nt) rs.rules in
      (* Check that we have the right number of operands (we only reduce full applications to non-terminals) *)
      if List.length parms = List.length operands then	
	let substlst = List.combine parms operands in
	let rec subst term = match term with
	    Tm(_) | Nt(_) -> term
	  | App(l,r) -> App((subst l), (subst r))
	  | Var(x)  -> try List.assoc x substlst
	    with Not_found -> term
	in
	  subst rhs
      else
	failwith ("Error: partial application to the terminal "^nt^".")
  in
    
  (* Look for the outermost reduction context in appterm, and if there is one perform the reduction.
     A reduction context is of the form C[_] where the hole contains a term of the form X t1 ... tn for some nonterminal X.
     
     The function takes two parameters:
     -appterm: the term in which we look for a context. 
     Let us assume that it is of the form T0 T1 T2 ... Tq where T0 is not an application.
     -operands: a list of operands terms that are applied to appterm (this parameter is used to 
     collect the list of parameters as we approach the operator node in the AST so that the parameters list
     is available when we reach the operator node and so we can perform the substitution)
     
     The function returns a triple: (found,outer,term) where
     If found=true then a reduction context has been found in appterm
     and 'term' contains the reducted term. 'outer' is set to true iff 
     the outermost redex-context is exactly the outermost application
     i.e. the context is C[_] = _ (and therefore C[appterm] = appterm)
     and Op is a non-terminal.
     
     If found=false then 'term' contains appterm and 'outer' has no meaning.
  *)
  let rec findredcontext_and_substitute appterm operands =
    match appterm with
	Tm(_) -> false,false,appterm
      | Var(_) -> failwith "Trying to reduce an open term!";
      | Nt(nt) -> true,true,(substitute nt operands)
      | App(t0_tqminusone,tq) -> 			
	  let f,o,t = findredcontext_and_substitute t0_tqminusone (tq::operands) in
	    if f then (* a context was found (and reduced) *)
	      true,o,
	  ( (* the outermost redex is exaclty the outermost application appterm = T0 T1 ... Tq *)
            if o then t 
	      (* the outermost context lies somewhere inside T0 or T1 or ... T_(q-1) *)
	    else App(t,tq)
	  )
	    else (* no context found: *)
	      (* then look for a context in the operand T_q *)
	      let f,o,t = findredcontext_and_substitute tq [] in
		f,false,App(t0_tqminusone, t)
  in
  let f,_,red = findredcontext_and_substitute appterm [] in
    f,red
;;

(* Perform an OI-derivation of the recursion scheme rs *)
let oi_derivation rs =

  let t = ref (Nt("S")) in 
    print_newline(); print_appterm !t;  print_newline();
    while (input_line stdin) <> "q" do
      let red,tred = step_reduce rs !t in
	t := tred;	
	
	print_appterm !t; print_newline(); print_newline();
    done;
;;

(** Return the type of an applicative term (assume that the term is well-typed) 
   @param fvtypes gives the types of the free variable in the term:
   it is a list of pair (var,typ) where 'typ' is the type of the variable 'var'.
**)
let rec appterm_type rs fvtypes = function
    Tm(f) -> terminal_type rs f
  | Nt(nt) -> nonterminal_type rs nt
  | Var(x) -> List.assoc x fvtypes
  | App(a,_) -> match appterm_type rs fvtypes a with 
        Gr -> failwith "Term is not well-typed!"
      | Ar(_,r) -> r
;;

(* return the order of an appterm *)
let appterm_order rs fvtypes t = typeorder (appterm_type rs fvtypes t);;



(** Long normal form (lnf) **)

(* We define the type of the right-hand side of a rule in lnf that we call RHS for short. It is given by:
   - the top abstraction: a list of abstracted variables,
   - the (leftmost) operator : either @, a variable or a nonterminal,
   - the list of operands that are themselves of the type RHS. *)
type lnfrhs = lnfabstractpart  * lnfapplicativepart
and lnfabstractpart = ident list
and lnfapplicativepart = 
    LnfAppVar of ident * lnfrhs list
  | LnfAppNt of nonterminal * lnfrhs list
  | LnfAppTm of terminal * lnfrhs list
;;
type lnfrule = nonterminal * lnfrhs ;;


(** [get_parameter_type rs x] returns the type of the formal parameter [x].
    (Recall that the formal parameter names of the grammar equation are required to be disjoint,
    hence there is at most one equation that uses a given parameter) **)
let get_parameter_type rs x = 
  (* find the equation that uses the parameter x *)
  try 
      let (nt,parms,_) = List.find (function (_,parms,_) -> List.mem x parms) rs.rules
      in
        let rec get_param_type parms typ = match parms,typ with
          | [],Gr -> failwith "get_parameter_type: ground-type terminal!"
          | p::_, Ar(l,_) when p = x -> l
          | _::q, Ar(l,r) -> (get_param_type q r)
          | _ -> failwith ("Type of non-terminal "^nt^" does not match with the number of specified parameters.")
        in 
        get_param_type parms (nonterminal_type rs nt)
   with Not_found -> failwith ("get_parameter_type: formal parameter '"^x^"' not used in the recursion scheme.")
;;


(* For the creation of fresh variables *)
let freshvar = ref 0;;
let new_freshvar() = incr(freshvar); "#"^(string_of_int !freshvar) ;;

(** [rule_to_lnf rs rule] converts a grammar rule into LNF
   @param rs is the recursion scheme
   @param rule is the rule to be converted
   @return (nt,lnfrule),vartypes 
    where [nt] is the nonterminal, lnfrule is the applicative term in LNF,
    and [vartypes] is an association list mapping variable names to their type
    (possibly containing new fresh variables introduced during the eta-expansion
**)
let rule_to_lnf rs (nt,param,rhs) = 
  (* create the association list mapping parameters' name to their type *)
  let fvtypes = create_paramtyplist nt param (nonterminal_type rs nt) in
  let newvarstypes = ref [] in
  let rec lnf appterm = lnf_aux (appterm_type rs fvtypes appterm) appterm
  and lnf_aux typ appterm =     
    let op,operands = appterm_operator_operands appterm in
      (* Create a list of fresh variables Phi_1 ... Phi_n where n is the arity of appterm *)
    let absvars = Array.to_list (Array.init (typearity typ) (fun i -> new_freshvar()) ) in
      (* create an association list mapping Phi_1 ... Phi_n to their respective types *)
    let absvars_types = create_paramtyplist "#not a nt#" absvars typ
      (* compute the lnfs of the operands *)
    and lnfoperands = List.map lnf operands in
      (* add 'lnf(Phi_1) ... lnf(Phi_n)'  to the list of operands *)    
    let lnfoperands_ext = lnfoperands@(List.map (function (v,t) -> lnf_aux t (Var(v))) absvars_types) in
      newvarstypes := !newvarstypes@absvars_types; (* accumulates the list of created fresh variables *)
      match op with
          Tm(t)  -> absvars, LnfAppTm(t, lnfoperands_ext)
	    | Var(v) -> absvars, LnfAppVar(v, lnfoperands_ext)
	    | Nt(nt) -> absvars, LnfAppNt(nt, lnfoperands_ext)
	    | App(_) -> failwith "eq_to_lnf: appterm_operator_operands returned wrong operator."
  in  
    (nt, (param, (snd (lnf rhs)))),(* the first element of the pair returned by 'lnf appterm' (the list of abstracted variables)  must be empty since the rhs of the rule is of order 0 *) 
    fvtypes@(!newvarstypes)  (* the association list mapping variables to their type *)
;;

let lnf_to_string (rs:recscheme) ((nt,lnfrhs):lnfrule) =
  let rec rhs_to_string (abs_part,app_part) =
    "\\"^(String.concat " " abs_part)^"."^(
      match app_part with
	  LnfAppTm(x,operands)
	|LnfAppVar(x,operands)
	|LnfAppNt(x,operands) -> let p = String.concat " " (List.map rhs_to_string operands) in
            "("^x^(if p = "" then "" else " "^p)^")")  
  in
    nt^" = "^(rhs_to_string lnfrhs)
;;

(** Convert a recscheme to lnf.
    @return (lnfrules,vartm_types) where 
    [lnfrules] is a list of rules in lnf and
    [vartm_types] is an association list mapping variable and terminal names to their type. **)
let rs_to_lnf rs = 
  let vartypes = ref [] in
  freshvar := 0; (* reinit the counter for the creation of fresh variables *)
  (List.map (function rule -> let lnfrule,newvtypes = rule_to_lnf rs rule in
                             vartypes := !vartypes@newvtypes;
                             lnfrule ) rs.rules
  ) , !vartypes@rs.sigma
;;


(****** Computation graphs *****)


(** Content of the node of the graph **)
type nodecontent = 
    NCntApp
  | NCntAbs of ident * ident list
  | NCntVar of ident
  | NCntTm of terminal
;;

(** The set of nodes is represented by an array of node contents **)
type cg_nodes = nodecontent array;;


(** The set of edges is represented by a map from node to an array of target nodes id **)
    (*IF-OCAML*) 
    module NodeEdgeMap = Map.Make(struct type t = int let compare = Pervasives.compare end) 
    type cg_edges = (int array) NodeEdgeMap.t;;
    (*ENDIF-OCAML*)
    (*F# 
    let NodeEdgeMap = Map.Make((Pervasives.compare : int -> int -> int))
    type cg_edges = Tagged.Map<int,int array,System.Collections.Generic.IComparer<int>>;;
    F#*)

(** The type of a computation graph **)
type computation_graph = cg_nodes * cg_edges;;


(** [graph_addedge edges src tar] adds an edge going from [src] to [tar] in the graph [gr].
    @param edges is the reference to a Map from node id to array of edges
    @param src is the source of the new edge
    @param tar is the target of the new edge
    **)
let graph_addedge (edges: cg_edges ref) source target =
    edges :=  NodeEdgeMap.add source (Array.append (try NodeEdgeMap.find source !edges
                                                    with Not_found -> [||])
                                                   [|target|]) !edges;
;;

(** [graph_childnode edges nodeid i] returns the i^th child of node [nodeid]**)
let graph_childnode edges nodeid i =
    (try 
        NodeEdgeMap.find nodeid edges
    with Not_found -> failwith "function child: the node does not exist or does not have any child!" ).(i)
;;


(** [graph_n_children edges nodeid] returns the number of children of the node [nodeid]**)
let graph_n_children edges nodeid =
    try  Array.length (NodeEdgeMap.find nodeid edges)
    with Not_found -> 0
;;


(** [graph_node_type varstype node] returns the type of node [node]
    @param vartm_types is an association list mapping variable and terminal names to types
    @param node is the requested node
    @return the type of the node    
    @note The type of a node is defined as follows:
        - type( @ ) is the ground type 
        - type( x:A ) = A where x is a variable or a terminal
        - type( \lambda x_1:A_1 ... x_n:A_n ) = A_1 -> ... -> A_n -> o
        **)
let rec graph_node_type vartm_types = function 
        NCntApp -> Gr
      | NCntVar(x) | NCntTm(x)  -> List.assoc x vartm_types
      | NCntAbs(_,[]) -> Gr
      | NCntAbs(_,x::vars) ->  Ar((List.assoc x vartm_types), (graph_node_type vartm_types (NCntAbs("",vars))))
;;


(** [hors_to_graph rs lnfrules] converts the recursion scheme [rs] into a computation graph.
    @param rs recursion scheme
    @param lnfrules the rules of the recursion scheme in LNF
    @return the compuation graph (nodes,edges)
**)
let hors_to_graph (rs:recscheme) lnfrules =
    (* The list of created nodes *)
    let nodes = ref [] in
    (* The edges: a map from node ids to array of edges *)
    let edges = ref (NodeEdgeMap.empty) in
    
   
    (* number of nodes created *)
    let nnodes = ref 0 in

    (* The first nodes of the graph are the non-terminal nodes.
       Create them and return an association list mapping non-terminal to their corresponding nodeid.
     *)
    let nt_nodeid = List.map (function nt,(abs_part,_) -> 
                                incr(nnodes);
                                nodes := (NCntAbs(nt, abs_part))::!nodes;
                                nt,(!nnodes-1) ) lnfrules in
    
    (* [create_subgraph nt rhs] creates the subgraph corresponding to the
       lnf term [rhs]. When set to a value different from "", the parameter [nt] specifies that 
       the subgraph to be created corresponds to the non-terminal [nt]. In such case, the root node is
       marked with [nt]. 
       Return the id of the root of the created subgraph. *)
    let rec create_subgraph nt (abs_part,app_part) =
        match app_part with
          (* If it's a non-terminal of ground type then there is no need to create an extra abstraction node,
             just fetch the nodeid from the association table *)
          LnfAppNt(nt, []) -> List.assoc nt nt_nodeid;
        | LnfAppVar(x, operands) | LnfAppTm(x, operands) | LnfAppNt(x, operands) ->
            let absnode_id = 
                if nt = "" then 
                begin
                    incr(nnodes);
                    nodes := (NCntAbs(nt, abs_part))::!nodes;
                    !nnodes-1;
                end
                else
                begin
                    List.assoc nt nt_nodeid
                end
            in
             
            
            let appnode_id = !nnodes in
            incr(nnodes);
            nodes := (match app_part with
                          LnfAppNt(_,_) -> NCntApp
                        | LnfAppTm(tm,_) -> NCntTm(tm)
                        | LnfAppVar(x,_) -> NCntVar(x)
                        )::(!nodes);

            graph_addedge edges absnode_id appnode_id;
            
            (* If it is an @ node then add the edge pointing to the operator *)
            (match app_part with LnfAppNt(_) -> 
                graph_addedge edges appnode_id (List.assoc x nt_nodeid);
             | _ -> ());

            List.iter (function u -> graph_addedge edges appnode_id (create_subgraph "" u)) operands;
            absnode_id
    in

    (* create the subgraph of each non-terminal *)
    List.iter (function nt,rhs -> let _ = create_subgraph nt rhs in ()) lnfrules;
    (Array.of_list (List.rev !nodes)),!edges
;;


(**************** Validators ***********)

exception InvalidPath of string;;


(** Validator for paths of Urzyczyn's tree based on De Mirand's decomposition. *)
let demiranda_validator path =
   let s = Array.of_list path in
   let f = ref 0 in
   let b = ref ((Array.length s)-1) in
   let read_forward ident = if s.(!f) = ident then incr(f) else raise (InvalidPath "unexpected terminal symbol") in
   let read_backward ident = if s.(!b) = ident then decr(b) else raise (InvalidPath "unexpected terminal symbol") in
   (* A function that checks for a prefix of a well-bracketed sequence starting at !f and finishing at e.
      It returns (lev,nopen) where level is the nesting level (0 for a well-bracketed sequence) at !f
      and nopen is the number of occurrences of the opening bracket '['. *)
   let well_bracketed_prefix e =
     let level = ref 0 in
     let nopen = ref 0 in
     while !f <= e do
       (match s.(!f) with
         "[" -> incr nopen; incr level
        | "]" when !level > 0 -> decr level;
        | _ -> raise (InvalidPath "unexpected terminal symbol"));
        incr f;
        if !f < e then read_forward "3";                                                       
     done;
     !level,!nopen
   in
   (* a function that checks read the longest well-bracketed sequence ending
      at b *)
   let read_backward_longest_well_bracketed () =
     let level = ref 0 and cont = ref true in
     while !cont && !b >= 0 do
       (match s.(!b) with
         "]" -> incr level;
                read_backward "]";
                if !b > 0 then read_backward "3";
        | "[" when !level >0 -> decr level;
                                read_backward "[";
                                if !b > 0 then read_backward "3";
        | "[" -> cont:= false;
        | _ -> raise (InvalidPath "unexpected terminal symbol") );
     done;
     (* If the whole sequence has been read then it must be completely well-bracketed *)
     if !cont && !level <> 0 then
       raise (InvalidPath "not a prefix of a well-bracketed sequence.") 
   in
   (* Check de Miranda's decomposition of the Urzyczin word. 
      returns nstars,nopen where nstars is the number of stars at the end of the sequence,
      and nopen is the number of opening brackets in the first part of the 
      de Miranda's decomposition. *)
   let demiranda_decompos() =
        (* We first proceed by reading the sequence backwards: *)
        (* 1 - counts the number of stars at the end of the sequence  *)
        let nbstars = ref 0 in
        while s.(!b) = "*" do 
        read_backward "*";
        incr nbstars;                                                               
        done;
        read_backward "3";
        (* 2 - read backwards the longest well-bracketed sequence *)
        read_backward_longest_well_bracketed();

        (* 3 - read forwards the remaining part of the sequence (the beginning), checking
        that it is a prefix of a well-bracketed sequence and counting the number of 
        occurrences of opening bracket '['. *)
        let level,nbopening = well_bracketed_prefix !b in
        
        !nbstars,nbopening
    in
    (* accepts the empty string *)
    if !b = -1 then true,"Empty path"
    else
        try                                                         
           match s.(!b) with
            "e" -> decr b; (* skip the 'e' at the end *)
                   let nbstars,nbopening = demiranda_decompos() in
                   (* Check that the number of star is equal to the number of '[' *)
                   if nbstars = nbopening then
                     true,"Valid maximal path"
                   else
                     false,"Invalid maximal path"

            | "r" ->  (* Check for a well-bracketed sequence from !f to !b-1 *)
                      if fst (well_bracketed_prefix (!b-1)) = 0 then
                        true,"Valid maximal path"
                      else
                        false,"Invalid maximal path"

            | "[" | "]" -> ignore(well_bracketed_prefix !b); true,"Valid prefix path"

            | "3" -> ignore(well_bracketed_prefix (!b-1)); true,"Valid prefix path"
            
            | "*" -> let nbstars,nbopening = demiranda_decompos() in
                     (* Check that the number of star is <= than the number of '[' *)
                     if nbstars <= nbopening then
                       true,"Valid prefix path"
                     else
                       false,"Invalid prefix path"
                       
            | _ -> false,"Invalid path containing undefined terminals"
        with InvalidPath(msg) -> false, "Invalid path, "^msg
;;




(**** Tests **)

(*

(* example of recursion scheme *)
let rs : recscheme = {
  nonterminals = [ "S", Gr;
                   "F", Ar(Gr,Ar(Gr,Gr)) ;
                   "G", Ar(Ar(Gr,Gr),Ar(Gr,Gr)) ];
  sigma = [ "f", Ar(Gr,Ar(Gr,Gr));
            "g", Ar(Gr,Gr);
            "t", Ar(Gr,Ar(Gr,Gr)) ;
            "e", Gr ];
  rules = [ "S",[],App(App(Tm("f"), Tm("e")),Tm("e"));
            "F",["x";"y"],App(App(Tm("f"), Var("x")), Var("y"));
            "G",["phi";"x"],App(Tm("g"), App(Tm("g"), Var("x")));
          ]
} ;;

string_of_type (Ar(Gr,Ar(Gr,Gr)));;
print_alphabet rs.sigma;;
print_rs rs;;
rs_check rs;;
oi_derivation rs;


(* HORS producing Urzyczyn's tree *)
let urz = {
    nonterminals = [ "S", Gr;
                     "D", Ar(Ar(Gr,Ar(Gr,Gr)), Ar(Gr,Ar(Gr,Ar(Gr,Gr)))) ;
                     "F", Ar(Gr,Gr) ;
                     "E", Gr ;
                     "G", Ar(Gr,Ar(Gr,Gr)) ;
                   ];
    sigma = [   "[", Ar(Gr,Gr) ;
                "]", Ar(Gr,Gr);
                "*", Ar(Gr,Gr);
                "3", Ar(Gr,Ar(Gr,Ar(Gr,Gr)));
                "e", Gr;
                "r", Gr;
            ];
    rules = [   "S",[],App(Tm("["),
                     App(App(App(App(Nt("D"), Nt("G")),Nt("E")),Nt("E")),Nt("E"))
                     ); 
                "D",["phi";"x";"y";"z"],
                    App(App(App(Tm("3"),
                        App(Tm("["), 
                        App(App(App(App(Nt("D"), App(App(Nt("D"), Var("phi")),Var("x"))),
                        Var("z")
                        ),
                        App(Nt("F"), Var("y"))
                        ),App(Nt("F"), Var("y")))
                        )),
                        App(Tm("]"), App(App(Var("phi"),Var("y")),Var("x")))),
                        App(Tm("*"), Var("z")));
                "F",["x"],App(Tm("*"),Var("x"));
                "E",[],Tm("e");
                "G",["u";"v"],Tm("r");
        ];
        
    rs_path_validator = Hog.demiranda_validator
 };;


*)