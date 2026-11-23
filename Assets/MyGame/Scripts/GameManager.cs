using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject[] targets;
    public GameObject parentOfTargets;
    public GameObject winSprite;
    public Text scoreText;
    public int winScore = 100;

    private int score = 0;
    private bool win = false;

    private void Start()
    {
        InvokeRepeating("Spawn",1f,1f);
    }

    private void Update()
    {
        //if the player hits 10 times, the player has won and the Spawn method will not called further. 
        if(win)
        {
            CancelInvoke("Spawn");
        }

        //every shoot (clicking left mouse button) produce a shooting sound.
        if (Input.GetMouseButtonDown(0))
        {
            gameObject.GetComponent<AudioSource>().Play();
        }
    }

    //Spawn a target at a random position within a specified x and y range.
    //Instantiate (make a concrete GameObject, i.e., a clone from the given prefeb target) the target as a child
    //of the GameObject ParentTargets. In this case transform.localPosition instead of transform.position is important!!
    private void Spawn()
    {
        if (targets == null || targets.Length == 0) return;

        if (parentOfTargets == null ) return;

            Camera cam = Camera.main;

    if (cam == null)
        return;

    // Sichtbaren Bereich der Kamera berechnen
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

    Vector3 spawnPosition = new Vector3(
        randomX,
        randomY,
        0
    );

    int randomIndex = Random.Range(0, targets.Length);

    GameObject newTarget = Instantiate(
        targets[randomIndex],
        parentOfTargets.transform
    );

    newTarget.transform.position = spawnPosition;

    Debug.Log("Target wurde erstellt: " + newTarget.name);
}


    //is called in the class Target
    public void IncrementScore(int points)
    {
        score += points;
        print(score);

        scoreText.text = score.ToString();

        if (score >= winScore)
        {
            win = true;
            winSprite.SetActive(true);
        }
    }
}
