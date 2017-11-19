using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkManager
{
    private class SocketState
    {
        public UdpClient client;
        public IPEndPoint endpoint;
    }
    public event Action<string> OnMessage;
    public event Action<Exception> OnDisconnect;

    private const string HOSTNAME = "127.0.0.1";
    private const int SERVER_PORT = 43210;

    private UdpClient _client = null;
    private SocketState _state = null;

    public void Connect()
    {
        var ipEndpoint = new IPEndPoint(IPAddress.Parse(HOSTNAME), SERVER_PORT);

        _client = new UdpClient();
        _client.Connect(ipEndpoint);

        _state = new SocketState
        {
            client = _client,
            endpoint = ipEndpoint
        };

        StartNetworkLoop();
    }

    public void SendMessageToServer(int msgType, object data)
    {
        var dataString = JsonUtility.ToJson(data);
        var message = new Message
        {
            msgType = msgType,
            jsonData = dataString
        };
        SendMessageToServer(JsonUtility.ToJson(message));
    }

    private void SendMessageToServer(string message)
    {
        if (_client == null) return;

        var bytes = Encoding.UTF8.GetBytes(message);
        try
        {
            _client.Send(bytes, bytes.Length);
        }
        catch (Exception ex)
        {
            var onDisconnect = OnDisconnect;
            if (onDisconnect != null)
            {
                onDisconnect(ex);
            }
        }
    }

    private void StartNetworkLoop()
    {
        _client.BeginReceive(ReceiveAsync, _state);
    }

    private void ReceiveAsync(IAsyncResult ar)
    {
        var state = ar.AsyncState as SocketState;

        byte[] rawData = state.client.EndReceive(ar, ref state.endpoint);

        try
        {
            string jsonString = Encoding.UTF8.GetString(rawData, 0, rawData.Length);
            Debug.Log(jsonString);

            var onMessage = OnMessage;
            if (onMessage != null)
            {
                onMessage(jsonString);
            }

            state.client.BeginReceive(ReceiveAsync, state);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
