using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static gridGenerator;
using System.Diagnostics;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;

public static class AI
{




    //##############################################################################################################

    #region Logic
    internal static void searchBestMove()
    {
        if (gridGenerator.Instance.UseFastCalculation)
        {
            int heighestScore = 0, bestMove = 0;
            Troop heighestTroop = null;
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
        else
        {
            ChessBoard chessBoard = new(cells);
            int TroopIndex = 0, MoveIndex = 0, FinalScore = 0;
            MiniMax(chessBoard, false, 3, true, ref TroopIndex, ref MoveIndex, ref FinalScore);
            Debug.LogError(TroopIndex + "," + MoveIndex + "," + FinalScore);
        }
    }

    static int MiniMax(ChessBoard _chessBoard, bool isPlayer, int depth, bool Initial, ref int TroopIndex, ref int MoveIndex, ref int FinalScore)
    {
        if (depth <= 1)
        {
            int currentScore = _chessBoard.getEvaluation(isPlayerWhite == false);
            int _White = 0, _Black = 0;
            foreach (var item in _chessBoard.pieces)
            {
                if (item.troopType != TroopType.none)
                {
                    if (item.isWhite)
                    {
                        _White++;
                    }
                    else
                    {
                        _Black++;
                    }
                }
            }
            return currentScore;
        }
        else
        {
            int index = 0, maxScore = int.MinValue;
            List<(int index, int move)> AllMoves = new();
            foreach (var item in _chessBoard.pieces)
            {
                if (item.troopType != TroopType.none && ((isPlayerWhite == item.isWhite) == isPlayer))
                {
                    var PredictedMoves = _chessBoard.getAllPredictable(item.troopType, index);
                    foreach (var _item in PredictedMoves.Item1)
                    {
                        AllMoves.Add((index, _item));
                    }
                    foreach (var _item in PredictedMoves.Item2)
                    {
                        if (PredictedMoves.Item1.Contains(_item) == false)
                        {
                            AllMoves.Add((index, _item));
                        }
                    }
                }
                index++;
            }

            foreach (var item in AllMoves)
            {
                ChessBoard __chessBoard = _chessBoard;
                __chessBoard.swipe(item.index, item.move);
                int currentScore = MiniMax(__chessBoard, isPlayer == false, depth - 1, false, ref TroopIndex, ref MoveIndex, ref FinalScore);
                Debug.Log(item.index + "->" + item.move + "-->>" + currentScore);
                if (Initial && (maxScore < currentScore))
                {
                    maxScore = currentScore;
                    TroopIndex = item.index;
                    MoveIndex = item.move;
                    FinalScore = currentScore;
                }
            }

            return maxScore;
        }
    }

    public static async Task<int> MiniMax(ChessBoard chessBoard, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (depth == 0 || chessBoard.GameOver())
        {
            return chessBoard.getEvaluation(!isPlayerWhite);
        }

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            int index = 0;
            foreach (ChessPiece item in chessBoard.pieces)
            {
                if (item.troopType != TroopType.none)
                {
                    var resultMoves = chessBoard.getAllPredictable(item.troopType, index);
                    Debug.LogError(resultMoves.Item1.Count);
                    foreach (var _item in resultMoves.Item1)
                    {
                        Debug.LogError(index + "," + _item);
                        List<ChessPiece> newPieces = chessBoard.swipeAndReturnNew(index, _item);

                        Debug.Log("hiii");
                        ChessBoard chessBoard1 = new(newPieces);
                        Debug.Log("hiii2");
                        int eval = await MiniMax(chessBoard1, depth - 1, alpha, beta, false);
                        Debug.Log("hiii23");
                        maxEval = Mathf.Max(maxEval, b: eval);
                        alpha = Mathf.Max(alpha, eval);
                        Debug.Log("hiii234");
                        if (beta <= alpha)
                        {
                            Debug.Log("hiii2345");
                            break;
                        }
                        Debug.Log(item + "," + _item + "," + depth);
                    }
                }
                index++;
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            int index = 0;
            foreach (var item in chessBoard.pieces)
            {
                if (item.troopType != TroopType.none)
                {
                    var resultMoves = chessBoard.getAllPredictable(item.troopType, index);
                    foreach (var _item in resultMoves.Item1)
                    {
                        var newPieces = chessBoard.swipeAndReturnNew(index, _item);
                        ChessBoard chessBoard1 = new(newPieces);
                        int eval = await MiniMax(chessBoard1, depth - 1, alpha, beta, true);
                        minEval = Mathf.Min(minEval, b: eval);
                        beta = Mathf.Min(beta, alpha);
                        if (beta <= alpha)
                        {
                            break;
                        }
                        Debug.Log(item + "," + _item + "," + depth);
                    }
                }
                index++;
            }
            return minEval;
        }
    }

    static bool GameOver(this ref ChessBoard chessBoard)
    {
        return false;
    }

    static internal int getEvaluation(this ref ChessBoard chessBoard, bool white)
    {
        int maxScore = 0;
        int index = 0;
        foreach (var item in chessBoard.pieces)
        {
            if (item.troopType != TroopType.none && item.isWhite == white)
            {
                maxScore += (int)item.troopType;
            }
            else if (item.troopType != TroopType.none)
            {
                maxScore -= (int)item.troopType;
            }
            index++;
        }

        return maxScore;
    }

    static internal int getEvaluation(this List<ChessPiece> pieces, bool white)
    {
        int maxScore = 0;
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].troopType != TroopType.none)
            {
                int currentScore = (int)pieces[i].troopType;
                maxScore = (white == pieces[i].isWhite) ? (maxScore += currentScore) : (maxScore -= currentScore);
            }
        }

        return maxScore;
    }


    internal static List<ChessPiece> swipeAndReturnNew(this ChessBoard chessBoard, int attacker, int defender)
    {
        var tempPieces = chessBoard.pieces;
        tempPieces[defender] = tempPieces[attacker];
        tempPieces[attacker] = new(TroopType.none, false);

        if (tempPieces == null) Debug.Log("Null");
        return tempPieces;
    }

    internal static int bestMoveFromSide(this ref ChessBoard chessBoard, bool white)
    {
        int maxScore = int.MinValue;
        int index = 0;
        foreach (var item in chessBoard.pieces)
        {
            if (item.troopType != TroopType.none && item.isWhite == white)
            {
                var result = chessBoard.getAllPredictable(item.troopType, index);
                foreach (var _item in result.Item2)
                {
                    int currentScore = (int)chessBoard.pieces[_item].troopType;
                    if (maxScore < currentScore)
                    {
                        maxScore = currentScore;
                    }
                }
            }
            index++;
        }
        return maxScore;
    }

    internal static (List<int>, List<int>) getAllPredictable(this ref ChessBoard chessBoard, TroopType troopType, int index)
    {
        List<int> _Predictable = new();
        List<int> _target = new();
        switch (troopType)
        {
            case TroopType.soldier:
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 0, -1, 1, false, true);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, -1, 1, true);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, -1, 1, true);
                break;
            case TroopType.camel:
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, 1, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, 1, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, -1, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, -1, 8);
                break;
            case TroopType.elephant:
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 0, 1, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 0, -1, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, 0, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, 0, 8);
                break;
            case TroopType.horse:
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 2, 1, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 2, -1, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -2, 1, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -2, -1, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, 2, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, 2, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, -2, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, -2, 1);
                break;
            case TroopType.queen:
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 0, 1, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 0, -1, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, 0, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, 0, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, 1, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, 1, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, -1, 8);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, -1, 8);
                break;
            case TroopType.king:
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 0, 1, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 0, -1, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, 0, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, 0, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, 1, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, 1, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, 1, -1, 1);
                castRay(ref chessBoard, ref _Predictable, ref _target, index, -1, -1, 1);
                break;
        }
        return (_Predictable, _target);
    }

    private static void castRay(ref ChessBoard chessBoard, ref List<int> predictableMoves, ref List<int> targetMoves, int index, int x, int y, int steps, bool soldierSide = false, bool soldierFront = false)
    {
        y = (isPlayerWhite == chessBoard.pieces[index].isWhite) ? y : -y;
        var returnedMoves = chessBoard.findPath(index, x, y, steps, out int? target);

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

    public static List<int> getIllegalKingMoves(this ChessBoard chessBoard, int index)
    {
        List<int> ignoreIndexes = new();

        foreach (var item in ((chessBoard.pieces[index].isWhite == isPlayerWhite) ? enemyTroops : PlayerTroops))
        {
            var predictedData = chessBoard.getAllPredictable(item.troopType, item.cell.index);
            foreach (var num in predictedData.Item1)
            {
                if (ignoreIndexes.Contains(num) == false)
                {
                    ignoreIndexes.Add(num);
                }
            }
        }
        return ignoreIndexes;
    }

    #endregion

    //##############################################################################################################

}
#region dataStructures
public struct ChessBoard
{
    internal List<ChessPiece> pieces;

    public ChessBoard(List<Cell> _cells)
    {
        pieces = new();
        foreach (var item in _cells)
        {
            if (item.troop != null)
            {
                pieces.Add(new(item.troop.troopType, item.troop.isWhite));
            }
            else
            {
                pieces.Add(new(TroopType.none, false));
            }
        }
    }

    public ChessBoard(List<ChessPiece> pieces)
    {
        this.pieces = pieces;
    }


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
                if (pieces[movePos].troopType != TroopType.none)
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

    internal List<(int index, int move, int Score)> getAllPossibleMoves(bool isPlayer)
    {
        List<(int index, int move, int Score)> possibleOutComes = new();

        Debug.LogError(pieces.Count);
        for (int i = 0; i < pieces.Count; i++)
        {
            var item = pieces[i];
            if (item.troopType != TroopType.none && item.isWhite == (isPlayer == isPlayerWhite))
            {
                var result = this.getAllPredictable(item.troopType, i);
                foreach (var _item in result.Item1)
                {
                    int highScore = 0;
                    var ss = this.swipeAndReturnNew(i, _item);
                    foreach (var __item in ss)
                    {
                        if (__item.troopType != TroopType.none)
                        {
                            int currentscore = ss.getEvaluation(!isPlayerWhite);
                            if (highScore < currentscore)
                            {
                                highScore = currentscore;
                            }
                        }
                    }
                    if (highScore != 0)
                    {
                        Debug.LogWarning(i + "," + _item + "," + highScore + "," + pieces[i].troopType + "," + pieces[_item].troopType);
                    }
                    possibleOutComes.Add((i, _item, highScore));
                }
            }
        }

        return possibleOutComes;
    }

    internal void swipe(int attacker, int defender)
    {
        var Attacker = pieces[attacker];
        pieces[defender] = Attacker;
        pieces[attacker] = new ChessPiece(TroopType.none, false);
    }

}

public struct ChessPiece
{
    public ChessPiece(TroopType troopType, bool isWhite)
    {
        this.troopType = troopType;
        this.isWhite = isWhite;
    }
    public TroopType troopType;
    public bool isWhite;

    public override string ToString()
    {
        return troopType.ToString() + "," + isWhite.ToString();
    }
}

#endregion