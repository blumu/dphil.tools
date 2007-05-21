// Knapik et al. unsafe example

name { "Knapik et al. unsafe example" }

validator { none }

terminals {
	f:o->o->o;
	h:o->o;
	g:o->o;	
	a:o;
	b:o;
}

nonterminals {
	S:o ;
	F:(o -> o)->o -> o -> o;
}

rules {
	S = F g a b;
	F phi x y = f (F (F phi x) y (h y)) (f (phi x) y);
}