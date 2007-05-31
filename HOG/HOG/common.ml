// function to detect if the assembly is running on Mono
let IsRunningOnMono = 
    System.Type.GetType ("Mono.Runtime") <> null;;