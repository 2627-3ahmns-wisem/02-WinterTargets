using UnityEngine;

public class Target : MonoBehaviour
{
    public int points = 10;
    public float speed = 3f;
    public float lifeTime = 3.5f;

    private GameManager gameManager;
    private Vector2 moveDirection;
    private float minX, maxX, minY, maxY;
    private float padding = 0.6f;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        Destroy(gameObject, 2f);

          
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
        // Kontrollierte Bewegung mit Time.deltaTime
        transform.Translate(moveDirection * speed * Time.deltaTime);

        // Prüfen, ob der linke oder rechte Rand erreicht wurde -> Richtung umkehren
        if (transform.position.x <= minX)
        {
            transform.position = new Vector3(minX, transform.position.y, transform.position.z);
            moveDirection.x = Mathf.Abs(moveDirection.x); // Nach rechts bewegen
        }
        else if (transform.position.x >= maxX)
        {
            transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
            moveDirection.x = -Mathf.Abs(moveDirection.x); // Nach links bewegen
        }
         if (transform.position.y <= minY)
        {
            transform.position = new Vector3(transform.position.x, minY, transform.position.z);
            moveDirection.y = Mathf.Abs(moveDirection.y); // Nach oben bewegen
        }
        else if (transform.position.y >= maxY)
        {
            transform.position = new Vector3(transform.position.x, maxY, transform.position.z);
            moveDirection.y = -Mathf.Abs(moveDirection.y); // Nach unten bewegen
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("Target getroffen!");
        gameManager.IncrementScore(points);
        Destroy(gameObject);
    }
}