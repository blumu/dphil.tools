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


// RichtxtConsoleox - text area 
let txtConsole = new RichTextBox()
txtConsole.Dock <- DockStyle.Bottom
txtConsole.ReadOnly <- true

// text
let setText s = txtConsole.Text <- s; txtConsole.SelectionStart <- txtConsole.TextLength;
let getText () = txtConsole.Text
setText ("Console:"^eol)


// redirect the debug output to the console textbox
Common.Debug_print := function str -> setText (getText()^eol^str);;

let mainform = new System.Windows.Forms.Form()

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
                                open_file Sys.argv.(1));
mainform.IsMdiContainer <- true;
mainform.Width  <- 1000
mainform.Height <- 600
mainform.Text <- "HOG Version "^VERSION
mainform.Controls.Add(txtConsole)

// menu bar and menus 
let mMain = mainform.Menu <- new MainMenu()
let mFile = mainform.Menu.MenuItems.Add("&File")
let mWindowList = mainform.Menu.MenuItems.Add("&Window")
let mHelp = mainform.Menu.MenuItems.Add("&Help")
mWindowList.MdiList <- true

// menu items 
let miOpen  = new MenuItem("Open &scheme...")
let miOpenStlt = new MenuItem("Open simply-typed lambda &term...")
let miOpenUlt = new MenuItem("Open &untyped lambda-term...")
let miOpenWs = new MenuItem("Open &worksheet...")
let miQuit  = new MenuItem("&Quit")
let miAbout = new MenuItem("&About...")

let _ = mFile.MenuItems.Add(miOpen)
let _ = mFile.MenuItems.Add(miOpenStlt)
let _ = mFile.MenuItems.Add(miOpenUlt)
let _ = mFile.MenuItems.Add(miOpenWs)
let _ = mFile.MenuItems.Add(miQuit)
let _ = mHelp.MenuItems.Add(miAbout)


type FileType = RecSchemeFile | SimplyTypedTermFile | UntypedTermFile | Worksheet

// openfile dialog box
let open_dialog defaulttype _ = 
    let d = new OpenFileDialog() in 
    d.Filter <- "Higher-order recursion scheme *.rs|*.rs|Simply-typed lambda term *.stlt|*.stlt|Untyped lambda term *.ult|*.ult|Traversal worksheet *.xml|*.xml|All files *.*|*.*";
    d.FilterIndex <- match defaulttype with RecSchemeFile -> 1 | SimplyTypedTermFile -> 2 | UntypedTermFile -> 3 | Worksheet -> 4 ;
    if d.ShowDialog() = DialogResult.OK then
        open_file d.FileName

let opAbout _ = 
    MessageBox.Show("HOG version "^VERSION^" by William Blum, 2007-2017"^Common.eol^"Graphs are generated using the MSAGL library from Microsoft.","About HOG") |> ignore

let opExitForm _ = mainform.Close ()

// callbacks 
miOpen.Click.Add(open_dialog RecSchemeFile)
miOpenStlt.Click.Add(open_dialog SimplyTypedTermFile)
miOpenUlt.Click.Add(open_dialog UntypedTermFile)
miOpenWs.Click.Add(open_dialog Worksheet)
miQuit.Click.Add(opExitForm)
miAbout.Click.Add(opAbout)
  

/// <summary>
/// The main entry point for the application.
/// </summary>
#if COMPILED
[<STAThread()>]
do Application.Run(mainform)
#endif