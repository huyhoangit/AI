using UnityEngine;

/// <summary>
/// Auto-runs at game start to fix missing script components
/// Attach this to any GameObject in the scene (like Main Camera or GameManager)
/// </summary>
public class AutoFixer : MonoBehaviour
{
    void Awake()
    {
        // Fix missing components immediately when the scene loads
        FixMissingPlayerComponents();
    }
    
    void FixMissingPlayerComponents()
    {
        Debug.Log("🔧 AutoFixer: Checking Player components...");
        
        // Fix Player1
        GameObject player1 = GameObject.Find("Player1");
        if (player1 != null)
        {
            EnsureChessPlayer(player1, 1);
        }
        
        // Fix Player2
        GameObject player2 = GameObject.Find("Player2");
        if (player2 != null)
        {
            EnsureChessPlayer(player2, 2);
            EnsureQuoridorAI(player2);
        }
        
        Debug.Log("✅ AutoFixer: Player components checked");
    }
    
    void EnsureChessPlayer(GameObject playerObj, int playerID)
    {
        if (playerObj.GetComponent<ChessPlayer>() == null)
        {
            var chessPlayer = playerObj.AddComponent<ChessPlayer>();
            chessPlayer.playerID = playerID;
            
            if (playerID == 1)
            {
                chessPlayer.col = 4;
                chessPlayer.row = 0;
            }
            else
            {
                chessPlayer.col = 4;
                chessPlayer.row = 8;
            }
            
            Debug.Log($"✅ Added ChessPlayer to {playerObj.name}");
        }
    }
    
    void EnsureQuoridorAI(GameObject playerObj)
    {
        if (playerObj.GetComponent<QuoridorAI>() == null)
        {
            var aiComponent = playerObj.AddComponent<QuoridorAI>();
            aiComponent.playerID = 2;
            Debug.Log($"✅ Added QuoridorAI to {playerObj.name}");
        }
    }
}
