using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    public int points = 10;
    public float speed = 3f;
    public float lifeTime = 3.5f;

    [Header("Lebenspunkte & Boss-Einstellungen")]
    public int maxHealth = 1;           // Normale Ziele = 1, Boss-Schneemann = 3
    public bool isBoss = false;         // Im Boss-Prefab anhaken!
    public GameObject bossDestroyEffectPrefab; // Großer Effekt nur beim endgültigen Abschuss

    [Header("Power-up Einstellungen")]
    public bool isPowerUp = false;



    [Header("Frost-Bombe (Eigene Mechanik)")]
    public bool isFrostBomb = false;          // Im Frost-Bomben-Prefab anhaken!
    public float explosionRadius = 3.5f;      // Wirkungsradius der Kettenreaktion
    public GameObject frostNovaEffectPrefab;


    [Header("Effekte")]
    public GameObject hitEffectPrefab;  // Kleiner Effekt bei normalem Treffer
    public GameObject powerUpEffectPrefab;      // Effekt 2: Goldener Effekt bei Power-up-Treffer
     




    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;

    private GameManager gameManager;
    private Vector2 moveDirection;
    private float minX, maxX, minY, maxY;
    private float padding = 0.6f;

    // Öffentliche Property zum Testen der aktuellen HP
    public int CurrentHealth => currentHealth;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        originalScale = transform.localScale;

        Destroy(gameObject, lifeTime);

        float randomX = Random.Range(-1f, 1f);
        float randomY = Random.Range(-1f, 1f);
        moveDirection = new Vector2(randomX, randomY).normalized;

        Camera cam = Camera.main;
        if (cam != null)
        {
            float height = cam.orthographicSize;
            float width = height * cam.aspect;

            minX = cam.transform.position.x - width + padding;
            maxX = cam.transform.position.x + width - padding;
            minY = cam.transform.position.y - height + padding;
            maxY = cam.transform.position.y + height - padding;
        }
    }

    private void Update()
    {
        // (Deine bestehende Bewegungs- und Randabprall-Logik bleibt hier komplett unverändert!)
        transform.Translate(moveDirection * speed * Time.deltaTime);

        if (transform.position.x <= minX)
        {
            transform.position = new Vector3(minX, transform.position.y, transform.position.z);
            moveDirection.x = Mathf.Abs(moveDirection.x);
        }
        else if (transform.position.x >= maxX)
        {
            transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
            moveDirection.x = -Mathf.Abs(moveDirection.x);
        }

        if (transform.position.y <= minY)
        {
            transform.position = new Vector3(transform.position.x, minY, transform.position.z);
            moveDirection.y = Mathf.Abs(moveDirection.y);
        }
        else if (transform.position.y >= maxY)
        {
            transform.position = new Vector3(transform.position.x, maxY, transform.position.z);
            moveDirection.y = -Mathf.Abs(moveDirection.y);
        }
    }

    private void OnMouseDown()
    {
        if (gameManager != null && gameManager.IsGameOver) return;

        TakeHit();
    }

    public void TakeHit()
    {
        currentHealth--;

        // FALL 1: Ziel überlebt den Treffer (z. B. Boss bei Treffer 1 und 2)
        if (currentHealth > 0)
        {
            // Kleiner Treffer-Effekt (keine Zerstörung!)
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1.0f);
            }

            // Combo weiterzählen & Treffer-Feedback geben
            if (gameManager != null)
            {
                gameManager.RegisterBossHit(currentHealth, maxHealth);
            }

            // Sichtbares Feedback: Kurz rot aufblinken & minimal kleiner werden
            StopAllCoroutines();
            StartCoroutine(HitFlashRoutine());
            return;
        }

        // FALL 2: Endgültiger Abschuss (currentHealth <= 0)
        GameObject finalEffect = (isBoss && bossDestroyEffectPrefab != null) ? bossDestroyEffectPrefab : hitEffectPrefab;
        if (finalEffect != null)
        {
            GameObject effect = Instantiate(finalEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2.0f);
        }

        if (gameManager != null)
        {
            if (isPowerUp)
            {
                gameManager.RegisterHit();
                gameManager.ActivateDoublePoints();
            }
            else
            {
                gameManager.IncrementScore(points);
                if (isBoss)
                {
                    Debug.Log("BOSS BESIEGT! Endgültiger Abschuss!");
                }
            }
        }

        if (isFrostBomb)
        {
            TriggerFrostExplosion();
        }

        Destroy(gameObject);




        GameObject effectToSpawn = hitEffectPrefab;

        if (isPowerUp && powerUpEffectPrefab != null)
        {
            effectToSpawn = powerUpEffectPrefab;
        }
        else if (isBoss && bossDestroyEffectPrefab != null)
        {
            effectToSpawn = bossDestroyEffectPrefab;
        }

        if (effectToSpawn != null)
        {
            GameObject effect = Instantiate(effectToSpawn, transform.position, Quaternion.identity);
            Destroy(effect, 1.5f); // Effekt räumt sich selbst auf -> stört das Spiel nicht
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
        // Kurzer Treffer-Impuls (aufblähen)
        transform.localScale = originalScale * 1.15f;

        yield return new WaitForSeconds(0.12f);

        if (spriteRenderer != null)
        {
            // Bleibt mit jedem Treffer leicht rötlicher, damit Schaden dauerhaft sichtbar ist
            float healthRatio = (float)currentHealth / maxHealth;
            spriteRenderer.color = Color.Lerp(new Color(1f, 0.45f, 0.45f), originalColor, healthRatio);
        }
        // Schrumpft pro Treffer leicht, damit man auch an der Größe sieht, dass HP fehlen
        float scaleFactor = 0.85f + (0.15f * ((float)currentHealth / maxHealth));
        transform.localScale = originalScale * scaleFactor;
    }







    private void TriggerFrostExplosion()
    {
        // 1. Sichtbaren Eis-Effekt auslösen
        if (frostNovaEffectPrefab != null)
        {
            GameObject nova = Instantiate(frostNovaEffectPrefab, transform.position, Quaternion.identity);
            Destroy(nova, 1.5f);
        }

        // 2. Alle Collider im Umkreis erfassen
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        int affectedTargets = 0;

        foreach (Collider2D col in hitColliders)
        {
            // Sich selbst überspringen (Endlosschleife verhindern!)
            if (col.gameObject == gameObject) continue;

            Target otherTarget = col.GetComponent<Target>();
            if (otherTarget != null && !otherTarget.isPowerUp)
            {
                affectedTargets++;

                // Grenzfall: Wenn ein zweites Bomben-Ziel daneben liegt, nur normale Punkte geben
                if (otherTarget.isFrostBomb)
                {
                    otherTarget.isFrostBomb = false;
                }

                // Normale Ziele werden zerstört, Boss-Ziele nehmen 1 Schaden
                otherTarget.TakeHit();
            }
        }

        Debug.Log($"[Frost-Bombe] Kettenreaktion ausgelöst! {affectedTargets} umliegende Ziele getroffen.");
    }

    // Zeichnet den Explosionsradius im Unity-Scene-Fenster zur Kontrolle (Gizmo)
    private void OnDrawGizmosSelected()
    {
        if (isFrostBomb)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}