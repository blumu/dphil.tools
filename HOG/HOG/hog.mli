(* Interface File *)
(* namespace Comlab *)

type typ = Gr | Ar of typ * typ ;;

val typeorder : typ -> int
val typearity : typ -> int

type ident = string;;
type alphabet = (ident * typ) list;;
type terminal = ident;;
type nonterminal = ident;;


(* applicative term *)
type appterm = Nt of nonterminal | Tm of terminal | Var of ident | App of appterm * appterm;;

type rule = nonterminal * ident list * appterm;;
type recscheme = { nonterminals : alphabet;
				   sigma : alphabet;
				   rules : rule list;
				   rs_path_validator : terminal list -> bool * string } ;;


val terminal_type : recscheme -> terminal -> typ
val get_parameter_type : recscheme -> ident -> typ

val string_of_alphabet : alphabet -> string
val string_of_appterm : appterm -> string
val string_of_rs : recscheme -> string
val appterm_operator_operands  : appterm -> appterm * appterm list
val step_reduce : recscheme -> appterm -> bool * appterm

val rs_check : recscheme -> string list


type lnfrhs = lnfabstractpart * lnfapplicativepart
and lnfabstractpart = ident list
and lnfapplicativepart = 
      LnfAppVar of ident * lnfrhs list
    | LnfAppNt of nonterminal * lnfrhs list
    | LnfAppTm of terminal * lnfrhs list
;;
type lnfrule = nonterminal * lnfrhs ;;

val lnf_to_string : recscheme -> lnfrule -> string
val rule_to_lnf : recscheme -> rule -> lnfrule * (ident*typ) list
val rs_to_lnf : recscheme -> (lnfrule list) * (ident*typ) list





(** Content of the node of the graph *)
type nodecontent = 
    NCntApp
  | NCntAbs of ident * ident list
  | NCntVar of ident
  | NCntTm of terminal
;;

(** The set of nodes is represented by an array of node contents *)
type cg_nodes = nodecontent array;;


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
val hors_to_graph : recscheme -> lnfrule list -> computation_graph

val hors_to_latexcompgraph : recscheme -> lnfrule list -> string

(** Validators *)
val default_validator : terminal list -> bool * string
val demiranda_validator :  terminal list -> bool * string
val reverse_demiranda_validator :  terminal list -> bool * string
