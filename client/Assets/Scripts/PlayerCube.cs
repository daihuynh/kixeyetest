using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCube : MonoBehaviour
{
    private static readonly Color[] COLORS = new[] {
        Color.red,
        Color.green,
        Color.blue,
        Color.magenta
    };

    public float moveSpeed = 0.1f;
    public float updateStateRate = 10; // 10 times per sec
    public Vector3 networkPosition = Vector3.zero;


    public bool IsLocalPlayer { get; set; }
    public int PlayerIndex { get; set; }
    public string UserName { get; set; }
    public GameManager GameManager { get; set; }
    public NetworkManager NetworkManager { get; set; }

    private Vector3 _movement = Vector3.zero;
    private Vector3 _lastPosition = Vector3.zero;

    void Start()
    {
        var renderer = GetComponent<Renderer>();
        renderer.material.color = COLORS[PlayerIndex];
        if (IsLocalPlayer)
        {
            float repeatTime = 1f / updateStateRate;
            InvokeRepeating("SendState", repeatTime, repeatTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsLocalPlayer)
        {
            _movement.x = Input.GetAxis("Horizontal");
            _movement.z = Input.GetAxis("Vertical");
        }
        else
        {
            var position = transform.position;
            transform.position = Vector3.Lerp(position, networkPosition, Time.fixedDeltaTime * 3f);
        }
    }

    private void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
            transform.position += (_movement * (moveSpeed * Time.fixedDeltaTime));
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void SendState()
    {
        var position = transform.position;

        if (position != _lastPosition)
        {

            _lastPosition = position;
            NetworkManager.SendMessageToServer((int)MessageType.PlayerState, new PlayerState
            {
                userName = UserName,
                x = position.x,
                y = position.y,
                z = position.z
            });
        }
    }
}