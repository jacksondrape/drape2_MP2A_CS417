using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    public int lives = 3;
    public GameObject[] foodStages;
    private int startingLives;

    private void Start()
    {
        startingLives = Mathf.Max(1, lives);
        UpdateFood();
    }

    private void LateUpdate() { UpdateFood(); }

    private void UpdateFood()
    {
        if (foodStages == null || foodStages.Length == 0) return;
        int stage = lives <= 0 ? foodStages.Length
            : Mathf.Min(foodStages.Length - 1, Mathf.FloorToInt(
                (1f - Mathf.Clamp01((float)lives / startingLives)) * foodStages.Length));
        for (int i = 0; i < foodStages.Length; i++)
            if (foodStages[i] != null) foodStages[i].SetActive(i == stage);
    }

    [ContextMenu("Test: Lose One Life")]
    private void TestLoseLife()
    {
        if (Application.isPlaying) LoseLife();
    }

    public void LoseLife()
    {
        if (lives <= 0)
        {
            return;
        }

        lives--;
        Debug.Log("Base lives: " + lives, this);

        if (lives == 0)
        {
            Debug.Log("Game over.", this);
        }
    }
}
