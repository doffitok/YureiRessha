using UnityEngine;

public class DayLogic : MonoBehaviour
{
    public int currentSecond { get; private set; } = 0;
    public int maxSeconds = 300;

    private bool isRunning = false;
    private float timer = 0f;

    void Update()
    {
        if (!isRunning) return;

        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            currentSecond++;
            if (currentSecond > maxSeconds)
                currentSecond = 0;

            timer = 0f;
        }
    }

    public void StartDay()
    {
        currentSecond = 0;
        isRunning = true;
    }
}