(** $Id$
	Description: Computation graph
	Author:		William Blum
**)

open FSharp.Compatibility.OCaml
open FSharp.Compatibility.OCaml.List
open Common
open Type
open Lnf

(************** Game concepts **************)

(** Type for the player **)
type Player = Opponent | Proponent

(** Permutes player O and P **)
let player_permute = function Proponent -> Opponent | Opponent -> Proponent

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
type cg_edges = int list array;;

(** [graph_addedge edges src tar] adds an edge going from [src] to [tar] in the graph [gr].
    @param edges is the reference to a Map from node id to array of edges
    @param src is the source of the new edge
    @param tar is the target of the new edge
    **)
let graph_addedge (edges:cg_edges) source target =
    edges.(source) <- edges.(source)@[target]
;;

(** [graph_childnode edges nodeid i] returns the i^th child of node [nodeid]**)
let graph_childnode edges nodeid i =
    (Array.of_list (edges.(nodeid))).(i)
;;


(** [graph_n_children edges nodeid] returns the number of children of the node [nodeid]**)
let graph_n_children edges nodeid =
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

(** Tell who plays a given node of the computatrion graph **)
let graphnode_player = function
                            NCntAbs(_,_) -> Opponent
                          | NCntVar(_) |NCntApp | NCntTm(_) -> Proponent
;;



(** [graphnodelabel_to_latex node] converts a node label into a latex command that prints the label **)
let graphnodelabel_to_latex = function
      NCntApp -> "@"
    | NCntVar(x) | NCntTm(x)  -> x
    | NCntAbs(_,vars) -> "\lambda "^(String.concat " " vars)
;;


(** The computation graph implemented as as an array of nodes, edges 
    and an `enabler` relationship between nodes **)
type computation_graph = class
    
    (** Array of nodes in the computation graph **)
    val nodes : cg_nodes

    (** Array of parent-child edges in the computation graph. 
        Each element in the array correspond to a node and contains the list of indices of its children. **)
    val edges : cg_edges

    (*******
         The following 4 arrays contain information about the nodes that are computed
         from the graph at initialization.
     *******)

    (** mapping from nodes to their enabler:
        - for @-nodes it's undefined and set to -1
        - for lambda-nodes it's the index of the parent node
        - for bound variable it's the index of the binding node
        - for free variable it's the index of the root (0) **)
    val enabler : int array

    (** mapping from nodes to their span. The span is the distance in the graph from the node to its enabler.
        - for @-nodes it's undefined (-1)
        - for the root it's also undefined (-1)
        - for other lambda-nodes it's 1
        - for bound variable it's the distance between the variable and its binder
        - for free variable it's the distance between the variable and the root (0) **)
    val span : int array

    (** mapping from nodes to their parameter index.
        - it's undefined (=-1) for free variables, @-nodes and lambda nodes
        - for bound variable it's the parameter index (the variable position in the list of variables abstracted by its binder)
    **)
    val parameterindex : int array

    (** The childindex is the index number of a child
        i.e. if a node n is the ith child of p then the childindex of n is define as i.

        The array childindex is defined as follows:
        - for variable nodes, @-nodes and the root it's undefined (-1)
        - for a lambda-node different from the root it's the childindex of the lambda-node
    **)
    val childindex : int array

    (** he_by_root.(i) = true iif the graph node i is hereditarily enabled by the root. **)
    val he_by_root : bool array


    new (nnodes,nedges) as x =
                    {nodes=nnodes;
                     edges=nedges;
                     enabler=Array.create (Array.length nnodes) (-1);
                     span=Array.create (Array.length nnodes) (-1);
                     parameterindex=Array.create (Array.length nnodes) (-1);
                     childindex=Array.create (Array.length nnodes) (-1);
                     he_by_root=Array.create (Array.length nnodes) false }
                   then x.compute_nodesinfo()

    (** [graph_node_label_with_id index] returns the label of the [index]th node.
         The index of the node is added as a suffix to the label. **)
    member x.node_label_with_idsuffix index =
        let node = x.nodes.(index) in
        let nodeid = string_of_int index in
       (graph_node_label node)^" ["^
        (match x.nodes.(index) with
            NCntApp -> nodeid
          | NCntTm(tm) -> nodeid
          | NCntVar(x) -> nodeid
          | NCntAbs("",vars) -> nodeid
          | NCntAbs(nt,vars) -> nt^":"^nodeid)^"]"

    (** [children_count grnodeindex] returns the number of child nodes of a graph node identified by 
    its index [grnodeindex] **)
    member x.children_count grnodeindex =
        List.length x.edges.(grnodeindex)

    (** [arity grnodeindex] returns the arity of a graph node identified by 
    its index [grnodeindex] **)
    member x.arity grnodeindex =
        match x.nodes.(grnodeindex) with
        // Arity of a lambda node is the number of abstracted variables
        |NCntAbs(_, variables) -> List.length variables
        // Arity of an @ node is the number of _operand_ children
        |NCntApp -> (x.children_count grnodeindex) - 1
        // Arity of a variable node is the number of children
        |NCntVar(_) | NCntTm(_) -> x.children_count grnodeindex

    (** [nth_child grnodeindex n] returns the nth child of the graph node number grnodeindex
        where the child index [n] is given using the computation graph convention
        (starts at 0 for @-nodes and at 1 for variable nodes and lambda-nodes) **)
    member x.nth_child grnodeindex n =
        let childArrayIndex =
            match x.nodes.(grnodeindex) with
            |NCntAbs(_,_) -> assert(n=0); 1 // a lambda nodes has only one child
            |NCntApp -> n
            |NCntVar(_) | NCntTm(_) -> n-1
        in        
        List.item childArrayIndex x.edges.(grnodeindex)

    (** [compute_nodesinfo()] fills up the information arrays
        [x.enabler] [x.span] [x.bindingindex] and [x.binderchild] **)
    member this.compute_nodesinfo() =
        let nnodes = Array.length this.nodes in
        let marked = Array.create nnodes false in
        let rec depth_first_search curnodeid path =
            if not marked.(curnodeid) then
            begin
                marked.(curnodeid) <- true;
                (match this.nodes.(curnodeid) with
                     (* The current node is a variable: compute its span (p), paramindex(i) and binder child index(j) by following the path to the root until reaching its binder. *)
                     NCntVar(x) -> let rec List_tryFindIndex i = function
                                                                | [] -> None
                                                                | t::_ when t=x -> Some i
                                                                | _::q -> List_tryFindIndex (i+1) q

                                    in
                                    let rec look_for_binder_in_path p = function
                                         [] -> // No binder found: it is a free variable.
                                                this.enabler.(curnodeid) <- 0; // its enabler is the root
                                                this.span.(curnodeid) <- p-1  // its span is the distance from the root
                                       | (b,_)::q -> match this.nodes.(b) with
                                                        NCntAbs(_, vars) ->
                                                                (match List_tryFindIndex 1 vars with // 1 because numering of parameter variables starts at 1
                                                                // Is the lambda-node binding our variable?
                                                                | Some i ->
                                                                    this.parameterindex.(curnodeid) <- i;
                                                                    this.enabler.(curnodeid) <- b; // record it as the enabler
                                                                    this.span.(curnodeid) <- p; // and save the span.

                                                                // If this lambda nodes does not bind our variable
                                                                | None ->
                                                                    // then continue to search for a binder along the path to the root
                                                                    look_for_binder_in_path (p+1) q)

                                                        | _ -> look_for_binder_in_path (p+1) q
                                    in
                                    look_for_binder_in_path 1 path;
                                    // the node is h.e. by the root iff its enabler is
                                    this.he_by_root.(curnodeid) <- this.he_by_root.(this.enabler.(curnodeid));

                    | NCntTm(_) -> // treat constant as free variables
                            this.enabler.(curnodeid) <- 0;
                            this.span.(curnodeid) <- List.length path;
                            this.he_by_root.(curnodeid) <- true

                    | NCntAbs(_,_) -> (match path with
                                       // if the path is empty then we are processing the root
                                       | [] ->
                                           this.he_by_root.(curnodeid) <- true; // the root is h.enabled by itself
                                       |(p,j)::_ -> this.enabler.(curnodeid) <- p;
                                                    this.childindex.(curnodeid) <- j;
                                                    // the node is h.e. by the root iff its enabler is
                                                    this.he_by_root.(curnodeid) <- this.he_by_root.(p) ;
                                       )
                    | NCntApp -> ();

                );


                List.iteri (function childindex -> function childid ->
                                match this.nodes.(curnodeid) with
                                // for app node, child numbering starts at 0
                                | NCntApp -> depth_first_search childid ((curnodeid,childindex)::path)
                                // for other nodes, child numbering starts at 1
                                | _ -> depth_first_search childid ((curnodeid,childindex+1)::path)
                         )
                        this.edges.(curnodeid);
              end
        in
          if nnodes > 0 then
            (* Performs a depth-first browsing of the tree nodes *)
            depth_first_search 0 []

 end

(** [create_empy_graph()] creates an empty graph **)
let create_empty_graph() = new computation_graph([||],[||])

(** [lnfrules_to_graph lnfrules] converts rules in lnf into a computation graph.
    @param lnfrules the rules of the recursion scheme in LNF
    @return the compuation graph (nodes,edges)
**)
let lnfrules_to_graph lnfrules =

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
                              | None -> newnode (NCntAbs("", abs_part)) in
            let appnode_id = newnode (match app_part with
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
    new computation_graph(nodes,edges)
;;

(** [rs_lnfrules_to_graph rs_lnfrules] converts the rules of a recursion scheme into a computation graph.
    @param rs_lnfrules the rules of the recursion scheme in LNF
    @return the compuation graph
**)
let rs_lnfrules_to_graph rs_lnfrules =
    lnfrules_to_graph rs_lnfrules
;;

(** [lnf_to_graph lnf] converts a lnf into a computation graph.
    @param lnf a lnf term
    @return the compuation graph
**)
let lnf_to_graph lnf =
    lnfrules_to_graph ["",lnf]
;;