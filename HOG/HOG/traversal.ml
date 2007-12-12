(** $Id: $
    Description: Traversals
    Author:      William Blum
**)
open Common
open Lnf


(** Type for the player **)
type Player = Opponent | Proponent

(** Type of the value leaves **)
type leaf_value = int
 
let string_of_value = string_of_int
 
(** Type of a generalized graph node i.e. a node of the computation graph with value-leaves attached to each node **)
type gen_node =   Internal of int  (* the index of a node of the computation graph *)
                 | ValueLeaf of int * leaf_value (* the index of the parent node in the computation graph
                                              and a value *)
(** Convert a generalized node to latex 
    @parem gr_nodes the array of nodes of the computaiton graph **)                                              
let gennode_to_latex gr_nodes = function
                           Internal(gr_inode) -> graphnodelabel_to_latex gr_nodes.(gr_inode)
                         | ValueLeaf(gr_inode,value) -> "{"^(string_of_value value)^"}_{"^(graph_node_label gr_nodes.(gr_inode))^"}"

(** Type of a traversal node **)
//type trav_node = gen_node * int

(** Traversal **)
//type traversal = trav_node array


                                              
(** Permutes player O and P **)
let player_permute = function Proponent -> Opponent | Opponent -> Proponent

(** Tell who plays a given node of the computatrion graph **)
let graphnode_player = function
                            NCntAbs(_,_) -> Opponent
                          | NCntVar(_) |NCntApp | NCntTm(_) -> Proponent
;;


(** Tells who plays a given generalized graph node
    @parem gr_nodes the array of nodes of the computation graph **)                                              
let gennode_player gr_nodes = function
      Internal(gr_i) -> graphnode_player gr_nodes.(gr_i)
    | ValueLeaf(gr_i,_) -> player_permute (graphnode_player gr_nodes.(gr_i))



(****** Sequence transformations ******)

/////////// O-view and P-view computation


(** [update_links_after_removing_a_section get_link update_link length suffix] 
    updates the links in a sequence of nodes-with-links to take into account the removal of a section of nodes from the sequence.
    @param get_link function that maps occurrences of the sequence [seq] to the length of their link
    @param update_link function that given an occurrences of the sequence [seq] and a link length
            returns the same node associated with the new link length
    @param length is the length of the removed section
    @param suffix is a list of nodes-with-links that immediately follow the section that is removed
    @return the sequence of node-with-links [seq] with updated links
     **)
let update_links_after_removing_a_section get_link update_link length suffix =
    List.mapi (fun i nd -> let link = get_link nd in
                           // no link?
                           if link = 0 then
                             nd
                           // dangling link?
                           else if link > i && link <= length then
                             failwith "Dangling link! This sequence does not respect visibility!"
                           // otherwise we update the link
                           else
                             update_link nd (link-length) 
                    ) suffix

(** [seq_Xview xplayer pos gr_nodes seq] computes the X-View where X is in {O,P}
     of the sequence of nodes-with-pointers [seq] at position [pos] 
    
    @param gr_nodes is the array of nodes of the computation graph 
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
let seq_Xview gr_nodes xplayer get_gennode get_link update_link seq pos = 
    let rec aux acc = function
          -1 -> acc
        | i ->  let gennode = get_gennode seq.(i) in
                let link = get_link seq.(i) in
                let player = gennode_player gr_nodes gennode in
                let nacc = seq.(i)::acc in
                if player = xplayer then
                    aux nacc (i-1) 
                else
                    if link = 0 then
                      nacc
                    else
                      aux (update_links_after_removing_a_section get_link
                                                                 update_link
                                                                 (link-1) // section length
                                                                 nacc)    // the part following the removed section
                          (i-link) 
    in Array.of_list (aux [] pos)
;;


/////////// Hereditary projection

(** Hereditary projection 
    @param gr_nodes is the array of nodes of the computation graph 
    @param get_link function that maps occurrences of the sequence [seq] to the length of their link
    @param update_link function that given an occurrences of the sequence [seq] and a link length
            returns the same node associated with the new link length
    @param seq is the input sequence of node-with-pointers (of generic type) 
    @param root is the index in [seq] of the root of the hereditary justification
    @return the subarray of [seq] consisting of the nodes that are hereditarily justified by [root]
**)
let heredproj gr_nodes getlink updatelink seq root =
  let n = Array.length seq in
  let p = n-root in
  let newindex = Array.create p (-1) in
  newindex.(0) <- 0; // the reference node is in the projection
  let k = ref 1 in
  for i = 1 to p-1 do
    // what is the length of the link in the original sequence?
    match getlink seq.(root+i) with
       // no link therefore not in the projection
       0 -> ()
       // link going beyond the reference node therefore not in the projection
     | l when l > i -> ()     
       // justifier not in the projection therefore it is not either
     | l when newindex.(i-l) = -1 -> ()
       // justifier in the projection: it must also be in
     | _ -> newindex.(i) <- !k;
            incr k
  done;
  array_map_filteri (fun i nd -> if i = 0 then
                                    Some(updatelink seq.(root+i) 0) // the root has no link anymore
                                 else
                                   match newindex.(i) with 
                                      -1 -> None
                                     | j -> Some(updatelink seq.(root+i) (j-newindex.(i-(getlink seq.(root+i))))))
                    newindex





(** Subterm projection with respect to some reference root node 

    @param gr_nodes is the array of nodes of the computation graph 
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
let subtermproj gr_nodes get_gennode getlink updatelink seq root =
  let n = Array.length seq in
  let p = n-root in
  let newindex = Array.create p (-1) in
  newindex.(0) <- 0; // the reference node is in the projection
  let k = ref 1 in
  for i = 1 to p-1 do
    let occ = seq.(root+i) in
    let link = getlink occ in
    match gennode_player gr_nodes (get_gennode occ) with 
       Proponent ->
         // It is a P-move therefore we keep the occurrence iff the immediately preceding occurrence was kept...
         if newindex.(i-1) <> -1 then
         begin
            newindex.(i) <- !k;
            incr k           
         end

      | Opponent ->
         // It is an O-move therefore we keep the occurrence iff its justifier was kept...
                 
            // if there the occurrence has no link then it is not in the projection
         if link = 0
            // if it has a link going beyond the reference node then it is not in the projection
         || link > i 
            // if its justifier is not in the projection then it is not in the projection
         || newindex.(i-link) = -1 then
            ()
         // otherwise its justifier is in the projection therefore it must also be in it
         else
           begin
             newindex.(i) <- !k;
             incr k
           end
  done;
  array_map_filteri (fun i nd -> let occi = seq.(root+i) in
                                 if i = 0 then
                                    Some(updatelink occi 0) // the root has no link anymore
                                 else
                                   match newindex.(i) with 
                                      -1 -> None // this nodes is not kept
                                     | newi -> // we keep this node
                                              let l = getlink occi in // link's length in the original sequence
                                              Some(updatelink   occi 
                                                                ( // is the pointer dangling after projection?
                                                                  if l > newi then 
                                                                    newi // yes so make it points to the root instead
                                                                  
                                                                  // the pointer is not dangling
                                                                  else
                                                                    // ...so we just need to update the link length 
                                                                    // to take into account that some nodes may have been removed
                                                                    newi-newindex.(i-l)
                                                                ))
                    )
                    newindex

