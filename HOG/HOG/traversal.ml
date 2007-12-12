(** $Id: $
    Description: Traversals
    Author:      William Blum
**)
open Lnf

//type trav_node_tag = int (* index of the node in the computation graph *)

//type trav_node = { tag: trav_node_tag; label: string; mutable link:int }

//type traversal = trav_node array

(** Type for the player **)
type Player = Opponent | Proponent

(** Type of the value leaves **)
type leaf_value = int
 
let string_of_value = string_of_int
 
(** Type for a node of the traversal **)
type trav_node =  Internal of int  (* the index of a node of the computation graph *)
                | ValueLeaf of int * leaf_value (* the index of the parent node in the computation graph
                                              and a value *)
(** Convert a traversal node to latex 
    @parem gr_nodes the array of nodes of the computaiton graph **)                                              
let travnode_to_latex gr_nodes = function
                           Internal(gr_inode) -> graphnodelabel_to_latex gr_nodes.(gr_inode)
                         | ValueLeaf(gr_inode,value) -> "{"^(string_of_value value)^"}_{"^(graph_node_label gr_nodes.(gr_inode))^"}"

                                              
(** Permutes player O and P **)
let player_permute = function Proponent -> Opponent | Opponent -> Proponent

(** Tell who plays a given node of the computatrion graph **)
let graphnode_player = function
                            NCntAbs(_,_) -> Opponent
                          | NCntVar(_) |NCntApp | NCntTm(_) -> Proponent
;;


(** Tell who plays a given traversal node
    @parem gr_nodes the array of nodes of the computaiton graph **)                                              
let travnode_player gr_nodes = function
      Internal(gr_i) -> graphnode_player gr_nodes.(gr_i)
    | ValueLeaf(gr_i,_) -> player_permute (graphnode_player gr_nodes.(gr_i))
