using UnityEngine;

/// <summary>
/// Quick script to disable Self-Play Trainer to prevent Q-table override
/// Attach to any GameObject and run to preserve your trained Q-table
/// </summary>
public class QuickTrainerDisabler : MonoBehaviour
{
    void Start()
    {
        DisableSelfPlayTrainer();
    }
    
    [ContextMenu("Disable Self-Play Trainer")]
    public void DisableSelfPlayTrainer()
    {
        QuoridorSelfPlayTrainer trainer = FindFirstObjectByType<QuoridorSelfPlayTrainer>();
        if (trainer != null)
        {
            if (trainer.autoStartTraining)
            {
                trainer.autoStartTraining = false;
                Debug.Log("✅ QuoridorSelfPlayTrainer.autoStartTraining set to FALSE");
                Debug.Log("🔒 Your trained Q-table is now protected from being overwritten");
            }
            else
            {
                Debug.Log("ℹ️ QuoridorSelfPlayTrainer.autoStartTraining is already FALSE");
            }
            
            // Also disable the trainer GameObject to be extra safe
            trainer.gameObject.SetActive(false);
            Debug.Log("🚫 QuoridorSelfPlayTrainer GameObject DISABLED");
        }
        else
        {
            Debug.Log("ℹ️ No QuoridorSelfPlayTrainer found in scene");
        }
    }
    
    [ContextMenu("Enable Self-Play Trainer")]
    public void EnableSelfPlayTrainer()
    {
        QuoridorSelfPlayTrainer trainer = FindFirstObjectByType<QuoridorSelfPlayTrainer>();
        if (trainer != null)
        {
            trainer.gameObject.SetActive(true);
            trainer.autoStartTraining = true;
            Debug.Log("🚀 QuoridorSelfPlayTrainer ENABLED and set to auto-start");
            Debug.LogWarning("⚠️ This will start training and may override your existing Q-table!");
        }
        else
        {
            Debug.LogWarning("❌ No QuoridorSelfPlayTrainer found in scene");
        }
    }
    
    [ContextMenu("Check Trainer Status")]
    public void CheckTrainerStatus()
    {
        QuoridorSelfPlayTrainer trainer = FindFirstObjectByType<QuoridorSelfPlayTrainer>();
        if (trainer != null)
        {
            Debug.Log("=== SELF-PLAY TRAINER STATUS ===");
            Debug.Log($"🎯 GameObject Active: {trainer.gameObject.activeInHierarchy}");
            Debug.Log($"🚀 Auto Start Training: {trainer.autoStartTraining}");
            Debug.Log($"🔒 Preserve Existing Q-Table: {trainer.preserveExistingQTable}");
            Debug.Log($"📊 Episodes: {trainer.numEpisodes}");
        }
        else
        {
            Debug.Log("ℹ️ No QuoridorSelfPlayTrainer found in scene");
        }
    }
}
