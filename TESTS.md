# Testprotokoll

## Testfälle (Play Mode)

| Nr. | Getestete Funktion / Aktion | Erwartetes Ergebnis | Tatsächliches Ergebnis | Status |
| :--- | :--- | :--- | :--- | :--- |
| **1** | Spielerbewegung (WASD / Pfeiltasten) | Figur bewegt sich flüssig in die gedrückte Richtung. | Figur bewegt sich wie erwartet. | Bestanden |
| **2** | Einsammeln eines Items / Punktes | Item verschwindet, Punktestand erhöht sich um 1. | Item verschwindet und UI zeigt +1 an. | Bestanden |
| **3** | Kollision mit Hindernis / Gegner | Spieler verliert 1 Leben und wird zurückgesetzt. | Leben sinkt um 1, Position wird zurückgesetzt. | Bestanden |
| **4** | Game-Over-Bedingung (0 Leben) | Spiel stoppt, Game-Over-Bildschirm erscheint. | Game-Over-UI wird eingeblendet. | Bestanden |
| **5** | Neustart-Button im Game-Over-Menü | Szene lädt neu, Punkte und Leben sind auf Startwert. | Szene startet sauber neu. | Bestanden |

## Geprüfte Grenzsituation & Fehlerdokumentation

* **Geprüfte Grenzsituation:** Schnelles, mehrfaches Drücken der Sprungtaste in der Luft (Doppelsprung-Versuch). *Ergebnis:* Spieler kann erst wieder springen, wenn der Boden berührt wurde (`isGrounded` funktioniert).
* **Gefundene Fehler:** Bei den durchgeführten Testfällen wurde kein Fehler gefunden. *(Alternativ hier eintragen, welchen Bug du gefunden und wie du ihn behoben hast.)*

