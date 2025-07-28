using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Complete Fixer - Fixes all issues in one go
/// Add this to any GameObject in the scene
/// </summary>
public class CompleteFixer : MonoBehaviour
{
    [Header("Fix Settings")]
    public bool fixMissingScripts = true;
    public bool fixTTSButton = true;
    public bool setupChatUI = true;
    public bool verboseLogging = true;
    
    void Awake()
    {
        Debug.Log("🚀 CompleteFixer: Starting comprehensive fix...");
        
        if (fixMissingScripts)
            FixMissingScripts();
            
        if (fixTTSButton)
            FixTTSButton();
            
        if (setupChatUI)
            SetupChatUI();
            
        Debug.Log("✅ CompleteFixer: All fixes completed!");
    }
    
    void FixMissingScripts()
    {
        Debug.Log("🔧 Fixing missing scripts...");
        
        // Fix Player1
        GameObject player1 = GameObject.Find("Player1");
        if (player1 != null)
        {
            FixPlayerObject(player1, 1);
        }
        else
        {
            Debug.LogWarning("⚠️ Player1 not found - creating new one");
            player1 = CreatePlayerObject("Player1", 1);
        }
        
        // Fix Player2
        GameObject player2 = GameObject.Find("Player2");
        if (player2 != null)
        {
            FixPlayerObject(player2, 2);
        }
        else
        {
            Debug.LogWarning("⚠️ Player2 not found - creating new one");
            player2 = CreatePlayerObject("Player2", 2);
        }
        
        // Fix GameManager references
        FixGameManagerReferences();
    }
    
    GameObject CreatePlayerObject(string name, int playerID)
    {
        GameObject playerObj = new GameObject(name);
        
        // Set position
        if (playerID == 1)
        {
            playerObj.transform.position = new Vector3(0, -0.24f, -4.4f);
        }
        else
        {
            playerObj.transform.position = new Vector3(0, -0.24f, 4.4f);
        }
        
        FixPlayerObject(playerObj, playerID);
        return playerObj;
    }
    
    void FixPlayerObject(GameObject playerObj, int playerID)
    {
        // Remove any missing script components
        RemoveMissingComponents(playerObj);
        
        // Add ChessPlayer
        ChessPlayer chessPlayer = playerObj.GetComponent<ChessPlayer>();
        if (chessPlayer == null)
        {
            chessPlayer = playerObj.AddComponent<ChessPlayer>();
            chessPlayer.playerID = playerID;
            
            if (playerID == 1)
            {
                chessPlayer.row = 0;
                chessPlayer.col = 4;
            }
            else
            {
                chessPlayer.row = 8;
                chessPlayer.col = 4;
            }
            
            if (verboseLogging)
                Debug.Log($"✅ Added ChessPlayer to {playerObj.name}");
        }
        
        // Add WallPlacer
        WallPlacer wallPlacer = playerObj.GetComponent<WallPlacer>();
        if (wallPlacer == null)
        {
            wallPlacer = playerObj.AddComponent<WallPlacer>();
            wallPlacer.playerID = playerID;
            wallPlacer.wallsRemaining = 10;
            
            if (verboseLogging)
                Debug.Log($"✅ Added WallPlacer to {playerObj.name}");
        }
        
        // Add QuoridorAI for Player2
        if (playerID == 2)
        {
            QuoridorAI aiComponent = playerObj.GetComponent<QuoridorAI>();
            if (aiComponent == null)
            {
                aiComponent = playerObj.AddComponent<QuoridorAI>();
                aiComponent.playerID = 2;
                
                if (verboseLogging)
                    Debug.Log($"✅ Added QuoridorAI to {playerObj.name}");
            }
        }
        
        // Add basic components
        if (playerObj.GetComponent<Collider>() == null)
        {
            playerObj.AddComponent<CapsuleCollider>();
        }
        
        if (playerObj.GetComponent<Renderer>() == null)
        {
            var meshFilter = playerObj.AddComponent<MeshFilter>();
            var meshRenderer = playerObj.AddComponent<MeshRenderer>();
            
            // Create simple cube mesh
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshFilter.mesh = cube.GetComponent<MeshFilter>().sharedMesh;
            meshRenderer.material = new Material(Shader.Find("Standard"));
            
            DestroyImmediate(cube);
        }
    }
    
    void FixGameManagerReferences()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("⚠️ GameManager not found!");
            return;
        }
        
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        if (player1 != null)
        {
            var chessPlayer1 = player1.GetComponent<ChessPlayer>();
            var wallPlacer1 = player1.GetComponent<WallPlacer>();
            
            if (chessPlayer1 != null)
                gameManager.player1 = chessPlayer1;
            if (wallPlacer1 != null)
                gameManager.wallPlacer1 = wallPlacer1;
        }
        
        if (player2 != null)
        {
            var chessPlayer2 = player2.GetComponent<ChessPlayer>();
            var wallPlacer2 = player2.GetComponent<WallPlacer>();
            
            if (chessPlayer2 != null)
                gameManager.player2 = chessPlayer2;
            if (wallPlacer2 != null)
                gameManager.wallPlacer2 = wallPlacer2;
        }
        
        if (verboseLogging)
            Debug.Log("✅ Fixed GameManager references");
    }
    
    void FixTTSButton()
    {
        Debug.Log("🔧 Fixing TTS Button...");
        
        // Find TTS Button
        Button ttsButton = GameObject.Find("TTSButton")?.GetComponent<Button>();
        if (ttsButton == null)
        {
            Debug.LogWarning("⚠️ TTSButton not found!");
            return;
        }
        
        // Find ChatUIController
        ChatUIController chatController = FindFirstObjectByType<ChatUIController>();
        if (chatController == null)
        {
            Debug.LogWarning("⚠️ ChatUIController not found!");
            return;
        }
        
        // Clear existing listeners and add new one
        ttsButton.onClick.RemoveAllListeners();
        ttsButton.onClick.AddListener(chatController.OnTTSClicked);
        
        if (verboseLogging)
            Debug.Log("✅ TTS Button fixed and connected to ChatUIController.OnTTSClicked");
    }
    
    void SetupChatUI()
    {
        Debug.Log("🔧 Setting up Chat UI...");
        
        // Find or create ChatUIController
        ChatUIController chatController = FindFirstObjectByType<ChatUIController>();
        if (chatController == null)
        {
            GameObject chatObj = new GameObject("ChatUIController");
            chatController = chatObj.AddComponent<ChatUIController>();
            Debug.Log("✅ Created ChatUIController");
        }
        
        // Find or create ChatService
        ChatService chatService = FindFirstObjectByType<ChatService>();
        if (chatService == null)
        {
            GameObject serviceObj = new GameObject("ChatService");
            chatService = serviceObj.AddComponent<ChatService>();
            Debug.Log("✅ Created ChatService");
        }
        
        // Connect ChatService to ChatUIController
        chatController.chatService = chatService;
        
        // Find or create MessageBubbleCreator
        MessageBubbleCreator bubbleCreator = FindFirstObjectByType<MessageBubbleCreator>();
        if (bubbleCreator == null)
        {
            GameObject bubbleObj = new GameObject("MessageBubbleCreator");
            bubbleCreator = bubbleObj.AddComponent<MessageBubbleCreator>();
            Debug.Log("✅ Created MessageBubbleCreator");
        }
        
        chatController.messageBubbleCreator = bubbleCreator;
        
        if (verboseLogging)
            Debug.Log("✅ Chat UI setup completed");
    }
    
    void RemoveMissingComponents(GameObject obj)
    {
        // Unity doesn't provide a direct way to remove missing components
        // This is a workaround - we'll just log the issue
        var components = obj.GetComponents<Component>();
        foreach (var component in components)
        {
            if (component == null)
            {
                Debug.LogWarning($"⚠️ Found missing component on {obj.name}");
            }
        }
    }
    
    [ContextMenu("Run Complete Fix")]
    public void RunCompleteFix()
    {
        FixMissingScripts();
        FixTTSButton();
        SetupChatUI();
        Debug.Log("✅ Manual fix completed!");
    }
    
    [ContextMenu("Test TTS Button")]
    public void TestTTSButton()
    {
        Button ttsButton = GameObject.Find("TTSButton")?.GetComponent<Button>();
        if (ttsButton != null)
        {
            Debug.Log("🔍 TTS Button found!");
            Debug.Log($"Button interactable: {ttsButton.interactable}");
            Debug.Log($"Button onClick listeners count: {ttsButton.onClick.GetPersistentEventCount()}");
            
            // Test click
            ttsButton.onClick.Invoke();
            Debug.Log("✅ TTS Button click test completed");
        }
        else
        {
            Debug.LogError("❌ TTS Button not found!");
        }
    }
} 