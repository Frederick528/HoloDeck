using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCore
{
    private int _turnCnt = 0;
    private bool _isGameStarted = true;
    private bool _isTurnProceeding = false;
    public int TurnCnt => _turnCnt;
    public bool IsGameStarted => _isGameStarted;
    public bool IsTurnProceeding => _isTurnProceeding;
    public void InitGame()
    {
        //Start Game
        //It's Called on Start() and ReTry();
        _turnCnt = 0;
        _isGameStarted = true;
        _isTurnProceeding = false;
    }

    public void EndGame()
    {
        //When the game ends, call this method.
        if (!_isGameStarted)
            throw new Exception("The Game is not Started");
        _isGameStarted = false;

        Debug.Log("Game Over");
    }

    public void StartFight()
    {
        //When the fight monsters, call this method.
        if (!_isGameStarted)
            throw new Exception("The Game is not Started");
        _turnCnt = 0;
        StartTurn();
    }

    public void StartTurn()
    {
        //When the start my turn, call this method.
        _turnCnt++;
        _isTurnProceeding = true;
        //curAp = maxAp
        //drawCard
    }

    public void EndTurn()
    {
        //When the click end turn, call this method.
        _isTurnProceeding = false;
        //throwCard
    }

    public void EndFight()
    {
        //When the clear room, call this method.
        if (!_isGameStarted)
            throw new Exception("The Game is not Started");
        //_turnCnt = 0;
        _isTurnProceeding = false;
        //reward
    }
}
