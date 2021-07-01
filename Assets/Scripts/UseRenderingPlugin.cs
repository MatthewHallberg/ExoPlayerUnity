using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


public class UseRenderingPlugin : MonoBehaviour {

    static System.IntPtr? _VideoPlayerClass;

    [DllImport("RenderingPlugin")]
    private static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h);

    [DllImport("RenderingPlugin")]
    private static extern System.IntPtr GetRenderEventFunc();

    [DllImport("RenderingPlugin")]
    private static extern int GetTextureID();

    bool textureCreated;

    void Start() {
        StartCoroutine(CallPluginAtEndOfFrames());
    }

    IEnumerator CallPluginAtEndOfFrames() {
        while (true) {
            // Wait until all frame rendering is done
            yield return new WaitForEndOfFrame();
            GL.IssuePluginEvent(GetRenderEventFunc(), 1);
            CreateOESTexture();
        }
    }

    void CreateOESTexture() {
        if (!textureCreated) {
            textureCreated = true;
            int textureID = GetTextureID();
            Debug.Log("Texture ID from Unity: " + textureID);
            Texture2D oesTex = Texture2D.CreateExternalTexture(
                0,
                0,
                TextureFormat.RGB24,
                false,
                true,
               (System.IntPtr)textureID);

            oesTex.Apply();

            // Set texture onto our material
            GetComponent<Renderer>().material.mainTexture = oesTex;
        }
    }

    static System.IntPtr VideoPlayerClass {
        get {
            if (!_VideoPlayerClass.HasValue) {
                System.IntPtr myVideoPlayerClass = AndroidJNI.FindClass("com/matthew/videoplayer/NativeVideoPlayer");
                _VideoPlayerClass = AndroidJNI.NewGlobalRef(myVideoPlayerClass);
                AndroidJNI.DeleteLocalRef(myVideoPlayerClass);
            }
            return _VideoPlayerClass.GetValueOrDefault();
        }
    }

    public void ButtonPressed() {
        PauseVideo();
    }

    void PauseVideo() {
        System.IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "pause", "(Ljava/lang/String;)V");
        jvalue[] videoId = new jvalue[1];
        videoId[0].l = AndroidJNI.NewStringUTF("0");
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, videoId);
    }
}
