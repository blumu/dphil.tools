// The Urzyczyn tree. This recursion scheme comes from Jolie De Miranda's thesis.

name { "Urzyczyn tree (unsafe RS)" }

validator { demiranda_urzyczyn }

terminals{
	[:o-> o;
	]:o -> o;
	*:o -> o;
	3:o -> o -> o -> o;
	e:o;
	r:o;
}

nonterminals {
	S:o ;
	D:(o -> o -> o) -> o -> o -> o -> o ;
	F:o -> o ;
	E:o; 
	G:o -> o -> o;
}

rules {
	S = [ (D G E E E) ;
	D \varphi x y z = 3 ([ (D (D \varphi x) z (F y) (F y))) (] (\varphi y x)) (* z);
	F x = * x ;
	E = e ;
	G u v = r ;
}