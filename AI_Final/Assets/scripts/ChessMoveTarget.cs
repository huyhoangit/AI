using UnityEngine;

public class ChessMoveTarget : MonoBehaviour
{
    public int targetX, targetY;
    private ChessPlayer chessPlayer;
    
    public void Initialize(int x, int y, ChessPlayer player)
    {
        targetX = x;
        targetY = y;
        chessPlayer = player;
    }
    
    private void OnMouseDown()
    {
        if (chessPlayer != null)
        {
            chessPlayer.MoveTo(targetX, targetY);
        }
    }
}
