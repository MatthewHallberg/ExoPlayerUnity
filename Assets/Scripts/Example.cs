using UnityEngine;

public class Example : MonoBehaviour {

    [SerializeField]
    CustomVideoPlayer player1;
    [SerializeField]
    CustomVideoPlayer player2;

    void Start() {
        player1.PrepareVideo("https://matthewhallberg.com/video/holo.mp4", OnPlayer1Prepared);
        player2.PrepareVideo("https://matthewhallberg.com/video/screen.mp4", OnPlayer2Prepared);
    }

    void OnPlayer1Prepared() {
        Debug.Log("1 Size: " + player1.GetWidth() + " : " + player1.GetHeight());
        player1.PlayVideo();
    }

    void OnPlayer2Prepared() {
        Debug.Log("2 Size: " + player2.GetWidth() + " : " + player2.GetHeight());
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
}
