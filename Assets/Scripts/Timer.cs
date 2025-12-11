using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float timerDuration; // Set timer duration in seconds
    public float remainingTime;
    public bool isTimerRunning = false;

    [SerializeField] private TextMeshProUGUI timerText; // Optional: UI text to display the timer

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        isTimerRunning = true;
        if (isTimerRunning)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
            {
                remainingTime = 0;
                isTimerRunning = false;
                TimerFinished(); // Call method when timer finishes
            }

            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60F);
                int seconds = Mathf.FloorToInt(remainingTime % 60F);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                Debug.Log("Time Decreasing... Time is: " + minutes + " : " + seconds);
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
        remainingTime = timerDuration;
        UpdateTimerDisplay(); // Optional: Reset the timer UI
    }

    public void ResetTimer()
    {
        remainingTime = timerDuration;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60F);
            int seconds = Mathf.FloorToInt(remainingTime % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            Debug.Log("Time Decreasing... Time is: " + minutes + " : " + seconds);
        }
    }

    private void TimerFinished()
    {
        Debug.Log("Timer Finished!");
        // Do something when the timer ends, like trigger an event or end a level.
    }
}
