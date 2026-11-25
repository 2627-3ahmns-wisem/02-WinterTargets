using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Wichtig für den Neustart der Runde

public class GameManager : MonoBehaviour
{
    [Header("Targets (2 = 60%, 1 = 30%, 0 = 10%)")]
    public GameObject[] targets;
    public GameObject parentOfTargets;

    [Header("UI & Spielzustand")]
    public Text scoreText;
    public Text timerText;          // UI-Text für den Countdown
    public GameObject gameOverPanel; // Panel für die Ergebnisanzeige am Ende
    public Text finalScoreText;      // UI-Text im Panel für den Endpunktestand

    [Header("Runden-Einstellungen")]
    public float roundDuration = 60f;

    
    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip hitSound;
    private AudioSource audioSource;


     private int score = 0;
    private float timeRemaining;
    private bool isGameOver = false;

    // Erlaubt dem Target-Skript abzufragen, ob das Spiel noch läuft
    public bool IsGameOver => isGameOver;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    
        timeRemaining = roundDuration;
        isGameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateTimerUI();
        InvokeRepeating("Spawn", 1f, 1f);
    }

    private void Update()
    {
        // Wenn die Runde vorbei ist, nichts mehr ausführen
        if (isGameOver) return;

        // Countdown herunterzählen
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame();
        }

        UpdateTimerUI();

        // Schuss-Sound nur während der aktiven Runde abspielen
        if (Input.GetMouseButtonDown(0))
        {
            if (audioSource != null && shootSound != null)
                audioSource.PlayOneShot(shootSound);
        }
    }


     
     
   public void IncrementScore(int points)
    {
        if (isGameOver) return;

        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        score += points;
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }





    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // Auf ganze Sekunden aufrunden (zeigt 60 bis 0 an)
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }

    private void EndGame()
    {
        isGameOver = true;
        CancelInvoke("Spawn");

        // Alle noch aktiven Ziele vom Bildschirm entfernen
        if (parentOfTargets != null)
        {
            foreach (Transform child in parentOfTargets.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Ergebnisanzeige einblenden
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Endpunktestand: " + score + " Punkte";
        }
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

        float randomChance = Random.Range(0f, 100f);
        int selectedIndex = 0;

        if (randomChance < 60f)
        {
            selectedIndex = 2;
        }
        else if (randomChance < 90f)
        {
            selectedIndex = 1;
        }
        else
        {
            selectedIndex = 0;
        }

        GameObject newTarget = Instantiate(
            targets[selectedIndex],
            parentOfTargets.transform
        );

        newTarget.transform.position = spawnPosition;
    }

   

    // Diese Methode wird vom Restart-Button im UI aufgerufen
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}