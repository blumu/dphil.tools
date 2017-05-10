(** $Id$
	Description: Lexer for Core Ml terms
	Author:		William Blum
**)
{
module Ml_lexer

open Ml_parser;;
open FSharp.Compatibility.OCaml;;
open Lexing;;
open Hashtbl;;

(* A little hash table to recognize keywords *)
let kwtable = Hashtbl.create 127;;

List.iter (function (k, t) -> Hashtbl.add kwtable k t )
  [ ("function", FUN);
    ("fun", FUN);
    ("let", LET);
    ("rec", REC);
    ("in", IN);
    ("and", AND);
    ("if", IF);
    ("then", THEN);
    ("else", ELSE);
    ("false", FALSE);
    ("true", TRUE);
    ("pred", PRED);
    ("succ", SUCC);
  ];;

let lookup s =
  match Hashtbl.tryfind kwtable s with
  | None -> IDENT s
  | Some v -> v;;

let incr_linenum (lexbuf : lexbuf) = lexbuf.EndPos <- lexbuf.EndPos.NextLine

}

rule token = parse
  ['A'-'Z' 'a'-'z' '0'-'9' '_' '\\' ']' '[' '\'' '*' '.' '$' '#' ]*   { lookup (lexeme lexbuf) }
| ['0'-'9']+	{ NUMBER (int_of_string (lexeme lexbuf)) }
| "?"         { ANYINT }
| "@"         { AT }
| "+"         { ADDOP }
| "="         { EQUAL }
| "("         { LP }
| ")"         { RP }
| "."         { DOT }
| [' ''\t']+  { token lexbuf }
| "|-"        { VDASH }
| "->"        { ARROW }
| ":"         { COLON }
| ","         { COMMA }
| ";"         { SEMICOLON }
| ";;"        { SEMISEMI }
| "(*"        { comment lexbuf; token lexbuf }
| "//"        { linecomment lexbuf; token lexbuf }
| ('\n' | '\r' '\n') { incr_linenum lexbuf; token lexbuf }
| eof         { EOF }
| _           { BADTOK }

and linecomment = parse
 "\n"       { incr_linenum lexbuf; }
| eof       { () }
| _         { linecomment lexbuf }

and comment = parse
  "*)"      { }
| "\n"      { incr_linenum lexbuf; comment lexbuf }
| eof	      { () }
| _         { comment lexbuf }
