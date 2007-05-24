// Tree whose paths are the reverses of Urzyczin's tree paths.

name { "Urzyczin tree (reverse)" }

validator { none } // demiranda_urzyczyn }

terminals{
	[:o-> o;
	]:o -> o;
	*:o -> o;
	3:o -> o -> o -> o;
	e:o->o;
	r:o->o;
	i:o;
	cons :o-> o ->o;
}

nonterminals {
	S:o ;
	D:o->(o -> (o->o) -> (o->o) -> o) -> (o->o) -> (o->o) -> (o->o) -> o ;
	F:(o -> o) -> o -> o ;
	E:o -> o; 
	G:o -> (o->o) -> (o->o) -> o;
	
//	K: o -> (o -> (o->o) -> (o->o) -> o) -> (o->o) -> o -> (o->o) -> (o->o) -> o;
	
	K: (o -> (o->o) -> (o->o) -> o) -> (o->o) -> o -> (o->o) -> (o->o) -> o;
}

rules {
	S = D ( [ i ) G E E E ;

//	D acc phi x y z = 3 (D ([ acc)   (K ([ acc) phi x)      z (F y) (F y))
//					    (phi (] acc) y x)
//					    (z (* acc)); 
//	K acc phi x   newacc q t = D (cons newacc acc) phi x q t ;


	D acc phi x y z = 3 (D ([ acc) (K phi x) z (F y) (F y))
					    (phi (] acc) y x)
					    (z (* acc)); 
	K phi x newacc q t = D newacc phi x q t ;

	F x acc2 = x (* acc2) ;
	E acc3 = e acc3;
	G acc4 u v = r acc4 ;
}