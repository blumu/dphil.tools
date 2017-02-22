HOG Tool
========

By William Blum. 2007-2017

> NOTE: The latest official release of this tool from 31 Oct 2008 was built on tested with version 1.9.6.2 of the F# compiler.
> The latest sources on Git were ported to F# 4.1 but not fully tested yet.

## Building
Back in 2008, `msbuild` and `Visual Studio` build tools did not work out-of-the box on Linux.
In order to support building on Linux, [OMake](http://omake.metaprl.org/index.html) was chosen
as the primary build system used for the project, using custom OMake rule to add support for F# and C#
in Omake. 10 years later, the F# tooling support on Linux has significantly improved so it should be possible to build the project on Linux using
`msbuild`, though I have not tested it.

As of 2017 the project is built using the FAKE tool.

### Dependencies

The following dependencies are required to build HOG:
- F# compiler. The original release from October 2008 was built on tested with version 1.9.6.2. Recently the project was ported to F# 4.1
(http://research.microsoft.com/fsharp/fsharp.aspx)
- C# compiler (included in the .Net ramework)
(http://www.microsoft.com/downloads/details.aspx?familyid=0856eacb-4362-4b0d-8edd-aab15c5e04f5&displaylang=en)
- Resource compiler (resgen.exe). It comes with the [.NET SDK](http://www.microsoft.com/downloads/details.aspx?FamilyID=fe6f2099-b7b4-4f47-a244-c96d69c35dec&displaylang=en)
as well as the [Windows SDK](http://www.microsoft.com/downloads/details.aspx?familyid=C2B1E300-F358-4523-B479-F53D234CDCCF&displaylang=en), install either of them.
- Microsoft Automatic Graph Layout engine for .NET (https://www.microsoft.com/en-us/research/project/microsoft-automatic-graph-layout/)

### Environment

The following environment variables need to be set:
- FXTOOLS Path to C# compiler (default: C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727)
- SDKTOOLS Path to resgen.exe (default: C:\Program Files\Microsoft SDKs\Windows\v6.0)
- FSHARP_HOME Path to F# compiler (default: C:\Program Files\FSharp-1.9.6.2)

## Building

To compile just run omake in the directory where you have uncompressed the HOG zip file:
        
        .\.paket\paket.bootstrapper.exe
        .\.paket\paket.exe restore
        .\packages\FAKE\tools\Fake.exe  build

## Installation

HOG needs to access the following DLLs from the MSAGL library in order to render the computation graphs:

    Microsoft.Msagl.dll
    Microsoft.Msagl.Drawing.dll
    Microsoft.Msagl.GraphViewerGDI.dll

These files are included in the binary package of HOG which can be downloaded from my
website (http://william.famille-blum.org/research/hog_exe.zip). To make sure HOG can find them 
you can copy the DLLs next to the HOG main executable file (HOG.exe).
