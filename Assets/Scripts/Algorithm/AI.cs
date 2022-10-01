using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static gridGenerator;
using Random = UnityEngine.Random;

public static class AI
{




    //##############################################################################################################

    #region Logic
    internal static void searchBestMove()
    {
        int heighestScore = 0, bestMove = 0;
        Troop heighestTroop = null;
        var enemyTroops = troops.Where(x => x.isWhite != isPlayerWhite).ToList();
        foreach (var item in enemyTroops)
        {
            item.predictMoves();
            var targets = item.targetMoves;
            foreach (var _item in targets)
            {
                var score = (int)cells[_item].troop.troopType;
                if (heighestScore < score)
                {
                    heighestScore = score;
                    heighestTroop = item;
                    bestMove = _item;
                }
            }
        }

        if (heighestScore != 0)
        {
            heighestTroop.Move(bestMove);
            return;
        }

    ReSearch:
        var randomTroop = enemyTroops[Random.Range(0, enemyTroops.Count)];
        if (randomTroop.randomMove() == false)
        {
            goto ReSearch;
        }
    }

    static void min()
    {

    }

    static void max()
    {

    }


    #endregion

    //##############################################################################################################

}
#region dataStructures
public class ChessBoard
{

    public ChessBoard(List<Cell> _cells)
    {
        foreach (var item in _cells)
        {
            if (item.troop != null)
            {
                pieces.Add(new(item.troop.troopType, item.troop.isWhite));
            }
            else
            {
                pieces.Add(new(null, false));
            }
        }
    }

    public ChessBoard(List<ChessPiece> pieces)
    {
        this.pieces = pieces;
    }

    readonly List<ChessPiece> pieces = new();

    internal List<int> findPath(int index, int x, int y, int steps, out int? target)
    {
        int _y = Mathf.CeilToInt(index / 8);
        int _x = index % 8;
        target = null;
        List<int> predictableMoves = new();
        for (int i = 1; i <= steps; i++)
        {
            int moveX = (_y + (y * i)) * 8;
            int moveY = (_x + (x * i));

            int movePos = moveX + moveY;
            if (moveX < 64 && movePos >= 0 && movePos < 64 && Mathf.CeilToInt(moveX / 8) == Mathf.CeilToInt(movePos / 8))
            {
                //legal moves
                if (pieces[movePos].troopType != null)
                {
                    if (pieces[movePos].isWhite != pieces[index].isWhite)
                    {
                        target = movePos;
                    }
                    break;
                }
                else
                {
                    predictableMoves.Add(movePos);
                }
            }
            else
            {
                break;
            }

            // if ((TempMove >= 0 && TempMove < 64) && (Mathf.FloorToInt(((predictableMoves == null || predictableMoves.Count < 1) ? index : predictableMoves[^1]) / line) != Mathf.FloorToInt(TempMove / line)))
            // {
            //     // Debug.Log(TempMove + ",,," + pieces.Count);
            //     if(pieces[TempMove].troopType != null)
            //     {
            //         if(pieces[TempMove].isWhite != pieces[index].isWhite)
            //         {
            //             target = TempMove;
            //             predictableMoves.Add(TempMove);
            //         }
            //         break;
            //     }
            //     predictableMoves.Add(TempMove);
            // }
            // else
            // {
            //     break;
            // }
        }
        return predictableMoves;
    }
}

public class ChessPiece
{
    public ChessPiece(TroopType? troopType, bool isWhite)
    {
        this.troopType = troopType;
        this.isWhite = isWhite;
    }
    public TroopType? troopType = null;
    public bool isWhite;
}

#endregion