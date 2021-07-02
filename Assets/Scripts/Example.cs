using UnityEngine;

public class Example : MonoBehaviour {

    [SerializeField]
    CustomVideoPlayer player1;
    [SerializeField]
    CustomVideoPlayer player2;

    void Start() {
        player1.PrepareVideo("https://matthewhallberg.com/video/holo.mp4", player1.PlayVideo);
        player2.PrepareVideo("https://matthewhallberg.com/video/screen.mp4", player2.PlayVideo);
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
