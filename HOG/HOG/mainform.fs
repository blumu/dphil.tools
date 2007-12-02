#light

(** Application version **)
let VERSION = "0.0.1";;

open System
open System.IO  
open System.Windows.Forms
open System.IO
open List
open Printf
open Hog_lexer
open Lexing

// Note: this must be executed *before* any control is created! If not, the resources will not
// be loaded properly.
do Application.EnableVisualStyles();

   

// RichTextBox - text area 
let textB = new RichTextBox()
textB.Dock <- DockStyle.Fill
textB.ReadOnly <- true

// text
let setText s = textB.Text <- s; textB.SelectionStart <- textB.TextLength;
let getText s = textB.Text
setText     "Console:\n"


let Debug_print str =
    setText (getText()^"\n"^str)
;;

exception ParseError of string

let parse_file parser lexer (fname:string) = 

    try
        let str  = new StreamReader(fname) in
        let text = str.ReadToEnd () in
        str.Close();
        
        Debug_print ("Opening "^fname^"\n");
        let stream =  new StreamReader(fname) 

        // Create the lexer, presenting the bytes to the lexer as ASCII regardless of the original
        // encoding of the stream (the lexer specification 
        // is designed to consume ASCII)
        let lexbuf = Lexing.from_text_reader System.Text.Encoding.ASCII stream 
        try             
            // Call the parser             
            let parsed_object = try  parser lexer lexbuf
                                with  Parsing.MissingSection -> raise (ParseError "Bad file format: some compulsory section is missing!")
                                    | e ->  let pos = lexbuf.EndPos
                                            raise (ParseError ((sprintf "error near line %d, character %d\n" (pos.pos_lnum+1) (pos.pos_cnum - pos.pos_bol +1))^"\n"^e.ToString()^"parsing aborted!\n"))

            stream.Close();
            
            (* Load the Windows form for the recursion scheme *)
            Debug_print ("File content:\n "^text^"\n");
            Some(parsed_object)

        finally stream.Close();
    with   :? System.IO.FileNotFoundException -> Debug_print ("File not found! ("^fname^")"); None
         | ParseError(msg) -> Debug_print msg; None
;;

 
let mainform =
    { new Form()
      with OnLoad(_) =
         if Array.length Sys.argv = 2 then
             if Sys.argv.(1) = "-help" then
                begin 
                  printf "usage: hog.exe <file>\n";
                  exit 1;
                end
              else
                ignore(parse_file Hog_parser.hog_specification Hog_lexer.token Sys.argv.(1));
      }

mainform.Width  <- 400
mainform.Height <- 300
mainform.Text <- "HOG Version "^VERSION
mainform.Controls.Add(textB)

// menu bar and menus 
let mMain = mainform.Menu <- new MainMenu()
let mFile = mainform.Menu.MenuItems.Add("&File")
let mHelp = mainform.Menu.MenuItems.Add("&Help")

// menu items 
let miOpen  = new MenuItem("&Open scheme...")
let miOpenLmd  = new MenuItem("&Open term...")
let miQuit  = new MenuItem("&Quit")
let miAbout = new MenuItem("&About...")

mFile.MenuItems.Add(miOpen)
mFile.MenuItems.Add(miOpenLmd)
mFile.MenuItems.Add(miQuit)
mHelp.MenuItems.Add(miAbout)


type FileType = RecSchemeFile | TermFile

// open dialog for recursion scheme
let open_dialog defaulttype _ = 
    let d = new OpenFileDialog() in 
    d.Filter <- "Higher-order recursion scheme *.rs|*.rs|Lambda term *.lmd|*.lmd|All files *.*|*.*";
    d.FilterIndex <- match defaulttype with RecSchemeFile -> 0 | TermFile -> 1;
    if d.ShowDialog() = DialogResult.OK then
        match defaulttype with 
            RecSchemeFile ->  
                     match parse_file Hog_parser.hog_specification Hog_lexer.token d.FileName with
                         None -> ()
                         | Some(o) -> let form = new Horsform.HorsForm(d.FileName, o)
                                      ignore(form.Show())
          | TermFile ->
                     //parse_file Ml_parser.program Ml_lexer.token d.FileName      
                     match parse_file Hog_parser.hog_specification Hog_lexer.token d.FileName with
                         None -> ()
                         | Some(o) -> let form = new Horsform.HorsForm(d.FileName, o)
                                      ignore(form.Show())          


let opAbout _ = 
    MessageBox.Show("HOG by William Blum, 2007","About HOG") |> ignore

let opExitForm _ = mainform.Close ()

// callbacks 
miOpen.Click.Add(open_dialog RecSchemeFile)
miOpenLmd.Click.Add(open_dialog TermFile)
miQuit.Click.Add(opExitForm)
miAbout.Click.Add(opAbout)
  

/// <summary>
/// The main entry point for the application.
/// </summary>
#if COMPILED
[<STAThread()>]
do Application.Run(mainform)
#endif