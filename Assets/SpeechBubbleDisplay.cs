using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechBubbleDisplay : MonoBehaviour
{
    public GameObject canvas;
    public TMPro.TextMeshProUGUI text;
    public string textToDisplay = "TestText";
    public float writingSpeed = 0.1f;
    private bool running = false;
    private float startTime = -1;
    public bool isFinished = false;
    public float endDelay = 0.5f;

    void Update()
    {
        if (running && startTime == -1)
        {
            startTime = Time.time;
        }
        if (running && text.text != textToDisplay && textToDisplay.Length > 0)
        {
            text.text = textToDisplay.Substring(0, Mathf.Min(textToDisplay.Length, (int)((Time.time - startTime) / writingSpeed)));
        }
        if (Time.time - startTime > textToDisplay.Length * writingSpeed + endDelay)
        {
            isFinished = true;
        }
    }

    public void Begin()
    {
        canvas.SetActive(true);
        running = true;
        isFinished = false;
    }

    public void End()
    {
        running = false;
        startTime = -1;
        canvas.SetActive(false);
        text.text = "";
    }
}
