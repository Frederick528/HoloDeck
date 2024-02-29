using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCore
{
    private bool _isGameStarted = true;
    public bool IsGameStarted => _isGameStarted;

    public void InitGame()
    {
        //Start Game
        //It's Called on Start() and ReTry();
        _isGameStarted = true;
    }

    public void EndGame()
    {
        //When the game ends, call this method.
        if (!_isGameStarted)
            throw new System.Exception("The Game is not Started");
        _isGameStarted = false;

        Debug.Log("Game Over");
    }
}
