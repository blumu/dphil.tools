// This is an order 3 unsafe recursion scheme generating a finite tree
// (the subterm (I x) is unsafe)

name { "Luke's order 3 unsafe  example" }

validator { none }

terminals {
    f:o->o->o;
    a:o;
}

nonterminals {
    S: o ;
    F: ((o -> o -> o)->o->o) -> o;
    H: ((o -> o -> o)->o->o) -> o -> o -> o;
    I: o -> o -> o -> o;
    G: (o -> o -> o)->o->o;
}

rules {
    S = F G;
    F psi = psi (H psi) a;
    H psi x y = psi (I x) y;
    I x u v = x;
    G phi z = f (phi (phi z a)a)a;
}
