(**
    Description: Main window
    Author:      William Blum
**)
module mainform

(** Application version **)
let VERSION = "0.0.13";;

open System
open System.Windows.Forms
open FSharp.Compatibility.OCaml
open Common

// Note: this must be executed *before* any control is created! If not, the resources will not
// be loaded properly.
do Application.EnableVisualStyles();

let mainform = new GUI.Main()

// Console text
let setText s = mainform.txtConsole.Text <- s; mainform.txtConsole.SelectionStart <- mainform.txtConsole.TextLength;
let getText () = mainform.txtConsole.Text
setText ("Console:"^eol)


// redirect the debug output to the console textbox
Common.Debug_print := function str -> setText (getText()^eol^str);;

// open a file containing either a term or a Rec scheme
let open_file filename =
    match Parsing.get_file_extension filename with 
    |  "rs" ->  match Parsing.parse_file Hog_parser.hog_specification Hog_lexer.token filename with
                | None -> ()
                | Some(o) -> (* Load the Windows form for the recursion scheme *)
                            let form = new View.HigherOrderRecursionScheme.Form(filename, o)
                            form.MdiParent <- mainform;
                            form.WindowState<-FormWindowState.Maximized;
                            ignore(form.Show())

    | "stlt" -> match Parsing.parse_file Ml_parser.term_in_context Ml_lexer.token filename with
                | None -> ()
                | Some(o) -> let form = new View.SimplyTypedLambdaTerm.Form(filename, o)
                             form.MdiParent <- mainform;
                             form.WindowState<-FormWindowState.Maximized;
                             ignore(form.Show())

    | "ult" -> match Parsing.parse_file Ulc_parser.term Ulc_lexer.token filename with
               | None -> ()
               | Some(o) -> let form = new View.UntypedLambdaTerm.Form(filename, o)
                            form.MdiParent <- mainform;
                            form.WindowState<-FormWindowState.Maximized;
                            ignore(form.Show())
                                      
                                      
    | "xml" -> Traversal_form.open_worksheet mainform filename
          
    | _ -> !Common.Debug_print ("Unknown file format!"^eol)
;;


mainform.Load.Add( fun _ -> if Array.length Sys.argv = 2 then
                             if Sys.argv.(1) = "-help" then
                                begin 
                                  ignore(MessageBox.Show(("usage: hog.exe <file>"^eol), "Usage"));
                                  exit 0;
                                end
                              else
                                let fullPath = System.IO.Path.GetFullPath(Sys.argv.(1))
                                open_file fullPath);
mainform.Text <- "HOG Version "^VERSION

type FileType = RecSchemeFile | SimplyTypedTermFile | UntypedTermFile | Worksheet

// openfile dialog box
let open_dialog defaulttype _ = 
    let d = new OpenFileDialog() in 
    d.Filter <- "Higher-order recursion scheme *.rs|*.rs|Simply-typed lambda term *.stlt|*.stlt|Untyped lambda term *.ult|*.ult|Traversal worksheet *.xml|*.xml|All files *.*|*.*";
    d.FilterIndex <- match defaulttype with RecSchemeFile -> 1 | SimplyTypedTermFile -> 2 | UntypedTermFile -> 3 | Worksheet -> 4 ;
    if d.ShowDialog() = DialogResult.OK then
        open_file d.FileName

// Add an "open" menu for each type of parseable object
let mFile = mainform.fileToolStripMenuItem

let addMenu (text:string) fileType =
    let menu = new ToolStripMenuItem(text)
    mFile.DropDownItems.Insert(0, menu)
    menu.Click.Add(open_dialog fileType)

addMenu "Open &untyped lambda-term..." UntypedTermFile
addMenu "Open simply-typed lambda &term..." SimplyTypedTermFile
addMenu "Open &scheme..." RecSchemeFile
addMenu "Open &worksheet..." Worksheet

// callbacks 
mainform.quitToolStripMenuItem.Click.Add(fun _ ->
    Application.Exit()
)

mainform.aboutToolStripMenuItem .Click.Add(fun _ -> 
    MessageBox.Show("HOG version "^VERSION^" by William Blum, 2007-2017"^Common.eol^"Graphs are generated using the MSAGL library from Microsoft.","About HOG") |> ignore
)
  

/// <summary>
/// The main entry point for the application.
/// </summary>
#if COMPILED
[<STAThread()>]
do Application.Run(mainform)
#endif