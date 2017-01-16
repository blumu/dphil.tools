HOG Tool
========

By William Blum. 2007-20017

> NOTE: The latest official release of this tool from 31 Oct 2008 was built on tested with version 1.9.6.2 of the F# compiler.
> The latest sources on Git were ported to F# 4.1 but not fully tested yet.

## Building
Back in 2008, `msbuild` and `Visual Studio` build tools did not work out-of-the box on Linux.
In order to support building on Linux, [OMake](http://omake.metaprl.org/index.html) was chosen
as the primary build system used for the project, using custom OMake rule to add support for F# and C#
in Omake. 10 years later, the F# tooling support on Linux has
significantly improved so it should be possible to build the project on Linux using
`msbuild`, though I have not tested it.

### Original instructions from 31 Oct 2008
The following dependencies are required to build HOG:

- (Optional) Omake build system (http://omake.metaprl.org/index.html)
- F# (the latest release was built on tested with version 1.9.6.2), latest sources were ported to F# 4.1  but not fully tested:
http://research.microsoft.com/fsharp/fsharp.aspx
- C# compiler (included in the .Net 2.0 Framework)
http://www.microsoft.com/downloads/details.aspx?familyid=0856eacb-4362-4b0d-8edd-aab15c5e04f5&displaylang=en
- resgen.exe utility. It comes with the .Net SDK as well as the Windows SDK. Install either of them:
    - .NET SDK: (http://www.microsoft.com/downloads/details.aspx?FamilyID=fe6f2099-b7b4-4f47-a244-c96d69c35dec&displaylang=en)
    - Windows SDK: (http://www.microsoft.com/downloads/details.aspx?familyid=C2B1E300-F358-4523-B479-F53D234CDCCF&displaylang=en)
- Microsoft Automatic Graph Layout engine for .NET (http://research.microsoft.com/research/msagl/)

The following environment variables need to be set:
- FXTOOLS Path to C# compiler (default: C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727)
- SDKTOOLS Path to resgen.exe (default: C:\Program Files\Microsoft SDKs\Windows\v6.0)
- FSHARP_HOME Path to F# compiler (default: C:\Program Files\FSharp-1.9.6.2)

To compile just run omake in the directory where you have uncompressed the HOG zip file:
        omake

### Building with OMake for Mono

To build on or for Mono use the following optional parameters:

-for compatibility with the Mono platform:

    omake FORMONO=

-for compilation with the Mono toolchain:

    omake WITHMONO=

-for debug build:

    omake DEBUG=

## Installation

HOG needs to access the following DLLs from the MSAGL library in order to render the computation graphs:

    Microsoft.Msagl.dll
    Microsoft.Msagl.Drawing.dll
    Microsoft.Msagl.GraphViewerGDI.dll

These files are included in the binary package of HOG which can be downloaded from my website (http://william.famille-blum.org/research/hog_exe.zip). To make sure HOG can find them you can either copy the DLLs next to the HOG main executable file (HOG.exe) or you can add them to the .NET Global Assembly Cache as follows: Launch "Visual Studio 2008 Command Prompt" and enter:

    gacutil /i "C:\Program Files\Microsoft\Microsoft Automatic Graph Layout\bin\Microsoft.Msagl.dll"
    gacutil /i "C:\Program Files\Microsoft\Microsoft Automatic Graph Layout\bin\Microsoft.Msagl.Drawing.dll"
    gacutil /i "C:\Program Files\Microsoft\Microsoft Automatic Graph Layout\bin\Microsoft.Msagl.GraphViewerGDI.dll"

(Replace 'C:\Program Files\Microsoft\Microsoft Automatic Graph Layout\bin' by the path to the directory where you copied the DLLs.)