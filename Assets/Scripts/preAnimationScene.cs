using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class PreAnimationScene : MonoBehaviour
{
    [SerializeField] private VideoPlayer vidPlayer;

    private void Start()
    {
        vidPlayer = GetComponent<VideoPlayer>();
        vidPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp){
        SceneManager.LoadScene(1);
    }
}
