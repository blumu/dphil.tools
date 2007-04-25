(* pour Eric, 25/04/2007 *)

let swap a i j =
	let t = a.(i) in
    a.(i)<-a.(j);
	a.(j) <- t;
;;

let reverse_subarray a d f =
  for i = 0 to (f-d+1)/2-1 do
    swap a (d+i) (f-i);
  done;
;;


let lexi_next p n =
  let i = ref (n-1) in
  while (!i>0) & (p.(!i-1)>= p.(!i)) do decr(i)  done;
  if !i = 0 then false
  else
  begin
	let lub = ref !i in
    for j = !i+1 to n-1 do
	   if p.(j) > p.(!i-1) & p.(j) <= p.(!lub) then lub := j;
    done;
	swap p !lub (!i-1);
(*	print_int (!i-1); print_newline();
	print_int !lub; print_newline();
	Array.iter print_int p;  print_newline();*)
	reverse_subarray  p !i (n-1);
	 true;
  end;
;;

(* 
let p = [|1;2;2;|] ;;
lexi_next p 3;;
*)

let enum_perm str =
  let print_perm p =
    Array.iter (fun i -> print_char (char_of_int i)) p;
	print_newline();
  in
  print_newline();
	let n = String.length str in
	let perm = Array.init n (fun i->int_of_char (String.get str i)) in
	Array.sort compare perm;
	let more = ref true in
	while !more do  
		print_perm perm;
		more := lexi_next perm n;
	done
;;

let _ = 
	print_string "Anagrammes de ";
	enum_perm (String.uppercase (read_line()))
;;

