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

    public void SetLooping(bool shouldLoop) {

        if (!IsPrepared()) {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
            ExoPlayerUnity.instance.SetLooping(shouldLoop, this);
        else {
            videoPlayer.isLooping = shouldLoop;
        }
    }

    //pass in format 0 - 1
    public void SetPlaybackPosition(float value) {
        float newTime = value * (float)videoPlayer.length;
        videoPlayer.Pause();
        videoPlayer.time = newTime;
        videoPlayer.Play();
    }

    //returns format 0-1
    public float GetCurrentPlaybackPercent() {

        if (!IsPrepared()) {
            return 0;
        }

        if (Application.platform == RuntimePlatform.Android)
            return ExoPlayerUnity.instance.GetPlaybackPercent(this);
        else {
            return (float)videoPlayer.time / (float)videoPlayer.length;
        }
    }

    public bool IsPlaying() {

        if (!IsPrepared()) {
            return false;
        }

        if (Application.platform == RuntimePlatform.Android)
            return ExoPlayerUnity.instance.IsPlaying(this);
        else {
            return videoPlayer.isPlaying;
        }
    }

    bool IsPrepared() {
        if (!isPrepared) {
            Debug.Log("Must Prepare Video First!");
        }
        return isPrepared;
    }
}
