// By william blum (http://william.famille-blum.org/software/index.html)
// Created in September 2006
#define APP_NAME		"LatexDaemon"
#define VERSION_DATE	"2 March 2007"
#define VERSION			0.904

// See changelog.html for the list of changes:.

// TODO:
//  At the moment, messages reporting that some watched file has been modified are not shown while the "make" 
//  thread is running. This is done in order to avoid printf interleaving. A solution would 
//  be to delay the printing of these messages until the end of the execution of the make "thread".
//  Another solution is to implement a separate thread responsible of the output of all other threads.

// Acknowledgment:
// - The MD5 class is a modification of CodeGuru's one: http://www.codeguru.com/Cpp/Cpp/algorithms/article.php/c5087
// - Command line processing routine from The Code Project: http://www.codeproject.com/useritems/SimpleOpt.asp
// - Console color header file (Console.h) from: http://www.codeproject.com/cpp/AddColorConsole.asp
// - Function CommandLineToArgvA and CommandLineToArgvW from http://alter.org.ua/en/docs/win/args/

#define _WIN32_WINNT  0x0400
#define _CRT_SECURE_NO_DEPRECATE
#include <windows.h>
#include <Winbase.h>
#include <conio.h>
#include <string>
#include <iostream>
#include <stdlib.h>
#include <stdio.h>
#include <tchar.h>
#include <direct.h>
#include <sys/stat.h>
#include <time.h>
#include "md5.h"
#include "console.h"
#include "SimpleOpt.h"
#include "SimpleGlob.h"
#include "CommandLineToArgv.h"
using namespace std;


//////////
/// Constants 

// console colors used
#define fgMsg			JadedHoboConsole::fg_green
#define fgErr			JadedHoboConsole::fg_red
#define fgWarning		JadedHoboConsole::fg_yellow
#define fgLatex			JadedHoboConsole::fg_lowhite
#define fgIgnoredfile	JadedHoboConsole::fg_gray
#define fgNormal		JadedHoboConsole::fg_lowhite
#define fgDepFile		JadedHoboConsole::fg_cyan
#define fgCommandLine	JadedHoboConsole::fg_magenta

#define DEFAULTPREAMBLE2_BASENAME       "preamble"
#define DEFAULTPREAMBLE2_FILENAME       DEFAULTPREAMBLE2_BASENAME ".tex"
#define DEFAULTPREAMBLE1_EXT            "pre"

#define PROMPT_STRING					"dameon>"

// Maximal length of an input command line at the prompt
#define PROMPT_MAX_INPUT_LENGTH			_MAX_PATH


// result of timestamp comparison
#define OUT_FRESHER	   0x00
#define SRC_FRESHER	   0x01
#define ERR_OUTABSENT  0x02
#define ERR_SRCABSENT  0x04

// constants corresponding to the different possible jobs which can be exectuted
enum JOB { Rest = 0 , Dvips = 1, Compile = 2, FullCompile = 3 } ;


//////////
/// Prototypes
void RestartMakeThread( JOB makejob, LPCTSTR mainfilebase);
void WatchTexFiles(LPCTSTR texpath, LPCTSTR mainfilebase, CSimpleGlob &glob);
DWORD launch(LPCTSTR cmdline);
bool fullcompile(LPCTSTR texbasename);
bool compile(LPCTSTR texbasename);
int compare_timestamp(LPCTSTR sourcefile, LPCTSTR outputfile);
bool dvips(LPCTSTR texbasename);
bool ps2pdf(LPCTSTR texbasename);
bool bibtex(LPCTSTR texbasename);
bool edit(LPCTSTR texbasename);
bool view(LPCTSTR texbasename);
bool openfolder(LPCTSTR texbasename);


//////////
/// Global variables

// critical section for handling printf across the different threads
CRITICAL_SECTION cs;

// is the preamble stored in an external file? (this option can be changed with a command line parameter)
bool UseExternalPreamble = true;

// Set to true when an external command is running (function launch())
bool ExecutingExternalCommand = false;

// what to do after compilation? (can be change using the -afterjob command argument)
JOB afterjob = Rest;

// handle of the "make" thread
HANDLE hMakeThread = NULL;

// handle of the comment prompt thread
HANDLE hCommandPromptThread;

// Event fired when the make thread needs to be aborted
HANDLE hEvtAbortMake = NULL;

// Event fired when the user wants to quit the program
HANDLE hEvtQuitProgram = NULL;

// Tex initialization file (can be specified as a command line parameter)
string texinifile = "latex"; // use latex by default

// preamble file name and basename
string preamble_filename = "";
string preamble_basename = "";

// Path where the main tex file resides
string texdir;
string texfullpath;


// by default the output extension is set to ".dvi", it must be changed to ".pdf" if pdflatex is used
string output_ext = ".dvi";

// type of the parameter passed to the "make" thread
typedef struct {
	JOB			makejob;
	LPCTSTR		mainfilebasename;
} MAKETHREADPARAM  ;


// define the ID values to indentify the option
enum { 
	// command line options
	OPT_USAGE, OPT_INI, OPT_NOWATCH, OPT_FORCE, OPT_PREAMBLE, OPT_AFTERJOB, 
	// prompt commands
	OPT_HELP, OPT_COMPILE, OPT_FULLCOMPILE, OPT_QUIT, OPT_BIBTEX, OPT_DVIPS, 
	OPT_PS2PDF, OPT_EDIT, OPT_VIEWOUTPUT, OPT_OPENFOLDER
};

// declare a table of CSimpleOpt::SOption structures. See the SimpleOpt.h header
// for details of each entry in this structure. In summary they are:
//  1. ID for this option. This will be returned from OptionId() during processing.
//     It may be anything >= 0 and may contain duplicates.
//  2. Option as it should be written on the command line
//  3. Type of the option. See the header file for details of all possible types.
//     The SO_REQ_SEP type means an argument is required and must be supplied
//     separately, e.g. "-f FILE"
//  4. The last entry must be SO_END_OF_OPTIONS.
//

// command line argument options
CSimpleOpt::SOption g_rgOptions[] = {
    { OPT_USAGE,		_T("-?"),			SO_NONE   },
    { OPT_USAGE,		_T("--help"),		SO_NONE   },
    { OPT_USAGE,		_T("-help"),		SO_NONE    },
    { OPT_INI,			_T("-ini"),			SO_REQ_CMB  },
    { OPT_INI,			_T("--ini"),		SO_REQ_CMB },
    { OPT_PREAMBLE,		_T("-preamble"),	SO_REQ_CMB  },
    { OPT_PREAMBLE,		_T("--preamble"),	SO_REQ_CMB },
    { OPT_AFTERJOB,		_T("-afterjob"),	SO_REQ_CMB  },
    { OPT_AFTERJOB,		_T("--afterjob"),	SO_REQ_CMB },
	{ OPT_NOWATCH,		_T("-nowatch"),		SO_NONE  },
    { OPT_NOWATCH,		_T("--nowatch"),	SO_NONE  },
    { OPT_FORCE,		_T("-force"),		SO_REQ_SEP  },
    { OPT_FORCE,		_T("--force"),		SO_REQ_SEP },
    SO_END_OF_OPTIONS                   // END
};

// prompt commands options
CSimpleOpt::SOption g_rgPromptOptions[] = {
    { OPT_HELP,			_T("-?"),			SO_NONE		},
    { OPT_HELP,			_T("-h"),			SO_NONE		},
    { OPT_HELP,			_T("-help"),		SO_NONE		},
    { OPT_USAGE,		_T("-u"),			SO_NONE		},
    { OPT_USAGE,		_T("-usage"),		SO_NONE		},
    { OPT_BIBTEX,		_T("-b"),			SO_NONE		},
    { OPT_BIBTEX,		_T("-bibtex"),		SO_NONE		},
    { OPT_DVIPS,		_T("-d"),			SO_NONE		},
    { OPT_DVIPS,		_T("-dvips"),		SO_NONE		},
    { OPT_PS2PDF,		_T("-p"),			SO_NONE		},
    { OPT_PS2PDF,		_T("-ps2pdf"),		SO_NONE		},
    { OPT_COMPILE,		_T("-c"),			SO_NONE		},
    { OPT_COMPILE,		_T("-compile"),		SO_NONE		},
    { OPT_FULLCOMPILE,	_T("-f"),			SO_NONE		},
    { OPT_FULLCOMPILE,	_T("-fullcompile"),	SO_NONE		},
    { OPT_INI,			_T("-ini"),			SO_REQ_CMB  },
    { OPT_PREAMBLE,		_T("-preamble"),	SO_REQ_CMB  },
    { OPT_AFTERJOB,		_T("-afterjob"),	SO_REQ_CMB  },
	{ OPT_QUIT,			_T("-q"),			SO_NONE		},
	{ OPT_QUIT,			_T("-quit"),		SO_NONE		},
	{ OPT_EDIT,			_T("-e"),			SO_NONE		},
	{ OPT_EDIT,			_T("-edit"),		SO_NONE		},
	{ OPT_VIEWOUTPUT,	_T("-v"),			SO_NONE		},
	{ OPT_VIEWOUTPUT,	_T("-view"),		SO_NONE		},
	{ OPT_OPENFOLDER,	_T("-o"),			SO_NONE		},
	{ OPT_OPENFOLDER,	_T("-open"),		SO_NONE		},
    SO_END_OF_OPTIONS                   // END
};



////////////////////



// show the usage of this program
void ShowUsage(TCHAR *progname) {
	cout << "USAGE: latexdameon [options] mainfile.tex [dependencies]" <<endl
		 << "where" << endl
		 << "* options can be:" << endl
		 << " --help" << endl 
		 << "   Show this help message." <<endl<<endl
	     << " --nowatch" << endl 
		 << "   Launch the compilation if necessary and then exit without watching for file changes."<<endl<<endl
		 << " --force {compile|fullcompile}" << endl
		 << "   . 'compile' forces the compilation of the .tex file at the start even when no modification is detected." << endl<<endl
		 << "   . 'fullcompile' forces the compilation of the preamble and the .tex file at the start even when no modification is detected." <<endl<<endl
		 << " --ini=inifile" << endl 
		 << "   Set 'inifile' as the initialization format file that will be used to compile the preamble." <<endl<<endl
		 << " --afterjob={dvips|rest}" << endl 
		 << "   . 'dvips' specifies that dvips should be run after a successful compilation of the .tex file," <<endl
		 << "   . 'rest' (default) specifies that nothing needs to be done after compilation."<<endl
		 << " --preamble={none|external}" << endl 
		 << "   . 'none' specifies that the main .tex file does not use an external preamble file."<<endl
		 << "   The current version is not capable of extracting the preamble from the .tex file, therefore if this switch is used the precompilation feature will be automatically desactivated."<<endl
		 << "   . 'external' (default) specifies that the preamble is stored in an external file. The daemon first look for a preamble file called mainfile.pre, if this does not exist it tries preamble.tex and eventually, if neither exists, falls back to the 'none' option."<<endl
		 << endl << "   If the files preamble.tex and mainfile.pre exist but do not correspond to the preamble of your latex document (i.e. not included with \\input{mainfile.pre} at the beginning of your .tex file) then you must set the 'none' option to avoid the precompilation of a wrong preamble." <<endl<<endl
		 << "* dependencies contains a list of files that your main tex file relies on. You can sepcify list of files using jokers, for example '*.tex *.sty'. However, only the dependencies that resides in the same folder as the main tex file will be watched for changes." <<endl<<endl
	     << "INSTRUCTIONS:" << endl
	     << "  1. Move the preamble from your .tex file to a new file named mainfile.pre" << endl 
	     << "  and insert '\\input{mainfile.pre}' at the beginning of your mainfile.tex file," << endl << endl
	     << "  2. start the daemon with the command \"latexdaemon mainfile.tex *.tex\" " << 
		    "(or \"latexdaemon -ini pdflatex mainfile.tex *.tex\" if you want to use pdflatex"<<
			"instead of latex) where main.tex is the main file of your latex project. "  << endl;
}

// update the title of the console
void SetTitle(string state)
{
	SetConsoleTitle((state + " -  "+ texinifile + "Daemon - " + texfullpath).c_str());
}

// Code executed when the -ini option is specified
void ExecuteOptionIni( string optionarg )
{
	texinifile = optionarg;
	cout << "-Intiatialization file set to \"" << texinifile << "\"" << endl;;
	if( (texinifile == "pdflatex") || (texinifile == "pdftex") )
		output_ext = ".pdf";
	else if ( (texinifile == "latex") || (texinifile == "tex") )
		output_ext = ".dvi";
}
// Code executed when the -preamble option is specified
void ExecuteOptionPreamble( string optionarg )
{
	UseExternalPreamble = (optionarg=="none") ? true : false ;
	if( UseExternalPreamble )
		cout << "-Use external preamble." << endl;
	else
		cout << "-No preamble." << endl;
}

// Code executed when the -force option is specified
void ExecuteOptionForce( string optionarg, JOB &force )
{
	if( optionarg=="fullcompile" ) {
		force = FullCompile;
		cout << "-Initial full compilation forced." << endl;
	}
	else {
		force = Compile;
		cout << "-Initial compilation forced." << endl;
	}
}

// Code executed when the -afterjob option is specified
void ExecuteOptionAfterJob( string optionarg )
{
	if( optionarg=="dvips" ) {
		afterjob = Dvips;
		cout << "-After-compilation job set to '" << optionarg << "'" << endl;
	}
	else {
		afterjob = Rest;
		cout << "-After-compilation job set to 'rest'" << endl;
	}
}

// perform the necessary compilation
void make(JOB makejob, LPCSTR mainfilebasename)
{
	if( makejob != Rest ) {
		bool bCompOk = true;
		if( makejob == Compile ) {
			SetTitle("recompiling...");
			bCompOk = compile(mainfilebasename);
		}
		else if ( makejob == FullCompile ) {
			SetTitle("recompiling...");
			bCompOk = fullcompile(mainfilebasename);
		}
		if( bCompOk && afterjob == Dvips ) {
			dvips(mainfilebasename);
		}

		SetTitle(bCompOk ? "monitoring" : "(errors) monitoring");
	}
}

// thread responsible of launching the external commands (latex) for compilation
void WINAPI MakeThread( void *param )
{
	ResetEvent(hEvtAbortMake);

	//////////////
	// perform the necessary compilations
	MAKETHREADPARAM *p = (MAKETHREADPARAM *)param;
	make(p->makejob, p->mainfilebasename);	
	EnterCriticalSection( &cs );
	cout << flush << fgCommandLine << PROMPT_STRING;
	LeaveCriticalSection( &cs ); 

	free(p);

	cout <<	flush;
	hMakeThread = NULL;
}


// thread responsible for parsing the commands send by the user.
void WINAPI CommandPromptThread( void *param )
{
	//////////////
	// perform the necessary compilations
	LPCTSTR mainfilebasename = ((MAKETHREADPARAM *)param)->mainfilebasename;

	//////////////
	// input loop
	bool wantsmore = true, printprompt=true;
	while(wantsmore)
	{
		if(hMakeThread) {
			// wait for the "make" thread to end
			WaitForSingleObject(hMakeThread, INFINITE);
		}

		if( printprompt ) {
			EnterCriticalSection( &cs );
			cout << flush << fgCommandLine << PROMPT_STRING;
			LeaveCriticalSection( &cs );
			printprompt = false;
		}

		// Read a command from the user
		TCHAR cmdline[PROMPT_MAX_INPUT_LENGTH+5] = _T("cmd -"); // add a dummy command name and the option delimiter
		cin.getline(&cmdline[5], PROMPT_MAX_INPUT_LENGTH);

		// Convert the command line into an argv table 
		int argc;
		LPTSTR *argvw;
		argvw = CommandLineToArgv(cmdline, &argc);

		// Parse the command line
		CSimpleOpt args(argc, argvw, g_rgPromptOptions, true);

		args.Next(); // get the first command recognized
		if (args.LastError() != SO_SUCCESS) {
			TCHAR *pszError = _T("Unknown error");
			switch (args.LastError()) {
			case SO_OPT_INVALID:
				pszError = _T("Unrecognized command");
				break;
			case SO_OPT_MULTIPLE:
				pszError = _T("This command takes multiple arguments");
				break;
			case SO_ARG_INVALID:
				pszError = _T("This command does not accept argument");
				break;
			case SO_ARG_INVALID_TYPE:
				pszError = _T("Invalid argument format");
				break;
			case SO_ARG_MISSING:
				pszError = _T("Required argument is missing");
				break;
			}
			EnterCriticalSection( &cs );
			LPCTSTR optiontext = args.OptionText();
			// remove the extra '-' that we appened before the command name
			if( args.LastError()== SO_OPT_INVALID && optiontext[0] == '-') 
				optiontext++;

			cout << fgErr << pszError << ": '" << optiontext << "' (use help to get command line help)" 
				 << endl << flush << fgCommandLine << PROMPT_STRING;
			LeaveCriticalSection( &cs );
			continue;
		}

		switch( args.OptionId() ) {
		case OPT_USAGE:
			EnterCriticalSection( &cs );
				ShowUsage(NULL);
				cout << endl << fgCommandLine << PROMPT_STRING;
			LeaveCriticalSection( &cs ); 
			break;
		case OPT_HELP:
			EnterCriticalSection( &cs );
			cout << fgCommandLine << "The following commands are available:" << endl
				 << "  b[ibtex] to run bibtex on the .tex file" << endl
				 << "  c[compile] to compile the .tex file using the precompiled preamble" << endl
				 << "  d[vips] to convert the .dvi file to postscript" << endl
				 << "  e[dit] to edit the .tex file" << endl
				 << "  f[ullcompile] to compile the preamble and the .tex file" << endl
				 << "  h[elp] to show this message" << endl
				 << "  o[pen] to open the folder containing the .tex file" << endl
				 << "  p[s2pdf] to convert the .ps file to pdf" << endl
				 << "  q[uit] to quit the program" << endl 
				 << "  u[sage] to show the help on command line parameters usage" << endl
				 << "  v[iew] to view the output file (dvi or pdf depending on the ini file used)" << endl << endl
				 << "You can also configure variables with:" << endl
				 << "  ini=inifile   set the initial format file to inifile" << endl
				 << "  preamble={none,external}   set the preamble mode" << endl
				 << "  afterjob={rest,dvips}   set the job executed after compilation of the .tex file" << endl << endl
				 << fgCommandLine << PROMPT_STRING;
			LeaveCriticalSection( &cs ); 
			break;
		case OPT_INI:		ExecuteOptionIni(args.OptionArg());			printprompt=true;	break;
		case OPT_PREAMBLE:	ExecuteOptionPreamble(args.OptionArg());	printprompt=true;	break;
		case OPT_AFTERJOB:	ExecuteOptionAfterJob(args.OptionArg());	printprompt=true;	break;
		case OPT_BIBTEX:		bibtex(mainfilebasename);		printprompt=true;	break;
		case OPT_DVIPS:			dvips(mainfilebasename);		printprompt=true;	break;
		case OPT_PS2PDF:		ps2pdf(mainfilebasename);		printprompt=true;	break;
		case OPT_EDIT:			edit(mainfilebasename);			printprompt=true;	break;
		case OPT_VIEWOUTPUT:	view(mainfilebasename);			printprompt=true;	break;
		case OPT_OPENFOLDER:	openfolder(mainfilebasename);	printprompt=true;	break;
		case OPT_COMPILE:
			RestartMakeThread(Compile, mainfilebasename);
			// wait for the "make" thread to end
			WaitForSingleObject(hMakeThread, INFINITE);
			break;
		case OPT_FULLCOMPILE:
			RestartMakeThread(FullCompile, mainfilebasename);
			// wait for the "make" thread to end
			WaitForSingleObject(hMakeThread, INFINITE);
			break;
		case OPT_QUIT:
			wantsmore = false;
			cout << fgNormal;
			break;		

		default:
			EnterCriticalSection( &cs );
			cout << fgErr << "Unrecognized command: '" << args.OptionText() << "' (use help to get command line help)" 
				 << endl << flush << fgCommandLine << PROMPT_STRING;
			LeaveCriticalSection( &cs );
			break;
		}
//		uiFlags |= (unsigned int) args.OptionId();	
	}

	free(param);
	// raise the event saying that the user wants to quit
	SetEvent(hEvtQuitProgram);
}

int _tmain(int argc, TCHAR *argv[])
{
	InitializeCriticalSection(&cs);

	// create the event used to abort the "make" thread
	hEvtAbortMake = CreateEvent(NULL,TRUE,FALSE,NULL);
	// create the event used to quit the program
	hEvtQuitProgram = CreateEvent(NULL,TRUE,FALSE,NULL);

	cout << APP_NAME << " " << VERSION << " by William Blum, " VERSION_DATE << endl << endl;;

    unsigned int uiFlags = 0;

	if (argc <= 1 ) {
		ShowUsage(argv[0]);
		return 1;
	}

	// default options, can be overwritten by command line parameters
	JOB force = Rest;
	bool Watch = true;

    CSimpleOpt args(argc, argv, g_rgOptions, true);
    while (args.Next()) {
        if (args.LastError() != SO_SUCCESS) {
            TCHAR * pszError = _T("Unknown error");
            switch (args.LastError()) {
            case SO_OPT_INVALID:
                pszError = _T("Unrecognized option");
                break;
            case SO_OPT_MULTIPLE:
                pszError = _T("Option matched multiple strings");
                break;
            case SO_ARG_INVALID:
                pszError = _T("Option does not accept argument");
                break;
            case SO_ARG_INVALID_TYPE:
                pszError = _T("Invalid argument format");
                break;
            case SO_ARG_MISSING:
                pszError = _T("Required argument is missing");
                break;
            }
            _tprintf(
                _T("%s: '%s' (use --help to get command line help)\n"),
                pszError, args.OptionText());
            continue;
        }

		switch( args.OptionId() ) {
			case OPT_USAGE:         ShowUsage(NULL);							return 0;
			case OPT_INI:			ExecuteOptionIni(args.OptionArg());			break;
			case OPT_NOWATCH:		Watch = false;								break;
			case OPT_FORCE:			ExecuteOptionForce(args.OptionArg(),force); break;
			case OPT_PREAMBLE:		ExecuteOptionPreamble(args.OptionArg());	break;
			case OPT_AFTERJOB:		ExecuteOptionAfterJob(args.OptionArg());	break;
			default:				break;
		}
        uiFlags |= (unsigned int) args.OptionId();
    }
	
    CSimpleGlob glob(uiFlags);
    if (SG_SUCCESS != glob.Add(args.FileCount(), args.Files()) ) {
        _tprintf(_T("Error while globbing files! Make sure the paths given as parameters are correct.\n"));
        return 1;
    }
	if( args.FileCount() == 0 ){
        _tprintf(_T("No input file specified!\n"));
        return 1;
    }

	cout << "-Main file: '" << glob.File(0) << "'\n";

	TCHAR drive[4];
	TCHAR mainfile[_MAX_FNAME];
	TCHAR ext[_MAX_EXT];
	TCHAR fullpath[_MAX_PATH];
	TCHAR dir[_MAX_DIR];

	_fullpath( fullpath, glob.File(0), _MAX_PATH );
	_tsplitpath( fullpath, drive, dir, mainfile, ext );

	// set the global variables
	texfullpath = fullpath;
	texdir = string(drive) + dir;

	// set console title
	SetTitle("Initialization");

	cout << "-Directory: " << drive << dir << "\n";

	if(  _tcsncmp(ext, ".tex", 4) )	{
		cerr << fgErr << "Error: the file has not the .tex extension!\n\n";
		return 1;
	}
	if( glob.FileCount()>1 ) {
		cout << "-Dependencies:\n";
		for (int n = 1; n < glob.FileCount(); ++n)
			_tprintf(_T("  %2d: '%s'\n"), n, glob.File(n));
	}
	else
		cout << "-No dependency.\n";
		
	if(glob.FileCount() == 0)
		return 1;

	// change current directory
	string path = string(drive) +  dir;
	_chdir(path.c_str());

	int res; // will contain the result of the comparison of the timestamp of the preamble file with the format file

	// check for the presence of the external preamble file
	if( UseExternalPreamble ) {
		// compare the timestamp of the preamble.tex file and the format file
		preamble_filename = string(mainfile) + "." DEFAULTPREAMBLE1_EXT;
		preamble_basename = string(mainfile);
		res = compare_timestamp(preamble_filename.c_str(), (preamble_basename+".fmt").c_str());
		UseExternalPreamble = !(res & ERR_SRCABSENT);
		if ( !UseExternalPreamble ) {
			// try with the second default preamble name
			preamble_filename = DEFAULTPREAMBLE2_FILENAME;
			preamble_basename = DEFAULTPREAMBLE2_BASENAME;
			res = compare_timestamp(preamble_filename.c_str(), (preamble_basename+".fmt").c_str());
			UseExternalPreamble = !(res & ERR_SRCABSENT);
			if ( !UseExternalPreamble ) {
				cout << fgWarning << "Warning: Preamble file not found! I have tried to look for " << mainfile << "." << DEFAULTPREAMBLE1_EXT << " and then " << DEFAULTPREAMBLE2_FILENAME << ")\nPrecompilation mode desactivated!\n";
			}
		}
	}

	if( UseExternalPreamble )
		cout << "-Preamble file: " << preamble_filename << "\n";

	if( force == FullCompile ) {
		make(FullCompile, mainfile);
	}
	else if ( force == Compile ) {
		make(Compile, mainfile);
	}
	// Determine what needs to be recompiled based on the files that have been touched since last compilation.
	else {
		// The external preamble file is used and the format file does not exist or has a timestamp
		// older than the preamble file : then recreate the format file and recompile the .tex file.
		if( UseExternalPreamble && ((res == SRC_FRESHER) || (res & ERR_OUTABSENT)) ) {
			if( res == SRC_FRESHER ) {
				 cout << fgMsg << "+ " << preamble_filename << " has been modified since last run.\n";
				 cout << fgMsg << "  Let's recreate the format file and recompile " << mainfile << ".tex.\n";
			}
			else {		
				cout << fgMsg << "+ " << preamble_basename << ".fmt does not exist. Let's create it...\n";
			}
			make(FullCompile, mainfile);
		}
		
		// either the preamble file exists and the format file is up-to-date  or  there is no preamble file
		else {
			// check if the main file has been modified since the creation of the dvi file
			int maintex_comp = compare_timestamp((string(mainfile)+".tex").c_str(), (string(mainfile)+output_ext).c_str());

			// check if a dependency file has been modified since the creation of the dvi file
			bool dependency_fresher = false;
			for(int i=1; !dependency_fresher && i<glob.FileCount(); i++)
				dependency_fresher = SRC_FRESHER == compare_timestamp(glob.File(i), (string(mainfile)+output_ext).c_str()) ;

			if ( maintex_comp & ERR_SRCABSENT ) {
				cout << fgErr << "File " << mainfile << ".tex not found!\n";
				return 2;
			}
			else if ( maintex_comp & ERR_OUTABSENT ) {
				cout << fgMsg << "+ " << mainfile << output_ext << " does not exist. Let's create it...\n";
				make(Compile, mainfile);
			}
			else if( dependency_fresher || (maintex_comp == SRC_FRESHER) ) {
				cout << fgMsg << "+ the main file or some dependency file has been modified since last run. Let's recompile...\n";
				make(Compile, mainfile);
			}
			// else 
			//   We have maintex_comp == OUT_FRESHER and dependency_fresher=false therefore 
			//   there is no need to recompile.
		}
	}

	if( Watch ) {
	    cout << fgMsg << "-- Watching directory " << path.c_str() << " for changes...\n" << fgNormal;
		SetTitle("monitoring");
		// Start the command shell thread
		MAKETHREADPARAM *p = new MAKETHREADPARAM;
		p->mainfilebasename = mainfile;
		DWORD commandpromptthreadID;
		hCommandPromptThread = CreateThread( NULL, 0,
				(LPTHREAD_START_ROUTINE) CommandPromptThread,
				(LPVOID)p,
				0,
				&commandpromptthreadID);

		WatchTexFiles(path.c_str(), mainfile, glob);
	}

	CloseHandle(hEvtAbortMake);
	CloseHandle(hEvtQuitProgram);
	return 0;
}



// compare the time stamp of source and target files
int compare_timestamp(LPCTSTR sourcefile, LPCTSTR outputfile)
{
	struct stat attr_src, attr_out;			// file attribute structures
	int res_src, res_out;

	res_src = stat(sourcefile, &attr_src);
	res_out = stat(outputfile, &attr_out);
	if( res_src || res_out ) // problem when getting the attributes of the files?
		return  (res_src ? ERR_SRCABSENT : 0) | (res_out ? ERR_OUTABSENT : 0);
	else
		return ( difftime(attr_out.st_mtime, attr_src.st_mtime) > 0 ) ? OUT_FRESHER : SRC_FRESHER;
}



// Launch an external program
DWORD launch(LPCTSTR cmdline)
{
	STARTUPINFO si;
	PROCESS_INFORMATION pi;
	LPTSTR szCmdline= _tcsdup(cmdline);

	ZeroMemory( &si, sizeof(si) );
	si.cb = sizeof(si);
	//si.lpTitle = "latex";
	//si.wShowWindow = SW_HIDE;
	//si.dwFlags = STARTF_USESHOWWINDOW;
	ZeroMemory( &pi, sizeof(pi) );

	EnterCriticalSection( &cs );
	ExecutingExternalCommand = true;
	
	// Start the child process. 
	if( !CreateProcess( NULL,   // No module name (use command line)
		szCmdline,      // Command line
		NULL,           // Process handle not inheritable
		NULL,           // Thread handle not inheritable
		FALSE,          // Set handle inheritance to FALSE
		0,
		//CREATE_NEW_CONSOLE,              // No creation flags
		NULL,           // Use parent's environment block
		NULL,           // Use parent's starting directory 
		&si,            // Pointer to STARTUPINFO structure
		&pi )           // Pointer to PROCESS_INFORMATION structure
	) {
		cout << fgErr << "CreateProcess failed ("<< GetLastError() << ") : " << cmdline <<".\n";
		return -1;
	}
	free(szCmdline);

	// Wait until child process exits or the make process is aborted.
	//WaitForSingleObject( pi.hProcess, INFINITE );	
	HANDLE hp[2] = {pi.hProcess, hEvtAbortMake};
	DWORD dwRet = 0;
	switch( WaitForMultipleObjects(2, hp, FALSE, INFINITE ) ) {
	case WAIT_OBJECT_0:
		// Get the return code
		GetExitCodeProcess( pi.hProcess, &dwRet);
		break;
	case WAIT_OBJECT_0+1:
		dwRet = -1;
		TerminateProcess( pi.hProcess,1);
		break;
	default:
		break;
	}

	// Close process and thread handles. 
	CloseHandle( pi.hProcess );
	CloseHandle( pi.hThread );

	ExecutingExternalCommand = false;
    LeaveCriticalSection( &cs ); 

	return dwRet;
}


// Recompile the preamble into the format file "texfile.fmt" and then compile the main file
bool fullcompile(LPCTSTR texbasename)
{
	// Check that external preamble exists
	if( UseExternalPreamble ) {
		EnterCriticalSection( &cs );
		string cmdline = string("pdftex -interaction=nonstopmode --src-specials -ini \"&" + texinifile + "\" \"\\input ")+preamble_filename+" \\dump\\endinput \"";
		cout << fgMsg << "-- Creation of the format file...\n";
		cout << "[running '" << cmdline << "']\n" << fgLatex;
		LeaveCriticalSection( &cs ); 
		if( launch(cmdline.c_str()) )
			return false;
	}
	return compile(texbasename);
}


// Compile the final tex file using the precompiled preamble
bool compile(LPCTSTR texbasename)
{
	string cmdline;

	EnterCriticalSection( &cs );
	cout << fgMsg << "-- Compilation of " << texbasename << ".tex ...\n";

	// External preamble used? Then compile using the precompiled preamble.
	if( UseExternalPreamble ) {
		// Remark on the latex code included in the following command line:
		//	 % Install a hook for the \input command ...
		///  \let\TEXDAEMONinput\input
		//   % which ignores the first file inclusion (the one inserting the preamble)
		//   \def\input#1{\let\input\TEXDAEMONinput}
		cmdline = string("pdftex -interaction=nonstopmode --src-specials \"&")+preamble_basename+"\" \"\\let\\TEXDAEMONinput\\input\\def\\input#1{\\let\\input\\TEXDAEMONinput} \\TEXDAEMONinput "+texbasename+".tex \"";
		
		//// Old version requiring the user to insert a conditional at the start of the main .tex file
		//string cmdline = string("pdftex -interaction=nonstopmode --src-specials \"&")+preamble_basename+"\" \"\\def\\incrcompilation{} \\input "+texbasename+".tex \"";
		
		cout << fgMsg << "[running '" << cmdline << "']\n" << fgLatex;
	}
	// no preamble: compile the latex file without the standard latex format file.
	else {
		cmdline = string("latex -interaction=nonstopmode -src-specials ")+texbasename+".tex";
		cout << fgMsg << " Running '" << cmdline << "'\n" << fgLatex;
	}
    LeaveCriticalSection( &cs ); 
	return 0==launch(cmdline.c_str());
}

// Convert the postscript file to pdf
bool ps2pdf(LPCTSTR texbasename)
{
	EnterCriticalSection( &cs );
	cout << fgMsg << "-- Converting " << texbasename << ".ps to pdf...\n";
	string cmdline = string("ps2pdf ")+texbasename+".ps";
	cout << fgMsg << " Running '" << cmdline << "'\n" << fgLatex;
    LeaveCriticalSection( &cs ); 
	return 0==launch(cmdline.c_str());
}

// Edit the .tex file
bool edit(LPCTSTR texbasename)
{
	EnterCriticalSection( &cs );
	cout << fgMsg << "-- editing " << texbasename << ".tex...\n";
    LeaveCriticalSection( &cs ); 
	HINSTANCE ret = ShellExecute(NULL, _T("open"),
		_T(texfullpath.c_str()),
		NULL,
		texdir.c_str(),
		SW_SHOWNORMAL);
	return (int)ret>32;
}

// View the output file
bool view(LPCTSTR texbasename)
{
	EnterCriticalSection( &cs );
	cout << fgMsg << "-- view " << texbasename << output_ext << "...\n";
    LeaveCriticalSection( &cs ); 
	HINSTANCE ret = ShellExecute(NULL, _T("open"),
		_T((texdir+texbasename+output_ext).c_str()),
		NULL,
		texdir.c_str(),
		SW_SHOWNORMAL);
	return (int)ret>32;
}

// Open the folder containing the .tex file
bool openfolder(LPCTSTR texbasename)
{
	EnterCriticalSection( &cs );
	cout << fgMsg << "-- open directory " << texdir << " ...\n";
    LeaveCriticalSection( &cs ); 
	HINSTANCE ret = ShellExecute(NULL, NULL,
		_T(texdir.c_str()),
		NULL,
		NULL,
		SW_SHOWNORMAL);
	return (int)ret>32;
}


// Convert the dvi file to postscript using dvips
bool dvips(LPCTSTR texbasename)
{
	EnterCriticalSection( &cs );
	cout << fgMsg << "-- Converting " << texbasename << ".dvi to postscript...\n";
	string cmdline = string("dvips ")+texbasename+".dvi -o "+texbasename+".ps";
	cout << fgMsg << " Running '" << cmdline << "'\n" << fgLatex;
    LeaveCriticalSection( &cs ); 
	return 0==launch(cmdline.c_str());
}

// Run bibtex on the tex file
bool bibtex(LPCTSTR texbasename)
{
	EnterCriticalSection( &cs );
	cout << fgMsg << "-- Bibtexing " << texbasename << "tex...\n";
	string cmdline = string("bibtex ")+texbasename;
	cout << fgMsg << " Running '" << cmdline << "'\n" << fgLatex;
    LeaveCriticalSection( &cs ); 
	return 0==launch(cmdline.c_str());
}


// Restart the make thread
void RestartMakeThread( JOB makejob, LPCTSTR mainfilebase) {
	if( makejob == Rest )
		return;

	/// abort the current "make" thread if it is already started
	if( hMakeThread ) {
		SetEvent(hEvtAbortMake);
		// wait for the "make" thread to end
		WaitForSingleObject(hMakeThread, INFINITE);
	}			

	// Create a new "make" thread.
	//  note: it is necessary to dynamically allocate a MAKETHREADPARAM structure
	//  otherwise, if we pass the address of a locally defined variable as a parameter to 
	//  CreateThread, the content of the structure may change
	//  by the time the make thread is created (since the current thread runs concurrently).
	MAKETHREADPARAM *p = new MAKETHREADPARAM;
	p->makejob = makejob;
	p->mainfilebasename = mainfilebase;
	DWORD makethreadID;
	hMakeThread = CreateThread( NULL,
			0,
			(LPTHREAD_START_ROUTINE) MakeThread,
			(LPVOID)p,
			0,
			&makethreadID);
}

// Return true if an external program is potentially being executed
bool IsExecutingExternal()
{
	return ExecutingExternalCommand;
//	return hMakeThread!=0;
}

void WatchTexFiles(LPCTSTR texpath, LPCTSTR mainfilebase, CSimpleGlob &glob)
{
	// get the digest of the dependcy files
	md5 *dg_deps = new md5 [glob.FileCount()];
    for (int n = 0; n < glob.FileCount(); ++n)
	{
		if( !dg_deps[n].DigestFile(glob.File(n)) ) {
			cerr << "File " << glob.File(n) << " cannot be found or opened!\n";
			return;
		}
	}

	// get the digest of the main tex file
	string maintexfilename = string(mainfilebase) + ".tex";
	md5 dg_tex = dg_deps[0];

	// get the digest of the file containing bibtex bibliography references
	string bblfilename = string(mainfilebase) + ".bbl";
	md5 dg_bbl;
	dg_bbl.DigestFile(bblfilename.c_str());
	
	// get the digest of the preamble file
	md5 dg_preamble;
	if( UseExternalPreamble && !dg_preamble.DigestFile(preamble_filename.c_str()) ) {
		cerr << "File " << preamble_filename << " cannot be found or opened!\n" << fgLatex;
		return;
	}

	//// open the directory to be monitored
	//HANDLE hDir = CreateFile(
	//	texpath, /* pointer to the directory containing the tex files */
	//	FILE_LIST_DIRECTORY,                /* access (read-write) mode */
	//	FILE_SHARE_READ|FILE_SHARE_DELETE|FILE_SHARE_WRITE,  /* share mode */
	//	NULL, /* security descriptor */
	//	OPEN_EXISTING, /* how to create */
	//	FILE_FLAG_BACKUP_SEMANTICS  , //| FILE_FLAG_OVERLAPPED, /* file attributes */
	//	NULL /* file with attributes to copy */
	//  );

	// open the directory to be monitored
	HANDLE hDir = CreateFile(
		texpath, /* pointer to the directory containing the tex files */
		FILE_LIST_DIRECTORY,                /* access (read-write) mode */
		FILE_SHARE_READ|FILE_SHARE_DELETE|FILE_SHARE_WRITE,  /* share mode */
		NULL, /* security descriptor */
		OPEN_EXISTING, /* how to create */
		FILE_FLAG_BACKUP_SEMANTICS  | FILE_FLAG_OVERLAPPED , /* file attributes */
		NULL /* file with attributes to copy */
	  );
	 

	BYTE buffer [1024*sizeof(FILE_NOTIFY_INFORMATION )];

	while( 1 )
	{
		FILE_NOTIFY_INFORMATION *pFileNotify;
		DWORD BytesReturned;

		//GetOverlappedResult(hDir, &overlapped, &BytesReturned, TRUE);
		//ReadDirectoryChangesW(
		//	 hDir, /* handle to directory */
		//	 &buffer, /* read results buffer */
		//	 sizeof(buffer), /* length of buffer */
		//	 FALSE, /* monitoring option */
		//	 //FILE_NOTIFY_CHANGE_SECURITY|FILE_NOTIFY_CHANGE_CREATION| FILE_NOTIFY_CHANGE_LAST_ACCESS|
		//	 FILE_NOTIFY_CHANGE_LAST_WRITE
		//	 //|FILE_NOTIFY_CHANGE_SIZE |FILE_NOTIFY_CHANGE_ATTRIBUTES |FILE_NOTIFY_CHANGE_DIR_NAME |FILE_NOTIFY_CHANGE_FILE_NAME
		//	 , /* filter conditions */
		//	 &BytesReturned, /* bytes returned */
		//	 NULL, /* overlapped buffer */
		//	 NULL); /* completion routine */
		OVERLAPPED overl;
		overl.hEvent = CreateEvent(NULL,FALSE,FALSE,NULL);
		ReadDirectoryChangesW(
			 hDir, /* handle to directory */
			 &buffer, /* read results buffer */
			 sizeof(buffer), /* length of buffer */
			 FALSE, /* monitoring option */
			 //FILE_NOTIFY_CHANGE_SECURITY|FILE_NOTIFY_CHANGE_CREATION| FILE_NOTIFY_CHANGE_LAST_ACCESS|
			 FILE_NOTIFY_CHANGE_LAST_WRITE
			 //|FILE_NOTIFY_CHANGE_SIZE |FILE_NOTIFY_CHANGE_ATTRIBUTES |FILE_NOTIFY_CHANGE_DIR_NAME |FILE_NOTIFY_CHANGE_FILE_NAME
			 , /* filter conditions */
			 &BytesReturned, /* bytes returned */
			 &overl, /* overlapped buffer */
			 NULL); /* completion routine */
		
		DWORD dwNumberbytes;
		GetOverlappedResult(hDir, &overl, &dwNumberbytes, FALSE);
		HANDLE hp[2] = {overl.hEvent, hEvtQuitProgram};
		DWORD dwRet = 0;
		switch( WaitForMultipleObjects(2, hp, FALSE, INFINITE ) ) {
			case WAIT_OBJECT_0:
				break;
			case WAIT_OBJECT_0+1: // the user asked to quit the program
				return;
			default:
				break;
			}
		CloseHandle(overl.hEvent);


		//EnterCriticalSection( &cs );
		//cout << "                         \r";

		//////////////
		// Check if some source file has changed and prepare the compilation requirement accordingly
		JOB makejob = Rest;
		pFileNotify = (PFILE_NOTIFY_INFORMATION)&buffer;
		do { 
			// Convert the filename from unicode string to oem string
			char filename[_MAX_FNAME];
			pFileNotify->FileName[min(pFileNotify->FileNameLength/2, _MAX_FNAME-1)] = 0;
			wcstombs( filename, pFileNotify->FileName, _MAX_FNAME );

			if( pFileNotify->Action != FILE_ACTION_MODIFIED ) {
				if(!IsExecutingExternal()) cout << fgIgnoredfile << ".\"" << filename << "\" touched\n" ;
			}
			else
			{
				md5 dg_new;
				// modification of the tex file?
				if( !_tcscmp(filename,maintexfilename.c_str()) ) {
					// has the digest changed?
					if( dg_new.DigestFile(maintexfilename.c_str()) && (dg_tex != dg_new) ) {
 						dg_tex = dg_new;
						if(!IsExecutingExternal()) cout << fgDepFile << "+ " << maintexfilename << " changed\n";
						makejob = max(Compile, makejob);
					}
					else {
						if(!IsExecutingExternal()) cout << fgIgnoredfile << ".\"" << filename << "\" modified but digest preserved\n" ;
					}
				}
				// modification of the bibtex file?
				else if( !_tcscmp(filename,bblfilename.c_str()) ) {
					// has the digest changed?
					if( dg_new.DigestFile(filename) && (dg_bbl != dg_new) ) {
 						dg_bbl = dg_new;
						if(!IsExecutingExternal()) cout << fgDepFile << "+ " << filename << "(bibtex) changed\n";
						makejob = max(Compile, makejob);
					}
					else {
						if(!IsExecutingExternal()) cout << fgIgnoredfile << ".\"" << filename << "\" modified but digest preserved\n" ;
					}
				}
				
				// modification of the preamble file?
				else if( UseExternalPreamble && !_tcscmp(filename,preamble_filename.c_str())  ) {
					if( dg_new.DigestFile(preamble_filename.c_str()) && (dg_preamble!=dg_new) ) {
						dg_preamble = dg_new;
						if(!IsExecutingExternal()) cout << fgDepFile << "+ \"" << preamble_filename << "\" changed (preamble file).\n";
						makejob = max(FullCompile, makejob);
					}
					else {
						if(!IsExecutingExternal()) cout << fgIgnoredfile << ".\"" << filename << "\" modified but digest preserved\n" ;
					}
				}
				
				// another file
				else {
					// is it a dependency file?
					int i;
					for(i=1; i<glob.FileCount(); i++)
						if(!_tcscmp(filename,glob.File(i))) break;

					if( i<glob.FileCount() ) {
						if ( dg_new.DigestFile(glob.File(i)) && (dg_deps[i]!=dg_new) ) {
							dg_deps[i] = dg_new;
							if(!IsExecutingExternal()) cout << fgDepFile << "+ \"" << glob.File(i) << "\" changed (dependency file).\n";
							makejob = max(Compile, makejob);
						}
						else {
							if(!IsExecutingExternal()) cout << fgIgnoredfile << ".\"" << filename << "\" modified but digest preserved\n" ;
						}
					}
					// not a revelant file ...				
					else {
						if(!IsExecutingExternal()) cout << fgIgnoredfile << ".\"" << filename << "\" modified\n";
					}
				}
			}

			pFileNotify = (FILE_NOTIFY_INFORMATION*) ((PBYTE)pFileNotify + pFileNotify->NextEntryOffset);
		}
		while( pFileNotify->NextEntryOffset );
		//LeaveCriticalSection( &cs ); 

		RestartMakeThread(makejob, mainfilebase);

		/*EnterCriticalSection( &cs );
		if(!IsExecutingExternal()) cout << fgMsg << "-- waiting for changes...\r";
		LeaveCriticalSection( &cs ); */

	}
//	CloseHandle(overlapped.hEvent);
    CloseHandle(hDir);

}

