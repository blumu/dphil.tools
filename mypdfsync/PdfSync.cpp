// Copyright William Blum 2008 http://william.famille-blum.org/
// PDF-source syncronizer based on .pdfsync file
// License: GPLv2


#include "PdfSync.h"

#include <crtdbg.h>
#include <stdio.h>
#include <tchar.h>
//#include <iostream>
//#include <fstream>


record2srcfile_node *Pdfsync::build_decision_tree(int leftrecord, int rightrecord)
{
    int n = rightrecord-leftrecord+1;
    // a single record?
    if (n == 1) {
        record2srcfile_leaf *leaf = new record2srcfile_leaf;
        leaf->header.type = Leaf;
        leaf->section = leftrecord;
        return (record2srcfile_node *)leaf;
    }
    else {
        int split = leftrecord + n / 2;
        record2srcfile_internal *node = new record2srcfile_internal;
        node->header.type = Internal;
        node->left = build_decision_tree(leftrecord,split-1);
        node->right = build_decision_tree(split,rightrecord);
        node->splitvalue = record_sections[split].firstrecord;
        return (record2srcfile_node *)node;
    }
}

void Pdfsync::delete_decision_tree(record2srcfile_node *root)
{
    _ASSERT(root);
    if (root->type == Leaf)
        delete (record2srcfile_leaf *)root;
    else {
        record2srcfile_internal *node = (record2srcfile_internal *)root;
        delete_decision_tree(node->left);
        delete_decision_tree(node->right);
        delete node;
    }
}


// get the index of the record section (in the array record_sections[]) containing a given record index
int Pdfsync::get_record_section(int record_index)
{
    _ASSERT( record2src_decitree );

    record2srcfile_node *cur = record2src_decitree;
    _ASSERT(cur);
    while (cur->type != Leaf) {
        record2srcfile_internal *node = (record2srcfile_internal *)cur;
        if (record_index > node->splitvalue)
            cur = node->right;
        else
            cur = node->left;
    }
    return ((record2srcfile_leaf *)cur)->section;
}



int Pdfsync::scan_and_build_index(FILE *fp)
{
  char jobname[_MAX_PATH];
	fscanf_s(fp, "%s\n", jobname, _countof(jobname)); // ignore the first line
	UINT versionNumber = 0;
	char version[10];
	fscanf_s(fp, "%s %u\n", version, _countof(version), &versionNumber);
	if (versionNumber != 1)
		  return 1;

	// add the initial tex file to the file stack
  src_scopes.clear();
	src_scope s;
	fgetpos(fp, &s.openline_pos);
	s.closeline_pos = -1;

	stack<hash_map<string,src_scope>::iterator> incstack; // stack of included files

  pair< hash_map<string,src_scope>::iterator, bool > pr;
	pr = src_scopes.insert(pair<string,src_scope>(string(jobname), s));
	if (pr.second == false)
	  	DBG_OUT("File cannot be added to the hastable!");
	else {                
		  incstack.push(pr.first);
	}

	pdfsheet_indexentry *cursheet = NULL; // pointer to the indexentry of the current pdf sheet being read
	record_section cursec; // section currenlty created
	cursec.srcfile = src_scopes.end(); // section not initiated
  record_sections.clear();
  pdfsheet_index.clear();

	CHAR filename[_MAX_PATH];
	fpos_t linepos;
  while (!feof(fp)) {
		  fgetpos(fp, &linepos);
		  char c = fgetc(fp);
		  switch (c)
      {
          case '(': 
	        {
		        if (cursec.srcfile != src_scopes.end()) {
			        record_sections.push_back(cursec);
			        cursec.srcfile = src_scopes.end();
		        }
		        fscanf_s(fp, "%s\n", filename, _countof(filename));

		        src_scope s;
		        s.openline_pos = linepos;
		        s.closeline_pos = -1;

		        pair< hash_map<string,src_scope>::iterator, bool > pr;
		        pr = src_scopes.insert(pair<string,src_scope>(string(filename), s));
            if( pr.second == false) {
			        DBG_OUT("File cannot be added to the hastable: probably because the same file is included twice!\n");
			        incstack.push(src_scopes.end());
            }
		        else {                
			        incstack.push(pr.first);
		        }
	        }
	        break;

          case ')':
	          if (cursec.srcfile != src_scopes.end()) {
		          record_sections.push_back(cursec);
		          cursec.srcfile = src_scopes.end();
	          }

            if (incstack.top()!=src_scopes.end())
	            incstack.top()->second.closeline_pos = linepos;
            incstack.pop();
	          fscanf_s(fp, "\n");
	          break;
          case 'l':
	          {
#if _DEBUG
	            if (cursheet) {
		            cursheet->endpos = linepos;
		            cursheet = NULL;
	            }
#endif
		          UINT columnNumber = 0, lineNumber = 0, recordNumber = 0;
		          if (fscanf_s(fp, " %u %u %u\n", &recordNumber, &lineNumber, &columnNumber) <2)
			          DBG_OUT("Bad 'l' line in the pdfsync file\n");
		          else {
			          if (cursec.srcfile == src_scopes.end()){ // section not initiated yet?
				          cursec.srcfile = incstack.top();
				          cursec.startpos = linepos;
                  cursec.firstrecord = recordNumber;
			          }
                #if _DEBUG
			          cursec.highestrecord = recordNumber;
                #endif
		          }
	          }        			  
	          break;
          case 'p':
	          {
		          if (fgetc(fp)=='*')
			          fgetc(fp);

		          UINT recordNumber = 0, xPosition = 0, yPosition = 0;
		          fscanf_s(fp, "%u %u %u\n", &recordNumber, &xPosition, &yPosition);
	          }			
	          break;
          case 's':
	        {
		        if (cursec.srcfile != src_scopes.end()) {
			        record_sections.push_back(cursec);
			        cursec.srcfile = src_scopes.end();
		        }
		        UINT sheetNumber = 0;
		        fscanf_s(fp, " %u\n", &sheetNumber);
		        pdfsheet_indexentry entry;
            fgetpos(fp, &entry.startpos);
            #if _DEBUG
		          entry.endpos = -1;
            #endif


		        pair< hash_map<int,pdfsheet_indexentry>::iterator, bool > pr;
		        pr = pdfsheet_index.insert(pair<int, pdfsheet_indexentry>(sheetNumber, entry));
		        if (pr.second == false)
			        DBG_OUT("Hastable error (pdfsheet_index)!\n");
		        else {                
			        cursheet = &pr.first->second;
		        }
            break;
	        }
          default:
            DBG_OUT("Malformed pdfsync file: unknown command '");
            DBG_OUT(c); DBG_OUT("'\\n");;
            break;
      }
	}
	if (cursec.srcfile != src_scopes.end()) {
		  record_sections.push_back(cursec);
		  cursec.srcfile = src_scopes.end();
	}
  _ASSERT(incstack.size()==1);

	// build the decision tree for the function mapping record number to filenames
	int n = record_sections.size();
	if(record2src_decitree) {
		  delete_decision_tree(record2src_decitree);
		  record2src_decitree = NULL;
	}
	if (n>0)
	  	record2src_decitree = build_decision_tree(0, n-1);

	return 0;
}



int Pdfsync::rebuild_index()
{
    FILE *fp;
    errno_t err;

    err = fopen_s(&fp, syncfilename.c_str(), "r");
    if(err!=0) {
        DBG_OUT("The file "); DBG_OUT(syncfilename); DBG_OUT(" cannot be opened\n");
        return 1;
    }
    scan_and_build_index(fp);
    fclose(fp);
    return 0;
}

UINT Pdfsync::pdf_to_source(UINT sheet, UINT x, UINT y, PSTR filename, UINT cchFilename, UINT *line, UINT *col)
{
    _ASSERT( record2src_decitree );

    FILE *fp;
    errno_t err;

    err = fopen_s(&fp, syncfilename.c_str(), "r");
    if(err!=0) {
        DBG_OUT("The file "); DBG_OUT(syncfilename); DBG_OUT(" cannot be opened\n");
        return PDFSYNCERR_SYNCFILE_CANNOT_BE_OPENED;
    }

    UINT closest_dist=-1; // distance to the closest pdf location
    UINT closest_record=-1;

    // find the entry in the index corresponding to this page
    hash_map<int,pdfsheet_indexentry>::const_iterator it = pdfsheet_index.find(sheet);
    if( it == pdfsheet_index.end() )
        return PDFSYNCERR_INVALID_PAGE_NUMBER;

    const pdfsheet_indexentry *sheet_entry = &it->second;

    // read the section of 'p' declarations (pdf locations)
    fsetpos(fp, &sheet_entry->startpos);
    fpos_t linepos;
    while (!feof(fp)) {
        fgetpos(fp, &linepos);
        char c = fgetc(fp);
        if (c=='(' || c==')' || c=='l' || c=='s') {
            _ASSERT(linepos == sheet_entry->endpos);
            break;
        }
        _ASSERT(c=='p'); // it's a pdf location
        // skip the optional *
        if (fgetc(fp)=='*')
          fgetc(fp);
        // read the location
        UINT recordNumber = 0, xPosition = 0, yPosition = 0;
        fscanf_s(fp, "%u %u %u\n", &recordNumber, &xPosition, &yPosition);
        // check whether it is closer that the closest point found so far
        UINT dist = (x-xPosition)*(x-xPosition) + (y-yPosition)*(y-yPosition);
        if (dist<closest_dist) {
            closest_record = recordNumber;
            closest_dist = dist;
        }
    }

    if(closest_record==-1 || closest_dist==-1)
      return PDFSYNCERR_NO_SYNC_AT_LOCATION; // the specified location was not found in the pdfsync file

    // We have a record number, we need to find its declaration ('l ...') in the syncfile

    // get the record section containing the record declaration
    int sec = this->get_record_section(closest_record);
    
    // get the file name from the record section
    strcpy_s(filename, cchFilename, record_sections[sec].srcfile->first.c_str());
    
    // find the record declaration in the section
    fsetpos(fp, &record_sections[sec].startpos);
    bool found = false;
    while (!feof(fp) && !found) {
        fgetpos(fp, &linepos);
        char c = fgetc(fp);
        _ASSERT(c=='l'); // the section contains only record declaration lines
        UINT columnNumber = 0, lineNumber = 0, recordNumber = 0;
        if (fscanf_s(fp, " %u %u %u\n", &recordNumber, &lineNumber, &columnNumber) <2)
            DBG_OUT("Bad 'l' line in the pdfsync file");
        else {
            if (recordNumber == closest_record){
              *line = lineNumber;
              *col = columnNumber;
              found = true;
            }
        }
    }
    fclose(fp);

    _ASSERT(found);

    return PDFSYNCERR_SUCCESS;
}

UINT Pdfsync::source_to_pdf(PCTSTR srcfilename, UINT line, UINT col, UINT *page, UINT *x, UINT *y)
{
    // TODO
    return PDFSYNCERR_SUCCESS;
}


