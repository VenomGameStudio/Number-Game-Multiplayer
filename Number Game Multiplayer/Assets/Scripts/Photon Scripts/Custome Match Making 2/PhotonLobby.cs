using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public static PhotonLobby instance;

    public GameObject StartBtn;
    public GameObject CancelBtn;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        StartBtn.SetActive(true);
    }

    public void StartBtnPressed()
    {
        StartBtn.SetActive(false);
        CancelBtn.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join random room");
        CreateRoom();
    }

    private void CreateRoom()
    {
        Debug.Log("Tring to create room");
        int randomRoomNumber = Random.Range(0, 10000);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)MultiplayerSettings.instance.maxPlayer };
        PhotonNetwork.CreateRoom("Room" + randomRoomNumber, roomOps);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Faild to creat room");
        CreateRoom();
    }

    public void OnClickCancelBtn()
    {
        CancelBtn.SetActive(false);
        StartBtn.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }
}