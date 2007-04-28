
// Interface File  

// Signatures can specify the namespace and/or module of the constructs defined.

//namespace Comlab

module Hogrammar

// Signature files may open namespaces.  The namespaces Microsoft.FSharp
// and Microsoft.FSharp.Compatibility.OCaml are implicitly opened.

open System


type typ = Gr | Ar of typ * typ ;;


type ident = string;;
type alphabet = (ident * typ) list;;
type terminal = ident;;
type nonterminal = ident;;

(* applicative term *)
type appterm = Nt of nonterminal | Tm of terminal | Var of ident | App of appterm * appterm;;

type rule = nonterminal * ident list * appterm;;
type recscheme = { nonterminals : alphabet;
				   sigma : alphabet;
				   rules : rule list } ;;



(*

// Signatures for functions and simple values are given using 'val':

val myFunction : int -> int -> int
val myInteger  : int 

// Modules are simply nested namespaces, and are similar to classes that
// only contain static members.  They are specified as follows:

module MyModule : begin
  type sting
  val someStinger : string -> sting 
  val someSting : sting
end

// Signatures for type definitions are given as follows:

// An abstract type (with some associated values):
type MyAbstractType
val create : string -> MyAbstractType
val get : MyAbstractType -> string

// A record type:
type MyRecordType =
 { Name: string;
   Date: System.DateTime }

// A class type:
type widget = class
  member Poke : int -> unit
  member Peek : unit -> int
  member WasPoked : bool
end

// More completely:
type MyClassType = class
  // A constructor:
  new : string * string -> MyClassType
  // A property:
  member MyProperty: string
  // A static property:
  static member MyStaticProperty: string
  // A method:
  member MyMethod : string * string -> int
  // A static method:
  static member MyStaticMethod : string * string -> int
  // An abstract method:
  abstract MyAbstractMethod : string -> string -> int
  // A published override:
  override MyOverride: string -> string -> int
  // An interface implemeentation:
  interface IComparable
end

// A discrimination type:
type MyDiscriminationType =
 | Case1 of int * int * string
 | Case2 of System.DateTime
 | Case3 of System.Exception


*)