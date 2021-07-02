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
                videoId = videoIds++,
            };

            currVideos.Add(currVideo);

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
            jvalue[] videoId = new jvalue[1];
            videoId[0].l = AndroidJNI.NewStringUTF(currVideo.videoId.ToString());
            AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, videoId);
        }
    }

    public void PauseVideo(CustomVideoPlayer player) {
        CurrentVideo currVideo = currVideos.FirstOrDefault(x => x.videoPlayer == player);
        if (currVideo != null) {
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Pause", "(Ljava/lang/String;)V");
            jvalue[] videoId = new jvalue[1];
            videoId[0].l = AndroidJNI.NewStringUTF(currVideo.videoId.ToString());
            AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, videoId);
        }
    }

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

    public void CreateOESTexture(string videoInfo) {
        //get video info from message
        string[] info = videoInfo.Split(',');
        int videoID = int.Parse(info[0]);
        int externalID = int.Parse(info[1]);

        Debug.Log("Texture ID from Unity: " + externalID);
        Texture2D oesTex = Texture2D.CreateExternalTexture(
            0,
            0,
            TextureFormat.RGB24,
            false,
            true,
            (System.IntPtr)externalID);

        oesTex.Apply();

        // Set texture onto our material
        CurrentVideo currVideo = currVideos.First(x => x.videoId == videoID);
        currVideo.textureID = externalID;
        currVideo.videoPlayer.rend.material.mainTexture = oesTex;
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
}
