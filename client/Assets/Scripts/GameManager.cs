using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    public IDictionary<string, PlayerCube> Players = new Dictionary<string, PlayerCube>();
    public PlayerCube LocalPlayer { get; set; }
    public bool IsSpectator = true;

    public PlayerCube GetPlayer(string userName)
    {
        PlayerCube player;
        Players.TryGetValue(userName, out player);

        return player;
    }
}
