// Some simply-typed lambda term (no recursion)

name { "A lambda term" }

validator { none }

// the terminals are the free variables of the term
terminals{
	\varphi:(o-> o)->o;
}

nonterminals {
	S:o ;
	B:o->o;
	C:((o -> o)->o)->o ;
	D:(o -> o) -> o; 
	E:o -> o;
	F:o -> o;
	G:o;
	H:o -> o;
	I:o -> o;
}

rules {
	S = \varphi B;
	B x = C D;
	C \psi = \varphi E;
	D u = u e ;
	E xp= F G ;
	F y = \psi H;
	G = \varphi I;
	H z = z;
	I xpp = xp;
}