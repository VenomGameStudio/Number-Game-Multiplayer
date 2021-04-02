using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AvatarSetup : MonoBehaviour
{
    private PhotonView photonView;

    public int charValue;
    public GameObject myChar;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView.IsMine)
            photonView.RPC("RPC_AddCharacter", RpcTarget.AllBuffered, PlayerInfo.instance.mySelectedChar);
    }

    [PunRPC]
    private void RPC_AddCharacter(int charNum) 
    {
        charValue = charNum;
        myChar = Instantiate(PlayerInfo.instance.allChars[charNum], transform.position, transform.rotation, transform);
    }
}