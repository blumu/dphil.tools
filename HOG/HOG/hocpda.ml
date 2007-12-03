(** Higher-order collapsible pushdown automata (HO-CPDA) module **)
open Hog
open Type

type state = int

type ident = string
type labelident = string
type terminal = ident

type stackelement = ident
type link = int  * int (* A link is specified by a pair j,k. The stack pointed to by this link is obtained
                          by perfoming k iterations of the 'pop_j' operation **)
                          
type hostack = El of stackelement * link | Stack of hostack list

type instruction = 
      Popn of int
    (** Popn(n) removes the top (n-1)-stack from the stack. **)

    | Push1 of stackelement * link 
    (** Push1(a,l) push the element 'a' with the associated link 'l' onto the top 1-stack. **)
    
    | Pushn of int
    (** Pushn(n+1) where n>=1 duplicates the top n-stack (within the top (n+1)-stack). **)
    
    | Collapse
    (** Collapse the stack to the prefix stack determined by the pointer associated to the top 0-element. **)
      
    | Goto of labelident
    (** Goto(l) jumps to the position labelled with 'l' **)
    
    | GotoIfTop0 of labelident * stackelement
    (** GotoIfTop0(q,f) jumps to instruction 'q' if the top 0-element in the stack is 'f'. Otherwise
    continues with the following instruction of the CPDA. **)

    | CaseTop0 of (stackelement list * labelident) list
    (** CaseTop0([stackelements_0,l0; ... ; stackelements_n,ln]) jumps to instruction labelled with [l_i] if the top 0-element in the stack belongs to [stackelements_i].
    If top0 matches several cases then the branching corresponding to the first matching one is taken.
    If top0 does not match any of the cases then the execution continues with the following instruction. **)

    | CaseTop0Do of (stackelement list * instruction) list
    (** CaseTop0Do([stackelements_0,l0; ... ; stackelements_n,ln]) executes instruction [I_i] if the top 0-element in the stack belongs to [stackelements_i].
    If top0 matches several cases then the instruction corresponding to the first matching one is executed.
    If top0 does not match any of the cases then the execution continues with the following instruction. **)
    
    | Emit of terminal * labelident list
     (** Emit(f,[l_1;  ;l_ar(f)]) emits the non-terminal f and then perform a Goto to 
        one of the label l_1 ... l_ar(f) chosen non-deterministically. 
     **)
     
    | Repeat of int * instruction
    (** Repeat(k,ins) executes k times the instruction ins **)

    | Label of labelident
    (** insert a label in the code of the CPDA **)
    
    | Halt 
    (** halt the CPDA **)
    
    | Failwith of string
    (** raise a CPDA-exception with an error message **)
    
    | Assert of string * (hostack -> bool)
    (** Asserts that a given property of the stack is met. **)    
    
    | Comment of string
    (** source code commentary **)
    


(** The type of a Higher-Order Collapsible Pushdown Automoton.
   Formally a Ho-CPDA is given by a 5-tuple (Sigma, Gamma, Q, delta, q0)
   where - Sigma is a typed alphabet,
         - Gamma is a stack alphabet,
         - Q is a set of states,
         - delta is the transition function QxGamma -> (QxOp  + { (f;q_1, ... q_ar(f)) : f \in Sigma, q_i \in Q })
 **)
type hocpda = 
{   
  n : int;                             (** Order of the CPDA, **)
  terminals_alphabet : alphabet;       (** This corresponds to Sigma in the formal definition, **)
  stack_alphabet : stackelement list;  (** This is Gamma in the formal definition, **)
  code : instruction array;            (** The code of the CPDA stored as an array of instructions. **)
  labels : (ident * int) list;         (** Association list mapping a label to the corresponding code address **)
  
  cpda_path_validator : terminal list -> bool * string
   (** Path validator is a function that checks whether a given path generated by the CPDA satisfies
       the desired specification. It returns a pair [(v,c)] where [v] tells whethere the validation is successful 
       and [c] is a certificate of the validation (a string message) **)

}

(** A generalized state is either a state (i.e. instruction pointer) 
    or a terminal together with a list of states for each parameter of
    the terminal.**)
type gen_state = State of state | TmState of terminal * state list;;

(** Type of a generalized configuration: (state,stk) where
      - current CPDA generalized state
      - current stack state **)
type gen_configuration = gen_state * hostack;;


(** Exception raised when the CPDA itself raises an exception with the Failwith CPDA instruction.
    The parameter contains the message given in the Failiwith CPDA instruction **)
exception Cpda_exception of string;;

(** Exception raised when an ASSERT instruction executed by the CPDA failed.
    The parameter contains a description of the assertion test. **)
exception Cpda_assertfailed of string;;


(** Exception raised when the execution of a CPDA instruction failed.
    The first parameter is the instruction name and the second parameter
    is a description of the problem. **)
exception Cpda_execution_failed of string * string;;



(** [empty_hostack n] constructs an empty stack of order n for n>=1 **)
let rec empty_hostack = function 
    1 -> Stack([])
  | n when n > 1 -> Stack([empty_hostack (n-1)])
  | _ -> failwith "Cannot create an empty stack of order 0!"
;;

exception EmptyHOStack;;
exception BadHOStackStructure;;

(** [top0 n stk] retrieves the top 0-element of the stack [stk].
   @param n is the stack order
   @param stk is the stack **)
let top0 n stk =
  let rec aux k stk = 
    match k,stk with
	0,El(a,l) -> a,l
      | 0,_ | _,El(_) -> raise BadHOStackStructure
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(top::_) -> aux (k-1) top
  in 
    aux n stk
;;

(** [popn j cpda stk] performs an order j pop on the stack [stk], with j>=1. **)
let popn j cpda stk =
  if j = 0 then raise (Cpda_execution_failed("POPN","Instruction pop0 undefined!"));
  let rec aux k stk = 
    match k,stk with
       0,_ | _,El(_) -> raise BadHOStackStructure
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(_::q) when k = j -> Stack(q)
      | _,Stack(top::q) -> Stack((aux (k-1) top)::q)
  in 
    aux cpda.n stk
;;

(** [pushj j cpda stk] performs an order j push where j>=2. **)
let pushj j cpda stk  =
  let rec incr_link = function
      El(a,(lj,lk)) when lj = j -> 
        El(a,(lj,lk+1)) 
    | El(a,l) -> El(a,l)
    | Stack(lst) -> Stack(List.map incr_link lst)
  in
  let rec aux k stk = 
    match k,stk with
       0,_ | _,El(_) -> raise BadHOStackStructure
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(t::q) when k = j -> Stack((incr_link t)::(t::q))
      | _,Stack(top::q) -> Stack((aux (k-1) top)::q)
  in 
    aux cpda.n stk
;;

(** [push1 cpda stk a l] performs an order 1 push of the element 'a' with the link 'l'. **)
let push1 cpda stk a l =
  if not (List.mem a cpda.stack_alphabet) then
    failwith "pushing an unknown stack element!";
    
  let rec aux k stk = 
    match k,stk with
       0,_ | _,El(_) -> raise BadHOStackStructure
      | 1,Stack(s) -> Stack(El(a,l)::s)
      | _,Stack([]) -> raise EmptyHOStack
      | _,Stack(top::q) -> Stack((aux (k-1) top)::q)
  in 
    aux cpda.n stk
;;


(** [collapse cpda stk] performs the collapse operation. **)
let collapse cpda stk =
  let ret = ref stk in
  let a,(j,k) = top0 cpda.n stk in
  (*for debugging purpose *)
  (*if k > 1 then failwith ("The CPDA is executing a COLLAPSE on a ("^(string_of_int j)^","^(string_of_int k)^")link");
    *)
  for i = 1 to k do
    ret := popn j cpda !ret;
  done;
  !ret
;;

exception CpdaHalt;;
exception JumpToInexistingCode;;

(** [mk_nextstate cpda ip] creates a state corresponding to the instruciton [ip+1]
    if it exists.
    @raise CpdaHalt if the end of the CPDA code is reached. **)
let rec mk_nextstate cpda ip = 
    if ip+1 >= Array.length cpda.code then
      raise CpdaHalt
    else
        (* skip commentaries and labels *)
        match cpda.code.(ip+1) with
        | Comment(_) | Label(_) -> mk_nextstate cpda (ip+1)
        | _ -> State(ip+1)
;;

(** [mk_state cpda i] creates a state corresponding to the instruciton [i].
    @raise JumpToInexistingCode if i does not lie within the program code.
    @return State(i) if [i] is a valid instruction number **)
let mk_state cpda i =
    if i >= Array.length cpda.code then
      raise JumpToInexistingCode
    else
      State(i)
;;

(** [hocpda_label_to_address cpda lab] returns the address corresponding to label lab. **)
let hocpda_label_to_address cpda lab =
    try  List.assoc lab cpda.labels 
    with Not_found -> raise (Cpda_execution_failed("(label referencing)","The code of the HOCPDA contains references to undefined labels!"))
;;


(** [execute cpda ip stk ins] execute the instruction [ins] of the CPDA with stack [stk] and current state [ip].
    @param cpda is the CPDA
    @param ip is the current instruction pointer of the CPDA
    @param stk current CPDA stack.
    @param ins is the instruction to execute.
    @return a generalized configuration corresponding to the new state of the CPDA.
    **)
let rec execute cpda ip stk =
    function
      Popn(n) -> (mk_nextstate cpda ip), (popn n cpda stk)
    | Push1(a,l) -> (mk_nextstate cpda ip), (push1 cpda stk a l) 
    | Pushn(j) -> (mk_nextstate cpda ip), (pushj j cpda stk);
    | Collapse -> (mk_nextstate cpda ip), (collapse cpda stk);
    | GotoIfTop0(q, t) -> if fst (top0 cpda.n stk) = t then 
                            (mk_state cpda (hocpda_label_to_address cpda q)),stk
                          else 
                            (mk_nextstate cpda ip),stk
    | CaseTop0(cases) -> (try 
                            let top0elem = fst (top0 cpda.n stk) in
                            let _, lab = List.find (function stkelems, _ -> List.mem top0elem stkelems) cases in
                            (mk_state cpda (hocpda_label_to_address cpda lab)),stk
                         with Not_found -> (mk_nextstate cpda ip),stk)
    | CaseTop0Do(cases) -> (try 
                              let top0elem = fst (top0 cpda.n stk) in
                              let _, ins = List.find (function stkelems, _ -> List.mem top0elem stkelems) cases in
                              execute cpda ip stk ins
                            with Not_found -> (mk_nextstate cpda ip),stk)
                         
                         
    | Goto(l) -> (mk_state cpda (hocpda_label_to_address cpda l)),stk
    | Emit(t,ql) -> TmState(t,(List.map (hocpda_label_to_address cpda) ql)),stk
    | Halt -> raise CpdaHalt
    | Failwith(msg) -> raise (Cpda_exception(msg))
    | Label(_)  -> raise (Cpda_execution_failed("LABEL", "The CPDA code contains unresolved labels!"))
    | Comment(_) -> raise (Cpda_execution_failed("COMMENT", "Trying to execute a commentary!"))
    | Repeat(k,ins) -> (* Execute k times the instruction ins *)
                       let conf = ref (State(ip),stk) in
                       for i = 0 to k-1 do
                         match !conf with
                              (* A terminal is emitted: abort the iteration. (we are 
                                dealing with an instruction of the form REPEAT k TIMES EMIT f ...) *)
                              TmState(_),_ -> ()
                           | State(_),newstk -> conf := execute cpda ip newstk ins
                       done; !conf
    
    | Assert(descr,test) -> if test stk then
                              (mk_nextstate cpda ip),stk
                            else
                              raise (Cpda_assertfailed(descr))
                              
;;

(** [hopcpda_step cpda genconf] performs a transition of the cpda.
    @param cpda is the CPDA
    @param genconf is a generalized configuration of the CPDA.
    @return a generalized configuration corresponding to the new state of the CPDA.
    **)
let hocpda_step cpda genconf =
    try 
        match genconf with
          TmState(_),_ -> failwith "The CPDA has just emitted a terminal and therefore has halted. You need to choose a terminal parameter in order to spawn the CPDA."
        | State(ip),stk -> execute cpda ip stk cpda.code.(ip)
    with   Cpda_execution_failed(ins,msg) -> failwith ("CPDA INSTRUCTION FAILED!\nInstruction: "^ins^"\nDescription:"^msg)
         | Cpda_exception(msg) -> failwith ("CPDA EXCEPTION RAISED!\nDescription: "^msg)
         | Cpda_assertfailed(msg) -> failwith ("CPDA ASSERTION FAILED!\nDescription of the test: "^msg)
;;

(** Create the initial configuration of a CPDA. **)
let hocpda_initconf cpda =
  State(0),(empty_hostack cpda.n);
;;



(** pretty-printer for ho-stacks **)
let rec string_of_stack stk =
    match stk with
        El(e,(j,k)) -> e^","^(string_of_int j)^","^(string_of_int k)
     | Stack(lst) -> "["^(String.concat "; " (List.map string_of_stack lst))^"]"
;; 

(** pretty-printer for generalized configuration **)
let string_of_genconfiguration = function
      State(ip), stk -> (string_of_int ip)^": " ^(string_of_stack stk)
    | TmState(f, lststates),stk -> f^"("^(String.concat "," (List.map string_of_int lststates))^"): "^(string_of_stack stk)
;;

(** pretty-printer for hocpda **)
let cLABEL_COLUMN_WITDH = 15;;
let cLINE_COLUMN_WITDH = 5;;
let string_of_hocpda cpda = 
    (* Prepare an array containing the lines to be marked with a label *)
    let labs = Array.of_list (List.rev cpda.labels) (* reverse the list so the labels occur by order of appearance in the code. *)
    in
    let ret = ref "" in
    let rec string_of_instr = function
          Popn(n) -> "POP"^(string_of_int n)
        | Push1(a,(j,k)) -> "PUSH1 "^a^" ("^(string_of_int j)^","^(string_of_int k)^")"
        | Pushn(j) -> "PUSH"^(string_of_int j)
        | Collapse -> "COLLAPSE"
        | GotoIfTop0(q, t) -> "GOTO "^q^" IF TOP1="^t
        | CaseTop0(cases) -> "CASETOP0 "^(String.concat " " (List.map (function a,q -> (String.concat " " a)^"->"^q) cases))
        | CaseTop0Do(cases) -> "CASETOP0DO "^(String.concat "  |  " (List.map (function a,i -> (String.concat " " a)^"->"^(string_of_instr i)) cases))
        | Goto(l) -> "GOTO "^l
        | Emit(t,[]) -> "EMIT "^t^" // after this instruction the machine halts."; (*the terminal is of ground-type therefore the machine necessarily blocks after emitting the terminal. *)
        | Emit(t,ql) -> "EMIT "^t^" "^(String.concat "," ql)
        | Repeat(k,ins) -> "REPEAT "^(string_of_int k)^" TIMES "^(string_of_instr ins)
        | Halt -> "HALT"
        | Failwith(msg) -> "FAILWITH \""^msg^"\""
        | Comment(com) -> "// "^com
        | Label(lab)  -> lab^":"
        | Assert(desc,_) -> "ASSERT "^desc 
    in
      let nextlab = ref 0 in
      for i = 0 to (Array.length cpda.code)-1 do
        let pad_right s n = 
            if String.length s >= n then s
            else String.sub (s^(String.make n ' ')) 0 n in
        let pad_left s n = 
            if String.length s >= n then s
            else let tmp = (String.make n ' ')^s in String.sub tmp ((String.length tmp)-n) n in
        
        if !nextlab < Array.length labs && i = (snd labs.(!nextlab))  then
        begin
            ret := !ret^(pad_left (string_of_int i) cLINE_COLUMN_WITDH)^" "^(pad_right (fst labs.(!nextlab)) cLABEL_COLUMN_WITDH )^":  "^ (string_of_instr cpda.code.(i))^"\n";
            incr(nextlab); 
        end
        else
            ret := !ret^(pad_left (string_of_int i) cLINE_COLUMN_WITDH)^" "^(String.make cLABEL_COLUMN_WITDH  ' ')^"   "^(string_of_instr cpda.code.(i))^"\n"
      done;
    "Order: "^(string_of_int cpda.n)
    ^"\n\nTerminals:\n"^(string_of_alphabet cpda.terminals_alphabet)
    ^"\nStack alphabet: "^(String.concat " " cpda.stack_alphabet)
    ^"\n\nCode:\n\n"^(!ret)
;;


(** Remove the labels occurring in the CPDA code and compute the adresses of the labels.
    [unlabel_code code]
    @param code a list of CPDA instructions
    @return (unlabelled_code,labels) where  [labels] is a mapping from labels to instruction addresses
    and [unlabelled_code] is an array containing the code with the labels removed.
**)
let unlabel_code code =
    (* - size is the size of the code to be generated,
       - label2ip is an association list mapping labels to their corresponding instruction pointer *)
    let size,label2ip = List.fold_left (function (s,labmap) -> function
                                              Label(label) -> s,(label,s)::labmap
                                            | Popn(_) | Push1(_,_)
                                            | Pushn(_) | Collapse
                                            | Goto(_) | GotoIfTop0(_,_)
                                            | CaseTop0(_) | CaseTop0Do(_) | Emit(_,_)
                                            | Repeat(_,_)  | Comment(_) | Assert(_,_)
                                            | Failwith(_) | Halt (* | Nop *)
                                                    -> s+1,labmap
                                       ) (0,[]) code in
    (Array.of_list (List.filter (function Label(_) -> false | _ -> true ) code)), label2ip 
;;



(** To specify which kind of CPDA must be generated. **)
type Cpdatype = Npda | Ncpda | Np1pda;;

(** [hors_to_cpda kind hors graph] converts a HORS to a CPDA.
    @param kind
        if set to Ncpda then the function generates the n-CPDA equivalent to the n-HORS,
        if set to Npda then it generates a n-PDA which is equivalent to the n-HORS provided that it is safe,
        if set to Np1Pda then it generates a n+1-PDA (experimental),
    @param hors the HO recursion scheme
    @param graph the computation graph of the HORS
    @param vartmtypes an association list mapping variable and terminal names to their type
    @return the corresponding CPDA
        
    @note See details of the algorithm in the STOC paper, Hague et al.
**)
let hors_to_cpda kind hors ((nodes_content:cg_nodes),(edges:cg_edges)) vartmtypes =
    (* compute the order of the grammar *)
    let order = List.fold_left max 0 (List.map (function nt,t -> typeorder t) hors.nonterminals) in 
    
    (* order of the generated CPDA/PDA *)
    let ordercpda = match kind with Ncpda | Npda -> order | Np1pda -> order+1 in
    
    (* Compute an array [varsinfo] of 4-tuples given information about each node of the computation graph.
       For a variable node x the tuple is (b,p,i,j) where:
        - b is the nodeid of the binder of x
        - p is the span of the variable x (distance between x and its binder in the path to the root)
        - i is the parameter index (the variable position in the list of variables abstracted by its binder)
        - j is the child-index of the binder node of x i.e. binder(x) is a j-child node.
       
       For a lambda-node different from the root the tuple is (p,j,-1,-1) :
        - p is the node id of the parent of the node
        - j is the child-index of the lambda-node

       For application-nodes or the root node the tuple is (-1,-1,-1,-1)
    *)
    let nodesinfo = Array.create (Array.length nodes_content) (-1,-1,-1,-1) in    
    let rec compute_varinfo curnodeid path = 
        (match nodes_content.(curnodeid) with
             (* The current node is a variable: compute its span (p), paramindex(i) and binder child index(j) by following the path to the root until reaching its binder. *)
             NCntVar(x) -> let rec List_memi i = function [] -> raise Not_found
                                                        | t::_ when t=x -> i
                                                        | _::q -> List_memi (i+1) q
                            
                            in
                            let rec find_binder p = function 
                                 [] -> failwith "(hors_to_cpda) Error: the computation tree contains a free variable!"
                               | (b,_)::q -> match nodes_content.(b) with
                                                NCntAbs(_, vars) -> 
                                                        (try  b, p,
                                                              (List_memi 1 vars),
                                                              (match q with [] -> 0 | (_,j)::_ -> j)
                                                        with Not_found -> find_binder (p+1) q)
                                                | _ -> find_binder (p+1) q
                            in 
                            nodesinfo.(curnodeid) <- find_binder 1 path;
            | NCntAbs(_,_) -> (match path with
                                (p,_)::(_,j)::_ -> nodesinfo.(curnodeid) <- p,j,-1,-1
                                | _ -> ())
            | _ -> ();
        );
        
        (* Performs a depth-first browsing of the tree nodes. To avoid going into loops 
            we do not take the first edge (labelled 0) of an @-node. *)
        Array.iteri (function childindex -> function nodeid ->
                            match nodes_content.(curnodeid),childindex with 
                              NCntApp, 0 -> ();
                            | NCntApp, _ -> compute_varinfo nodeid ((curnodeid,childindex)::path)
                            | _ -> compute_varinfo nodeid ((curnodeid,childindex+1)::path)
                     )
                    (try NodeEdgeMap.find curnodeid edges with Not_found -> [||]);
    in
        (* Explore the sub-**tree** rooted at each non-terminal node *)
        Array.iteri (function inode -> (function NCntAbs(nt,_) when nt<>"" -> compute_varinfo inode []; | _ -> ()) ) nodes_content;
    

    (* make a stack element out of a node id *)
    let stackelement_of_nodeid = string_of_int in

    (* build an array containing the type of each node
       and one containing the order. *)
    let nodes_type = Array.map (graph_node_type vartmtypes) nodes_content in
    let nodes_order = Array.map typeorder nodes_type in

    (* Experimental simulation of Collapse with pops. *)
    let primeCollapse =
        (* Compute a mapping that associate to each node [node] a number [n] such that
           if the top 0-element of the stack is [node], then the COLLAPSE instruction will be simulated by Popn(n).
           It is not necessary to consider non-lambda nodes since no COLLAPSE can ever happen at such nodes.
        *)
        let nodes_pushorder = Hashtbl.create (1+order) in        
            Array.iteri (function inode -> function ord -> 
                        match nodes_content.(inode) with
                            (* If it is an abstraction node that does not correspond to a non-terminal then
                               it is a j child with j>0 *)
                             NCntAbs("",_) ->  Hashtbl.add nodes_pushorder 
                                                            (match kind with
                                                                  Ncpda -> 0 (* primeCollapse will not be used anyway in a CPDA... *)
                                                                | Npda -> 
                                                                    if ord = 0 then 1 (* perform a pop1 *)
                                                                    else order-ord+1  (* perform a pop{order-ord+1} *) 
                                                                | Np1pda -> order-ord+1 (* perform a pop{order-ord+1} *) 
                                                             )
                                                            inode
                            (* If it is an abstraction node corresponding to a non-terminal then it is a 0-child node. *)
                            | NCntAbs(_,_) -> Hashtbl.add nodes_pushorder 1 inode
                            
                            | _ -> ()
                    ) nodes_order;
        
        (* build the cases *)                    
        let cases_pop = ref [] in
        for popord = 0 to order do        
            let nodes = Hashtbl.find_all nodes_pushorder popord in
            if nodes <> [] then
                cases_pop := ((List.map stackelement_of_nodeid nodes),Popn(popord))::!cases_pop
        done; 
        (* First assert that the simulation of the collapse by a pop is sound and then do the pop. *)
        [Assert("that the top link is of type (j,1) so that COLLAPSE can be simulated by a POP_{n-j+1}.",
                                                (function hostack -> match top0 ordercpda hostack with _,(_,1) -> true | _ -> false ));
                                                 CaseTop0Do( !cases_pop )]
    in


    let make_nodeproc_label nodei = "NODE"^(string_of_int nodei) in
    
    
    (* [build_node_procedure nodeid] build the procedure associated to the node [nodeid] of the graph
        @param nodeid is the identifier of the graph node 
        @returns a list of instructions. *)
    let build_node_procedure nodeid nodecontent =
        let child = graph_childnode edges nodeid in
        let startlabel = make_nodeproc_label nodeid in
            match nodecontent with
              NCntApp -> [Label(startlabel);Push1(string_of_int (child 0),(1,1));Goto("start")]
            | NCntAbs(_) -> [Label(startlabel);Push1(string_of_int (child 0),(1,1));Goto("start")]
            | NCntTm(f) -> let ar = typearity (terminal_type hors f) in
                           (* [param_label i] returns the label f_i *)
                           let param_label i = startlabel^"_"^(string_of_int i) in
                           (* Create the label f_1 ... f_ar(f) *)
                           let switchlabels =  Array.to_list (Array.init ar (function i -> param_label (i+1))) in
                           let rec gencode_params = function
                               | 0 -> []
                               | k -> (gencode_params (k-1))@
                                       [Label(param_label k);
                                        Push1(string_of_int (child (k-1)),(0,0));
                                        Goto("start")]
                           in
                            [Label(startlabel);Emit(f,switchlabels)]@(gencode_params ar)
            | NCntVar(x) -> let l = typeorder (List.assoc x vartmtypes) in
                            let b,p,i,j = nodesinfo.(nodeid) in
                            (* generate the instructions simulating push1^{Ei(top1),k} (see details in the STOC paper, Hague et al.) *)
                            let push1_Ei_top1() = 
                                
                                (* Generate the code that performs a case analysis on the top 0-element. *)
                                let switchingtable = ref [] in
                                let make_case_label nodei = 
                                    let label = startlabel^"_"^(string_of_int nodei) in
                                    switchingtable := ([string_of_int nodei], label)::!switchingtable;
                                    label
                                in        
                                let build_case top1_nodeid top1_nodecontent = 
                                    (match top1_nodecontent with
                                       (* If j=0 then we generate a case only for application nodes of the tree whose first edge points to the binder of x. *)                                           
                                           NCntApp when j=0 && (graph_childnode edges top1_nodeid 0) = b ->
                                            [Label(make_case_label top1_nodeid);
                                                Push1(string_of_int (graph_childnode edges top1_nodeid i),(order-l+1,1));
                                                Goto("start")]
                                        (* If j>0, we generate a case only for variable nodes of the correct type 
                                           i.e. the type of the binder of x. *)
                                         | NCntVar(_) when j>0 && nodes_type.(b) = nodes_type.(top1_nodeid) -> 
                                            [Label(make_case_label top1_nodeid);
                                                Push1(string_of_int (graph_childnode edges top1_nodeid (i-1)),(order-l+1,1));
                                                Goto("start")]
                                         | _ -> []
                                    ) in
                                let cases_code = List.flatten (Array.mapi build_case nodes_content) in 
                                CaseTop0(!switchingtable)::Failwith("Unexpected top 0-element!")::cases_code
                            in
                            Label(startlabel)::
                            (
                                match kind with
                                  (* If we generate an n-PDA then we simulate the Collapse instruction by a
                                   pop of order n-l+1 *)
                                  Npda ->
                                    (if l>=1 then [Pushn(order-l+1)] else [] )
                                        @[Repeat(p,Popn(1))]@primeCollapse

                                    (* let whilelab = startlabel^".while"
                                    and donelab = startlabel^".done" in
                                      (if l>=1 then [Pushn(order-l+1)] else [] )
                                      @[Popn(1);
                                        Label(whilelab);
                                          GotoIfTop0(donelab, (stackelement_of_nodeid b));
                                          primeCollapse;
                                          Popn(1);
                                        Goto(whilelab);                                        
                                        Label(donelab);
                                        primeCollapse] *)
                                 | Ncpda ->
                                    (if l>=1 then [Pushn(order-l+1)] else [] )
                                        @[Repeat(p,Popn(1));Collapse]
                                 | Np1pda -> 
                                    [Pushn(order-l+1);Repeat(p,Popn(1))]@primeCollapse
                            )@push1_Ei_top1() (* the code in push1_Ei_top1 is responsible of jumping to 'Start'.*)
    in
    
    (* The first instructions are responsible of jumping to the procedure
       associated to the node of the graph that is read from the top-1 stack.
       The following array of labels is used to perform the case analysis. *)
    let switchingtable = Array.to_list (Array.init (Array.length nodes_content)
                                        (function i -> [stackelement_of_nodeid i],(make_nodeproc_label i)) ) in
    let procedures_code = List.flatten (Array.mapi build_node_procedure nodes_content) in
    let entirecode = Push1(stackelement_of_nodeid 0,(0,0))::
                     Label("start")::
                     CaseTop0(switchingtable)::
                     procedures_code in
    let unlabelledcode, labels = unlabel_code entirecode in
        {   n = ordercpda;
            terminals_alphabet = hors.sigma;
            (* the stack alphabet is the set of nodes of the graph *) 
            stack_alphabet = Array.to_list (Array.init (Array.length nodes_content) stackelement_of_nodeid);
            code = unlabelledcode;
            labels = labels;
            cpda_path_validator = hors.rs_path_validator;
        }
;;



(***** Tests ****)
(*

let test = { n = 5;
	     terminals_alphabet = ["f",Gr];
	     stack_alphabet = ["e1";"e2"];
	     code = [|Push1("e1",(0,0)); GotoIfTop0("toto","e2"); Push1("e1",(0,0)); Collapse|];
	     labels = ["toto",1];
	     cpda_path_validator = function s -> true,""
};;

let conf = (0, Stack([Stack([Stack([Stack([Stack([El("e1",(0,0))])])])])]))
;;

hocpda_step test (hocpda_initconf test);; 


let testcpda = {    n = 5;
                    terminals_alphabet = ["f",Gr; "g",Ar(Gr,Gr); ];
                    stack_alphabet = ["e1";"e2"];
                    code = [|Push1("e1",(0,0)); Emit("g",[1;2]); GotoIfTop0(1,"e2"); Push1("e1",(0,0)); Collapse|];
               }
*)















(*

(** [hocpda_conf_step cpda conf opponent] does the same
    as [hocpda_conf_step cpda opponent] except that it
    first sets the cpda in the configuration [conf] before
    executing a step of the cpda.
    
    @param opponent is a function of type [terminal -> state list -> state option] modeling the (history-free) strategy of the opponent player.
    It is  called each time an operation emits a Sigma-constant f. The Opponent is then responsible of chosing one direction among
    1..ar(f), and does so by returning the corresponding state from the list passed in the second parameter.
    If and only f is of arity 0, opponent can return None.
    **)
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
    the emitted terminal. **)
let opp_strategy a q =
    print_string ("CPDA emits terminal "^a);
    match q with 
        [] -> None
      | p1::_ -> print_string "Opponent choses "; print_int p1; Some(p1)
;;


(** Type for higher-level instructions. 
    It is used for generating intermediate code containing labels. **)
type hli_labelident = string;;
type hlinstr =    HliLabel of hli_labelident
                | HliPopn of int
                | HliPush1 of stackelement * link 
                | HliPushn of int
                | HliCollapse
                | HliGoto of hli_labelident
                | HliGotoIfTop0 of hli_labelident * stackelement
                | HliCaseTop0 of (stackelement * hli_labelident) list
                | HliEmit of terminal * hli_labelident list
                | HliNop
                | HliHalt
                | HliIter of int * hlinstr
                | HliFailwith of string
                | HliComment of string
                ;;

(** Convert higher-level code to low-level CPDA code.
    [hlicode_to_cpdacode hlicode]
    @param hlicode list of higher-level instructions
    @return an array of low-level instructions **)    
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
                                            | HliCaseTop0(_)
                                            | HliEmit(_,_)
                                            | HliComment(_)
                                            | HliFailwith(_) | HliHalt -> s+1,labmap
                                            | HliIter(k,_) when k>=0 -> s+k,labmap
                                            | HliIter(_,_) -> failwith "Wrong number of iterations in HliIter."
                                            | HliNop -> s,labmap                                             
                                       ) (0,[]) hlicode in
    
    let ip_from_label label = 
        try List.assoc label label2ip
        with Not_found -> failwith "The high-level source code contains references to undefined labels!"
    in
    let code = Array.create size (Goto(0)) in  
    let curip = ref 0 in
    let rec gen_instruction = function 
          HliLabel(_) -> ();
        | HliPopn(i) -> code.(!curip) <- Popn(i); incr(curip);
        | HliPush1(a,l) -> code.(!curip) <- Push1(a,l); incr(curip);
        | HliPushn(n) -> code.(!curip) <- Pushn(n); incr(curip);
        | HliCollapse-> code.(!curip) <- Collapse; incr(curip);
        | HliGoto(label) -> code.(!curip) <- Goto(ip_from_label label) ; incr(curip);
        | HliGotoIfTop0(label,a) -> code.(!curip) <- GotoIfTop0((ip_from_label label), a) ; incr(curip);
        | HliCaseTop0(cases) -> code.(!curip) <- CaseTop0( (List.map (function a,l -> a,(ip_from_label l)) cases)) ; incr(curip);
        | HliEmit(t,slst) -> code.(!curip) <- Emit(t,(List.map ip_from_label slst)); incr(curip);
        | HliComment(com) -> code.(!curip) <- Comment(com); incr(curip);
        | HliHalt -> code.(!curip) <- Halt ; incr(curip);
        | HliFailwith(msg) -> code.(!curip) <- Failwith(msg) ; incr(curip);
        | HliIter(k,ins) -> for i = 1 to k do
                                gen_instruction ins;
                            done;
        | HliNop -> ();
    in
    List.iter gen_instruction hlicode;
    code
;;

*)
