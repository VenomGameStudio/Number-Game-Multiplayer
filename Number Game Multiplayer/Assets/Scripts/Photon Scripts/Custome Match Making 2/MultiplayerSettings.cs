using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerSettings : MonoBehaviour
{
    public static MultiplayerSettings instance;

    public bool delayStart;
    public int maxPlayer;

    public int menuSceneIndex;
    public int multiplayerGameSceneIndex;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
