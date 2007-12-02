(** $Id$
	Description: Lexer for Core Ml terms
	Author:		William Blum
**)

{
open Ml_parser;; open Lexing;; open List;;
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

let lookup s = try Hashtbl.find kwtable s with Not_found -> IDENT s;;

let lineno = ref 1;;
let colno = ref 0;;
}

rule token = parse
  ['A'-'Z''a'-'z']['A'-'Z''a'-'z''0'-'9''_']*
			{ colno:= !colno + (String.length (lexeme lexbuf)); lookup (lexeme lexbuf) }
| ['0'-'9']+		{ colno:= !colno + (String.length (lexeme lexbuf)); NUMBER (int_of_string (lexeme lexbuf)) }
| "?"			{ incr colno; ANYINT }
| "@"			{ incr colno; AT }
| "+"			{ incr colno; ADDOP }
| "="			{ incr colno; EQUAL }
| "("			{ incr colno; LPAR }
| ")"			{ incr colno; RPAR }
| "."			{ incr colno; DOT }
| [' ''\t']+	{ incr colno; token lexbuf }
| "->"			{ colno := !colno + 2; ARROW }
| ";;"			{ colno := !colno + 2; SEMISEMI }
| "(*"			{ colno := !colno + 2; comment lexbuf; token lexbuf }
| "//"			{ colno := !colno + 2; linecomment lexbuf; token lexbuf }
| '\n'			{ colno := 0; incr lineno; token lexbuf }
| _			{ BADTOK }
| eof			{ EOF }

and linecomment = parse
 "\n"			{ colno := 0; incr lineno; }
| _			{ colno:= !colno + (String.length (lexeme lexbuf) ); linecomment lexbuf }
| eof			{ () }

and comment = parse
  "*)"			{ colno:= !colno + 2 }
| "\n"			{ colno := 0; incr lineno; comment lexbuf }
| _			{ colno:= !colno + (String.length (lexeme lexbuf)); comment lexbuf }
| eof			{ () }
