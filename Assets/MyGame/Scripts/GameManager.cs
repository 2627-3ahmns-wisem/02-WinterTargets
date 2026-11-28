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

    [Header("Power-up Einstellungen")]
    public float powerUpDuration = 5f;
    public Text powerUpStatusText;    // Optionaler UI-Text für Wirkung & Restdauer

    [Header("UI & Spielzustand")]
    public Text scoreText;
    public Text timerText;
    public GameObject gameOverPanel;
    public Text finalScoreText;

    [Header("Runden-Einstellungen")]
    public float roundDuration = 60f;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip powerUpSound;    // Optionaler Sound für Power-up
    private AudioSource audioSource;



    [Header("Combo & Bonus Einstellungen")]
    public int comboThreshold = 3;      // Ab 3 Treffern in Folge gibt es Bonus
    public int comboBonusPoints = 15;   // Höhe der Bonuspunkte
    public Text comboText; 

    private int currentCombo = 0;
    private int lastBonusAwarded = 0;

    public int CurrentCombo => currentCombo;
    public int LastBonusAwarded => lastBonusAwarded;


    private int score = 0;
    private float timeRemaining;
    private bool isGameOver = false;

    // Power-up Zustand & Restdauer (über Properties auch von außen/Tests lesbar)
    private bool isDoublePointsActive = false;
    private float powerUpTimeRemaining = 0f;

    public bool IsGameOver => isGameOver;
    public bool IsDoublePointsActive => isDoublePointsActive;
    public float PowerUpTimeRemaining => powerUpTimeRemaining;

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


        if (scoreText != null) scoreText.text = "0";
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateTimerUI();
        UpdatePowerUpUI();
        InvokeRepeating("Spawn", 1f, 1f);
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
        }
        UpdateTimerUI();

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
        powerUpTimeRemaining = powerUpDuration; // Setzt/verlängert Dauer auf 5 Sekunden

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

    // Wenn nichts getroffen wurde ODER das getroffene Objekt kein Target ist -> Fehlschuss!
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

    // Ab 3 Treffern in Folge Bonuspunkte berechnen
    if (currentCombo >= comboThreshold)
    {
        lastBonusAwarded = comboBonusPoints;
        score += lastBonusAwarded;
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
        UpdatePowerUpUI();
        CancelInvoke("Spawn");

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
    }

    private void Spawn()
    {
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

        // Prüfen, ob das seltene Power-up spawnt (z. B. 10 % Chance)
        if (powerUpPrefab != null && Random.Range(0f, 100f) < powerUpSpawnChance)
        {
            prefabToSpawn = powerUpPrefab;
        }
        else
        {
            // Normale Spawn-Verteilung (60% / 30% / 10%)
            float randomChance = Random.Range(0f, 100f);
            int selectedIndex = 0;

            if (randomChance < 60f)
                selectedIndex = 2;
            else if (randomChance < 90f)
                selectedIndex = 1;
            else
                selectedIndex = 0;

            prefabToSpawn = targets[selectedIndex];
        }

        GameObject newTarget = Instantiate(prefabToSpawn, parentOfTargets.transform);
        newTarget.transform.position = spawnPosition;


        
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}