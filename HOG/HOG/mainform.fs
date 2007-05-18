#light

open System
open System.IO  
open System.Windows.Forms
open System.IO
open List
open Printf
open Hog_lexer
open Lexing


let mainform = new Form()
mainform.Width  <- 400
mainform.Height <- 300
mainform.Visible <- true 
mainform.Text <- "HOG"

// menu bar and menus 
let mMain = mainform.Menu <- new MainMenu()
let mFile = mainform.Menu.MenuItems.Add("&File")
let mHelp = mainform.Menu.MenuItems.Add("&Help")

// menu items 
let miOpen  = new MenuItem("&Open...")
let miQuit  = new MenuItem("&Quit")
let miAbout = new MenuItem("&About...")

mFile.MenuItems.Add(miOpen)
mFile.MenuItems.Add(miQuit)
mHelp.MenuItems.Add(miAbout)

// RichTextBox - text area 
let textB = new RichTextBox()
textB.Dock <- DockStyle.Fill
textB.ReadOnly <- true
mainform.Controls.Add(textB)

// text state 
let setText s = textB.Text <- s; textB.SelectionStart <- textB.TextLength;
let getText s = textB.Text

// initial state   
setText     "Console:\n"



let Debug_print str =
    setText (getText()^"\n"^str)
;;

exception ParseError of string

let parse_file (fname:string) = 

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
            let parsed_hors = try  Hog_parser.hog_specification Hog_lexer.token lexbuf
                              with  Parsing.MissingSection -> raise (ParseError "Bad file format: some compulsory section is missing!")
                                  | e ->  let pos = lexbuf.EndPos
                                          let err = (sprintf "error near line %d, character %d\n" pos.pos_lnum  (pos.pos_cnum - pos.pos_bol))^"\n"^e.ToString()
                                          Debug_print err;
                                          raise (ParseError "parsing aborted!")

            (* Load the recursion scheme form *)
            Debug_print ("File content:\n "^text^"\n");

            let form = new Horsform.HorsForm(fname, parsed_hors) in
            ignore(form.Show())
        finally stream.Close();
    
    with :? System.IO.FileNotFoundException -> Debug_print ("File not found! ("^fname^")");
;;

  

// open dialog
let openRecschemeFile _ = 
    let d = new OpenFileDialog() in 
    d.Filter <- "Higher-order recursion scheme *.rs|*.rs|All files *.*|*.*";
    d.FilterIndex <- 1;
    if d.ShowDialog() = DialogResult.OK then
        ignore(parse_file d.FileName)

let opAbout _ = 
    MessageBox.Show("HOG by William Blum, 2007","About HOG") |> ignore

let opExitForm _ = mainform.Close ()

// callbacks 
let _ = miOpen.Click.Add(openRecschemeFile)
let _ = miQuit.Click.Add(opExitForm)
let x = miAbout.Click.Add(opAbout)

     

let main() = 
    Application.EnableVisualStyles();
    mainform.Show();
    //mainform.add_Load (fun _ _ -> 
    if Array.length Sys.argv = 2 then
         if Sys.argv.(1) = "-help" then
            begin 
              printf "usage: hog.exe <file>\n";
              exit 1;
            end
          else
            ignore(parse_file Sys.argv.(1));
            
             //);

    //ignore(mainform.ShowDialog())
    Application.Run(mainform) 
;;


/// <summary>
/// The main entry point for the application.
/// </summary>
[<STAThread()>]
do main();;
