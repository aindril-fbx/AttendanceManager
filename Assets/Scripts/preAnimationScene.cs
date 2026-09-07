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
        if (PlayerPrefs.GetFloat("startup", 1f) == 0f){
            SceneManager.LoadScene(1);
        }
    }

    private void OnVideoFinished(VideoPlayer vp){
        SceneManager.LoadScene(1);
    }
}
