using UnityEngine;

public class GameSetup : MonoBehaviour
{
    public static GameSetup instance;

    public Transform[] spawnPoints;

    private void OnEnable()
    {
        if (instance == null)
            instance = this;
    }
}