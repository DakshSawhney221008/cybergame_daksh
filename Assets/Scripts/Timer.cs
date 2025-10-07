using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Timer : NetworkBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] TMP_Text timerTxt;
    [SerializeField] Slider timerSlider;
    [Tooltip("Duration of the timer in seconds.")]
    public float duration = 10f;

    [Tooltip("Invoked when the timer finishes.")]
    public UnityEvent onTimerComplete;

    private float timeRemaining;
    private bool isRunning = false;

    void Update()
    {
        if(isServer)
        {
            if (!isRunning) return;

            timeRemaining -= Time.deltaTime;
            UpdateTimerUI(timeRemaining, duration);

            if (timeRemaining <= 0f)
            {
                isRunning = false;
                timeRemaining = 0f;
                onTimerComplete?.Invoke();
            }
        }
    }

    [ObserversRpc]
    private void UpdateTimerUI(float timeRemaining, float duration)
    {
        timerTxt.text = $"{(int)timeRemaining / 60}: {(timeRemaining % 60):F0}";
        timerSlider.value = timeRemaining / duration;
    }

    /// <summary>
    /// Starts the timer with the given duration.
    /// </summary>
    [ServerRpc]
    public void StartTimer(float customDuration = -1f)
    {
        timeRemaining = customDuration > 0 ? customDuration : duration;
        isRunning = true;
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    [ServerRpc]
    public void StopTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// Resets the timer without starting it.
    /// </summary>
    [ServerRpc]
    public void ResetTimer()
    {
        timeRemaining = duration;
        isRunning = false;
    }

    /// <summary>
    /// Returns whether the timer is currently running.
    /// </summary>
    public bool IsRunning()
    {
        return isRunning;
    }

    /// <summary>
    /// Returns how much time is left on the timer.
    /// </summary>
    public float GetTimeRemaining()
    {
        return timeRemaining;
    }
}
