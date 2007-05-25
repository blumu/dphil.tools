// 
name { "s" }

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
