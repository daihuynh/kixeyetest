using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MessageType
{
    Hearbeat = 0,
    JoinRoomRequest,
    JoinRoomResponse,
    CreatePlayer,
    PlayerState
}

[Serializable]
public class Message
{
    public int msgType;
    public string jsonData;
}


[Serializable]
public class HeartBeatMessage
{
    public int ackId;
    public int timestamp;
}

[Serializable]
public class JoinRoomRequest
{
    public string userName;
}

[Serializable]
public class JoinRoomResponse
{
    public bool isSuccess;
}

[Serializable]
public class CreatePlayer
{
    public string userName;
    public int playerIndex;
}

[Serializable]
public class PlayerState
{
    public string userName;
    public float x, y,z;
}