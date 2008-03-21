// A safe order 3 example dervied from urzyczin unsafe rs (but not equivalent)

name { "A safe order 3 RS derived from urzyczin's rs" }

validator {none}

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
	D \varphi x y z = 3 ([ (D G z (F y) (F y))) (] (\varphi y x)) (* z);
	F x = * x ;
	E = e ;
	G u v = r ;
}