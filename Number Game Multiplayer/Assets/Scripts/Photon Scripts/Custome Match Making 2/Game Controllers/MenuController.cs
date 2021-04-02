using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public void OnClickCharPick(int charNum)
    {
        if(PlayerInfo.instance != null)
        {
            PlayerInfo.instance.mySelectedChar = charNum;
            PlayerPrefs.SetInt("MyCharacter", charNum);
        }
    }
}
