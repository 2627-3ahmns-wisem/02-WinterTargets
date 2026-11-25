# Frosty Targets – 3AHMNS

Unity-Version: **6000.3.24f1**. Projekt: Frosty Targets.

## Projekt öffnen
1. Unity Hub öffnen, **Add project from disk** wählen und diesen Ordner auswählen.
2. Unity **6000.3.24f1** verwenden und den ersten Paketimport abwarten.
3. Szene `Assets/MyGame/Scenes/ShootingScene.unity` öffnen, **Play** drücken.
4. `Assets/MyGame/Scripts/GameManager.cs` und `Target.cs` untersuchen.

## Hinweis bei `UnityEngine.UI` / `Text`-Fehlern
Das Paket **Unity UI (uGUI)** ist unter `Packages/manifest.json` als `com.unity.ugui` eingetragen. Falls trotzdem CS0234 oder CS0246 erscheint: `Window > Package Management > Package Manager` öffnen und unter **In Project** prüfen, ob Unity UI geladen ist. Im Paketmanager **Refresh/Resolve** ausführen; gegebenenfalls Unity schließen und den Ordner `Library` löschen, dann erneut öffnen. Nicht die `using UnityEngine.UI`-Zeile entfernen: sie wird für die vorhandene Punkteanzeige gebraucht.

## Unterricht
Das vorhandene Spawning, der Score und der Sieg bei zehn Treffern sind **Ausgangscode**. Schüler:innen erweitern diese Mechaniken anhand der GitHub-Issues. Der Erstimport ist eine Ausgangsversion; keine Aufgaben sind vorweg implementiert.





## Issue 01 Erklärung
Das Spiel wird mit "private void Start()" gestartet. Es erscheinen Zielscheiben in zufälligen Positionen durch "private void Spawn". Die Zielscheiben werden angeklickt und somit "zerstört" mit "private void OnMouseDown". Jedes mal wenn man eine Zielscheibe erfolgreich zerstört, wird ein Score hinzugefügt mit "public void IncrementScore()". Sobald man 10 Zielscheiben erfolgreich "zerstört" hat hat man das Spiel gewonnen.

## ASSET-Quellen - Frosty Targets
1. Hintergrundgrafik ("Winter Landscape")
   - Quelle: [Link zur Webseite]
   - Urheber: [Name des Künstlers]
   - Lizenz: CC0 1.0 (Public Domain)

2. Treffer-Sound ("Ice Shatter / Snow Hit")
   - Quelle: [Link zur Sound-Datei]
   - Urheber: [Username / Künstler]
   - Lizenz: CC0 / Pixabay Content License

3. Ziel-Sprites (Schneemann, Geschenk, Eisblock) & Treffer-Partikel
   - Quelle: Eigene Erstellung / [Quelle eintragen]

