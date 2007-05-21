// Knapik et al. safe example

name { "Knapik et al. safe example" }

validator { none }

terminals {
	f:o->o->o;
	h:o->o;
	g:o->o;	
	a:o;
	b:o;
}

nonterminals {
	S: o ;
	G: o -> o -> o;
}

rules {
	S = G (g a) b;
	G z y = f (G (G z y) (h y)) (f z y);
}