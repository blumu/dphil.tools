(** $Id$
    File: hog_lexer.mll
	Description: Lexer for HO-grammars
	Author:		William Blum
**)

{
open Hog_parser;;
open Lexing;;
open List;;
open Hashtbl;;

(* hash table for the keywords.
  Be careful: here you should only add keywords which are recognized by the 
  regular expression ['A'-'Z' 'a'-'z' '_']['A'-'Z' 'a'-'z' '0'-'9' '_' '.' '$' '#' '-']*
  which is defined in the token rule further down (the kwtable is only used when the token read has 
  the format of an identifier).  
*)
let kwtable =
    let tbl = Hashtbl.create 127 in
    List.iter (function (k, t) -> Hashtbl.add tbl k t )
      [ ("name",  SEC_NAME );
        ("validator",  SEC_VALIDATOR  );
        ("terminals",  SEC_TERMINALS );
        ("nonterminals",  SEC_NONTERMINALS );
        ("rules",  SEC_RULES );
        ("none",  NONE );
        ("demiranda_urzyczyn",  DEMIRANDA );
  ];
    tbl;;

let lookup s = try Hashtbl.find kwtable s with Not_found -> ATOM s;;


(*IF-OCAML*)
let incr_linenum lexbuf =
	let pos = lexbuf.Lexing.lex_curr_p in
		lexbuf.Lexing.lex_curr_p <- { 
			pos with
				Lexing.pos_lnum = pos.Lexing.pos_lnum + 1;
				Lexing.pos_bol = pos.Lexing.pos_cnum;
		}
;;
(*ENDIF-OCAML*)
(*F#
let incr_linenum (lexbuf : lexbuf) = lexbuf.EndPos <- lexbuf.EndPos.NextLine
F#*)

}

rule token = parse
  //['A'-'Z' 'a'-'z' '0'-'9' '_' ']' '[' '*' ]
  ['A'-'Z' 'a'-'z' '0'-'9' '_' ']' '[' '*' '.' '$' '#' ]*   { lookup (lexeme lexbuf) }			
| '"' [^'"']* '"'    { ATOM(lexeme lexbuf) }

//| ['0'-'9']+	     { NUMBER (int_of_string (lexeme lexbuf)) }

| [' ' '\t']+	     { token lexbuf }
//| "(*"			 { comment lexbuf; token lexbuf }
| "//"			     { linecomment lexbuf; token lexbuf }
| ('\n' | '\r' '\n') { incr_linenum lexbuf; token lexbuf }

//| "|"              { OR }
//| "&"              { AND }
| "("                { LP }
| ")"                { RP }
| "{"                { LCB }
| "}"                { RCB }
| ";"                { SEMICOLON }
| ":"                { COLON }
//| "!"              { NEG }
| "="                { EQUAL }
//| "<"              { LT }
//| ">"              { GT }
| "->"               {  ARROW }
//| "!="             {  NOTEQUAL }
//| "<="             {  LE }
//| ">="             {  GE }
//| ".."             {  TWODOTS }
//| "<->"            {  EQUIV }
        
| eof			     { EOF }
| _				     { BADTOK }

             
and linecomment = parse
 ('\n' | '\r' '\n')	   { incr_linenum lexbuf;  }
| _					   { linecomment lexbuf }
| eof				   { () }

and comment = parse
  //"*)"			   {  }
| ('\n' | '\r' '\n')   { incr_linenum lexbuf; comment lexbuf }
| _			           { comment lexbuf }
| eof			       { () }
