#include "pdfsync.h"
#include <iostream>
#include <fstream>

#define WINEDT_PATTERN    "\"[Open(|%f|);SelPar(%l,8)]\""

void main()
{
    Pdfsync sync("C:\\Users\\William\\Documents\\dphil\\latex\\Current\\thesis\\TeXAux\\thesis.pdfsync");
    sync.process_syncfile();
    UINT line, col;
    char srcname[_MAX_PATH];
    UINT err = sync.pdf_to_source(4,5854576,50717736, srcname,_countof(srcname),&line,&col); // record 101
    if (err)
      cout << "cannot sync from pdf to source!" <<endl;
    else
      cout << "source file: " << srcname << " line: " << line << " column: " << col <<endl;

    
    char pattern[_MAX_PATH];

    sprintf_s(pattern, "\"[Open(|%s|);SelPar(%d,8)]\"", srcname, line);

    ShellExecuteA(NULL, NULL,
          "C:\\Program Files\\WinEdt Team\\WinEdt\\winedt.exe", 
          pattern, NULL, SW_SHOWNORMAL);

}

//"C:\Program Files\WinEdt Team\WinEdt\winedt.exe" "[Open(|%f|);SelPar(%l,8)]"