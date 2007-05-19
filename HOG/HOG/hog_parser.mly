/** $Id$
	Description: Parser for HO-grammars
	Author:		William Blum
**/

%{
open Hog

type validators = None | Demiranda;;
let parsed_validator = ref None;;
let parsed_nonterminals = ref [];;
let parsed_terminals = ref [];;
let parsed_rules = ref [];;
let parsed_name  = ref "";;


let section_name_present = ref false;;
let section_nonterminals_present = ref false;;
let section_validator_present = ref false;;
let section_terminals_present = ref false;;
let section_rules_present = ref false;;
%}

%token BADTOK EOF
%token ARROW
%right ARROW

%token COMMA COLON SEMICOLON LCB RCB LP RP EQUAL
%left SEMICOLON LP RP LCB RCB

%token SEC_NAME SEC_VALIDATOR SEC_TERMINALS SEC_NONTERMINALS SEC_RULES
%token NONE DEMIRANDA

%token<string> ATOM


%type <Hog.recscheme> hog_specification

%start		          hog_specification



%%

/* SPECIFICATION OF A HO-GRAMMAR */

hog_specification:
			  sectionlist                                       { if not !section_name_present 
																   || not !section_validator_present 
																   || not !section_terminals_present 
																   || not !section_nonterminals_present 
																   || not !section_rules_present 
																   then
																	   raise Parsing.MissingSection
																   else
 																	   { nonterminals = !parsed_nonterminals;
																		sigma = !parsed_terminals;
																		rules = !parsed_rules;        
																		rs_path_validator = match !parsed_validator with
																								  None ->  (function s -> true,"")
																								| Demiranda -> Hog.demiranda_validator
																	   }
																}
;

sectionlist :													{ }
				| section sectionlist							{ }
  ;

section :    sec_name											{ section_name_present := true }
           | sec_validator										{ section_validator_present := true }
           | sec_terminals										{ section_terminals_present := true }
           | sec_nonterminals									{ section_nonterminals_present := true }
           | sec_rules											{ section_rules_present := true }
           ;

           
sec_name : SEC_NAME LCB sec_name_content RCB					{ }

sec_name_content : ident										{ parsed_name := $1 }


sec_validator : SEC_VALIDATOR LCB sec_validator_content RCB     { }

sec_validator_content :   DEMIRANDA                             { parsed_validator := Demiranda }
						| NONE                                  { parsed_validator := None }

;

sec_terminals : SEC_TERMINALS LCB terminal_list RCB				{ parsed_terminals := $3 }



terminal_list :													{ [] }
                | ident COLON typ SEMICOLON terminal_list		{ ($1,$3)::$5 }
;



typ :    ident													{ if $1 = "o" then Gr else failwith "Invalid type!" }
        | LP typ RP												{ $2 }
        | typ ARROW typ 										{ Ar($1,$3) }
;






sec_nonterminals : SEC_NONTERMINALS LCB terminal_list RCB		{ parsed_nonterminals := $3 }


sec_rules : SEC_RULES LCB rule_list RCB					        { parsed_rules := $3 }

rule_list :                                                     { [] }
                    | rule SEMICOLON rule_list                  { $1::$3 }

rule :              ident paramlist EQUAL applicative_term      { $1,$2,$4 }
;

															
applicative_term :	 terminal_nonterminal    					{ $1 }
                    | application								{ $1 }
;

terminal_nonterminal: ident										{ if List.exists (function a,_ -> a = $1) !parsed_nonterminals then
																	Nt($1)
																  else if List.exists (function a,_ -> a = $1) !parsed_terminals then
																	Tm($1)
																  else
																	Var($1)
																}


application :	  bracketed_application bracketed_application	{ App($1,$2) }
				| application bracketed_application				{ App($1,$2) }
;


bracketed_application :             LP application RP			{ $2 }
								| terminal_nonterminal			{ $1 }
					
;




paramlist:														{ [] }
					| ident paramlist							{ $1::$2 }
;

ident         : ATOM											{ $1 }            
              ;

