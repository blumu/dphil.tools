let rec list_replace l i a = match l with
	  [] -> failwith "impossible"
	| t::q -> if i == 0 then t,a::q else 
		let b,qrep = list_replace  q (i-1) a in b,t::qrep
;;

let rec permutation = function
   [] -> [[]]
 | a::q ->
	let res = ref (List.map (fun l -> a::l) (permutation q)) in
	for i = 0 to (List.length q)-1 do
		let (b,qp) = list_replace q i a in
		res := !res@(List.map (fun u -> b::u) (permutation qp)) 
	done; !res
;;

let list_of_string str =
  let n = String.length str in
  let rec aux i =
    if n = i then []
    else (String.get str i)::(aux (i+1))
  in aux 0
;;

let main() = 
List.iter (fun s -> (List.iter print_char s); print_newline();)
		(permutation (list_of_string (read_line())));;


		
		
	