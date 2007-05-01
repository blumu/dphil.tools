(** Higher-order collapsible pushdown automata (HO-CPDA) module *)
open Hog;;


type state = int

type ident = string
type terminal = ident

type stackelement = ident
type link = int  * int (* A link is specified by a pair j,k. The stack pointed to by this link is obtained
                          by perfoming k iterations of the 'pop_j' operation *)
type operation = 
      Popn of int
    (** Popn(n) removes the top (n-1)-stack from the stack. *)

    | Push1 of stackelement * link 
    (** Push1(a,l) push the element 'a' with the associated link 'l' onto the top 1-stack. *)
    
    | Pushn of int
    (** Pushn(n+1) where n>=1 duplicates the top n-stack (within the top (n+1)-stack). *)
    
    | Collapse
    (** Collapse the stack to the prefix stack determined by the pointer associated to the top 0-element. *)
    
    | Goto of state
    (** Goto(q) will jump to instruction 'q' *)
    
    | GotoIfTop0 of state * terminal
    (** GotoIfTop0(q,f) will jump to instruction 'q' if the top 0-element in the stack is 'f' *)
    
    | Emit of terminal * state list
     (** Emit(f,[q_1;  ;q_ar(f)]) will emit the non-terminal f and then perform a Goto to 
        one of the state q_1 ... q_ar(f) chosen non-deterministically. 
     *)

type hostack = El of stackelement * link | Stack of hostack list

(** The type of a higher-order collapsible pushdown automoton *)
type hocpda = 
{   
  n : int;                             (** Order of the CPDA *)
  terminals_alphabet : Hog.alphabet;   (** This corresponds to Sigma in the formal definition *)
  stack_alphabet : stackelement list;  (** Gamma in the formal definition *)
  code: operation array;               (** The code of the CPDA stored as an array of instructions. *)
  
  (* Current configuration: *)
  mutable ip: int;                     (** Current CPDA state (i.e. instruction pointer) *)
  mutable stk: hostack;                (** Current stack state *)
}

(** [empty_hostack n] constructs an empty stack of order n *)
let rec empty_hostack = function 
    0 -> Stack([])
  | n -> Stack([empty_hostack (n-1)])
;;

let test = { n = 5;
	     terminals_alphabet = ["f",Gr];
	     stack_alphabet = ["e1";"e2"];
	     code = [|Push1("e1",(0,0)); GotoIfTop0(1,"e2"); Push1("e1",(0,0)); Collapse|];
	     ip = 0;
	     stk = Stack([Stack([Stack([Stack([Stack([El("e1",(0,0))])])])])]); (* empty_hostack 5;*)
};;

exception EmptyHOStack;;
exception BadHOStackStructure;;

(** Retrieve the top 0-element. *)
let top0 cpda =
  let rec aux k stk = 
    match k,stk with
	0,El(a,l) -> a,l
      | 0,_ | _,El(_) -> raise BadHOStackStructure
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(top::_) -> aux (k-1) top
  in 
    aux cpda.n cpda.stk
;;

(** [popn cpda n] performs an order n pop on the stack, with n>=1. *)
let popn cpda n =
  if n = 0 then failwith "Instruction pop0 undefined!";
  let rec aux k stk = 
    match k,stk with
       0,_ | _,El(_) -> raise BadHOStackStructure
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(_::q) when k = n -> Stack(q)
      | _,Stack(top::q) -> Stack((aux (k-1) top)::q)
  in 
    cpda.stk <- aux cpda.n cpda.stk
;;

(** Perform an order j push where j>=2. *)
let pushj cpda j =
  let rec aux k stk = 
    match k,stk with
       0,_ | _,El(_) -> raise BadHOStackStructure
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(t::q) when k = j -> Stack(t::(t::q))
      | _,Stack(top::q) -> Stack((aux (k-1) top)::q)
  in 
    cpda.stk <- aux cpda.n cpda.stk
;;

(** [push1 cpda (a,l)] performs an order 1 push of the element 'a' with the link 'l'. *)
let push1 cpda a l =
  let rec aux k stk = 
    match k,stk with
       0,_ | _,El(_) -> raise BadHOStackStructure
      | 1,Stack(s) -> Stack(El(a,l)::s)
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(top::q) -> Stack((aux (k-1) top)::q)
  in 
    cpda.stk <- aux cpda.n cpda.stk
;;


(** [collapse cpda] performs the collapse operation. *)
let collapse cpda =
  let a,(j,k) = top0 cpda in
  for i = 1 to k do
    popn cpda j;
  done
;;

exception CpdaHalt;;
exception JumpToInexistingCode;;

(** Increment the instruction pointer. If the end of the code is reached then
raise the exception CpdaHalt. *)
let incr_ip cpda =
    cpda.ip <- cpda.ip +1;
    if cpda.ip = Array.length cpda.code then
      raise CpdaHalt
;;

(** [jump cpda i] performs a jump to the instruction i (set cpda.ip to i). 
    If i does not lie within the program code then raise the exception
    JumpToInexistingCode *)
let jmp cpda i =    
    if i < (Array.length cpda.code) then
      raise JumpToInexistingCode
    else
      cpda.ip <- i;
;;

(** [hopcpda_step cpda opponent] performs a transition of the cpda.
    @param cpda is the CPDA
    @param opponent is a function of type [terminal -> state list -> state option] modeling the (history-free) strategy of the opponent player.
    It is  called each time an operation emits a Sigma-constant f. The Opponent is then responsible of chosing one direction among
    1..ar(f), and does so by returning the corresponding state from the list passed in the second parameter.
    If and only f is of arity 0, opponent can return None.
    *)
let hocpda_step cpda opponent =
  let execute = function
      Popn(n) -> popn cpda n; incr_ip cpda;
    | Push1(a,l) -> push1 cpda a l; incr_ip cpda;
    | Pushn(j) -> pushj cpda j; incr_ip cpda;
    | Collapse -> collapse cpda; incr_ip cpda;
    | GotoIfTop0(q, t) -> if fst (top0 cpda) = t then 
                            jmp cpda q
                          else 
                            incr_ip cpda;
    | Goto(q) -> jmp cpda q;
    | Emit(t,ql) -> match opponent t ql with 
                        None -> raise CpdaHalt
                      | Some(i) -> jmp cpda i;
  in
    execute cpda.code.(cpda.ip)
;;

(** Initialization of a CPDA. *)
let hocpda_init cpda =
  cpda.stk <- empty_hostack cpda.n;
  cpda.ip <- 0;
;;

(** Define a strategy for the opponent that always choose the first parameter of
    the emitted terminal. *)
let opp_strategy a q =
    print_string ("CPDA emits terminal "^a);
    match q with 
        [] -> None
      | p1::_ -> print_string "Opponent choses "; print_int p1; Some(p1)
;;

hocpda_step test opp_strategy;; 
