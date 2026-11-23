using UnityEngine;

public class Target : MonoBehaviour
{
    public int points = 10;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        Destroy(gameObject, 2f);
    }

    private void OnMouseDown()
    {
        Debug.Log("Target getroffen!");
        gameManager.IncrementScore(points);
        Destroy(gameObject);
    }
}