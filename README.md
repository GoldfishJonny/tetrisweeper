## Fork of Tetrisweeper by [KertisJones](https://github.com/KertisJones)

This repository is a fork of [KertisJones/Tetrisweeper](https://github.com/OriginalAuthor/Tetrisweeper) by **KertisJones**.

### What’s changed in this fork

- **Steam integration removed**  
  All Steamworks and overlay callbacks have been stripped out to simplify dependencies and build process.
- **WebSocket-based GameState API**  
  Introduced a lightweight WebSocket server in `server` and a matching client in Unity that:
  - Publishes the current game state (`/state` endpoint)  
  - Accepts movement commands (`/command` endpoint)  

This makes it possible for external agents (bots, AIs) to connect and solve Tetrisweeper programmatically.

## Setup Tetrisweeper API
To setup the Tetrisweeper API server

Clone or pull the repository and install dependencies:
```{bash}
git clone https://github.com/GoldfishJonny/tetrisweeper
cd tetrisweeper/src
npm install
```
Start the server:
```{bash}
node server.js
```
> **Important**: Make sure the server is running *before* pressing **PLAY MARATHON** on the modified Tetrisweeper. When the game initializes it will connect to ws://localhost:3250/ws and begin sending state updates; after that you can push commands at any time using the /command endpoint. The PLAY MARATHON command will be added later, so the websocket connects the moment it is up.

## HTTP Endpoints
### GET ```/state```
Returns the latest game state.
```{bash}
http://localhost:3250/state
```
- 200 OK: Returns JSON with State of the Board
- 404 Not Found: Response body: No game state available

### POST ```/command```
Sends commands to Open Game.

```{bash}
http://localhost:3250/command
```

- 204 No Content: Command successfully sent.

Use these two endpoints to pull game state and push control commands.

## Available Commands
These are the following commands via ```POST /command```

| Command         | Description                                          | Parameters |
|-----------------|------------------------------------------------------|------------|
| `moveleft`        | Move the active tetromino one cell to the left       | `None`|
| `moveright`       | Move the active tetromino one cell to the right      | `None` |
| `softdrop`        | Soft-drop the active tetromino                        | `None` |
| `harddrop`        | Hard-drop the active tetromino                        | `None` |
| `rotate`          | Rotate the active tetromino clockwise                 | `None` |
| `rotateccw`       | Rotate the active tetromino counterclockwise          | `None` |
| `hold`            | Hold the current tetromino                            | `None` |
| `reveal`          | Reveal specific tetromino                             | `x, y` |
| `flag`            | Flag specific tetromino                               | `x, y` |
| `chord`           | Reveal Chord for specific tetromino                   | `x, y` |
|`chordflag`        | Flag Chord for specific tetromino                     | `x, y` |
