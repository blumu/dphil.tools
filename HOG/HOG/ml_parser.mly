/** $Id$
	Description: Parser for Core Ml
	Author:		William Blum
**/

%{
open Ml_structs;;
open Type;;
%}

%token<string>	IDENT
%token<int>	NUMBER

%token          BADTOK EOF
%token          VDASH DOT LP RP
%token          FUN LET AND REC
%token          EQUAL IN ARROW AT
%token          IF THEN ELSE
%token          ADDOP
%token          TRUE FALSE
%token          SUCC PRED
%token          COMMA SEMISEMI COLON SEMICOLON
%token          ANYINT


%left           FUN ELSE ARROW IN
%left           EQUAL


%start		                            term_in_context

%type<Ml_structs.ml_termincontext>      term_in_context
%type<Ml_structs.ml_expr>               term
%type<Ml_structs.ml_context>            context



%%
term_in_context:
  context VDASH term                                            {$1,$3}  
   
context :													    { [] }
                | IDENT COLON typ           		            { [$1,$3] }
                | IDENT COLON typ COMMA context		            { ($1,$3)::$5 }
;



typ :    IDENT													{ if $1 = "o" then Gr else failwith "Invalid type!" }
        | LP typ RP												{ $2 }
        | typ ARROW typ 										{ Ar($1,$3) }
;



term:
  expression SEMISEMI                                           {$1}

expression:
  declaration                                                   {$1}
| abstraction                                                   {$1}
| application                                                   {$1}
| ifthenelse                                                    {$1}
| variable                                                      {$1}
| boolean                                                       {MlBool($1)}
| integer                                                       {MlInt($1)}
| ANYINT                                                        {AnyInt}
| condition                                                     {$1};

ifthenelse:
  IF expression THEN expression ELSE expression                 { If($2,$4,$6) };

condition:
   expression EQUAL expression                          { EqTest($1,$3) };


boolean:
    TRUE                                                        { true }
  | FALSE                                                       { false };

integer:
     NUMBER                                                     { $1 };

declaration:
  LET decl_list_next                                            {$2}
| LET REC recdecl_list_next                                     {$3};

decl_list_next:
   ident_list EQUAL expression IN expression                    { Let([List.hd $1, List.tl $1,$3],$5) }
 | ident_list EQUAL expression AND decl_list_next               { match $5 with
								      Let(decl,inexp) ->
									Let(((List.hd $1, List.tl $1,$3)::decl),inexp)
								    |  _ -> failwith "bug in parser!" }

recdecl_list_next:
   ident_list EQUAL expression IN expression                     { Letrec([List.hd $1, List.tl $1,$3],$5) } ;
/* need to manage mutual recursive definition:
  | ident_list EQUAL expression AND decl_list_next  {};  
*/


 

application:
 | operator operand                                             { MlAppl($1,$2) }
 | operator AT operand                                          { MlAppl($1,$3) };

operand:
  variable                                                      { $1 }
 | boolean                                                      { MlBool($1) }
 | integer                                                      { MlInt($1) }
 | SUCC                                                         { Succ }
 | ANYINT                                                       { AnyInt }
 | PRED                                                         { Pred }
 | LP expression RP                                             { $2 };

operator:
  operand                                                       { $1 }
 | application                                                  { $1 };

abstraction:
  FUN ident_list ARROW expression                               { List.fold_right (fun x e -> Fun(x,e)) $2 $4 };

ident_list:
  IDENT                                                         { [$1] }
 | IDENT ident_list                                             { $1::$2};

variable:
  IDENT                                                         { MlVar($1) };

