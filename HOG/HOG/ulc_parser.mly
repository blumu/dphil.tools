/** $Id$
  Description: Parser for the Untyped Lambda Calculus (ULC)
  Author: William Blum
**/

%{
(*F#
open FSharp.Compatibility.OCaml;;
open FSharp.Compatibility.OCaml.List;;
F#*)

open Type;;
open Ulc;;
%}

%token<string>  IDENT
%token<int>     NUMBER

%token          BADTOK EOF
%token          DOT LP RP
%token          LAMBDA LET
%token          EQUAL IN AT
%token          COMMA COLON SEMISEMI

%left           LAMBDA IN
%left           EQUAL

%start                                  term

%type<Ulc.ulc_expression>                   term

%%

term:
  expression                                                    {$1}

expression:
  declaration                                                   {$1}
| abstraction                                                   {$1}
| application                                                   {$1}
| variable                                                      {$1}
;

declaration:
  LET decl_list_next                                            {$2}
;

decl_list_next:
   IDENT EQUAL expression IN expression                         { ulc_expression.Let($1,$3,$5) }
;

application:
 | operator operand                                             { ulc_expression.App($1,$2) }
 | operator AT operand                                          { ulc_expression.App($1,$3) }
;

operand:
  variable                                                      { $1 }
 | LP expression RP                                             { $2 }
;

operator:
  operand                                                       { $1 }
 | application                                                  { $1 }
;

abstraction:
  LAMBDA ident_list DOT expression                              { List.fold_right (fun x e -> ulc_expression.Lam(x,e)) $2 $4 }
;

ident_list:
  IDENT                                                         { [$1] }
 | IDENT ident_list                                             { $1::$2}
;

variable:
  IDENT                                                         { ulc_expression.Var($1) }
;

