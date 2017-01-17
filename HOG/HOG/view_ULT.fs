(** Description: View used for Simply-typed lambda-terms
    Author:      William Blum
**)
module View.UntypedLambdaTerm

open System.Drawing
open System.Windows.Forms
open Common
open Type
open Lnf

open Compgraph
open Ulc

let keywords = 
   [  "lam";"Lam";"Lambda";"lambda";
      "in";
      "let" ]
;;

let colorizeCode(rtb: # RichTextBox) = 
    let text = rtb.Text 
    rtb.SelectAll()
    rtb.SelectionColor <- rtb.ForeColor

    keywords |> List.iter (fun keyword -> 
        let mutable keywordPos = rtb.Find(keyword, RichTextBoxFinds.MatchCase ||| RichTextBoxFinds.WholeWord)
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

            keywordPos <- rtb.Find(keyword, keywordPos + rtb.SelectionLength, RichTextBoxFinds.MatchCase ||| RichTextBoxFinds.WholeWord)
    );
    rtb.Select(0, 0)

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

    
    val mutable ulcExpression : ulc_expression;
    val mutable vartmtypes : (ident*typ) list;
    val mutable compgraph : computation_graph;
    val mutable filename : string;

    new (filename, _ulcExpression) as this =
       { 
            ulcExpression = _ulcExpression;
            vartmtypes = [];
            compgraph = create_empty_graph();
            filename = "";
       }
       then 
        // convert the term to LNF
        let term = Ulc.ulcexpression_to_ulcterm [] this.ulcExpression
        let term_as_lnf = Ulc.ulcterm_to_lnf term

        this.InitializeComponent term_as_lnf

        // 
        // codeRichTextBox
        // 
        this.codeRichTextBox.Text <- string_of_ulc_expression (this.ulcExpression)
                                    
        colorizeCode this.codeRichTextBox;

        this.Text <- sprintf "Untyped lambda term - %s" filename
        this.filename <- filename

        // create the computation graph from the LNF of the term
        this.compgraph <- lnf_to_graph term_as_lnf
  end
