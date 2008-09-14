#light 
open System.IO

(** Raised by the parser when a section in missing in the file **)
exception MissingSection;;


exception ParseError of string

let parse_file parser lexer (fname:string) = 

    try
        let str  = new StreamReader(fname) in
        let text = str.ReadToEnd () in
        str.Close();
        
        !Common.Debug_print ("Opening "^fname^Common.eol);
        let stream =  new StreamReader(fname) 

        // Create the lexer, presenting the bytes to the lexer as ASCII regardless of the original
        // encoding of the stream (the lexer specification 
        // is designed to consume ASCII)
        let lexbuf = Lexing.from_text_reader System.Text.Encoding.ASCII stream 
        try             
            // Call the parser             
            let parsed_object = try  parser lexer lexbuf
                                with  MissingSection -> raise (ParseError "Bad file format: some compulsory section is missing!")
                                    | e ->  let pos = lexbuf.EndPos
                                            raise (ParseError ((sprintf "error near line %d, character %d" (pos.pos_lnum+1) (pos.pos_cnum-pos.pos_bol+1))^Common.eol^e.ToString()^"parsing aborted!"^Common.eol))

            stream.Close();
            
            //Debug_print ("File content:"^eol^" "^text^eol);
            Some(parsed_object)

        finally stream.Close();
    with   :? System.IO.FileNotFoundException -> !Common.Debug_print ("File not found! ("^fname^")"); None
         | ParseError(msg) -> !Common.Debug_print msg; None
;;


(** Return the extension part of a filename **)
let get_file_extension filename =
    try 
        let extpart = String.rindex filename '.' in
        String.sub filename (extpart+1) ((String.length filename)-extpart-1)        
    with Not_found -> ""
;;