using UnityEngine;
using UnityEngine.UI;

public class TimeCounterTrigger : MonoBehaviour
{
    public Text timeText;        // Reference to display the time
    public bool isTiming;        // Is the timer actively running?
    private float startTime;     // Time when the timer began

    private int ticks; // ticks

    

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (GetComponent<Collider>().isTrigger) 
            {
                if (!isTiming) 
                {
                    startTime = Time.time;
                    isTiming = true;
                } 
                else if(isTiming)
                {
                    isTiming = false; // Stop the timer
                    // DisplayTime();
                    Time.timeScale = 0.0f;
                }

                //Disable Renderer
                GetComponent<Renderer>().enabled = false;
            }
        }
    }

 

    void Update()
    {   Debug.Log(isTiming);
        if (isTiming)
        {
            float elapsedTime = Time.time - startTime;
            ticks = (int)(elapsedTime * 100); // 100 ticks per sec
            DisplayTime();
            // Debug.Log(elapsedTime);
        }
    }

    void DisplayTime(float timeToDisplay = 0.0f)
    {
        int minutes = ticks / 6000;
        int seconds = (ticks % 6000) / 100;
        int displayTick = ticks % 100;
        timeText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, displayTick);
    }
}
