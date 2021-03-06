﻿(** Description: View used for Simply-typed lambda-terms
    Author:      William Blum
**)
module View.SimplyTypedLambdaTerm

open Common
open Type
open Lnf
open Compgraph
open Coreml

let keywords = 
   [  "and";"as";
      "begin"; 
      "do";"done";
      "else";"end";
      "false";"for";"fun";"function";
      "if";"in";
      "let";
      "match";
      "or"; 
      "rec";
      "then";"to";"true";
      "while";"with";"="; "->"; "|"; "|-"; ]
;;

/// Form used to show a simply-typed lambda term
type Form = 
  class
    inherit GUI.Lambdaterm

    member this.InitializeComponent lnfterm =
        // 
        // graphButton
        // 
        this.btGraph.Click.Add(fun _ -> Traversal_form.ShowCompGraphWindow this.MdiParent this.filename this.compgraph ["",lnfterm]);
        
        //
        // Play traversal
        //
        this.btCalculator.Click.Add(fun _ -> Traversal_form.ShowTraversalCalculatorWindow this.MdiParent this.filename this.compgraph ["",lnfterm] (fun _ _ -> ()));
        
        //select_pstrcontrol (createAndAddPstringCtrl [||])

    
    val mutable lmdterm : ml_termincontext;
    val mutable vartmtypes : (ident*typ) list;
    val mutable compgraph : computation_graph;
    val mutable filename : string;

    new (filename,newterm) as this =
       { 
            lmdterm = newterm;
            vartmtypes = [];
            compgraph = create_empty_graph();
            filename = "";
       }
       then 
        let annotterm = annotate_termincontext this.lmdterm
        // convert the term to LNF
        let lnfterm = //try 
                        annotatedterm_to_lnf annotterm
                      //with MissingVariableInContext -> 

        this.InitializeComponent lnfterm;

        // 
        // codeRichTextBox
        // 
        this.codeRichTextBox.Text <- (string_of_polyalphabet_aux ", " (fst annotterm))
                                    ^"|-"^(string_of_mlterm (snd this.lmdterm))
                                    ^" : "^(string_of_polytype (fst (snd annotterm)));
                                    
        Richtext.colorizeCode this.codeRichTextBox keywords

        this.Text <- ("Simply-typed lambda term - "^filename);
        this.filename <- filename;

        this.outputTextBox.Text <- "Eta-long normal form:"^eol
                                   ^(lnf_to_string lnfterm);

        // create the computation graph from the LNF of the term
        this.compgraph <- lnf_to_graph lnfterm
  end
