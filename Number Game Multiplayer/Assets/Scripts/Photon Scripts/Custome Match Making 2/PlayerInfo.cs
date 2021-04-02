using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public static PlayerInfo instance;

    public int mySelectedChar;
    public GameObject[] allChars;

    private void OnEnable()
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

    private void Start()
    {
        if (PlayerPrefs.HasKey("MyCharacter"))
            mySelectedChar = PlayerPrefs.GetInt("MyCharacter");
        else
        {
            mySelectedChar = 0;
            PlayerPrefs.SetInt("MyCharacter", mySelectedChar);
        }
    }
}