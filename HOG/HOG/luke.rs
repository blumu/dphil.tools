// Luke's example.
// This is an order 2 unsafe recursion scheme

name { "Luke's example" }

validator { none }

terminals {
	h:o->o;
	g:o->o->o;	
	a:o;
}

nonterminals {
	S:o ;
	H:o -> o;
	F:(o -> o)->o ;
}

rules {
	S = H a ;
	H z = F (g z) ;
	F phi = phi ( phi ( F h)) ;
}