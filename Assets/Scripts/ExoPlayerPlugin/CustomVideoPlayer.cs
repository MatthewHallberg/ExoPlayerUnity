using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class CustomVideoPlayer : MonoBehaviour {

    [HideInInspector]
    public Renderer rend;

    VideoPlayer videoPlayer;

    void Awake() {
        videoPlayer = GetComponent<VideoPlayer>();
        rend = GetComponent<Renderer>();

        //set default states
        videoPlayer.source = VideoSource.Url;
        videoPlayer.playOnAwake = false;
    }

    public void PrepareVideo(string url, UnityAction callback) {
        ExoPlayerUnity.instance.PrepareVideo(url, callback, this);
    }

    public void PlayVideo() {
        ExoPlayerUnity.instance.PlayVideo(this);
    }

    public void PauseVideo() {

    }
}
