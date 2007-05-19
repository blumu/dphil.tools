// The Urzyczin tree. This recursion scheme comes from Jolie De Miranda's thesis.

name { "Urzyczin tree" }

validator { demiranda_urzyczyn }

terminals{
	[:o -> o;
	]:o -> o;
	*:o -> o;
	3:o -> o -> o -> o;
	e:o;
	r:o;
}

non-terminals {
	S:o ;
	D:(o -> o -> o) -> o -> o -> o -> o ;
	F:o -> o ;
	E:o; 
	G:o -> o -> o;
}

rules {
	S = [ (D G E E E) ;
	D phi x y z = 3 ([ (D (D phi x) z (F y) (F y))) (] (phi y x)) (* z);
	F x = * x ;
	E = e ;
	G u v = r ;
}