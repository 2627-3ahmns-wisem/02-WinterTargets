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

    public void IncrementScore(int points)
    {
        if (isGameOver) return;

        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        // Wenn Power-up aktiv ist, Punkte verdoppeln
        int finalPoints = isDoublePointsActive ? points * 2 : points;
        score += finalPoints;

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
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