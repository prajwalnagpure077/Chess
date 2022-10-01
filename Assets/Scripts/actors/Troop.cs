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
    internal bool isWhite;

    List<int> predictableMoves = new();
    internal List<int> targetMoves = new();

    float lastTouchTime = 0;
    internal void Init(int place, TroopType troopType, bool isPlayer)
    {
        troops.Add(this);

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
            Destroy(collider);
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
        switch (troopType)
        {
            case TroopType.soldier:
                castRay(ref chessBoard, 0, -1, 1, false, true);
                castRay(ref chessBoard, 1, -1, 1, true);
                castRay(ref chessBoard, -1, -1, 1, true);
                break;
            case TroopType.camel:
                castRay(ref chessBoard, 1, 1, 8);
                castRay(ref chessBoard, -1, 1, 8);
                castRay(ref chessBoard, 1, -1, 8);
                castRay(ref chessBoard, -1, -1, 8);
                break;
            case TroopType.elephant:
                castRay(ref chessBoard, 0, 1, 8);
                castRay(ref chessBoard, 0, -1, 8);
                castRay(ref chessBoard, 1, 0, 8);
                castRay(ref chessBoard, -1, 0, 8);
                break;
            case TroopType.horse:
                castRay(ref chessBoard, 2, 1, 1);
                castRay(ref chessBoard, 2, -1, 1);
                castRay(ref chessBoard, -2, 1, 1);
                castRay(ref chessBoard, -2, -1, 1);
                castRay(ref chessBoard, 1, 2, 1);
                castRay(ref chessBoard, -1, 2, 1);
                castRay(ref chessBoard, 1, -2, 1);
                castRay(ref chessBoard, -1, -2, 1);
                break;
            case TroopType.queen:
                castRay(ref chessBoard, 0, 1, 8);
                castRay(ref chessBoard, 0, -1, 8);
                castRay(ref chessBoard, 1, 0, 8);
                castRay(ref chessBoard, -1, 0, 8);
                castRay(ref chessBoard, 1, 1, 8);
                castRay(ref chessBoard, -1, 1, 8);
                castRay(ref chessBoard, 1, -1, 8);
                castRay(ref chessBoard, -1, -1, 8);
                break;
            case TroopType.king:
                castRay(ref chessBoard, 0, 1, 1);
                castRay(ref chessBoard, 0, -1, 1);
                castRay(ref chessBoard, 1, 0, 1);
                castRay(ref chessBoard, -1, 0, 1);
                castRay(ref chessBoard, 1, 1, 1);
                castRay(ref chessBoard, -1, 1, 1);
                castRay(ref chessBoard, 1, -1, 1);
                castRay(ref chessBoard, -1, -1, 1);
                break;
        }
    }

    void castRay(ref ChessBoard chessBoard, int x, int y, int steps, bool soldierSide = false, bool soldierFront = false)
    {
        y = (isPlayerWhite == isWhite) ? y : -y;
        var returnedMoves = chessBoard.findPath(cell.index, x, y, steps, out int? target);

        //soldierSide
        if (soldierSide)
        {
            if (target != null)
            {
                predictableMoves.Add(target ?? 0);
                targetMoves.Add(target ?? 0);
            }
            return;
        }

        //soldierFront
        if (soldierFront)
        {
            if (target == null)
            {
                foreach (var item in returnedMoves)
                {
                    predictableMoves.Add(item);
                }
            }
            return;
        }

        //add all moves
        foreach (var item in returnedMoves)
        {
            if (predictableMoves.Contains(item) == false)
            {
                predictableMoves.Add(item);
            }
        }

        if (target != null)
        {
            predictableMoves.Add(target ?? 0);
            targetMoves.Add(target ?? 0);
        }


        // if (target != null)
        // {
        //     if (considerTarget != false)
        //     {
        //         if(considerTarget == true)
        //         {
        //             returnedMoves.Remove(target ?? 0);
        //         }
        //         targetMoves.Add(target ?? 0);
        //     }
        // }
        // if (considerTarget != true && (considerTarget == false && target == null))
        // {
        //     foreach (var item in returnedMoves)
        //     {
        //         if (predictableMoves.Contains(item) == false)
        //         {
        //             predictableMoves.Add(item);
        //         }
        //     }
        // }
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
        bool _switchTurn = false;
        if (respectPrediction && predictableMoves.Contains(i) == false)
        {
            Move(cell.index);
            return;
        }
        if (cells[i].troop != null && cells[i].troop != this && cells[i].troop.isWhite != isWhite)
        {
            gridGenerator.takeDown(i);
            _switchTurn = true;
        }
        else if (i != cell.index)
        {
            _switchTurn = true;
        }
        var movePos = cells[i].transform.position;
        movePos.y = 2;
        setCell(i);
        deselect();
        transform.DOMove(movePos, Vector3.Distance(transform.position, movePos) * 0.05f).SetEase(Ease.OutCirc).onComplete += () =>
        {
            if (_switchTurn)
            {
                switchTurn();
            }
        };
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
