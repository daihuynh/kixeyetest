# Kix Eye Test

## Screenshot:
![Alt text](https://github.com/daihuynh/kixeyetest/blob/master/demo.png?raw=true "Screenshot")

## OS
- MacOS: 10.12

## Server
- Nodejs: v8.9.1
- Clusters: 1 master vs 2 workers.
- Transportation: [UDP](https://nodejs.org/api/dgram.html#dgram_socket_send_msg_offset_length_port_address_callback)

## Client:
- Unity: 5.6.4p1.

## Ideas:
- Room only supports 4 players, the others who join room laster will be spectators.
- Player state: username and x,y,z for position.
- Player when joined room will be synced with room state: spawning other player Prefabs and getting their lastest states. Lastest state is query in MongoDB with "playerStates" collection.
- Player only updates state to server only if position is changed.
- Client has 2 modes: player mode or spectator mode.
  - Player mode: 1 local player and 1-3 remote players.
  - Spectator mode: all are remote players.

## Lack of features
- Disconnect event (by sending heartbeat).
- Leave room event.
