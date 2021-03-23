using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class CustomMatchController : MonoBehaviourPunCallbacks
{
    [Header("Lobby")]
    [SerializeField] private GameObject lobbyConnectBtn;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_InputField playerName;
    [SerializeField] private Transform roomContainer;
    [SerializeField] private GameObject roomListingPrefab;

    [Header("Room")]
    [SerializeField] private int gameSceneIndex;
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private GameObject startBtn;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private GameObject playerListingPrefab;
    [SerializeField] private TMP_Text roomNameDisplay;

    private string customRoomName;
    private int customRoomSize;
    private bool customRoom;

    private List<RoomInfo> roomListing;

    #region Creat & Join Custome Room 
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        lobbyConnectBtn.SetActive(true);

        roomListing = new List<RoomInfo>();

        if (PlayerPrefs.HasKey("NickName"))
        {
            if (PlayerPrefs.GetString("NickName") == "")
                PhotonNetwork.NickName = "Player" + Random.Range(0, 1000);
            else
                PhotonNetwork.NickName = PlayerPrefs.GetString("NickName");
        }
        else
            PhotonNetwork.NickName = "Player" + Random.Range(0, 1000);
        playerName.text = PhotonNetwork.NickName;
    }

    public void PlayerNameUpdate(string nameInput)
    {
        PhotonNetwork.NickName = nameInput;
        PlayerPrefs.SetString("NickName", nameInput);
    }

    public void JoinLobbyOnClick()
    {
        menuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int tempIndex;
        foreach (RoomInfo room in roomList)
        {
            if (roomListing != null)
                tempIndex = roomListing.FindIndex(ByName(room.Name));
            else
                tempIndex = -1;

            if (tempIndex != -1)
            {
                roomListing.RemoveAt(tempIndex);
                Destroy(roomContainer.GetChild(tempIndex).gameObject);
            }
            if (room.PlayerCount > 0)
            {
                roomListing.Add(room);
                ListRoom(room);
            }
        }
    }

    static System.Predicate<RoomInfo> ByName(string name)
    {
        return delegate (RoomInfo room)
        {
            return room.Name == name;
        };
    }

    private void ListRoom(RoomInfo room)
    {
        if (room.IsOpen && room.IsVisible)
        {
            GameObject tempListing = Instantiate(roomListingPrefab, roomContainer);
            RoomButton tempButton = tempListing.GetComponent<RoomButton>();
            tempButton.SetRoom(room.Name, room.MaxPlayers, room.PlayerCount);
        }
    }

    public void OnRoomNameChange(string nameIn)
    {
        customRoomName = nameIn;
    }

    public void OnRoomSizeChange(string sizeIn)
    {
        customRoomSize = int.Parse(sizeIn);
    }

    public void CreatCustomRoom()
    {
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)customRoomSize };
        PhotonNetwork.CreateRoom(customRoomName, roomOptions);
    }

    public void CustomMatchMakingCancel()
    {
        menuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        PhotonNetwork.LeaveLobby();
    }

    #endregion

    private void ClearPlayerList()
    {
        for (int i = playerContainer.childCount - 1; i >= 0; i--)
            Destroy(playerContainer.GetChild(i).gameObject);
    }

    private void ListPlayer()
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            GameObject tempListing = Instantiate(playerListingPrefab, playerContainer);
            TMP_Text tempText = tempListing.transform.GetChild(0).GetComponent<TMP_Text>();
            tempText.text = player.NickName; 
        }
    }

    public override void OnJoinedRoom()
    {
        if(customRoom)
        {
            roomPanel.SetActive(true);
            lobbyPanel.SetActive(false);
            roomNameDisplay.text = PhotonNetwork.CurrentRoom.Name;
            if (PhotonNetwork.IsMasterClient)
                startBtn.SetActive(true);
            else
                startBtn.SetActive(false);

            ClearPlayerList();
            ListPlayer();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ClearPlayerList();
        ListPlayer();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ClearPlayerList();
        ListPlayer();
        if (PhotonNetwork.IsMasterClient)
            startBtn.SetActive(true);
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            customRoom = true;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(gameSceneIndex);
        }
    }

    public void LeaveBtnCall()
    {
        customRoom = false;
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        StartCoroutine(RejoinLobby());
    }

    private IEnumerator RejoinLobby()
    {
        yield return new WaitForSeconds(1);
        PhotonNetwork.JoinLobby();
    }
}