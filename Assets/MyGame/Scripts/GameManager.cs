using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Targets & Power-up")]
    public GameObject[] targets;      // Deine 3 normalen Ziele (Index 0, 1, 2)
    public GameObject powerUpPrefab;  // Das neue Power-up-Prefab
    [Range(0f, 100f)]
    public float powerUpSpawnChance = 10f; // 10 % Wahrscheinlichkeit (selten)
    public GameObject parentOfTargets;




    [Header("Audio & Musik")]
    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip powerUpSound;
    public AudioClip bossHitSound;
    public AudioClip backgroundMusic;    // NEU: Hintergrundmusik-Clip
    public Slider volumeSlider;          // NEU: UI-Slider (0.0 bis 1.0)
    public Toggle muteToggle;            // NEU: Optionaler Mute-Schalter (Stumm)

    private AudioSource audioSource;     // Bestehende AudioSource für Soundeffekte (SFX)
    private AudioSource musicSource;     // NEU: Eigene AudioSource nur für Hintergrundmusik
    private const string VOLUME_KEY = "MasterVolume";





    [Header("Power-up Einstellungen")]
    public float powerUpDuration = 5f;
    public Text powerUpStatusText;    // Optionaler UI-Text für Wirkung & Restdauer

    [Header("Combo & Bonus Einstellungen")]
    public int comboThreshold = 3;      // Ab 3 Treffern in Folge gibt es Bonus
    public int comboBonusPoints = 15;   // Höhe der Bonuspunkte
    public Text comboText;              // UI-Text für Combo & Bonus

    [Header("Schwierigkeits-Einstellungen")]
    public float startSpawnInterval = 1.2f;     // Langsamer Start (1.2 Sekunden)
    public float minSpawnInterval = 0.35f;      // Untergrenze: Nie schneller als 0.35s
    public float startSpeedMultiplier = 1.0f;   // Normale Geschwindigkeit am Anfang
    public float maxSpeedMultiplier = 2.2f;     // Obergrenze: Maximal 2.2x so schnell




    [Header("Boss-Einstellungen")]
    public GameObject bossPrefab;         // Dein Boss-Schneemann-Prefab
    public int bossScoreInterval = 100;   // Nachvollziehbare Bedingung: Alle 100 Punkte erscheint ein Boss
   

    private int nextBossScoreThreshold;
    private GameObject activeBoss;



    [Header("Spezialeffekte")]
    public GameObject comboBonusEffectPrefab;



    [Header("Runden-Einstellungen")]
    public float roundDuration = 60f;


    [Header("UI & Spielzustand")]
    public Text scoreText;
    public Text highScoreText;       // NEU: UI-Text für den Highscore (im Spiel oder GameOver-Panel)
    public Text timerText;
    public GameObject gameOverPanel;
    public Text finalScoreText;

    // Konstante für PlayerPrefs und private Variable
    private const string HIGH_SCORE_KEY = "HighScore";
    private int highScore = 0;

    public int HighScore => highScore; 



    

    private int score = 0;
    private float timeRemaining;
    private bool isGameOver = false;

    // Power-up Zustand & Restdauer
    private bool isDoublePointsActive = false;
    private float powerUpTimeRemaining = 0f;

    // Combo Zustand
    private int currentCombo = 0;
    private int lastBonusAwarded = 0;

    // Schwierigkeit & Spawn-Timer
    private float currentSpawnInterval;
    private float currentSpeedMultiplier;
    private float spawnTimer;

    // Öffentliche Properties für Tests & UI
    public bool IsGameOver => isGameOver;
    public bool IsDoublePointsActive => isDoublePointsActive;
    public float PowerUpTimeRemaining => powerUpTimeRemaining;
    public int CurrentCombo => currentCombo;
    public int LastBonusAwarded => lastBonusAwarded;
    public float CurrentSpawnInterval => currentSpawnInterval;
    public float CurrentSpeedMultiplier => currentSpeedMultiplier;

    
    
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        timeRemaining = roundDuration;
        isGameOver = false;
        isDoublePointsActive = false;
        powerUpTimeRemaining = 0f;

        currentCombo = 0;
        lastBonusAwarded = 0;
        UpdateComboUI();

        // Schwierigkeit auf Startwerte setzen
        currentSpawnInterval = startSpawnInterval;
        currentSpeedMultiplier = startSpeedMultiplier;
        spawnTimer = currentSpawnInterval;

        if (scoreText != null) scoreText.text = "0";
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateTimerUI();
        UpdatePowerUpUI();

        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        UpdateHighScoreUI();


        nextBossScoreThreshold = bossScoreInterval;
        activeBoss = null;






        audioSource = GetComponent<AudioSource>();

        // Eigene AudioSource für Musik erzeugen, damit SFX und Musik getrennt laufen
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.5f; // Musik etwas dezenter als Soundeffekte

        // Gespeicherte Lautstärke laden (Standard: 1.0 = 100%)
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1.0f);
        AudioListener.volume = savedVolume;

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (muteToggle != null)
        {
            muteToggle.isOn = (savedVolume <= 0.001f);
            muteToggle.onValueChanged.AddListener(SetMute);
        }

        // Hintergrundmusik starten (verhindert mehrfaches Abspielen)
        if (backgroundMusic != null && !musicSource.isPlaying)
        {
           musicSource.Play();
        }

    }

    private void Update()
    {
        if (isGameOver) return;

        // Runden-Countdown
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame();
            return;
        }
        UpdateTimerUI();

        // Schwierigkeit anhand des Rundenfortschritts berechnen (0 = Anfang, 1 = Ende)
        float progress = Mathf.Clamp01(1f - (timeRemaining / roundDuration));
        currentSpawnInterval = Mathf.Lerp(startSpawnInterval, minSpawnInterval, progress);
        currentSpeedMultiplier = Mathf.Lerp(startSpeedMultiplier, maxSpeedMultiplier, progress);

        // Dynamisches Spawnen über Timer
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            Spawn();
            spawnTimer = currentSpawnInterval;
        }

        // Power-up Countdown
        if (isDoublePointsActive)
        {
            powerUpTimeRemaining -= Time.deltaTime;
            if (powerUpTimeRemaining <= 0f)
            {
                powerUpTimeRemaining = 0f;
                isDoublePointsActive = false;
                Debug.Log("Power-up abgelaufen: Normale Punkte wieder aktiv.");
            }
            UpdatePowerUpUI();
        }

        // Schuss & Fehlschuss-Erkennung
        if (Input.GetMouseButtonDown(0))
        {
            if (audioSource != null && shootSound != null)
                audioSource.PlayOneShot(shootSound);

            CheckForMiss();
        }
    }

    public void ActivateDoublePoints()
    {
        if (isGameOver) return;

        isDoublePointsActive = true;
        powerUpTimeRemaining = powerUpDuration;

        if (audioSource != null && powerUpSound != null)
            audioSource.PlayOneShot(powerUpSound);
        else if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        Debug.Log("Power-up aktiviert: 2x Punkte für " + powerUpDuration + " Sekunden!");
        UpdatePowerUpUI();
    }

    private void CheckForMiss()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider == null || hit.collider.GetComponent<Target>() == null)
        {
            ResetCombo();
        }
    }

    public void RegisterHit()
    {
        if (isGameOver) return;

        currentCombo++;
        lastBonusAwarded = 0;

        if (currentCombo >= comboThreshold)
        {
            lastBonusAwarded = comboBonusPoints;
            score += lastBonusAwarded;

            // Spezialeffekt an der Trefferposition auslösen
            if (comboBonusEffectPrefab != null && Camera.main != null)
            {
                Vector3 hitWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                hitWorldPos.z = 0f;
                GameObject bonusFx = Instantiate(comboBonusEffectPrefab, hitWorldPos, Quaternion.identity);
                Destroy(bonusFx, 1.5f);
            }

            Debug.Log($"COMBO {currentCombo}x! +{lastBonusAwarded} Bonuspunkte!");
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        UpdateComboUI();
    }

    public void ResetCombo()
    {
        if (currentCombo > 0)
        {
            Debug.Log($"Fehlschuss! Combo von {currentCombo} auf 0 zurückgesetzt.");
        }
        currentCombo = 0;
        lastBonusAwarded = 0;
        UpdateComboUI();
    }

    public void IncrementScore(int points)
    {
        if (isGameOver) return;

        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        RegisterHit();

        int finalPoints = isDoublePointsActive ? points * 2 : points;
        score += finalPoints;

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }


        if (bossPrefab != null && activeBoss == null && score >= nextBossScoreThreshold)
        {
            SpawnBoss();
            nextBossScoreThreshold += bossScoreInterval;
        }

    }

    private void UpdateComboUI()
    {
        if (comboText == null) return;

        if (currentCombo > 0)
        {
            comboText.gameObject.SetActive(true);
            if (lastBonusAwarded > 0)
            {
                comboText.text = $"COMBO: {currentCombo}x (+{lastBonusAwarded} BONUS!)";
            }
            else
            {
                comboText.text = $"COMBO: {currentCombo}x";
            }
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }

    private void UpdatePowerUpUI()
    {
        if (powerUpStatusText == null) return;

        if (isDoublePointsActive)
        {
            powerUpStatusText.gameObject.SetActive(true);
            powerUpStatusText.text = $"2x PUNKTE! ({powerUpTimeRemaining:F1}s)";
        }
        else
        {
            powerUpStatusText.gameObject.SetActive(false);
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }

    private void EndGame()
    {
        isGameOver = true;
        isDoublePointsActive = false;
        powerUpTimeRemaining = 0f;
        currentCombo = 0;

        UpdatePowerUpUI();
        UpdateComboUI();

        if (parentOfTargets != null)
        {
            foreach (Transform child in parentOfTargets.transform)
            {
                Destroy(child.gameObject);
            }
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Endpunktestand: " + score + " Punkte";

        
        bool isNewHighScore = false;
        if (score > highScore)
        {
           highScore = score;
          PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
          PlayerPrefs.Save(); // Sofort dauerhaft auf die Festplatte schreiben
          isNewHighScore = true;
             Debug.Log("Neuer Highscore gespeichert: " + highScore);
        }

         UpdateHighScoreUI();

         if (finalScoreText != null)
        {
          if (isNewHighScore)
                finalScoreText.text = $"Endpunktestand: {score} Punkte\nNEUER HIGHSCORE: {highScore}!";
         else
              finalScoreText.text = $"Endpunktestand: {score} Punkte\nHighscore: {highScore}";
        }


        if (musicSource != null && musicSource.isPlaying)
        {
           musicSource.Stop();
        }
    }




    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }




    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "Highscore: " + highScore;
        }
    }

    // Praktisch zum Testen direkt über Rechtsklick auf das Skript im Inspector
    [ContextMenu("Highscore zurücksetzen (Test)")]
   
   
   
    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
        PlayerPrefs.Save();
        highScore = 0;
        UpdateHighScoreUI();
        Debug.Log("Highscore wurde auf 0 zurückgesetzt.");
    }
    





    private void Spawn()
    {  
        int selectedIndex = Random.Range(0, targets.Length);
        



        if (isGameOver) return;
        if (targets == null || targets.Length < 3) return;
        if (parentOfTargets == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        float height = cam.orthographicSize;
        float width = height * cam.aspect;

        float randomX = Random.Range(
            cam.transform.position.x - width + 0.8f,
            cam.transform.position.x + width - 0.8f
        );

        float randomY = Random.Range(
            cam.transform.position.y - height + 0.8f,
            cam.transform.position.y + height - 0.8f
        );

        Vector3 spawnPosition = new Vector3(randomX, randomY, 0);

        GameObject prefabToSpawn;

       if (powerUpPrefab != null && Random.Range(0f, 100f) < powerUpSpawnChance)
        {
            prefabToSpawn = powerUpPrefab;
        }
        else
        {
            

            // 15 % Chance auf Frost-Bombe (Element 3), falls vorhanden
            if (targets.Length >= 4 && Random.Range(0f, 100f) < 15f)
            {
                selectedIndex = 3;
            }
            else
            {
                float randomChance = Random.Range(0f, 100f);
                if (randomChance < 60f)
                    selectedIndex = 2;
                else if (randomChance < 90f)
                    selectedIndex = 1;
                else
                    selectedIndex = 0;
            }

            prefabToSpawn = targets[selectedIndex];
        }

        GameObject newTarget = Instantiate(prefabToSpawn, parentOfTargets.transform);
        newTarget.transform.position = spawnPosition;
    }




    public void RegisterBossHit(int remainingHealth, int maxHealth)
{
    if (isGameOver) return;

    // Zählt als gültiger Treffer in der Combo
    RegisterHit();

    if (audioSource != null)
    {
        if (bossHitSound != null)
            audioSource.PlayOneShot(bossHitSound);
        else if (hitSound != null)
            audioSource.PlayOneShot(hitSound);
    }

    Debug.Log($"Boss getroffen! Verbleibende Lebenspunkte: {remainingHealth}/{maxHealth}");
}

    private void SpawnBoss()
    {
        if (isGameOver || bossPrefab == null || parentOfTargets == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        float height = cam.orthographicSize;
        float width = height * cam.aspect;

        Vector3 spawnPos = new Vector3(
            Random.Range(cam.transform.position.x - width + 1.0f, cam.transform.position.x + width - 1.0f),
            Random.Range(cam.transform.position.y - height + 1.0f, cam.transform.position.y + height - 1.0f),
            0f
        );

        activeBoss = Instantiate(bossPrefab, parentOfTargets.transform);
        activeBoss.transform.position = spawnPos;

        Debug.Log($"BOSS ERSCHIENEN bei {score} Punkten!");
    }







    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VOLUME_KEY, volume);
        PlayerPrefs.Save();

        // Mute-Toggle automatisch anpassen, falls Slider auf 0 gezogen wird
        if (muteToggle != null)
        {
            muteToggle.SetIsOnWithoutNotify(volume <= 0.001f);
        }
    }

    public void SetMute(bool isMuted)
    {
        if (isMuted)
        {
            AudioListener.volume = 0f;
            if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(0f);
        }
        else
        {
            float restoredVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1.0f);
            if (restoredVolume <= 0.001f) restoredVolume = 0.5f;

            AudioListener.volume = restoredVolume;
            if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(restoredVolume);
            PlayerPrefs.SetFloat(VOLUME_KEY, restoredVolume);
            PlayerPrefs.Save();
        }
    }
}