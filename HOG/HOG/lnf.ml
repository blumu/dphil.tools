(** $Id$
	Description: Eta-long normal form
	Author:		William Blum
**)

(** Long normal form (lnf) **)


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






(***** Data-structure to store the computation graph *****)

(* for inclusion in a .mli file if it is created in the future.

(*IF-OCAML*) 
module NodeEdgeMap :
  sig
    type key = int
    type +'a t
    val empty : 'a t
    val is_empty : 'a t -> bool
    val add : key -> 'a -> 'a t -> 'a t
    val find : key -> 'a t -> 'a
    val remove : key -> 'a t -> 'a t
    val mem : key -> 'a t -> bool
    val iter : (key -> 'a -> unit) -> 'a t -> unit
    val map : ('a -> 'b) -> 'a t -> 'b t
    val mapi : (key -> 'a -> 'b) -> 'a t -> 'b t
    val fold : (key -> 'a -> 'b -> 'b) -> 'a t -> 'b -> 'b
    val compare : ('a -> 'a -> int) -> 'a t -> 'a t -> int
    val equal : ('a -> 'a -> bool) -> 'a t -> 'a t -> bool
  end

(** The set of edges is represented by a map from node to an array of target nodes id *)
type cg_edges = (int array) NodeEdgeMap.t;;

(*ENDIF-OCAML*)
(*F# 
val NodeEdgeMap : Map.Provider<int,int array>

(** The set of edges is represented by a map from node to an array of target nodes id *)
type cg_edges = Tagged.Map<int,int array,System.Collections.Generic.IComparer<int>>;;
F#*)


(** The type of a computation graph *)
type computation_graph = cg_nodes * cg_edges;;

val graph_childnode : cg_edges -> int -> int -> int 
val graph_n_children : cg_edges -> int -> int 
val graph_node_type : (ident*typ) list -> nodecontent -> typ
*)




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
(*
    (*IF-OCAML*) 
    module NodeEdgeMap = Map.Make(struct type t = int let compare = Pervasives.compare end) 
    type cg_edges = (int array) NodeEdgeMap.t;;
    (*ENDIF-OCAML*)
    (*F# 
    let NodeEdgeMap = Map.Make((Pervasives.compare : int -> int -> int))
    type cg_edges = Tagged.Map<int,int array,System.Collections.Generic.IComparer<int>>;;
    F#*)
*)  

type cg_edges = int list array;;

(** The type of a computation graph **)
type computation_graph = cg_nodes * cg_edges;;

(** [create_empy_graph] creates an empty graph **)
let create_empty_graph() = [||],[||]

(** [graph_addedge edges src tar] adds an edge going from [src] to [tar] in the graph [gr].
    @param edges is the reference to a Map from node id to array of edges
    @param src is the source of the new edge
    @param tar is the target of the new edge
    **)
let graph_addedge (edges:cg_edges) source target =
(*    edges :=  NodeEdgeMap.add source (Array.append (try NodeEdgeMap.find source !edges
                                                    with Not_found -> [||])
                                                   [|target|]) !edges;*)
    edges.(source) <- edges.(source)@[target]
;;

(** [graph_childnode edges nodeid i] returns the i^th child of node [nodeid]**)
let graph_childnode edges nodeid i =
(*    (try 
        NodeEdgeMap.find nodeid edges
    with Not_found -> failwith "function child: the node does not exist or does not have any child!" ).(i) *)
    (Array.of_list (edges.(nodeid))).(i)
;;


(** [graph_n_children edges nodeid] returns the number of children of the node [nodeid]**)
let graph_n_children edges nodeid =
    (*try  Array.length (NodeEdgeMap.find nodeid edges)
    with Not_found -> 0 *)
    List.length edges.(nodeid)
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

(** [graph_node_label node] returns the label of [node] **)
let graph_node_label = function 
      NCntApp -> "@"
    | NCntVar(x) | NCntTm(x)  -> x
    | NCntAbs(_,vars) -> LAMBDA_SYMBOL^(String.concat " " vars)
;;

(** [graph_node_label_with_id nodes_array index] returns the label of the [index]th node in the
    array nodes_array. The index of the node is added as a suffix to the label. **)
let graph_node_label_with_idsuffix nodes_array index =
    let node = nodes_array.(index)
    and nodeid = string_of_int index in
   (graph_node_label node)^" ["^
    (match nodes_array.(index) with 
        NCntApp -> nodeid
      | NCntTm(tm) -> nodeid
      | NCntVar(x) -> nodeid
      | NCntAbs("",vars) -> nodeid
      | NCntAbs(nt,vars) -> nt^":"^nodeid)^"]"
;;


(** [graphnodelabel_to_latex node] converts a node label into a latex command that prints the label **)
let graphnodelabel_to_latex= function 
      NCntApp -> "@"
    | NCntVar(x) | NCntTm(x)  -> x
    | NCntAbs(_,vars) -> "\lambda "^(String.concat " " vars)
;;

(** [lnfrs_to_graph rs lnfrules] converts rules in lnf into a computation graph.
    @param lnfrules the rules of the recursion scheme in LNF
    @return the compuation graph (nodes,edges)
**)
let lnfrs_to_graph lnfrules =
                                
    (* Compute the number of nodes to be created in the graph *)
    let rec calc_nb_nodes (_,app_part) =
        match app_part with
          LnfAppNt(nnt, []) -> 0
        | LnfAppVar(_, operands) | LnfAppTm(_, operands)
        | LnfAppNt(_, operands) ->
            List.fold_left (fun acc u -> acc + calc_nb_nodes u) 2 operands
        | LnfAppAbs(abs, operands) ->
            List.fold_left (fun acc u -> acc + calc_nb_nodes u) (2+(calc_nb_nodes abs)) operands
    in                                
    (* number of nodes to be created in the graph *)
    let nnodes = List.fold_left (fun acc (_,rhs) -> acc + calc_nb_nodes rhs) 0 lnfrules in
    

    (* The array of nodes *)
    let nodes = Array.create nnodes NCntApp in
    (* The array of edges list *)
    let edges = Array.create nnodes [] in
       
    (* Number of nodes created *)
    let inode = ref (-1) in
    (* Node creation *)
    let newnode node = incr(inode);
                       nodes.(!inode) <- node; 
                       !inode  in

    (* The first nodes of the graph are the non-terminal nodes.
       [nt_nodeid] gives an association list mapping non-terminal to their corresponding nodeid.
     *)
    let nt_nodeid = List.map (function nt,(abs_part,_) -> 
                                nt,(newnode (NCntAbs(nt, abs_part))) ) lnfrules in
                                

    (* [create_subgraph nodeid rhs] creates the subgraph corresponding to the
        lnf term [rhs]. [nodeid] is an optional argumet that specifies the node index in the array of nodes 
        that should be attributed to the root of created subgraph (in the case where the index has been reserved for it).
        If nodeid=None then a new index is created in the array of nodes.
       
       @return the id of the root of the created subgraph. *)
    let rec create_subgraph nodeid (abs_part,app_part) =
        match app_part with
          (* If it's a non-terminal of ground type then there is no need to create an extra abstraction node,
             just fetch the nodeid from the association table *)
          LnfAppNt(nnt, []) -> List.assoc nnt nt_nodeid;
        
        
        | LnfAppVar(_, operands) | LnfAppTm(_, operands)
        | LnfAppNt(_, operands) | LnfAppAbs(_, operands) ->
            let absnode_id = match nodeid with
                                Some(id) -> id
                              | None -> newnode (NCntAbs("", abs_part))
            and appnode_id = newnode (match app_part with
                                          LnfAppNt(_,_) | LnfAppAbs(_,_) -> NCntApp
                                        | LnfAppTm(tm,_) -> NCntTm(tm)
                                        | LnfAppVar(y,_) -> NCntVar(y))
            in
            graph_addedge edges absnode_id appnode_id;
            
            (match app_part with 
                (* If it is an @ node whose prime node is a non-terminal 
                   then add the operator-link that points to the lambda-node corresponding to that 
                   non-terminal *)
                  LnfAppNt(nnt,_) -> 
                    graph_addedge edges appnode_id (List.assoc nnt nt_nodeid);
                (* If it is an @ node whose prime node is an abstraction
                   then we add the operator-link that points to that lambda-node *)
                | LnfAppAbs(abs, _) ->
                    graph_addedge edges appnode_id (create_subgraph None abs);
                | _ -> ());

            List.iter (function u -> graph_addedge edges appnode_id (create_subgraph None u)) operands;
            absnode_id
    in

    (* create the subgraph of each non-terminal *)
    List.iter (function nt,rhs -> let _ = create_subgraph (Some(List.assoc nt nt_nodeid)) rhs in ()) lnfrules;
    nodes,edges
;;




