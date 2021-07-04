using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class CustomVideoPlayer : MonoBehaviour {

    [HideInInspector]
    public Renderer rend;

    VideoPlayer videoPlayer;
    UnityAction prepareCallback;

    bool isPrepared;

    void Awake() {
        videoPlayer = GetComponent<VideoPlayer>();
        rend = GetComponent<Renderer>();

        //set default states
        videoPlayer.source = VideoSource.Url;
        videoPlayer.playOnAwake = false;
    }

    public void PrepareVideo(string url, UnityAction callback) {
        prepareCallback = callback;
        if (Application.platform == RuntimePlatform.Android)
            ExoPlayerUnity.instance.PrepareVideo(url, callback, this);
        else {
            videoPlayer.url = url;
            videoPlayer.prepareCompleted += OnPreparedUnity;
            videoPlayer.Prepare();
        }
        isPrepared = true;
    }

    void OnPreparedUnity(VideoPlayer player) {
        prepareCallback?.Invoke();
        videoPlayer.prepareCompleted -= OnPreparedUnity;
    }

    public void PlayVideo() {

        if (!IsPrepared()) {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
            ExoPlayerUnity.instance.PlayVideo(this);
        else {
            videoPlayer.Play();
        }
    }

    public void PauseVideo() {

        if (!IsPrepared()) {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
            ExoPlayerUnity.instance.PauseVideo(this);
        else {
            videoPlayer.Pause();
        }
    }

    public int GetWidth() {

        if (!IsPrepared()) {
            return 0;
        }

        if (Application.platform == RuntimePlatform.Android)
            return ExoPlayerUnity.instance.GetWidth(this);
        else {
            return (int)videoPlayer.width;
        }
    }

    public int GetHeight() {

        if (!IsPrepared()) {
            return 0;
        }

        if (Application.platform == RuntimePlatform.Android)
            return ExoPlayerUnity.instance.GetHeight(this);
        else {
            return (int)videoPlayer.height;
        }
    }

    bool IsPrepared() {
        if (!isPrepared) {
            Debug.Log("Must Prepare Video First!");
        }
        return isPrepared;
    }
}
