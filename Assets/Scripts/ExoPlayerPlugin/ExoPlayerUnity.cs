using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using System.Collections.Generic;

public class ExoPlayerUnity : MonoBehaviour {

    public static ExoPlayerUnity instance;

    [DllImport("RenderingPlugin")]
    static extern System.IntPtr GetRenderEventFunc();

    //TODO: Make all this a class or struct and use Linq
    List<CustomVideoPlayer> players = new List<CustomVideoPlayer>();
    List<int> texturesToCreate = new List<int>();
    Dictionary<int, UnityAction> callbacks = new Dictionary<int, UnityAction>();

    System.IntPtr? _VideoPlayerClass;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public void PrepareVideo(string url, UnityAction callback, CustomVideoPlayer player) {

        if (!players.Contains(player)) {

            //add video to list and get ID
            players.Add(player);
            int videoID = players.IndexOf(player);

            //keep track of callback
            callbacks.Add(videoID, callback);

            //call plugin function
            System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Prepare", "(Ljava/lang/String;Ljava/lang/String;)V");
            jvalue[] videoParams = new jvalue[2];
            videoParams[0].l = AndroidJNI.NewStringUTF(videoID.ToString());
            videoParams[1].l = AndroidJNI.NewStringUTF(url);
            AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, videoParams);

            //add to textures list to create on render thread
            texturesToCreate.Add(videoID);

            //start rendering updates
            if (UpdateTextureRoutine == null) {
                UpdateTextureRoutine = StartCoroutine(CallPluginAtEndOfFrames());
            }
        }
    }

    public void PlayVideo() {

    }

    public void PauseVideo() {
        System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Pause", "(Ljava/lang/String;)V");
        jvalue[] videoId = new jvalue[1];
        videoId[0].l = AndroidJNI.NewStringUTF("0");
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, videoId);
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
        players[videoID].rend.material.mainTexture = oesTex;
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
