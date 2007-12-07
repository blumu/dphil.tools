(** $Id$
	Description: Common functions
	Author:		William Blum
**)

// function to detect if the assembly is running on Mono
let IsRunningOnMono = 
    System.Type.GetType ("Mono.Runtime") <> null;;
    
    
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
