(** $Id$
    Description: Main window
    Author:      William Blum
**)

#light

(** Application version **)
let VERSION = "0.0.5";;

open System
open System.Windows.Forms
open System.IO
open List
open Common
open Printf
open Hog_lexer
open Lexing

// Note: this must be executed *before* any control is created! If not, the resources will not
// be loaded properly.
do Application.EnableVisualStyles();

   

// RichtxtConsoleox - text area 
let txtConsole = new RichTextBox()
txtConsole.Dock <- DockStyle.Bottom
txtConsole.ReadOnly <- true

// text
let setText s = txtConsole.Text <- s; txtConsole.SelectionStart <- txtConsole.TextLength;
let getText s = txtConsole.Text
setText     ("Console:"^eol)


// redirect the debug output to the console textbox
Common.Debug_print := function str -> setText (getText()^eol^str);;



let mainform = new System.Windows.Forms.Form()



// open a file containing either a term or a Rec scheme
let open_file filename =
    match Parsing.get_file_extension filename with 
            "rs" ->  match Parsing.parse_file Hog_parser.hog_specification Hog_lexer.token filename with
                         None -> ()
                         | Some(o) -> (* Load the Windows form for the recursion scheme *)
                                      let form = new Horsform.HorsForm(filename, o)
                                      form.MdiParent <- mainform;
                                      form.WindowState<-FormWindowState.Maximized;
                                      ignore(form.Show())
          | "lmd" -> match Parsing.parse_file Ml_parser.term_in_context Ml_lexer.token filename with
                         None -> ()
                         | Some(o) -> let form = new Lmdtermform.TermForm(filename, o)
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
let miOpenLmd = new MenuItem("Open &term...")
let miOpenWs =  new MenuItem("Open &worksheet...")
let miQuit  = new MenuItem("&Quit")
let miAbout = new MenuItem("&About...")

mFile.MenuItems.Add(miOpen)
mFile.MenuItems.Add(miOpenLmd)
mFile.MenuItems.Add(miOpenWs)
mFile.MenuItems.Add(miQuit)
mHelp.MenuItems.Add(miAbout)



type FileType = RecSchemeFile | TermFile | Worksheet

// openfile dialog box
let open_dialog defaulttype _ = 
    let d = new OpenFileDialog() in 
    d.Filter <- "Higher-order recursion scheme *.rs|*.rs|Lambda term *.lmd|*.lmd|Traversal worksheet *.xml|*.xml|All files *.*|*.*";
    d.FilterIndex <- match defaulttype with RecSchemeFile -> 1 | TermFile -> 2 | Worksheet -> 3 ;
    if d.ShowDialog() = DialogResult.OK then
        open_file d.FileName



let opAbout _ = 
    MessageBox.Show("HOG by William Blum, 2007-2008","About HOG") |> ignore

let opExitForm _ = mainform.Close ()

// callbacks 
miOpen.Click.Add(open_dialog RecSchemeFile)
miOpenLmd.Click.Add(open_dialog TermFile)
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