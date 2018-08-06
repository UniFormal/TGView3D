using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MoviePlayerControl : MonoBehaviour {
    public MoviePlayerSample moviePlayerSample;
    public Text playButtonText;
    public string[] playButtonStrings = new string[2];
    
    public void Start()
    {
        SetPlayButtonText(false);
    }

    // Invert play/pause state
    public void TogglePaused()
    {
        bool newPauseState = !moviePlayerSample.videoPaused;
        moviePlayerSample.SetPaused(newPauseState);
        SetPlayButtonText(newPauseState);
    }

    void SetPlayButtonText(bool state)
    {
        playButtonText.text = (state) ? playButtonStrings[0] : playButtonStrings[1];
    }

    public void Rewind()
    {
        moviePlayerSample.Rewind();
    }

}
