(** Description: This module implements the basic operations on traversals (P-view, O-view, projection, extension, ...).
    Author:  William Blum
**)
module Traversal

open FSharp.Compatibility.OCaml
open Common
open Lnf
open Compgraph


(** Traversal nodes used to represent node occurrences in traversals, given some contextual computation graph.

    The nodes encoded in type `computation_graph only accounts for "structural nodes" of the computation graph.
    Those are the nodes that more or less correspond to syntactic tokens in the abstract syntax tree representation
    of a term, recursion scheme or program.

    Traversal nodes provide a more generic definition used to establish the duality with game semantics
    (e.g. answer moves, eta-expanded nodes). In addition to structural nodes, traversal nodes can also be
      - value leaves -representing values returned by various part of the term or program, such as PCF interpreted constants),
      - or "ghost nodes" - representing fictious term resulting from (possibly many) eta-expansions of some sub-term.

    Traversal nodes are always defined in the context of some `computation_graph`. In the definition below,
    the graph node indices refer to nodes of some fixed contextual computation graph of type `computation_graph`.

***)
module TraversalNode = struct

    (** Type of the value leaves. For simplicity we just encode them as integers **)
    type leaf_value = int

    (** Convert a value-leaf into a string **)
    let string_of_value = string_of_int

    (** Type of a generalized graph node i.e. a node of the full computation graph with value-leaves attached to each node **)
    type gen_node =
        (* a custom node that is not in the computation graph *)
        | Custom
        (* a custom ghost lambda node (i.e. not in the original computation graph) labelled by an integer *)
        | GhostLambda of int
        (* a custom ghost variable node (i.e. not in the original computation graph) labelled by an integer *)
        | GhostVariable of int
        (* the index of a node of the computation graph *)
        | StructuralNode of int
        (* a value leaf given by the index of the parent node in the computation graph and the value *)
        | ValueLeaf of int * leaf_value

    (** [is_app_or_constant gennd]
        @return true iff [gennd] is the gennode is a @-node, a constant node or a leaf of a @/constant node
    **)
    let is_app_or_constant (graph:computation_graph) = function
        | Custom
        | GhostLambda _
        | GhostVariable _ -> false
        | StructuralNode(gr_i) | ValueLeaf(gr_i,_) ->
            match graph.nodes.(gr_i) with
            | NCntAbs(_,_) | NCntVar(_) -> false
            | NCntApp | NCntTm(_) -> true

    (** [is_lambda gennd]
        @return true iff [gennd] is a lambda node
    **)
    let is_lambda (graph:computation_graph) = function
        | Custom
        | GhostVariable _ 
        | ValueLeaf(_,_) -> false
        | GhostLambda _ -> true
        | StructuralNode(gr_i)  ->
            match graph.nodes.(gr_i) with
            | NCntAbs(_,_) -> true
            | NCntVar(_) | NCntApp | NCntTm(_) -> false

    (** [is_variable gennd]
        @return true iff [gennd] is a variable node
    **)
    let is_variable (graph:computation_graph) = function
        | Custom
        | ValueLeaf(_,_)
        | GhostLambda _ -> false
        | GhostVariable _ -> true
        | StructuralNode(gr_i) ->
            match graph.nodes.(gr_i) with
            | NCntVar(_) -> true
            | NCntAbs(_,_) | NCntApp | NCntTm(_) -> false

    (** [arity gennd]
        @return the arity of the generalize node [gennd]
    **)
    let arity (graph:computation_graph) = function
        | Custom
        | ValueLeaf(_,_)
        | GhostLambda _
        | GhostVariable _ -> 0
        | StructuralNode(gr_i) ->
            graph.arity gr_i

    (** Convert a generalized node to latex. **)
    let toLatex (graph:computation_graph) = function
        | Custom -> "?"
        | GhostLambda i -> "{\ghostlmd^{"^(string_of_int i)^"}}"
        | GhostVariable i -> "{\ghostvar^{"^(string_of_int i)^"}}"
        | StructuralNode(gr_inode) -> graphnodelabel_to_latex graph.nodes.(gr_inode)
        | ValueLeaf(gr_inode,value) -> "{"^(string_of_value value)^"}_{"^(graph_node_label graph.nodes.(gr_inode))^"}"

    (** [gennode_player gennode] Tells who plays a given generalized graph node.
        @param gennode the generalized graph node
         **)
    let toPlayer (graph:computation_graph) = function
        | Custom -> Opponent
        | GhostLambda _ -> Opponent
        | GhostVariable _ -> Proponent
        | StructuralNode(gr_i) -> graphnode_player graph.nodes.(gr_i)
        | ValueLeaf(gr_i,_) -> player_permute (graphnode_player graph.nodes.(gr_i))

end

(** Traversal **)
(* TODO: consider using the following type to pass traversal around as argument to the function below.
type traversal =
    {
        get_gennode : int -> TraversalNode.gen_node;
        get_link : int -> int;
        update_link : int -> unit
    }
*)

(****** Sequence transformations ******)

(**** O-view and P-view computation ****)


(** [update_links_after_removing_a_section get_link update_link length suffix]
    updates the links in a sequence of nodes-with-links to take into account the removal of a section of nodes from the sequence.
    @param get_link function that maps occurrences of the sequence [seq] to the length of their link
    @param update_link function that given an occurrences of the sequence [seq] and a link length
            returns the same node associated with the new link length
    @param length is the length of the removed section
    @param suffix is a list of nodes-with-links that immediately follow the section that is removed
    @return the suffix sequence with updated links.
     **)
let update_links_after_removing_a_section get_link update_link length suffix =
    List.mapi (fun i nd -> let link = get_link nd in
                           (* no link? *)
                           if link <= i then
                             nd
                           (* dangling link? *)
                           else if link > i && link <= i-length then
                             failwith "Dangling link! This sequence does not respect visibility!"
                           (* otherwise we update the link *)
                           else
                             update_link nd (link-length);
                    ) suffix

(** [seq_Xview xplayer pos gr_nodes seq] computes the X-View where X is in {O,P}
     of the sequence of nodes-with-pointers [seq] at position [pos]

    @param gr is the computation graph
    @param xplayer is the player reference: if xplayer=Proponent the P-view is computed,
           otherwise it is the O-view
    @param get_gennode function that maps occurrences of the sequence [seq] to their corresponding generalized node in the computation graph
    @param get_link function that maps occurrences of the sequence [seq] to the length of their link
    @param update_link function that given an occurrences of the sequence [seq] and a link length
            returns the same node associated with the new link length
    @param seq is the input sequence of node-with-pointers (of generic type)
    @param pos is the position in [seq] where to start the view computation
    @return the subarray of [seq] corresponding to its P-view/O-view
      **)
let seq_Xview (gr:computation_graph) xplayer get_gennode get_link update_link seq pos =
    let rec aux acc = function
          -1 -> acc
        | i ->  let gennode = get_gennode seq.(i) in
                let link = get_link seq.(i) in
                let player = TraversalNode.toPlayer gr gennode in
                let nacc = seq.(i)::acc in
                if player = xplayer then
                    aux nacc (i-1)
                else
                    if link = 0 then
                      nacc
                    else
                      aux (update_links_after_removing_a_section get_link
                                                                 update_link
                                                                 (link-1) (* section length *)
                                                                 nacc)    (* the part following the removed section *)
                          (i-link)
    in Array.of_list (aux [] pos)
;;

(** [seq_Xview_ilast gr xplayer get_gennode get_link update_link seq initpos nsteps]
    returns the index in the original sequence of the occurrence appearing at the nsteps^th last position in
    the X-view at initpos.
    This is done by computing nsteps steps of the P-view at initpos.

    Remark: This function can be used to find the binder of a node in the X-view
    PROVIDED that the computation graph is CYCLE FREE . **)
let seq_Xview_ilast (gr:computation_graph) xplayer get_gennode get_link update_link seq initpos =
    let rec aux cur = function
                      | nsteps when nsteps <= 0 -> cur
                      | _ when cur = 0 -> failwith "seq_Xview_ilast: The X-view has less than 'nsteps' occurrences!"
                      | nsteps when TraversalNode.toPlayer gr (get_gennode seq.(cur)) = xplayer -> aux (cur-1) (nsteps-1)
                      | nsteps -> let link = get_link seq.(cur) in
                             if link = 0 then
                               failwith "seq_Xview_ilast: The X-view has less than 'nsteps' occurrences!"
                             else
                               aux (cur-link) (nsteps-1)
    in aux initpos
;;

(** [seq_find_lastocc_in_Xview gr xplayer get_gennode get_link update_link seq initpos graphnode]
    returns the index in the original sequence of the last occurrence in the X-view of the graph node 'graphnode'.
    Remark: This function can be used to find the binder of a node in the X-view. **)
let seq_find_lastocc_in_Xview (gr:computation_graph) xplayer get_gennode get_link update_link seq initpos graphnode =
    let rec aux = function
                 | -1 -> failwith "seq_find_lastocc_in_Xview: Occurrence not found!"
                 | cur ->   if get_gennode seq.(cur) = graphnode then
                                cur
                            else if TraversalNode.toPlayer gr (get_gennode seq.(cur)) = xplayer then
                                aux (cur-1)
                            else
                                let link = get_link seq.(cur) in
                                if link = 0 then
                                   failwith "seq_find_lastocc_in_Xview: Occurrence not found!"
                                else
                                   aux (cur-link)
    in aux initpos
;;

(** [seq_occs_in_Xview gr xplayer get_gennode get_link update_link seq pos]
    returns the list of occurrences that are in the X-view. **)
let seq_occs_in_Xview (gr:computation_graph) xplayer get_gennode get_link update_link seq =
    let rec aux acc = function
                      | 0 -> 0::acc
                      | j when j < 0 -> acc
                      | cur when TraversalNode.toPlayer gr (get_gennode seq.(cur)) = xplayer -> aux (cur::acc) (cur-1)
                      | cur -> let link = get_link seq.(cur) in
                               if link = 0 then
                                 cur::acc
                               else
                                 aux (cur::acc) (cur-link)
    in aux []
;;

(*** Hereditary projection ***)

(** Hereditary projection
    @param get_link function that maps occurrences of the sequence [seq] to the length of their link
    @param update_link function that given an occurrences of the sequence [seq] and a link length
            returns the same node associated with the new link length
    @param seq is the input sequence of node-with-pointers (of generic type)
    @param root is the index in [seq] of the reference occurrence of the root of the hereditary justification
    @return the subarray of [seq] consisting of the nodes that are hereditarily justified by [root]
**)
let heredproj getlink updatelink seq root =
  let n = Array.length seq in
  (* calculate the new position of the occurrences following the reference node *)
  if root < 0 || root  >= n then // check that the root occurrence is valid
      [||]
  else
      let p = n-root in
      let newindex = Array.create p (-1) in
      newindex.(0) <- 0; (* the reference node is in the projection *)
      let k = ref 1 in
      for i = 1 to p-1 do
        (* what is the length of the link in the original sequence? *)
        match getlink seq.(root+i) with
           (* no link therefore not in the projection *)
           0 -> ()
           (* link going beyond the reference node therefore not in the projection *)
         | l when l > i -> ()
           (* justifier not in the projection therefore it is not either *)
         | l when newindex.(i-l) = -1 -> ()
           (* justifier in the projection: it must also be in *)
         | _ -> newindex.(i) <- !k;
                incr k
      done;
      (* filter the subsequence starting at the root *)
      array_map_filteri (fun i _ -> if i = 0 then
                                        Some(updatelink seq.(root+i) 0) (* the root has no link anymore *)
                                     else
                                       match newindex.(i) with
                                          -1 -> None
                                         | j -> Some(updatelink seq.(root+i) (j-newindex.(i-(getlink seq.(root+i))))))
                        newindex

(** Determine if an occurrence in a traversal is hereditarily justified by another occurrence *)
let is_hereditarily_justified getlink seq source_occurrence reference_occurrence =
  let rec follow_link_from occ =
    if occ = reference_occurrence then
        true
    else
        match getlink seq.(occ) with
        (* no link therefore not in the projection *)
        | 0 -> false
        (* link goes beyond the reference occurrence *)
        | l when occ - l < reference_occurrence -> false
        (* justifier not in the projection therefore it is not either *)
        | l -> follow_link_from (occ - l)
  in
  follow_link_from source_occurrence

(** Subterm projection with respect to some reference root node

    @param gr is the computation graph
    @param get_gennode function that maps occurrences of the sequence [seq] to their corresponding generalized node in the computation graph
    @param get_link function that maps occurrences of the sequence [seq] to the length of their link
    @param update_link function that given an occurrences of the sequence [seq] and a link length
            returns the same node associated with the new link length
    @param seq is the input sequence of node-with-pointers (of generic type)
    @param root is the index in [seq] of the root of the subterm that we want to project on.
    @return the subarray of [seq] consisting of the nodes that are hereditarily justified by [root]

    @remark: [root] must be the occurrence index of a lambda-node (Opponent)
    although this transformation is also well-defined if [root] is a Proponent node.
**)
let subtermproj (gr:computation_graph) get_gennode getlink updatelink seq root =
  let n = Array.length seq in

  if root < 0 || root  >= n then // check that the root occurrence is valid
      [||]
  else
      (* 1 - Calculate the occurence positions that are preserved. *)
      let p = n-root in
      let newindex = Array.create p (-1) in
      newindex.(0) <- 0; (* the reference node is in the projection *)
      let k = ref 1 in
      for i = 1 to p-1 do
        let occ = seq.(root+i) in
        let link = getlink occ in
        match TraversalNode.toPlayer gr (get_gennode occ) with
           Proponent ->
             (* It is a P-move therefore we keep the occurrence iff the immediately preceding occurrence was kept... *)
             if newindex.(i-1) <> -1 then
             begin
                newindex.(i) <- !k;
                incr k
             end

          | Opponent ->
             (* It is an O-move therefore we keep the occurrence iff its justifier was kept... *)

               (* if there the occurrence has no link then it is not in the projection *)
             if link = 0
               (* if it has a link going beyond the reference node then it is not in the projection *)
               || link > i
               (* if its justifier is not in the projection then it is not in the projection *)
               || newindex.(i-link) = -1 then
                ()
             (* otherwise its justifier is in the projection therefore it must also be in it *)
             else
               begin
                 newindex.(i) <- !k;
                 incr k
               end
      done;
      (* 2 - Removes the nodes and update the links *)
      array_map_filteri (fun i _ -> let occi = seq.(root+i) in
                                     if i = 0 then
                                        Some(updatelink occi 0) (* the root has no link in the projection. *)
                                     else
                                       match newindex.(i) with
                                          -1 -> None (* this nodes is not kept *)
                                         | newi -> (* we keep this node *)
                                                  let l = getlink occi in (* link's length in the original sequence *)
                                                  Some(updatelink   occi
                                                                    ( (* is the pointer dangling after taking the projection? *)
                                                                      if i < l || newindex.(i-l) = -1 then
                                                                        newi (* yes so make it points to the root instead *)

                                                                      (* the pointer is not dangling *)
                                                                      else
                                                                        (* ...so we just need to update the link length
                                                                          to take into account the fact that some nodes may have been removed *)
                                                                        newi-newindex.(i-l)
                                                                    ))
                        )
                        newindex

(*** Star and extension ***)

(** Traversal star: remove the @ and constant nodes and adjust the pointers appropriately.

    @param gr is the computation graph
    @param get_gennode function that maps occurrences of the sequence [seq] to their corresponding generalized node in the computation graph
    @param get_link function that maps occurrences of the sequence [seq] to the length of their link
    @param update_link function that given an occurrences of the sequence [seq] and a link length
            returns the same node associated with the new link length
    @param seq is the input sequence of node-with-pointers (of generic type)
    @param pos is the position from which we start to compute the star operation
**)
let star (gr:computation_graph) get_gennode getlink updatelink seq pos =
  if pos < 0 then [||]
  else
      (* array mapping old index to new index. Cell containing -1 correspond to node that must removed from the sequence [seq]. *)
      let newindex = Array.create (pos+1) (-1) in

      (* number of nodes that have not been removed *)
      let n_notremoved = ref 0 in

      (* 1 - Calculate the occurence positions in [occ] that are preserved. *)
      for i = 0 to pos do
        if TraversalNode.is_app_or_constant gr (get_gennode seq.(i)) then
            newindex.(i) <- -1
        else
          begin
            newindex.(i) <- !n_notremoved;
            incr n_notremoved
          end;
      done;

      (* 2 - Removes the nodes and update the links *)
      array_map_filteri (fun i -> function
                                  | -1 -> None (* we do not keep this node *)
                                  | newindexi -> (* we keep the node *)
                                      let occi = seq.(i) in
                                      let j = i - (getlink occi) in (* justifier index *)
                                      (* if the justifier is removed then make it point to the immediate predecessor of the justifier *)
                                      if newindex.(j) = -1 then
                                        Some(updatelink occi (newindexi-newindex.(j-1)))
                                      else
                                        Some(updatelink occi (newindexi-newindex.(j)))
                        )
                        newindex

;;

(** Traversal extension: add a dummy initial node at the beginning of the traversal and
    make all occurences of @/constant-nodes point to their predecessor.

    @param gr is the computation graph
    @param get_gennode function that maps occurrences of the sequence [seq] to their corresponding generalized node in the computation graph
    @param get_link function that maps occurrences of the sequence [seq] to the length of their link
    @param update_link function that given an occurrences of the sequence [seq] and a link length
            returns the same node associated with the new link length
    @param createdummy function that creates a dummy node that does not correspond to any node of the computation tree.
    It takes a label and a link as parameter and is of type [string -> int -> 'a] where 'a is the type of the nodes of [seq].
    @param seq is the input sequence of node-with-pointers (of generic type)
    @param pos is the position from which we start to compute the extension operation
**)
let extension (gr:computation_graph) get_gennode getlink updatelink createdummy seq pos =
  (* append a dummy node in front of the sequence. *)
  let ext = Array.init (pos+2) (function  0 -> createdummy "\diamond" 0 | i -> seq.(i-1)) in
  (* make the @/constant-nodes point to their predecessor. *)
  for i = 1 to pos+1 do
    if TraversalNode.is_app_or_constant gr (get_gennode ext.(i)) then
      ext.(i) <- updatelink ext.(i) 1
  done;
  ext
;;

(** Implement OCaml's List.filter_map missing from F# *)
let filter_map l = List.fold_right (fun h q -> match h with Some x -> x::q | None -> q) l [] ;;

(** Calculate the arity threshold (see paper "On-the-fly eta-expansion for traversing and normalizing terms" [Blum 2017] 
    This value defines the maxium number of children of a ghost lambda node that is necessary to be visited
    through eta-expansion in order to fully normalize a term.
    (Recall: Children nodes of lambda nodes are numbered from 1 onwards.)

    arth(t) = max |l|-|n|
              where (l,n) ranges over { l in N_lambda, N in N_var, and l and n occur consecutively in t after the justifier of the last occurrence in t }

    @param gr is the computation graph
    @param get_gennode function that maps occurrences of the sequence [seq] to their corresponding generalized node in the computation graph
    @param get_link function that maps occurrences of the sequence [seq] to the length of their link
    @param update_link function that given an occurrences of the sequence [seq] and a link length
            returns the same node associated with the new link length
**)
let aritythreshold (gr:computation_graph) get_gennode getlink updatelink seq =
    let n = Array.length seq in
    let occ = n-1 in
    let link = getlink seq.(occ) in
    if link <=1 then 
        0
    else
        let jp = occ - link in
        let between_last_node_and_its_justifier =
            filter_map
                (List.mapi
                    (fun l lnode ->
                        let n = l + 1 in
                        if l >= jp && n <= occ-1 then
                            let nnode = seq.(n) in
                            if TraversalNode.is_lambda gr (get_gennode lnode)
                            && TraversalNode.is_variable gr (get_gennode nnode) then
                                Some (get_gennode lnode, get_gennode nnode)
                            else
                                None
                        else
                            None)
                    (Array.to_list seq)) in

        let max_list = List.fold_left max 0 in
        let diff (l,n) = (TraversalNode.arity gr l) - (TraversalNode.arity gr n) in

        max_list (List.map diff between_last_node_and_its_justifier)
