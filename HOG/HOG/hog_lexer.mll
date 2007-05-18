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
        ("non-terminals",  SEC_NONTERMINALS );
        ("rules",  SEC_RULES );
        ("none",  NONE );
        ("demiranda_urzyczin",  DEMIRANDA );
  ];
    tbl;;

let lookup s = try Hashtbl.find kwtable s with Not_found -> ATOM s;;

let lineno = ref 1;;
let colno = ref 0;;
}

rule token = parse
  //['A'-'Z' 'a'-'z' '_']
  ['A'-'Z' 'a'-'z' '0'-'9' '_' '.' '$' '#' '-' ']' '[' '*' ]*
			{ colno:= !colno + (String.length (lexeme lexbuf)); lookup (lexeme lexbuf) }			
| '"' [^'"']* '"'  { ATOM(lexeme lexbuf) }

//| ['0'-'9']+	     { colno:= !colno + (String.length (lexeme lexbuf)); NUMBER (int_of_string (lexeme lexbuf)) }

| [' ' '\t']+	     { incr colno; token lexbuf }
//| "(*"			       { colno := !colno + 2; comment lexbuf; token lexbuf }
| "//"			       { colno := !colno + 2; linecomment lexbuf; token lexbuf }
| ['\n' '\r']		   { colno := 0; incr lineno; token lexbuf }

//| "|"              { incr colno; OR }
//| "&"              { incr colno; AND }
| "("              { incr colno; LP }
| ")"              { incr colno; RP }
| "{"              { incr colno; LCB }
| "}"              { incr colno; RCB }
| ";"              { incr colno; SEMICOLON }
| ":"              { incr colno; COLON }
//| "!"              { incr colno; NEG }
| "="              { incr colno; EQUAL }
//| "<"              { incr colno; LT }
//| ">"              { incr colno; GT }
| "->"             { colno := !colno +2; ARROW }
//| "!="             { colno := !colno +2; NOTEQUAL }
//| "<="             { colno := !colno +2; LE }
//| ">="             { colno := !colno +2; GE }
//| ".."             { colno := !colno +2; TWODOTS }
//| "<->"            { colno := !colno +3; EQUIV }
        
| eof			         { EOF }
| _				         { BADTOK }

             
and linecomment = parse
 "\n"              { colno := 0; incr lineno; }
| _                { colno:= !colno + (String.length (lexeme lexbuf) ); linecomment lexbuf }
| eof              { () }

and comment = parse
  //"*)"			       { colno:= !colno + 2 }
| "\n"			       { colno := 0; incr lineno; comment lexbuf }
| _			           { colno:= !colno + (String.length (lexeme lexbuf)); comment lexbuf }
| eof			       { () }
