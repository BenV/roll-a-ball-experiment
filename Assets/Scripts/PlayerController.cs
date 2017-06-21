using UnityEngine;

// Include the namespace required to use Unity UI
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.Collections;

public class PlayerController : MonoBehaviour {
    private const string PICK_UP_TAG = "Pick Up";

    // Create public variables for player speed, time allowed, and for the Text UI game objects
    public int numLevels = 5;
    public float minSpawnTime = 5;
    public float maxSpawnTime = 10;
    public float speed;
    public float timeAllowed;
    public Text countText;
    public Text gameOverText;
    public GameObject timer;
    public GameObject cubePrefab;
    public GameObject explosionPrefab;

    // Static variables will retain their value even after we re-load the scene
    private static int level = 1;

    // Create private references to the rigidbody component on the player, and the count of pick up objects picked up so far
    private Rigidbody rb;
    private int count;
    private float timeRemaining;
    private GameObject pickUps;

    // At the start of the game..
    void Start() {
        // Assign the Rigidbody component to our private rb variable
        rb = GetComponent<Rigidbody>();

        // Set the count to zero
        count = 0;

        // Set the time remaining equal to the "lose" time
        timeRemaining = timeAllowed;

        // Run the SetScoreText function to update the UI (see below)
        SetScoreText();

        // Set the text property of our Game Over Text UI to an empty string, making the game over message blank
        gameOverText.text = "";

        // Start the initial cube spawn timer
        pickUps = GameObject.Find("Pick Ups");
        SpawnCube();
    }

    void Update() {
        // If the game is already over we don't need to do anything here
        if (GameOver()) {
            CancelInvoke("SpawnCube");
            return;
        }

        // Update the timer and check to see if the player has lost
        UpdateTimer();
        if (GameLost()) {
            gameOverText.text = "You Lose :(";
        }
    }

    void SpawnCube() {
        // Create a new cube and give it a random position
        GameObject newCube = Instantiate(cubePrefab, pickUps.transform);
        float existingY = newCube.transform.position.y;
        newCube.transform.position = new Vector3(Random.Range(-5, 5), existingY, Random.Range(-5, 5));

        // Modify the spawn time to make cubes spawn faster as the difficulty increases
        float modifier = (1.0f - DifficultyMultiplier() / 2.0f);

        // Invoke schedules a method to be called at some time in the future
        Invoke("SpawnCube", Random.Range(minSpawnTime, maxSpawnTime) * modifier);
    }

    void UpdateTimer() {
        // Subtract time at a faster rate depending on our difficulty multiplier
        float dt = Time.deltaTime * (DifficultyMultiplier() + 1);

        // Subtract from the time remaining and update the timer UI. Use Mathf.Max to make sure
        // we don't go below zero when subtracting.
        timeRemaining = Mathf.Max(0, timeRemaining - dt);
        float timeLeft = timeRemaining / timeAllowed;
        timer.transform.localScale = new Vector3(timeLeft, 1, 1);
    }

    bool GameLost() {
        // The player has lost if no more time is remaining
        return timeRemaining <= 0;
    }

    bool GameWon() {
        // The player has won if there are no "Pick ups" remaining, and they have not lost
        return !GameLost() && GameObject.FindGameObjectsWithTag(PICK_UP_TAG).Length == 0;
    }

    bool GameOver() {
        // The game is over if the player has either won or lost
        return GameWon() || GameLost();
    }

    float DifficultyMultiplier() {
        // Multiplier is 0 on level 1, and increases linearly for each level
        return (level - 1.0f) / (numLevels - 1.0f);
    }

    // Each physics step..
    void FixedUpdate() {
        // Set some local float variables equal to the value of our Horizontal and Vertical Inputs
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Create a Vector3 variable, and assign X and Z to feature our horizontal and vertical float variables above
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // Add a physical force to our Player rigidbody using our 'movement' Vector3 above,
        // multiplying it by 'speed' - our public player speed that appears in the inspector
        rb.AddForce(movement * speed);
    }

    // When this game object intersects a collider with 'is trigger' checked,
    // store a reference to that collider in a variable named 'other'..
    void OnTriggerEnter(Collider other) {
        // ..and if the game object we intersect has the tag 'Pick Up' assigned to it..
        if (other.gameObject.CompareTag(PICK_UP_TAG)) {
            // Make the other game object (the pick up) inactive, to make it disappear
            other.gameObject.SetActive(false);

            // Create an explosion
            Instantiate(explosionPrefab, other.gameObject.transform.position, Quaternion.identity);

            // Add one to the score variable 'count'
            count = count + 1;

            // Run the 'SetScoreText()' function (see below)
            SetScoreText();

            // If the game is not over add to the available time to give the player a boost
            if (!GameOver()) {
                // Reduce the amount of the time bonus depending on the difficulty multiplier
                float modifier = DifficultyMultiplier() * 0.75f;
                timeRemaining = Mathf.Min(timeAllowed, timeRemaining + (1.0f - modifier));
            }
        }
    }

    // Create a standalone function that can update the 'countText' UI and check if the required amount to win has been achieved
    void SetScoreText() {
        // Update the text field of our 'countText' variable
        countText.text = "Level: " + level + "\nCount: " + count.ToString();

        // If there are no more objects in the scene with the "Pick Up" tag, then the player has won 
        if (GameWon()) {
            // Set the text value of our game over text
            if (level == numLevels) {
                gameOverText.text = "YOU DID IT!!!";
            } else {
                gameOverText.text = "Nice work, you beat level " + level + "!";
                Invoke("StartNextLevel", 5);
            }
        }
    }

    void StartNextLevel() {
        // Increment the level (this is shorthand for "level = level + 1", and reload the scene
        level++;
        SceneManager.LoadScene(0);
    }
}
