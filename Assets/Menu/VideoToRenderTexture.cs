using UnityEngine;
using UnityEngine.Video;

public class VideoToRenderTexture : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Assign the VideoPlayer in the inspector
    public RenderTexture renderTexture; // Assign the RenderTexture in the inspector

    void Start()
    {
        // Set the render mode and assign the render texture
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.Play(); // Play the video
    }
}
