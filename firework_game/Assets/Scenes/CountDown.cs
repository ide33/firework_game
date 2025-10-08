using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour
{
    public int countdownMinutes = 3;
    private float countdownSeconds;
    private Text TimerText;

    private void Start()
    {
        TimerText = GetComponent<Text>();
        countdownSeconds = countdownMinutes * 60;
    }

    void Update()
    {
        countdownSeconds -= Time.deltaTime;
        var span = new TimeSpan(0, 0, (int)countdownSeconds);
        TimerText.text = span.ToString(@"m\:ss");

        if (countdownSeconds <= 0)
        {
            // 0•b‚É‚È‚Á‚½‚Æ‚«‚Ìˆ—
        }
    }
}
