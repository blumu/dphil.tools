(** $Id$
    Description:    Common functions
    Author:         William Blum
**)
module Common
open FSharp.Compatibility.OCaml

// function to detect if the assembly is running on Mono
let IsRunningOnMono =
    System.Type.GetType ("Mono.Runtime") <> null;;

(** End-of-line sequence **)
let eol =
    (*IF-OCAML*)
     "\n"
    (*ENDIF-OCAML*)
    (*F#
    System.Environment.NewLine
     F#*) ;;

let string_of_char =
        String.make 1;;

(*IF-OCAML*)
let LAMBDA_SYMBOL = "\\";; (* Caml does not support sources in UTF-8 *)
(*ENDIF-OCAML*)
(*F#
let LAMBDA_SYMBOL = "λ";;
 F#*)


let array_fold_lefti (f: int -> 'a -> 'b -> 'a) (acc:'a) (arr:'b array): 'a =
  let i = ref 0 in
  Array.fold_left
    (fun acc x ->
      let res = f !i acc x in i := !i + 1; res)
    acc arr
;;

let rec list_tryfind f l =
    match l with
    | [] -> None
    | t::_ when f t -> Some t
    | _::q -> list_tryfind f q
;;

(** [array_map_filteri f a] is the canonical combination of Array.mapi and Array.filteri.
    @param f maps element of the list [a] to an optional value of a generic type
    @param l is the input array **)
let array_map_filteri f a =
    let aux i acc x =
        match f i x with
          None -> acc
        | Some(u) -> Array.concat [acc;[|u|]]
    in array_fold_lefti aux [||] a
;;

(** Combination of List.map and List.filter **)
let rec map_filter f = function
                        |[]  -> []
                        |t::q -> match f t with
                                 | None -> map_filter f q
                                 | Some(a) -> a::map_filter f q
;;


let Debug_print = ref print_string;;



(**  apply a function to an optional argument. Does nothing if the argument is none **)
let do_onsome f = function
                  | None -> ()
                  | Some(c) -> f c