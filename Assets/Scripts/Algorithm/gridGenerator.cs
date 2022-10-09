using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using static AI;

public class gridGenerator : Singleton<gridGenerator>
{
    [Header("game controller")]
    [SerializeField] internal bool showNumbers = false, UseFastCalculation = true, useRandomInCase = false;

    [Header("Board")]
    [SerializeField] Cell cellPrefab;
    [SerializeField] Color darkCell = Color.black, lightCell = Color.white;

    [Header("pieces")]
    [SerializeField] Troop troop;
    [SerializeField] internal Color darkTroop = Color.black, lightTroop = Color.white;
    [SerializeField] internal Texture soldierTexture, elephantTexture, camelTexture, horseTexture, queenTexture, kingTexture;

    internal static List<Cell> cells = new();
    internal static List<Vector3> cellsPos = new();
    internal static List<Troop> troops = new();
    internal static List<Troop> enemyTroops = new();
    internal static List<Troop> PlayerTroops = new();
    internal static Camera cam;
    internal static Troop currentSelectedTroop = null;

    internal static bool isPlayerWhite = true;
    internal static bool IsPlayerTurn = true;

    public override void Awake()
    {
        Instance = this;
        spawnInitials();
        cam = Camera.main;
    }

    void spawnInitials()
    {
        cells = new();
        troops = new();
        enemyTroops = new();
        PlayerTroops = new();
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                spawnCell(x, y);
            }
        }


        //Soldier
        for (int i = 48; i < 56; i++)
        {
            spawnTroop(TroopType.soldier, i, true);
        }

        //elephant
        spawnTroop(TroopType.elephant, 56, true);
        spawnTroop(TroopType.elephant, 63, true);

        //horse
        spawnTroop(TroopType.horse, 57, true);
        spawnTroop(TroopType.horse, 62, true);

        //camel
        spawnTroop(TroopType.camel, 58, true);
        spawnTroop(TroopType.camel, 61, true);

        //queen
        spawnTroop(TroopType.queen, 60, true);

        //king
        spawnTroop(TroopType.king, 59, true);



        //Soldier
        for (int i = 8; i < 16; i++)
        {
            spawnTroop(TroopType.soldier, i, false);
        }

        //elephant
        spawnTroop(TroopType.elephant, 0, false);
        spawnTroop(TroopType.elephant, 7, false);

        //horse
        spawnTroop(TroopType.horse, 1, false);
        spawnTroop(TroopType.horse, 6, false);

        //camel
        spawnTroop(TroopType.camel, 2, false);
        spawnTroop(TroopType.camel, 5, false);

        //queen
        spawnTroop(TroopType.queen, 3, false);

        //king
        spawnTroop(TroopType.king, 4, false);

    }

    void spawnCell(int x, int y)
    {
        Vector3 spawnPos = new Vector3(x, 0, -y);
        var currentCell = Instantiate(cellPrefab, spawnPos, Quaternion.identity);
        cells.Add(currentCell);
        cellsPos.Add(currentCell.transform.position);
        int index = (y * 8) + x;
        Color cellColor = ((x + y) % 2 != 0) ? darkCell : lightCell;
        currentCell.init(cellColor, showNumbers, index);
    }

    void spawnTroop(TroopType troopType, int place, bool isPlayer)
    {
        Vector3 spawnPos = cellsPos[place];
        spawnPos.y = 2;
        var currentTroop = Instantiate(troop, spawnPos, Quaternion.identity);
        currentTroop.Init(place, troopType, isPlayer);
    }

    internal static void takeDown(int attackTroop, int defentTroop, out bool gameContinue)
    {
        if (GameManager.GameContinues(attackTroop, defentTroop))
        {
            gameContinue = true;
            var takeDownTroop = cells[defentTroop].troop;
            if (takeDownTroop != null)
            {
                takeDownTroop.cell.troop = null;
                troops.Remove(takeDownTroop);
                Destroy(takeDownTroop.gameObject);
            }
        }
        else
        {
            gameContinue = false;
        }
    }

    internal static void switchTurn()
    {
        IsPlayerTurn = !IsPlayerTurn;
        if (IsPlayerTurn)
        {

        }
        else
        {
            searchBestMove();
        }
    }

    public async Task<(int score, int? troopIndex, int? moveIndex)> GoDeep(ChessBoard chessBoard, int depth, bool isPlayer)
    {
        int score = int.MinValue;
        int? troopIndex = null, moveIndex = null;

        if (depth <= 1)
        {
            int maxScore = 0;
            foreach (var item in chessBoard.pieces)
            {
                if (item.troopType != TroopType.none)
                {
                    if (item.isWhite == (isPlayer == isPlayerWhite))
                    {
                        maxScore += (int)item.troopType;
                    }
                    else
                    {
                        maxScore -= (int)item.troopType;
                    }
                }
            }
            score = maxScore;
            Debug.LogError(maxScore);
        }
        else
        {
            int index = 0;
            foreach (var item in chessBoard.pieces)
            {
                if (item.troopType != TroopType.none && ((isPlayer) ? isPlayerWhite : isPlayerWhite == false))
                {
                    var result = chessBoard.getAllPredictable(item.troopType, index);
                    if (isPlayer == false)
                    {
                        string log = index.ToString();
                        foreach (var _log in result.Item1)
                        {
                            log += "-" + _log;
                        }
                    }
                    foreach (var _item in result.Item1)
                    {
                        int currentScore = 0;
                        if (result.Item2.Contains(_item))
                        {
                            currentScore = (int)chessBoard.pieces[_item].troopType;
                        }
                        var newPieces = chessBoard.swipeAndReturnNew(index, _item);
                        ChessBoard _chessBoard = new(newPieces);
                        var returnValue = await GoDeep(_chessBoard, depth - 1, !isPlayer);
                        Debug.LogError(returnValue.score + "," + isPlayer);
                        currentScore = returnValue.score;
                        if (isPlayer && score > currentScore)
                        {
                            score = currentScore;
                            troopIndex = index;
                            moveIndex = _item;
                        }
                        else if (isPlayer == false && score < currentScore)
                        {
                            score = currentScore;
                            troopIndex = index;
                            moveIndex = _item;
                        }
                    }
                }
                index++;
            }
        }

        if (isPlayer)
        {
            score = -score;
        }
        return (score, troopIndex, moveIndex);
    }

    [ContextMenu("Highlight enemy")]
    void HighlightEnemy()
    {
        ChessBoard chessBoard = new(cells);
        var result = chessBoard.getAllPossibleMoves(false);

        foreach (var item in result)
        {
            Debug.DrawLine(cells[item.index].transform.position + new Vector3(0, 3, 0), cells[item.move].transform.position + new Vector3(0, 3, 0), Color.red, 10);
            cells[item.move].moveLit(true);
        }
    }

    // public (int score, int? troopIndex, int? moveIndex) GoDeep(ChessBoard chessBoard, int depth, bool isPlayer)
    // {
    //     Debug.Log("Deep 1");
    //     int? troopIndex = null, MoveIndex = null;
    //     if (depth < 1)
    //     {
    //         Debug.Log("Deep 2");
    //         int Evaluation = chessBoard.getEvaluation((isPlayer) ? !isPlayerWhite : isPlayerWhite);
    //         Debug.Log((isPlayer) ? -Evaluation : Evaluation);
    //         return (((isPlayer) ? -Evaluation : Evaluation), troopIndex, MoveIndex);
    //     }
    //     else
    //     {
    //         Debug.Log("Deep 3");
    //         int index = 0;
    //         int HeighestEvaluation = int.MinValue;
    //         foreach (var item in chessBoard.pieces)
    //         {
    //             Debug.Log("Deep 3.5");
    //             if (item.troopType != null && (item.isWhite == ((isPlayer) ? !isPlayerWhite : isPlayerWhite)))
    //             {
    //                 Debug.Log("Deep 4");
    //                 var result = chessBoard.getAllPredictable(item.troopType ?? TroopType.soldier, index);
    //                 int _index = 0;
    //                 foreach (var _item in result.Item1)
    //                 {
    //                     var _troopType = chessBoard.pieces[_item].troopType;
    //                     ChessBoard _chessboard = new(chessBoard.swipeAndReturnNew(index, _index));
    //                     int Evaluation = ((_troopType == null) ? 0 : (int)_troopType) + ((_troopType != TroopType.king) ? GoDeep(_chessboard, depth - 1, !isPlayer).score : 0);
    //                     if (Evaluation > HeighestEvaluation)
    //                     {
    //                         HeighestEvaluation = Evaluation;
    //                         troopIndex = index;
    //                         MoveIndex = _item;
    //                         Debug.Log("Deep 5 -> " + HeighestEvaluation + "," + troopIndex + "," + MoveIndex + "," + isPlayer + "," + depth);
    //                     }
    //                     _index++;
    //                 }
    //                 Debug.Log("Deep 6");
    //             }
    //             index++;
    //             Debug.Log("Deep 7");
    //         }
    //         Debug.LogError("Deep 8");
    //         return (HeighestEvaluation, troopIndex, MoveIndex);
    //     }
    // }
}
