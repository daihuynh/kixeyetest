using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    [Header("Prefab")]
    public PlayerCube playerPrefab;

    [Space]
    [Header("UI")]
    public Text txtStatus;
    public Button btnConnect;

    [Space]
    [Header("Network")]
    public MainThreadInvoker mainThreadInvoker;

    private GameManager _gameManager = new GameManager();
    private NetworkManager _networkManager = new NetworkManager();
    private IDictionary<int, Action<string>> _messageHandlers = new Dictionary<int, Action<string>>();
    private string _userName;

    private void Awake()
    {
        _userName = "user" + TimeUtility.GetCurrentUnixTimestamp();
    }

    private void Start()
    {
        _networkManager.OnMessage += HandleMessage;
        _networkManager.OnDisconnect += HandleDisconnect;
        _messageHandlers.Add((int)MessageType.JoinRoomResponse, HandleJoinRoomResponseMessage);
        _messageHandlers.Add((int)MessageType.CreatePlayer, HandleCreatePlayerMessage);
        _messageHandlers.Add((int)MessageType.PlayerState, HandlePlayerStateMessage);
    }

    public void HandleBtnConnect_Click()
    {
        btnConnect.gameObject.SetActive(false);

        _networkManager.Connect();

        // Request join room
        _networkManager.SendMessageToServer((int)MessageType.JoinRoomRequest, new JoinRoomRequest
        {
            userName = _userName
        });
    }

    private void Heartbeat() {
        
    }

    private void HandleDisconnect(Exception ex)
    {
        mainThreadInvoker.Add(() => {
            txtStatus.text = "Disconnected from server";

            var userNames = _gameManager.Players.Keys;
            int count = userNames.Count;
            foreach (var key in userNames)
            {
                _gameManager.Players[key].enabled = false;
            }
        });
    }

    private void HandleMessage(string jsonMessage)
    {
        try
        {
            var message = JsonUtility.FromJson<Message>(jsonMessage);
            Action<string> handler;
            if (_messageHandlers.TryGetValue(message.msgType, out handler))
            {
                handler(message.jsonData);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void HandleJoinRoomResponseMessage(string jsonData)
    {
        var data = JsonUtility.FromJson<JoinRoomResponse>(jsonData);

        _gameManager.IsSpectator = !data.isSuccess;

        mainThreadInvoker.Add(() =>
        {
            txtStatus.text = data.isSuccess ? "PLAYER MODE" : "SPECTATOR MODE";
        });
    }

    private void HandleCreatePlayerMessage(string jsonData)
    {
        var data = JsonUtility.FromJson<CreatePlayer>(jsonData);

        mainThreadInvoker.Add(() =>
        {
            var player = Instantiate(playerPrefab, new Vector3(0, 0.5f, 0), Quaternion.identity);
            player.UserName = data.userName;
            player.PlayerIndex = data.playerIndex;

            player.GameManager = _gameManager;
            player.NetworkManager = _networkManager;

            bool isLocalPlayer = data.userName.Equals(_userName);

            // Spawn local
            if (isLocalPlayer)
            {
                player.IsLocalPlayer = true;
                _gameManager.LocalPlayer = player;
            }

            _gameManager.Players.Add(data.userName, player);
        });
    }

    private void HandlePlayerStateMessage(string jsonData)
    {
        var data = JsonUtility.FromJson<PlayerState>(jsonData);

        mainThreadInvoker.Add(() =>
        {
            var playerCube = _gameManager.GetPlayer(data.userName);
            if (playerCube && !playerCube.IsLocalPlayer)
            {
                playerCube.networkPosition = new Vector3(data.x, data.y, data.z);
            }
        });
    }
}
