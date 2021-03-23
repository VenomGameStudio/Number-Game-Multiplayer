using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;

public class DelayStartWaitingRoomController : MonoBehaviourPunCallbacks
{
    [SerializeField] private int multyplayerSceneIndex;
    [SerializeField] private int menusceneIndex;
    [SerializeField] private int minPlayersToStart;
    [SerializeField] private TMP_Text playerCountDisplay;
    [SerializeField] private TMP_Text timeToStartDesplay;
    [SerializeField] private float maxWaitTime;
    [SerializeField] private float maxFullGameWaitTime;

    private PhotonView photonView;
    private int playerCount;
    private int roomSize;
    // bool variables for getting game state
    private bool readyToCoundDown;
    private bool readyToStart;
    private bool startingGame;
    // countdown timer value
    private float timerToStartGame;
    private float notFullGameTimer;
    private float fullGameTimer;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        fullGameTimer = maxFullGameWaitTime;
        notFullGameTimer = maxWaitTime;
        timerToStartGame = maxWaitTime;

        PlayerCountUpdate();
    }

    private void PlayerCountUpdate()
    {
        playerCount = PhotonNetwork.PlayerList.Length;
        roomSize = PhotonNetwork.CurrentRoom.MaxPlayers;
        playerCountDisplay.text = playerCount + ":" + roomSize;

        if (playerCount == roomSize)
            readyToStart = true;
        else if (playerCount >= minPlayersToStart)
            readyToCoundDown = true;
        else
        {
            readyToCoundDown = false;
            readyToStart = false;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerCountUpdate();

        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("RPC_SendTimer", RpcTarget.Others, timerToStartGame);
    }

    [PunRPC]
    private void RPC_SendTimer(float timIn)
    {
        timerToStartGame = timIn;
        notFullGameTimer = timIn;
        if (timIn < fullGameTimer)
            fullGameTimer = timIn;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PlayerCountUpdate();
    }

    private void Update()
    {
        WaitingForMorePlayers();
    }

    private void WaitingForMorePlayers()
    {
        if (playerCount <= 1)
            RestTimer();

        if(readyToStart)
        {
            fullGameTimer -= Time.deltaTime;
            timerToStartGame = fullGameTimer;
        }
        else if(readyToCoundDown)
        {
            notFullGameTimer -= Time.deltaTime;
            timerToStartGame = notFullGameTimer;
        }

        string tempTimer = string.Format("{0:00}", timerToStartGame);
        timeToStartDesplay.text = tempTimer;
        if (timerToStartGame <= 0f)
        {
            if (startingGame)
                return;
            StartGame();
        }
    }

    private void RestTimer()
    {
        timerToStartGame = maxWaitTime;
        notFullGameTimer = maxWaitTime;
        fullGameTimer = maxFullGameWaitTime;
    }

    private void StartGame()
    {
        startingGame = true;
        if (!PhotonNetwork.IsMasterClient)
            return;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(multyplayerSceneIndex);
    }

    public void DelayCancel()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(menusceneIndex);
    }
}