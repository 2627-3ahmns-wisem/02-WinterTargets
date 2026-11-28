Titel: Eigene Spielmechanik: Eis-Kettenreaktions-Ziel (Frost-Bombe)
Spielregel:
Neben den regulären Zielen erscheint selten eine spezielle Frost-Bombe (eisblaues Ziel). Wird diese angeklickt, löst sie eine Schockwelle im Umkreis von 3.5 Einheiten aus, die alle anderen regulären Ziele in diesem Radius sofort mit abschließt und deren Punkte (inklusive Combo-Zähler) gutschreibt. Boss-Ziele werden durch die Schockwelle nicht sofort zerstört, sondern erhalten lediglich 1 Schadenspunkt.
Erwartete Wirkung:
Das Spiel erhält eine taktische Risikokomponente: Der Spieler wird belohnt, wenn er kurz wartet, bis sich mehrere bewegliche Ziele nahe der Frost-Bombe sammeln, um mit einem einzigen Schuss mehrere Treffer gleichzeitig zu erzielen und die Combo schnell aufzubauen.
Akzeptanzkriterien:
1. Sichtbarer Typ & Auslösung: Die Frost-Bombe ist im Spiel optisch unterscheidbar und löst beim Anklicken eine sichtbare Eis-Explosion im definierten Radius aus.
2. Kettenreaktion & Punktevergabe: Alle normalen Ziele innerhalb des Explosionsradius werden durch C# erfasst, zerstört und ihre Punkte zum Score addiert.
3. Grenzfall (Keine Ziele im Radius / Boss-Schutz): Befinden sich keine weiteren Ziele im Radius, gibt die Frost-Bombe nur ihre eigenen Basispunkte ohne Fehler; ein Boss-Ziel im Radius nimmt maximal 1 Schaden, statt sofort vernichtet zu werden.
