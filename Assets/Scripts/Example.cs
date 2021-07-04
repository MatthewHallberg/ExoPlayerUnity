using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour {

    [SerializeField]
    CustomVideoPlayer player1;
    [SerializeField]
    CustomVideoPlayer player2;

    [SerializeField]
    Slider slider;

    void Start() {
        player1.PrepareVideo("https://matthewhallberg.com/video/holo.mp4", OnPlayer1Prepared);
        player2.PrepareVideo("https://matthewhallberg.com/video/screen.mp4", OnPlayer2Prepared);
    }

    void OnPlayer1Prepared() {
        Debug.Log("1 Size: " + player1.GetWidth() + " : " + player1.GetHeight());
        player1.SetLooping(true);
        player1.PlayVideo();
        StartCoroutine(UpdatSliderRoutine());
    }

    IEnumerator UpdatSliderRoutine() {
        while (true) {
            yield return new WaitForSeconds(.5f);
            slider.value = player1.GetCurrentPlaybackPercent();
        }
    }

    void OnPlayer2Prepared() {
        Debug.Log("2 Size: " + player2.GetWidth() + " : " + player2.GetHeight());
        player2.SetLooping(true);
        player2.PlayVideo();
    }

    public void Play() {
        player1.PlayVideo();
        player2.PlayVideo();
    }

    public void Pause() {
        player1.PauseVideo();
        player2.PauseVideo();
    }

    public void Stop() {
        StopAllCoroutines();
    }

    public void OnSliderValueChanged(float value) {
        //player1.SetPlaybackPosition(value);
        //player2.SetPlaybackPosition(value);
    }
}
