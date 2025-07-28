using UnityEngine;

public class WallSlot : MonoBehaviour
{
    public int x, y;
    public bool isHorizontal;
    private WallPlacer wallPlacer;
    private int slotIndex;
    private Renderer slotRenderer;
    private Collider slotCollider;
    
    private void Awake()
    {
        slotRenderer = GetComponent<Renderer>();
        slotCollider = GetComponent<Collider>();
        
        // Hide slot by default
        if (slotRenderer != null)
            slotRenderer.enabled = false;
            
        // Disable collider by default
        if (slotCollider != null)
            slotCollider.enabled = false;
    }
    
    public void Initialize(WallPlacer placer, int index, int row, int col, bool horizontal)
    {
        this.wallPlacer = placer;
        this.slotIndex = index;
        this.x = col;
        this.y = row;
        this.isHorizontal = horizontal;
    }
    
    public void SetVisible(bool visible)
    {
        if (slotRenderer != null)
            slotRenderer.enabled = visible;
        if (slotCollider != null)
            slotCollider.enabled = visible;
    }
    
    private void OnMouseDown()
    {
        if (wallPlacer != null && slotCollider != null && slotCollider.enabled)
        {
            Debug.Log($"🖱️ Clicked wall slot {slotIndex} at [{y}, {x}], horizontal: {isHorizontal}");
            // Pass the actual slotIndex for player clicks
            wallPlacer.PlaceWall(slotIndex, y, x, isHorizontal);
        }
    }
    
    private void OnMouseEnter()
    {
        if (wallPlacer != null && slotCollider != null && slotCollider.enabled)
        {
            // Highlight this slot
            if (slotRenderer != null)
                slotRenderer.material.color = Color.yellow;
        }
    }
    
    private void OnMouseExit()
    {
        if (wallPlacer != null && slotCollider != null && slotCollider.enabled)
        {
            // Remove highlight
            if (slotRenderer != null && wallPlacer != null)
                slotRenderer.material.color = wallPlacer.validWallColor;
        }
    }
}
