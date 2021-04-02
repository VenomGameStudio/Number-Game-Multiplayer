using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.IO;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public static PhotonRoom instance;

    public bool isGameLoaded;
    public int currentScene;
    public int playersInRoom;
    public int myNumberInRoom;
    public int playerInGame;
    public float startingTime;

    private PhotonView photonView;
    private Player[] photonPlayers;

    public bool readyToCount;
    public bool readyToStart;
    private float lessThenMaxPlayer;
    private float atMaxPlayer;
    private float timeToStart;


    private void Awake()
    {
        //if (instance == null)
            instance = this;
        /*else
        {
            if (instance != this)
            {
                instance = this;
                //Destroy(gameObject);
            }
        }*/

        DontDestroyOnLoad(gameObject);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnFinishLoding;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnFinishLoding;
    }

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        readyToCount = false;
        readyToStart = false;
        lessThenMaxPlayer = startingTime;
        atMaxPlayer = 6;
        timeToStart = startingTime;
    }

    private void Update()
    {
        if (MultiplayerSettings.instance.delayStart)
        {
            if (playersInRoom == 1)
            {
                ResetTimer();
            }
            if (!isGameLoaded)
            {
                if (readyToStart)
                {
                    atMaxPlayer -= Time.deltaTime;
                    lessThenMaxPlayer = atMaxPlayer;
                    timeToStart = atMaxPlayer;
                }
                else if (readyToCount)
                {
                    lessThenMaxPlayer -= Time.deltaTime;
                    timeToStart = lessThenMaxPlayer;
                }
                Debug.Log("Display time to start to the player : " + timeToStart);
                if (timeToStart <= 0)
                {
                    StartGame();
                }
            }
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined the room");
        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom = photonPlayers.Length;
        myNumberInRoom = playersInRoom;
        PhotonNetwork.NickName = myNumberInRoom.ToString();

        if (MultiplayerSettings.instance.delayStart)
        {
            Debug.Log("Players in room (" + playersInRoom + ":" + MultiplayerSettings.instance.maxPlayer + ")");
            if (playersInRoom > 1)
                readyToCount = true;
            if (playersInRoom == MultiplayerSettings.instance.maxPlayer)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient)
                    return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
        else
            StartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("New Player Joined");
        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom++;
        if (MultiplayerSettings.instance.delayStart)
        {
            if (playersInRoom > 1)
                readyToCount = true;
            if (playersInRoom == MultiplayerSettings.instance.maxPlayer)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient)
                    return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
    }

    private void StartGame()
    {
        isGameLoaded = true;
        if (!PhotonNetwork.IsMasterClient)
            return;
        if(MultiplayerSettings.instance.delayStart)
            PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(MultiplayerSettings.instance.multiplayerGameSceneIndex);
    }

    private void ResetTimer()
    {
        lessThenMaxPlayer = startingTime;
        timeToStart = startingTime;
        atMaxPlayer = 6;
        readyToCount = false;
        readyToStart = false;
    }

    private void OnFinishLoding(Scene scene,LoadSceneMode mode)
    {
        currentScene = scene.buildIndex;
        if(currentScene == MultiplayerSettings.instance.multiplayerGameSceneIndex)
        {
            isGameLoaded = true;
            if (MultiplayerSettings.instance.delayStart)
                photonView.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
            else
                RPC_CreatePlayer();
        }
    }

    [PunRPC]
    private void RPC_LoadedGameScene()
    {
        playerInGame++;
        if (playerInGame == PhotonNetwork.PlayerList.Length)
            photonView.RPC("RPC_CreatePlayer", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_CreatePlayer()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPlayerPrefabs", "PhotonPlayer"), transform.position, Quaternion.identity, 0);
    }
}