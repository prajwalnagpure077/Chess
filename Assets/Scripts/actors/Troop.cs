using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;
using static gridGenerator;
using Random = UnityEngine.Random;

public class Troop : MonoBehaviour
{
    [SerializeField] new Renderer renderer;
    [SerializeField] new Collider collider;

    internal Cell cell;
    internal TroopType troopType;
    internal bool isWhite, isPlayer;

    List<int> predictableMoves = new();
    internal List<int> targetMoves = new();

    float lastTouchTime = 0;
    internal void Init(int place, TroopType troopType, bool isPlayer)
    {
        troops.Add(this);

        this.isPlayer = isPlayer;

        if (isPlayerWhite)
        {
            isWhite = isPlayer;
        }
        else
        {
            isWhite = !isPlayer;
        }

        renderer.material.color = (isWhite) ? gridGenerator.Instance.lightTroop : gridGenerator.Instance.darkTroop;

        if (isPlayer == false)
        {
            enemyTroops.Add(this);
            Destroy(collider);
        }
        else
        {
            PlayerTroops.Add(this);
        }

        this.troopType = troopType;

        renderer.material.mainTexture = troopType switch
        {
            TroopType.soldier => gridGenerator.Instance.soldierTexture,
            TroopType.elephant => gridGenerator.Instance.elephantTexture,
            TroopType.camel => gridGenerator.Instance.camelTexture,
            TroopType.horse => gridGenerator.Instance.horseTexture,
            TroopType.queen => gridGenerator.Instance.queenTexture,
            TroopType.king => gridGenerator.Instance.kingTexture,
            _ => null
        };

        setCell(place);
    }

    private void OnMouseDown()
    {
        if (IsPlayerTurn)
        {
            lastTouchTime = Time.time;
            select();
        }
    }

    private void OnMouseDrag()
    {
        if (IsPlayerTurn)
        {
            Vector3 touchPos = Input.mousePosition;
            touchPos.z = 1;
            Vector3 movePos = gridGenerator.cam.ScreenToWorldPoint(touchPos);
            // movePos.x = Mathf.RoundToInt(movePos.x);
            movePos.y = 2;
            // movePos.z = Mathf.RoundToInt(movePos.z);
            transform.position = movePos;
        }
    }

    private void OnMouseUp()
    {
        if (IsPlayerTurn)
        {
            if ((Time.time - lastTouchTime) > 0.2f)
            {
                deselect();
                if (Physics.Linecast(transform.position - new Vector3(0, 0.1f, 0), transform.position - new Vector3(0, 5, 0), out RaycastHit hit))
                {
                    Move(hit.collider.GetComponent<Cell>().index, true);
                }
                else
                {
                    Move(cell.index);
                }
            }
            else
            {
                Vector3 snapPos = cell.transform.position;
                snapPos.y = 2;
                transform.position = snapPos;
            }
        }
    }

    void select()
    {
        if (currentSelectedTroop != null)
        {
            currentSelectedTroop.deselect();
        }
        currentSelectedTroop = this;

        predictMoves();
        foreach (var item in predictableMoves)
        {
            cells[item].moveLit(true);
        }
        foreach (var item in targetMoves)
        {
            cells[item].attackLit(true);
        }
    }

    void deselect()
    {
        foreach (var item in cells)
        {
            item.moveLit(false);
            item.attackLit(false);
        }
        currentSelectedTroop = null;
    }

    internal void predictMoves()
    {
        predictableMoves.Clear();
        targetMoves.Clear();
        ChessBoard chessBoard = new(cells);
        var predictedData = chessBoard.getAllPredictable(troopType, cell.index);
        predictableMoves = predictedData.Item1;
        targetMoves = predictedData.Item2;
        if (troopType == TroopType.king)
        {
            var ignoreMoves = chessBoard.getIllegalKingMoves(cell.index);
            foreach (var item in ignoreMoves)
            {
                if (predictableMoves.Contains(item))
                {
                    predictableMoves.Remove(item);
                    if (targetMoves.Contains(item))
                    {
                        targetMoves.Remove(item);
                    }
                }
            }
        }
    }

    internal bool randomMove()
    {
        predictMoves();
        if (predictableMoves != null && predictableMoves.Count > 0)
        {
            Move(predictableMoves[Random.Range(0, predictableMoves.Count)]);
            return true;
        }
        else
        {
            return false;
        }
    }

    internal void Move(int i, bool respectPrediction = false)
    {
        bool continueGame = true;
        bool _switchTurn = false;
        if (respectPrediction && predictableMoves.Contains(i) == false)
        {
            Move(cell.index);
            return;
        }
        if (cells[i].troop != null && cells[i].troop != this && cells[i].troop.isWhite != isWhite)
        {
            gridGenerator.takeDown(cell.index, i, out continueGame);
            _switchTurn = true;
        }
        else if (i != cell.index)
        {
            _switchTurn = true;
        }
        if (continueGame)
        {
            var movePos = cells[i].transform.position;
            movePos.y = 2;
            setCell(i);
            deselect();
            Debug.Log(troopType + "," + isWhite + "," + i);
            transform.DOMove(movePos, Vector3.Distance(transform.position, movePos) * 0.02f).SetEase(Ease.Linear).onComplete += () =>
            {
                if (_switchTurn)
                {
                    switchTurn();
                }
            };
        }
    }

    void setCell(int i)
    {
        if (cell != null)
        {
            cell.troop = null;
        }

        cell = cells[i];
        cell.troop = this;
    }

#if UNITY_EDITOR
    [ContextMenu("debugMoves")]
    void debugMoves()
    {
        predictMoves();
        foreach (var item in predictableMoves)
        {
            Debug.Log(item);
        }
    }
#endif
}
