(**
  Description: Lexer for untyped lambda calculus (ULC)
  Author: William Blum
**)
{
module Ulc_lexer

open Ulc_parser;;
open FSharp.Compatibility.OCaml;;
open Lexing;;
open Hashtbl;;

(* A little hash table to recognize keywords *)
let kwtable = Hashtbl.create 127;;

List.iter (function (k, t) -> Hashtbl.add kwtable k t )
  [ ("Lam", LAMBDA);
    ("lam", LAMBDA);
    ("lambda", LAMBDA);
    ("Lambda", LAMBDA);
    ("let", LET);
  ];;

let lookup s =
  match Hashtbl.tryfind kwtable s with
  | None -> IDENT s
  | Some v -> v;;

let incr_linenum (lexbuf : lexbuf) = lexbuf.EndPos <- lexbuf.EndPos.NextLine

}

rule token = parse
| "lambda"           { LAMBDA }
| ['A'-'Z' 'a'-'z' '0'-'9' '_' '\\' ']' '[' '\'' '*' '$' '#' ]*   { lookup (lexeme lexbuf) }
| ['0'-'9']+         { NUMBER (int_of_string (lexeme lexbuf)) }
| "@"                { AT }
| "="                { EQUAL }
| "("                { LP }
| ")"                { RP }
| "."                { DOT }
| [' ''\t']+         { token lexbuf }
| ":"                { COLON }
| ","                { COMMA }
| ";;"               { SEMISEMI }
| "(*"               { comment lexbuf; token lexbuf }
| "//"               { linecomment lexbuf; token lexbuf }
| ('\n' | '\r' '\n') { incr_linenum lexbuf; token lexbuf }
| eof                { EOF }
| _                  { BADTOK }

and linecomment = parse
 "\n"       { incr_linenum lexbuf; }
| eof       { () }
| _         { linecomment lexbuf }

and comment = parse
  "*)"      { }
| "\n"      { incr_linenum lexbuf; comment lexbuf }
| eof       { () }
| _         { comment lexbuf }
