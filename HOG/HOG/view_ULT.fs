(** Description: View used for Simply-typed lambda-terms
    Author:      William Blum
**)
module View.UntypedLambdaTerm

let keywords = 
   [  "lam";"Lam";"Lambda";"lambda";
      "in";
      "let" ]
;;

/// Form used to show a simply-typed lambda term
type Form(filename, ulcExpression) as this = 
  class
    inherit GUI.Lambdaterm()

    // convert the term to LNF
    let term = Ulc.ulcexpression_to_ulcterm [] ulcExpression
    let term_as_lnf = Ulc.ulcterm_to_lnf term
        
    // create the computation graph from the LNF of the term
    let compgraph = Compgraph.lnf_to_graph term_as_lnf

    let initializeComponent (form:#GUI.Lambdaterm) lnfterm =
        // "Show graph" button
        form.btGraph.Click.Add(fun _ -> Traversal_form.ShowCompGraphWindow form.MdiParent filename compgraph ["",lnfterm]);
        // Play traversal
        form.btCalculator.Click.Add(fun _ -> Traversal_form.ShowTraversalCalculatorWindow form.MdiParent filename compgraph ["",lnfterm] (fun _ _ -> ()));

        // Rich-text code control
        form.codeRichTextBox.Text <- Ulc.string_of_ulc_expression (ulcExpression)
        Richtext.colorizeCode form.codeRichTextBox keywords
        this.Text <- sprintf "Untyped lambda term - %s" filename

    do
        initializeComponent this term_as_lnf

    member this.InitializeComponent lnfterm =
        initializeComponent this lnfterm

  end
