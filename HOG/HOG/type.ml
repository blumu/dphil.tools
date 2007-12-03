(** $Id$
	Description: Structures for encoding simple-types
	Author:		 William Blum
**)

type typ = Gr | Ar of typ * typ ;;

(* order and arity of a type *)
let rec typeorder = function  Gr ->  0 | Ar(a,b) -> max (1+ typeorder a) (typeorder b) ;;
let rec typearity = function  Gr ->  0 | Ar(_,b) -> 1+ (typearity b) ;;


let rec string_of_type = function
    Gr -> "o"
  | Ar(Ar(_) as a, b) -> "("^(string_of_type a)^") -> "^(string_of_type b)
  | Ar(a,b) -> (string_of_type a)^" -> "^(string_of_type b)
;;

let string_of_alphabet_aux sep a =
	let string_of_letter (l,t) =   
	  l^":"^(string_of_type t) ;
	in
	(* List.fold_left (function acc -> function l -> acc^(string_of_letter l)) "" a *)
	String.concat sep (List.map string_of_letter a) 
;;

let string_of_alphabet a = (string_of_alphabet_aux "\n" a)^"\n";;

let print_alphabet a = print_string (string_of_alphabet a);;

