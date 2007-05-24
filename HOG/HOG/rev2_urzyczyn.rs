// The tree whose paths are the reverses of Urzyczin's tree paths.

// Second method.

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
	SP:o;

	B0:o;
	B1:o;
	B2:o->o;

	R1:o->o;
	R2:o->o;
	A1:o->o;
	A2:o->o;
	A3:o->o;
}

rules {
	S = SP;

	B0 = e;
	B1 = B0;
	B2 p = 2 ([ p)
	         (] (B2 p));

			 
	SP = 3	(* (R1 B1) )
			(] (A2 B0) )
			 r ;

	R1 phi = 3  (* (R2 (B2 phi)) )
				(] (A3 phi) )
				([ phi );

	R2 phi = 3  (* (R2 (B2 phi)) )
				(] (A3 phi) )
				([ phi );
				
	// s>0 & l=0
	A1 phi = 2  (] (A3 phi) )
				([ phi);
				
	// s=0 & l>0
	A2 phi = 2  (] (A2 B0) )
				([ B0 );

	// s>0 & l>0
	A3 phi = 2  (] (A3 phi) )
				([ phi );


}