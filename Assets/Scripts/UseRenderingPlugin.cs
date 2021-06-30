using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;


public class UseRenderingPlugin : MonoBehaviour {

    static IntPtr? _VideoPlayerClass;

    [DllImport("RenderingPlugin")]
    private static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h);

    [DllImport("RenderingPlugin")]
    private static extern IntPtr GetRenderEventFunc();

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
               (IntPtr)textureID);

            oesTex.Apply();


            // Set texture onto our material
            GetComponent<Renderer>().material.mainTexture = oesTex;
        }
    }

    static IntPtr VideoPlayerClass {
        get {
            if (!_VideoPlayerClass.HasValue) {
                try {
                    IntPtr myVideoPlayerClass = AndroidJNI.FindClass("com/matthew/videoplayer/NativeVideoPlayer");

                    if (myVideoPlayerClass != IntPtr.Zero) {
                        _VideoPlayerClass = AndroidJNI.NewGlobalRef(myVideoPlayerClass);

                        AndroidJNI.DeleteLocalRef(myVideoPlayerClass);
                    } else {
                        Debug.LogError("Failed to find NativeVideoPlayer class");
                        _VideoPlayerClass = IntPtr.Zero;
                    }
                } catch (Exception ex) {
                    Debug.LogError("Failed to find NativeVideoPlayer class");
                    Debug.LogException(ex);
                    _VideoPlayerClass = IntPtr.Zero;
                }
            }
            return _VideoPlayerClass.GetValueOrDefault();
        }
    }

    public void ButtonPressed() {
        //PlayVideo();
    }

    void PlayVideo() {
        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "playVideo", "()V");
        jvalue[] blankParams = new jvalue[0];
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, blankParams);
    }

    void SendMessageToPlugin() {
        IntPtr methodID = AndroidJNI.GetStaticMethodID(VideoPlayerClass, "Test", "(Ljava/lang/String;)V");
        jvalue[] blankParams = new jvalue[1];
        blankParams[0].l = AndroidJNI.NewStringUTF("HelloFromUnity");
        AndroidJNI.CallStaticVoidMethod(VideoPlayerClass, methodID, blankParams);
    }
}
