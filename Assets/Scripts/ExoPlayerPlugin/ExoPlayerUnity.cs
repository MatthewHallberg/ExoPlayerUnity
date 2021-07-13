using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public class ExoPlayerUnity : MonoBehaviour {

    public static ExoPlayerUnity instance;

    [DllImport("RenderingPlugin")]
    static extern System.IntPtr GetRenderEventFunc();

    [DllImport("RenderingPlugin")]
    static extern void RegisterUnityTextureID(int videoId, int UnityTextureID);

    [DllImport("RenderingPlugin")]
    static extern void DeleteSurfaceID(int videoId);

    class CurrentVideo {
        public CustomVideoPlayer videoPlayer;
        public UnityAction callback;
        public int videoId;
        public int textureID;
    }

    List<CurrentVideo> currVideos = new List<CurrentVideo>();
    List<int> texturesToCreate = new List<int>();
    int videoIds = 0;

    System.IntPtr? _VideoPlayerClass;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public void PrepareVideo(string url, UnityAction onPrepared, CustomVideoPlayer player) {

        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);

        if (currVideo == null) {

            currVideo = new CurrentVideo {
                videoPlayer = player,
                callback = onPrepared,
                videoId = videoIds++
            };

            currVideos.Add(currVideo);

            Texture2D videoTex = new Texture2D(
                1080,
                1920,
                TextureFormat.ARGB32,
                false);

            videoTex.Apply();

            //pass texture to plugin
            RegisterUnityTextureID(currVideo.videoId, (int)videoTex.GetNativeTexturePtr());

            // Set texture onto our material
            currVideo.videoPlayer.rend.material.mainTexture = videoTex;

            //call plugin function
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Prepare", "(Ljava/lang/String;Ljava/lang/String;)V");
            jvalue[] videoParams = new jvalue[2];
            videoParams[0].l = AndroidJNI.NewStringUTF(currVideo.videoId.ToString());
            videoParams[1].l = AndroidJNI.NewStringUTF(url);
            AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, videoParams);

            //add to textures list to create on render thread
            texturesToCreate.Add(currVideo.videoId);

            //start rendering updates
            if (UpdateTextureRoutine == null) {
                UpdateTextureRoutine = StartCoroutine(CallPluginAtEndOfFrames());
            }
        }
    }

    public void OnVideoPrepared(string playerID) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoId == int.Parse(playerID));
        if (currVideo != null) {
            currVideo.callback?.Invoke();
        }
    }

    public void PlayVideo(CustomVideoPlayer player) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);
        if (currVideo != null) {
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Play", "(Ljava/lang/String;)V");
            AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, GetVideoIDParams(currVideo.videoId));
        }
    }

    public void PauseVideo(CustomVideoPlayer player) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);
        if (currVideo != null) {
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Pause", "(Ljava/lang/String;)V");
            AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, GetVideoIDParams(currVideo.videoId));
        }
    }

    public void StopVideo(CustomVideoPlayer player) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);
        if (currVideo != null) {

            //call stop from exo player
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Stop", "(Ljava/lang/String;)V");
            AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, GetVideoIDParams(currVideo.videoId));

            //delete surface texture from update list
            DeleteSurfaceID(currVideo.textureID);

            //delete reference from here
            currVideos.Remove(currVideo);
            currVideo = null;
        }
    }

    public float Duration(CustomVideoPlayer player) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);
        if (currVideo != null) {
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetLength", "(Ljava/lang/String;)J");
            return AndroidJNI.CallStaticLongMethod(VideoPlayerClass, methodID, GetVideoIDParams(currVideo.videoId));
        }

        return 0;
    }

    public float GetPlaybackPercent(CustomVideoPlayer player) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);
        if (currVideo != null) {
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetPlaybackPosition", "(Ljava/lang/String;)D");
            return (float)AndroidJNI.CallStaticDoubleMethod(VideoPlayerClass, methodID, GetVideoIDParams(currVideo.videoId));
        }

        return 0;
    }

    public void SetPlaybackPercent(CustomVideoPlayer player, float val) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);
        if (currVideo != null) {
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "SetPlaybackPosition", "(DLjava/lang/String;)V");
            jvalue[] setPlaybackPositionParams = new jvalue[2];
            setPlaybackPositionParams[0].d = val;
            setPlaybackPositionParams[1].l = AndroidJNI.NewStringUTF(currVideo.videoId.ToString());
            AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, setPlaybackPositionParams);
        }
    }

    public int GetWidth(CustomVideoPlayer player) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);
        if (currVideo != null) {
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetWidth", "(Ljava/lang/String;)I");
            return AndroidJNI.CallStaticIntMethod(VideoPlayerClass, methodID, GetVideoIDParams(currVideo.videoId));
        }
        return 0;
    }

    public int GetHeight(CustomVideoPlayer player) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);
        if (currVideo != null) {
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetHeight", "(Ljava/lang/String;)I");
            return AndroidJNI.CallStaticIntMethod(VideoPlayerClass, methodID, GetVideoIDParams(currVideo.videoId));
        }
        return 0;
    }

    public bool IsPlaying(CustomVideoPlayer player) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);
        if (currVideo != null) {
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "GetIsPlaying", "(Ljava/lang/String;)Z");
            return AndroidJNI.CallStaticBooleanMethod(VideoPlayerClass, methodID, GetVideoIDParams(currVideo.videoId));
        }
        return false;
    }

    public void SetLooping(bool looping, CustomVideoPlayer player) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);
        if (currVideo != null) {
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "SetLooping", "(ZLjava/lang/String;)V");
            jvalue[] setLoopingParams = new jvalue[2];
            setLoopingParams[0].z = looping;
            setLoopingParams[1].l = AndroidJNI.NewStringUTF(currVideo.videoId.ToString());
            AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, setLoopingParams);
        }
    }

    jvalue[] GetVideoIDParams(int videoID) {
        jvalue[] videoParams = new jvalue[1];
        videoParams[0].l = AndroidJNI.NewStringUTF(videoID.ToString());
        return videoParams;
    }

    System.IntPtr VideoPlayerClass {
        get {
            if (!_VideoPlayerClass.HasValue) {
                System.IntPtr myVideoPlayerClass = AndroidJNI.FindClass("com/matthew/videoplayer/NativeVideoPlayer");
                _VideoPlayerClass = AndroidJNI.NewGlobalRef(myVideoPlayerClass);
                AndroidJNI.DeleteLocalRef(myVideoPlayerClass);
            }
            return _VideoPlayerClass.GetValueOrDefault();
        }
    }

    #region Rendering
    Coroutine UpdateTextureRoutine;
    IEnumerator CallPluginAtEndOfFrames() {
        while (true) {

            yield return new WaitForEndOfFrame();

            //create textures on render thread only
            if (texturesToCreate.Count > 0) {
                foreach (int id in texturesToCreate) {
                    GL.IssuePluginEvent(GetRenderEventFunc(), id);
                }
                texturesToCreate.Clear();
            }

            //update all textures
            GL.IssuePluginEvent(GetRenderEventFunc(), -1);
        }
    }

    #endregion
}
