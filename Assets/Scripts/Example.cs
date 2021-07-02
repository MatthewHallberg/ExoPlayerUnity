using UnityEngine;

public class Example : MonoBehaviour {

    [SerializeField]
    CustomVideoPlayer player1;

    void Start() {
        player1.PrepareVideo("https://matthewhallberg.com/video/holo.mp4", OnVideo1Prepared);
    }


    void OnVideo1Prepared() {
        player1.PlayVideo();
        Debug.Log("Video 1 prepare complete!!!");
    }
}
