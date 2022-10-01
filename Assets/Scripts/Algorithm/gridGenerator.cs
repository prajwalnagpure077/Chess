using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static AI;

public class gridGenerator : Singleton<gridGenerator>
{
    [Header("game controller")]
    [SerializeField] bool showNumbers = false;

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

    internal static void takeDown(int index)
    {
        var takeDownTroop = cells[index].troop;
        if (takeDownTroop != null)
        {
            takeDownTroop.cell.troop = null;
            troops.Remove(takeDownTroop);
            Destroy(takeDownTroop.gameObject);
        }
    }

    internal static void switchTurn()
    {
        Debug.Log("hello");
        IsPlayerTurn = !IsPlayerTurn;
        if (IsPlayerTurn)
        {

        }
        else
        {
            searchBestMove();
        }
    }
}
