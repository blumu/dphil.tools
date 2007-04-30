(* Higher-order collapsible pushdown automata (HO-CPDA) module *)



type state = int

type ident = string
type terminal = ident
type link = int  * int (* A link is specified by a pair n,k. The stack pointed to by this link is obtained
                          by perfoming k iterations of the 'pop_n' operation *)
type operation = 
      Popn of int
    (* Popn(n) removes the top (n-1)-stack from the stack. *)

    | Push1 of terminal * link 
    (* Push1(a,l) push the element 'a' with the associated link 'l' onto the top 1-stack. *)
    
    | Pushn of int
    (* Pushn(n+1) where n>=1 duplicates the top n-stack (within the top (n+1)-stack). *)
    
    | Collapse
    (* Collapse the stack to the prefix stack determined by the pointer associated to the top 0-element. *)
    
    | GotoIf of state * terminal
    (* GotoIf(q,f) will jump to the state 'q' if the top 0-element in the stack is 'f' *)
    
    | Emit of terminal * state list
     (* Emit(f,[q_1;  ;q_ar(f)]) will emit the non-terminal f and then perform a Goto to 
        one of the state q_1 ... q_ar(f) chosen non-deterministically. 
     *)


type stackelement = ident

type stack = El of stackelement | Stack of stack list

type hostack = ident list

type hocpda = 
{   terminals_alphabet : Hog.alphabet;  (* This corresponds to Sigma in the formal definition *)
    stack_alphabet : stackelement list; (* Gamma in the formal definition *)
    nstates: int;                       (* Number of states *)
    transitions: (operation list) array; (* an array of length nstates. Each cell contains the list of instructions that
                                           will be executed in the corresponding state. *)
                                       
    stack: hostack;                     (* current state of the stack *)
}


(* [hopcpda_step cpda opponent] perform a transition of the cpda
    @param cpda is the cpda 
    @param opponent is a function modeling the (history-free) strategy of the opponent player. It is called
    each time an operation emits a Sigma-constant f. The Opponent is then responsible of chosing one direction among
    1..ar(f). The type of [opponent] is [state list -> state]
    *)
let hopcpda_step cpda opponent =

;;


let ho