(** Higher-order collapsible pushdown automata (HO-CPDA) module *)
open Hog


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

(** The type of a Higher-Order Collapsible Pushdown Automoton.
   Formally a Ho-CPDA is given by a 5-tuple (Sigma, Gamma, Q, delta, q0)
   where - Sigma is a typed alphabet,
         - Gamma is a stack alphabet,
         - Q is a set of states,
         - delta is the transition function QxGamma -> (QxOp  + { (f;q_1, ... q_ar(f)) : f \in Sigma, q_i \in Q })
 *)
type hocpda = 
{   
  n : int;                             (** Order of the CPDA *)
  terminals_alphabet : alphabet;       (** This corresponds to Sigma in the formal definition *)
  stack_alphabet : stackelement list;  (** Gamma in the formal definition *)
  code : operation array;               (** The code of the CPDA stored as an array of instructions. *)
}

(** A generalized state is either a state (i.e. instruction pointer) 
    or a terminal together with a list of states for each parameter of
    the terminal.*)
type gen_state = State of state | TmState of terminal * state list;;

(** Type of a generalized configuration: (state,stk) where *)
(**   - current CPDA generalized state *)
(**   - current stack state *)
type gen_configuration = gen_state * hostack;;


(** [empty_hostack n] constructs an empty stack of order n for n>=1 *)
let rec empty_hostack = function 
    1 -> Stack([])
  | n -> Stack([empty_hostack (n-1)])
;;

let test = { n = 5;
	     terminals_alphabet = ["f",Gr];
	     stack_alphabet = ["e1";"e2"];
	     code = [|Push1("e1",(0,0)); GotoIfTop0(1,"e2"); Push1("e1",(0,0)); Collapse|];
};;

let conf = (0, Stack([Stack([Stack([Stack([Stack([El("e1",(0,0))])])])])]))
;;


exception EmptyHOStack;;
exception BadHOStackStructure;;

(** [top0 cpda stk] retrieves the top 0-element of the stack [stk]. *)
let top0 cpda stk =
  let rec aux k stk = 
    match k,stk with
	0,El(a,l) -> a,l
      | 0,_ | _,El(_) -> raise BadHOStackStructure
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(top::_) -> aux (k-1) top
  in 
    aux cpda.n stk
;;

(** [popn j cpda stk] performs an order j pop on the stack [stk], with j>=1. *)
let popn j cpda stk =
  if j = 0 then failwith "Instruction pop0 undefined!";
  let rec aux k stk = 
    match k,stk with
       0,_ | _,El(_) -> raise BadHOStackStructure
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(_::q) when k = j -> Stack(q)
      | _,Stack(top::q) -> Stack((aux (k-1) top)::q)
  in 
    aux cpda.n stk
;;

(** [pushj j cpda stk] performs an order j push where j>=2. *)
let pushj j cpda stk  =
  let rec aux k stk = 
    match k,stk with
       0,_ | _,El(_) -> raise BadHOStackStructure
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(t::q) when k = j -> Stack(t::(t::q))
      | _,Stack(top::q) -> Stack((aux (k-1) top)::q)
  in 
    aux cpda.n stk
;;

(** [push1 cpda stk a l] performs an order 1 push of the element 'a' with the link 'l'. *)
let push1 cpda stk a l =
  let rec aux k stk = 
    match k,stk with
       0,_ | _,El(_) -> raise BadHOStackStructure
      | 1,Stack(s) -> Stack(El(a,l)::s)
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(top::q) -> Stack((aux (k-1) top)::q)
  in 
    aux cpda.n stk
;;


(** [collapse cpda stk] performs the collapse operation. *)
let collapse cpda stk =
  let ret = ref stk in
  let a,(j,k) = top0 cpda stk in
  for i = 1 to k do
    ret := popn j cpda !ret;
  done;
  !ret
;;

exception CpdaHalt;;
exception JumpToInexistingCode;;

(** [mk_nextstate cpda ip] creates a state corresponding to the instruciton [ip+1]
    if it exists.
    @raise CpdaHalt if the end of the CPDA code is reached. *)
let mk_nextstate cpda ip = 
    if ip+1 >= Array.length cpda.code then
      raise CpdaHalt
    else
      State(ip+1)
;;

(** [mk_state cpda i] creates a state corresponding to the instruciton [i].
    @raise JumpToInexistingCode if i does not lie within the program code.
    @return State(i) if [i] is a valid instruction number *)
let mk_state cpda i =
    if i >= Array.length cpda.code then
      raise JumpToInexistingCode
    else
      State(i)
;;

(** [hopcpda_step cpda genconf] performs a transition of the cpda.
    @param cpda is the CPDA
    @param genconf is a generalized configuration of the CPDA.
    *)
let hocpda_step cpda genconf =
    match genconf with
    | TmState(_),_ -> failwith "The current configuration is a terminal! The CPDA cannot perform a transition until the opponent decides which parameter to follow."
    | State(ip),stk ->
      let execute = function
          Popn(n) -> (mk_nextstate cpda ip), (popn n cpda stk)
        | Push1(a,l) -> (mk_nextstate cpda ip), (push1 cpda stk a l) 
        | Pushn(j) -> (mk_nextstate cpda ip), (pushj j cpda stk);
        | Collapse -> (mk_nextstate cpda ip), (collapse cpda stk);
        | GotoIfTop0(q, t) -> if fst (top0 cpda stk) = t then 
                                (mk_state cpda q),stk
                              else 
                                (mk_nextstate cpda ip),stk
        | Goto(q) -> (mk_state cpda q),stk
        | Emit(t,ql) -> TmState(t,ql),stk
      in
        execute cpda.code.(ip)
;;

(** Create the initial configuration of a CPDA. *)
let hocpda_initconf cpda =
  State(0),(empty_hostack cpda.n);
;;


(** pretty-printer for ho-stacks *)
let rec string_of_stack stk =
    match stk with
        El(e,(j,k)) -> e^","^(string_of_int j)^","^(string_of_int k)
     | Stack(lst) -> "["^(String.concat "; " (List.map string_of_stack lst))^"]"
;; 

(** pretty-printer for generalized configuration *)
let string_of_genconfiguration = function
      State(ip), stk -> (string_of_int ip)^": " ^(string_of_stack stk)
    | TmState(f, lststates),stk -> f^"("^(String.concat "," (List.map string_of_int lststates))^"): "^(string_of_stack stk)
;;

(** pretty-printer for hocpda **)
let string_of_hocpda cpda = 
    let ret = ref "" in
    let string_of_instr = function
          Popn(n) -> "POP"^(string_of_int n)
        | Push1(a,(j,k)) -> "PUSH1 "^a^" ("^(string_of_int j)^","^(string_of_int k)^")"
        | Pushn(j) -> "PUSH"^(string_of_int j)
        | Collapse -> "COLLAPSE"
        | GotoIfTop0(q, t) -> "GOTO "^(string_of_int q)^" IF TOP1="^t
        | Goto(q) -> "GOTO "^(string_of_int q)
        | Emit(t,ql) -> "EMIT "^t^" "^(String.concat "," (List.map string_of_int ql));
    in
      for i = 0 to (Array.length cpda.code)-1 do
        ret := !ret^(string_of_int i)^":"^(string_of_instr cpda.code.(i))^"\n"
      done;
    "Order: "^(string_of_int cpda.n)
    ^"\n\nTerminals:\n"^(string_of_alphabet cpda.terminals_alphabet)
    ^"\nStack alphabet: "^(String.concat " " cpda.stack_alphabet)
    ^"\n\nCode:\n"^(!ret)
;;

hocpda_step test (hocpda_initconf test);; 






(** [hocpda_conf_step cpda conf opponent] does the same
    as [hocpda_conf_step cpda opponent] except that it
    first sets the cpda in the configuration [conf] before
    executing a step of the cpda.
    
    @param opponent is a function of type [terminal -> state list -> state option] modeling the (history-free) strategy of the opponent player.
    It is  called each time an operation emits a Sigma-constant f. The Opponent is then responsible of chosing one direction among
    1..ar(f), and does so by returning the corresponding state from the list passed in the second parameter.
    If and only f is of arity 0, opponent can return None.
    *)
    (*
let hocpda_conf_step cpda (q,stk) opponent =
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
    cpda.ip <- q;
    cpda.stk <- stk;
    execute cpda.code.(q)
;;


(** Define a strategy for the opponent that always choose the first parameter of
    the emitted terminal. *)
let opp_strategy a q =
    print_string ("CPDA emits terminal "^a);
    match q with 
        [] -> None
      | p1::_ -> print_string "Opponent choses "; print_int p1; Some(p1)
;;

*)



(** Type for higher-level instructions. 
    It is used for generating intermediate code containing labels. *)
type hli_labelident = string;;
type hlinstr =    HliLabel of hli_labelident
                | HliPopn of int
                | HliPush1 of stackelement * link 
                | HliPushn of int
                | HliCollapse
                | HliGoto of hli_labelident
                | HliGotoIfTop0 of hli_labelident * terminal
                | HliEmit of terminal * state list
                | HliNop;;

(** Convert higher-level code to low-level CPDA code.
    [hlicode_to_cpdacode hlicode]
    @param hlicode list of higher-level instructions
    @return an array of low-level instructions *)    
let hlicode_to_cpdacode hlicode =
    (* - size is the size of the code to be generated,
       - label2ip is an association list mapping labels to their corresponding instruction pointer *)
    let size,label2ip = List.fold_left (function (s,labmap) -> function
                                              HliLabel(label) -> s,(label,s)::labmap
                                            | HliPopn(_)
                                            | HliPush1(_,_)
                                            | HliPushn(_)
                                            | HliCollapse
                                            | HliGoto(_)
                                            | HliGotoIfTop0(_,_)
                                            | HliEmit(_,_) -> s+1,labmap
                                            | HliNop -> s,labmap
                                       ) (0,[]) hlicode in
    
    let code = Array.create size (Goto(0)) in  
    let curip = ref 0 in
    let gen_instruction = function 
          HliLabel(_) -> ();
        | HliPopn(i) -> code.(!curip) <- Popn(i); incr(curip);
        | HliPush1(a,l) -> code.(!curip) <- Push1(a,l); incr(curip);
        | HliPushn(n) -> code.(!curip) <- Pushn(n); incr(curip);
        | HliCollapse-> code.(!curip) <- Collapse; incr(curip);
        | HliGoto(label) -> code.(!curip) <- Goto(List.assoc label label2ip) ; incr(curip);
        | HliGotoIfTop0(label,f) -> code.(!curip) <- GotoIfTop0((List.assoc label label2ip), f) ; incr(curip);
        | HliEmit(t,slst) -> code.(!curip) <- Emit(t,slst); incr(curip);
        | HliNop -> ();
    in
    List.iter gen_instruction hlicode;
    code
;;
