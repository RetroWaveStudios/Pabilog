using TMPro;
using UnityEngine;

public class IncreasingTimer : MonoBehaviour
{
    public float elapsedTime = 0f; // Tracks the elapsed time
    public bool isTimerRunning = false;

    [SerializeField] private TextMeshProUGUI timerText; // Optional: UI text to display the timer
    private void Update()
    {
        isTimerRunning = true;
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime; // Increase the elapsed time by the time since the last frame
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(elapsedTime / 60F);
                int seconds = Mathf.FloorToInt(elapsedTime % 60F);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                Debug.Log("Time Incresing... Time is: " + minutes + " : " + seconds);
            }
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerDisplay(); // Optional: Reset the timer UI
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60F);
            int seconds = Mathf.FloorToInt(elapsedTime % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            Debug.Log("Time Incresing... Time is: " + minutes + " : " + seconds);
        }
    }
}
