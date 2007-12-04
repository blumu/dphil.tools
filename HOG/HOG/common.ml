// function to detect if the assembly is running on Mono
let IsRunningOnMono = 
    System.Type.GetType ("Mono.Runtime") <> null;;
    
    
let string_of_char =
        String.make 1;;