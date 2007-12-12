﻿(** $Id$
    Description: Lambda-term window
    Author:      William Blum
**)

open System
(* open System.Collections.Generic *)
open System.ComponentModel
open System.Data
open System.Drawing
open System.Text
open System.Windows.Forms
open System.IO
open Common
open Printf
open Type
open Lnf
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
      "while";"with";"="; "->"; "|"; "|-"; ]   ;;

      
#light;;
let colorizeCode(rtb: # RichTextBox) = 
    let text = rtb.Text 
    rtb.SelectAll()
    rtb.SelectionColor <- rtb.ForeColor

    keywords |> List.iter (fun keyword -> 
        let mutable keywordPos = rtb.Find(keyword, Enum.combine[RichTextBoxFinds.MatchCase; RichTextBoxFinds.WholeWord])
        while (keywordPos <> -1) do 
            let underscorePos = text.IndexOf("_", keywordPos)
            let commentPos = text.LastIndexOf("//", keywordPos)
            let newLinePos = text.LastIndexOf('\n', keywordPos)
            let mutable quoteCount = 0
            let mutable quotePos = text.IndexOf("\"", newLinePos + 1, keywordPos - newLinePos)
            while (quotePos <> -1) do
                quoteCount <- quoteCount + 1
                quotePos <- text.IndexOf("\"", quotePos + 1, keywordPos - (quotePos + 1))
            
            if (newLinePos >= commentPos && 
                underscorePos <> keywordPos + rtb.SelectionLength  && 
                quoteCount % 2 = 0) 
             then
                rtb.SelectionColor <- Color.Blue;

            keywordPos <- rtb.Find(keyword, keywordPos + rtb.SelectionLength, Enum.combine[RichTextBoxFinds.MatchCase; RichTextBoxFinds.WholeWord])
    );
    rtb.Select(0, 0)

#light

        
type TermForm = 
  class
    inherit GUI.Lambdaterm as base

    member this.InitializeComponent lnfterm =
        // 
        // codeRichTextBox
        // 
        this.codeRichTextBox.Text <- string_of_mltermincontext this.lmdterm;
        colorizeCode this.codeRichTextBox;

        // 
        // graphButton
        // 
        this.btExportGraph.Click.Add(fun e -> Traversal_form.ShowCompGraphWindow this.MdiParent this.filename this.compgraph ["",lnfterm]);
        
        //
        // Play traversal
        //
        this.btCalculator.Click.Add( fun e -> Traversal_form.ShowTraversalCalculatorWindow this.MdiParent this.filename this.compgraph ["",lnfterm] );

    
    val mutable lmdterm : ml_termincontext;
    val mutable vartmtypes : (ident*typ) list;
    val mutable compgraph : computation_graph;
    val mutable filename : string;

    new (filename,newterm) as this =
       { lmdterm = newterm;
         vartmtypes = [];
         compgraph = create_empty_graph();
         filename = "";
         }
       
       then 
        // convert the term to LNF
        let lnfterm = //try 
                        lmdterm_to_lnf this.lmdterm
                      //with MissingVariableInContext -> 

        this.InitializeComponent lnfterm;

        this.Text <- ("Simply-typed lambda term - "^filename);
        this.filename <- filename;

        this.outputTextBox.Text <- "Eta-long normal form:"^eol
                                   ^(lnf_to_string lnfterm);

        // create the computation graph from the LNF of the term
        this.compgraph <- lnfrs_to_graph [("",lnfterm)]
  end
