using System;

[Serializable]
public class GameState
{
    public enum State
    {
        WaitingForPlayerInput,
        PlayerMoving,
        AITurn,
        GameOver
    }

    public State CurrentState { get; private set; }

    public GameState()
    {
        CurrentState = State.WaitingForPlayerInput;
    }

    public void SetState(State newState)
    {
        CurrentState = newState;
    }

    public bool IsGameOver()
    {
        return CurrentState == State.GameOver;
    }
}