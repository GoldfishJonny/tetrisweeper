using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections;

// DTOs (make sure these match your existing definitions)
[Serializable]
public class TileDto {
    public int x, y;
    public bool isRevealed, isFlagged;
    public int nearbyMines;
    public string aura;
}
[Serializable]
public class PieceDto {
    public string type;
    public List<Vector2Int> cells;
}
[Serializable]
public class BoardStateDto {
    public int linesCleared;
    public int level;
    public List<TileDto> board;
    public PieceDto nextPiece;
    public PieceDto heldPiece;
    public List<Vector2Int> ghostCells;
}

[Serializable]
public class CommandDto {
    public string command;
    public int x;
    public int y;
    
}

public static class JsonHelper {
    [Serializable]
    private class Wrapper<T> { public T[] commands; }

    public static T[] FromJsonArray<T>(string jsonArray) {
        // wrap the incoming array in an object with a "commands" field
        string wrapped = $"{{\"commands\":{jsonArray}}}";
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        return wrapper.commands;
    }
}

public class RemoteStatePusher : MonoBehaviour {
    // WebGL plugin externs
    [DllImport("__Internal")] static extern void WSInit(string url);
    [DllImport("__Internal")] static extern void WSSend(string json);
    [DllImport("__Internal")] static extern string WSReceive();

    GameManager    _gm;
    TetrominoSpawner _spawner;

    private HoldTetromino _hold;

    void Awake() {
        _gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        _spawner = UnityEngine.Object.FindFirstObjectByType<TetrominoSpawner>();
        _hold = UnityEngine.Object.FindFirstObjectByType<HoldTetromino>();
    }

    IEnumerator Start() {
    #if UNITY_WEBGL && !UNITY_EDITOR
        WSInit("ws://localhost:3250/ws");
        yield return new WaitForSeconds(0.1f);
        Debug.Log("[RemoteStatePusher] WS initialized");
    #endif
        // hook into your game events
        GameManager.OnNewPieceEvent    += PushCurrentState;
        GameManager.OnHardDropEvent    += PushCurrentState;
        GameManager.OnLeftStuckEvent   += PushCurrentState;
        GameManager.OnRightStuckEvent  += PushCurrentState;
        GameManager.OnMinoLockEvent    += PushCurrentState;
        yield break;
    }

    void OnDestroy() {
        GameManager.OnNewPieceEvent    -= PushCurrentState;
        GameManager.OnHardDropEvent    -= PushCurrentState;
        GameManager.OnLeftStuckEvent   -= PushCurrentState;
        GameManager.OnRightStuckEvent  -= PushCurrentState;
        GameManager.OnMinoLockEvent    -= PushCurrentState;
    }

    void PushCurrentState() {
        var dto = GetBoardState();
        var json = JsonUtility.ToJson(dto);
        Debug.Log($"[RemoteStatePusher] Sending state: {json}");
    #if UNITY_WEBGL && !UNITY_EDITOR
        WSSend(json);
    #endif
    }

    void Update() {
    #if UNITY_WEBGL && !UNITY_EDITOR
        var msg = WSReceive();
        if (!string.IsNullOrEmpty(msg)) {
            Debug.Log($"[RemoteCommandReceiver] Received command: {msg}");
            DispatchCommands(msg);
        }
    #endif
    }
    
    BoardStateDto GetBoardState() {
        var dto = new BoardStateDto {
            linesCleared = _gm.linesCleared,
            level = _gm.level,
            board = new List<TileDto>()
        };
        for (int x = 0; x < _gm.sizeX; x++) {
            for (int y = 0; y < _gm.sizeY; y++) {
                var go = GameManager.gameBoard[x][y];
                if (go == null) continue;
                var tile = go.GetComponent<Tile>();
                dto.board.Add(new TileDto {
                    x = x,
                    y = y,
                    isRevealed = tile.isRevealed,
                    isFlagged = tile.isFlagged,
                    nearbyMines = tile.isRevealed ? tile.nearbyMines : -1,
                    aura = tile.aura.ToString()
                });
            }
        }
        dto.nextPiece = _spawner.tetrominoPreviewList.Count > 0
            ? BuildPieceDto(_spawner.tetrominoPreviewList[0])
            : null;
        var holdComp = UnityEngine.Object.FindFirstObjectByType<HoldTetromino>();
        dto.heldPiece = (holdComp != null && holdComp.heldTetromino != null)
            ? BuildPieceDto(holdComp.heldTetromino)
            : null;
        return dto;
    }

    PieceDto BuildPieceDto(GameObject piece) {
        var group = piece.GetComponent<Group>();
        var cells = new List<Vector2Int>();
        foreach (var t in group.GetChildTiles())
            cells.Add(new Vector2Int(t.coordX, t.coordY));
        return new PieceDto {
            type = group.tetrominoType.ToString(),
            cells = cells
        };
    }

    void DispatchCommands(string json) {
    // if it starts with '[' treat as array
    if (json.TrimStart().StartsWith("[")) {
        var list = JsonHelper.FromJsonArray<CommandDto>(json);
        foreach (var cmd in list) HandleSingle(cmd);
    }
    else {
        var cmd = JsonUtility.FromJson<CommandDto>(json);
        HandleSingle(cmd);
    }
}


    void HandleSingle(CommandDto cmd) {
        var activeObj = _spawner.currentTetromino;
        if (activeObj == null) return;
        var group = activeObj.GetComponent<Group>();
        switch (cmd.command.ToLower()) {
            case "moveleft":
                group.PressLeft();
                group.ReleaseLeft();
                break;
            case "moveright":
                group.PressRight();
                group.ReleaseRight();
                break;
            case "releaseleft":
                group.ReleaseLeft();
                break;
            case "releaseright":
                group.ReleaseRight();
                break;
            case "rotate":
                group.PressRotateClockwise();
                break;
            case "rotateccw":
                group.PressRotateCounterClockwise();
                break;
            case "softdrop":
                group.PressSoftDrop();
                group.ReleaseSoftDrop();
                break;
            case "releasesoftdrop":
                group.ReleaseSoftDrop();
                break;
            case "harddrop":
                group.PressHardDrop();
                break;
            case "hold":
                _hold.PressHold();
                break;
            case "reveal":
                DoReveal(cmd.x, cmd.y);
                break;
            case "flag":
                DoFlag(cmd.x, cmd.y);
                break;
            case "chord":
                DoChord(cmd.x, cmd.y);
                break;
            case "chordflag":
                DoChordFlag(cmd.x, cmd.y);
                break;
            default:
                Debug.LogWarning($"Unknown command: {cmd.command}");
                break;
        }
    }

    void DoReveal(int x, int y) {
        var tile = _gm.GetGameTile(x, y);
        if (tile == null) return;
        tile.Reveal(true, /*isManual=*/true);
    }

    void DoFlag(int x, int y) {
        var tile = _gm.GetGameTile(x, y);
        if (tile == null) return;
        tile.FlagToggle();
    }

    void DoChord(int x, int y) {
        var tile = _gm.GetGameTile(x, y);
        if (tile == null) return;
        tile.Chord();
    }

    void DoChordFlag(int x, int y) {
        var tile = _gm.GetGameTile(x, y);
        if (tile == null) return;
        tile.ChordFlag();
    }
}