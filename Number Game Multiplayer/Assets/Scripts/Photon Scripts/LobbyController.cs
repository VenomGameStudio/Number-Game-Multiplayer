using TMPro;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviourPunCallbacks
{
    // Delay and Quick Match
    [SerializeField] private int waitingRoomIndex;
    [SerializeField] private int gameSceneIndex;
    [SerializeField] private GameObject delayStartBtn;
    [SerializeField] private GameObject delayCancelBtn;
    [SerializeField] private GameObject quickStartBtn;
    [SerializeField] private GameObject quickCancelBtn;
    [SerializeField] private int roomSize;

    [Header("Lobby")]
    [SerializeField] private GameObject lobbyConnectBtn;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_InputField playerName;
    [SerializeField] private Transform roomContainer;
    [SerializeField] private GameObject roomListingPrefab;

    [Header("Room")]
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private GameObject customRoomStartBtn;
    [SerializeField] private GameObject customRoomReadyBtn;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private GameObject playerListingPrefab;
    [SerializeField] private TMP_Text roomNameDisplay;

    private string customRoomName;
    private int customRoomSize;

    private List<RoomInfo> roomListing;

    public bool quickStart = false;
    public bool delayStart = false;
    public bool customRoom = false;

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        delayStartBtn.SetActive(true);
        quickStartBtn.SetActive(true);
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

    public void QuickStart()
    {
        quickStart = true;
        delayStart = false;
        customRoom = false;
        quickStartBtn.SetActive(false);
        quickCancelBtn.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Quick Start");
    }

    public void DelayStart()
    {
        delayStart = true;
        quickStart = false;
        customRoom = false;
        delayStartBtn.SetActive(false);
        delayCancelBtn.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Delay Start");
    }

    #region Creat/Join Room

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join random room!");
        CreateRoom();
    }

    private void CreateRoom()
    {
        Debug.Log("Create Room");
        int randomRoomNum = Random.Range(0, 1000);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)roomSize };
        PhotonNetwork.CreateRoom("Room" + randomRoomNum, roomOps);
        Debug.Log(randomRoomNum);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (customRoom)
        {
            Debug.Log("Failed to create a custom room.");
        }
        else
        {
            Debug.Log("Failed to create a room.");
            CreateRoom();
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        if (delayStart)
        {
            Debug.Log("Joing Waiting room");
            SceneManager.LoadScene(waitingRoomIndex);
        }
        
        if (quickStart)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Starting Game");
                PhotonNetwork.LoadLevel(gameSceneIndex);
            }
        }

        if (customRoom)
        {
            roomPanel.SetActive(true);
            lobbyPanel.SetActive(false);
            roomNameDisplay.text = PhotonNetwork.CurrentRoom.Name;
            if (PhotonNetwork.IsMasterClient)
            {
                customRoomReadyBtn.SetActive(false);
                customRoomStartBtn.SetActive(true);
            }
            else
            {
                customRoomReadyBtn.GetComponent<Image>().color = Color.red;
                customRoomReadyBtn.SetActive(true);
                customRoomStartBtn.SetActive(false);
            }

            ClearPlayerList();
            ListPlayer();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Lobby Joined successfully");
    }

    #endregion

    #region Creat & Join Custom Room 

    public void PlayerNameUpdate(string nameInput)
    {
        PhotonNetwork.NickName = nameInput;
        PlayerPrefs.SetString("NickName", nameInput);
    }

    public void JoinLobbyOnClick()
    {
        customRoom = true;
        quickStart = false;
        delayStart = false;
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

    #endregion

    #region Room/Player Listing
    private void ClearPlayerList()
    {
        for (int i = playerContainer.childCount - 1; i >= 0; i--)
        {
            Debug.Log(playerContainer.GetChild(i).gameObject + "Destroyed");
            Destroy(playerContainer.GetChild(i).gameObject);
        }
    }

    private void ListPlayer()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject tempListing = Instantiate(playerListingPrefab, playerContainer);
            TMP_Text tempText = tempListing.transform.GetChild(0).GetComponent<TMP_Text>();
            tempText.text = player.NickName;
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
        {
            customRoomStartBtn.GetComponent<Image>().color = Color.green;
            customRoomStartBtn.SetActive(true);
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Player Left");
        /*foreach (GameObject obj in roomContainer)
        {
            Destroy(obj);
        }*/
    }

    public void ReadyBtnCall()
    {
        customRoomReadyBtn.GetComponent<Image>().color = Color.green;
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(gameSceneIndex);
        }
    }

    public void LeaveBtnCall()
    {
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
    #endregion

    #region Photon Leave Buttons
    public void DelayCancel()
    {
        delayCancelBtn.SetActive(false);
        delayStartBtn.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }

    public void QuickCancel()
    {
        quickCancelBtn.SetActive(false);
        quickStartBtn.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }

    public void CustomMatchMakingCancel()
    {
        customRoom = false;
        menuPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        PhotonNetwork.LeaveLobby();
    }
    #endregion
}
