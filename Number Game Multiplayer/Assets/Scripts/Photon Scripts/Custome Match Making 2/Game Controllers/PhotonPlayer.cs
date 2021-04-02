using UnityEngine;
using Photon.Pun;
using System.IO;

public class PhotonPlayer : MonoBehaviour
{
    private PhotonView photonView;

    public GameObject myAvatar;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        int spawanPick = Random.Range(0, GameSetup.instance.spawnPoints.Length);
        if (photonView.IsMine)
            myAvatar = PhotonNetwork.Instantiate(Path.Combine("PhotonPlayerPrefabs", "PlayerAvatar"),
                GameSetup.instance.spawnPoints[spawanPick].position, GameSetup.instance.spawnPoints[spawanPick].rotation, 0);
    }

}