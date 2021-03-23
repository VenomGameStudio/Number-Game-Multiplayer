using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.SceneManagement;

public class GameSetupController : MonoBehaviour
{
    void Start()
    {
        CreatePlayer();
    }

    private void CreatePlayer()
    {
        Debug.Log("Creating Player");
        PhotonNetwork.Instantiate(Path.Combine("PhotonPlayerPrefabs", "PhotonPlayer"), Vector3.zero, Quaternion.identity);
    }

    public void Exit()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }
}