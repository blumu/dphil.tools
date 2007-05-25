// The tree whose paths are the reverses of Urzyczin's tree paths.

// Second (experimental) method.
//
// Still buggy since this recscheme is order 1!
// (Urzyczyn's word language is not context-free therfore
// one needs the power of an order 2 pda at least.)


name { "Urzyczin tree (reverse)" }

validator { reverse_demiranda_urzyczyn }

terminals{
	[:o -> o;
	]:o -> o;
	*:o -> o;
	e:o;
	r:o;

	
	2:o -> o -> o;	
	3:o -> o -> o -> o;
}

nonterminals {
	S:o ;

	B0:o;
	B1:o->o;
	B2:o->o;

	R0:o;
	R1:o->o->o;
	
	A00:o;
	A_l0sGT0:o->o;
	A_lGT0:o->o->o;
}

rules {
	S = R0;

	B0 = e;
	B1 p = p;
	B2 p = 2 ([ p)
	         (] (B2 p));

			 
	R0 = 3	(* (R1 (A_lGT0 (A_l0sGT0 (B1 B0) ) (B1 B0)) B0) )
			(] (A_lGT0 A00 e) )
			 r ;

	R1 x y = 3  (* (R1 (A_l0sGT0 (B2 y)) (B2 y)) )
				(] x )
				([ y );

	// s>0 & l=0
	A00 = 2 (] (A_lGT0 A00 e) )
				r;
				
	// s=0 & l>0
	A_l0sGT0 z = 2  (] (A_lGT0 (A_l0sGT0 z) z) )
					([ z );

	// l>0
	A_lGT0 x z = 2  (] (A_lGT0 (A_lGT0 x z) z) )
					([ x );


}