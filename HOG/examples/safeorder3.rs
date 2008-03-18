// A safe order 3 example

name { "A safe order 3 example" }

validator { none }

terminals {
	a:o;
	u:o->o;
}

nonterminals {
	S: o ;
	F: ((o -> o) -> o)-> o;
	G: ((o -> o) -> o)-> o -> o;
	H: (o -> o) -> o;
}

rules {
	S = F H;
	F f = f ( G f );
	G f x = f u;
	H g = g a;	
}