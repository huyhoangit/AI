using UnityEngine;

/// <summary>
/// Component cho mỗi ô vuông trên bàn cờ Quoridor
/// Lưu trữ thông tin vị trí và xử lý tương tác
/// </summary>
public class BoardSquare : MonoBehaviour
{
    [Header("Board Position")]
    public int row; // Hàng (0-8)
    public int col; // Cột (0-8)
    
    [Header("Square State")]
    public bool isOccupied = false; // Có quân cờ trên ô không
    public bool isHighlighted = false; // Có đang được highlight không
    public GameObject occupyingPiece; // Quân cờ đang đứng trên ô
    
    [Header("Visual")]
    public Material originalMaterial; // Material gốc
    public Material highlightMaterial; // Material khi highlight
    
    private Renderer squareRenderer;
    private BoardManager boardManager;
    
    void Start()
    {
        // Lấy component cần thiết
        squareRenderer = GetComponent<Renderer>();
        boardManager = FindFirstObjectByType<BoardManager>();
        
        // Lưu material gốc
        if (squareRenderer != null)
        {
            originalMaterial = squareRenderer.material;
        }
        
        // Đặt tag nếu chưa có
        if (gameObject.tag == "Untagged")
        {
            gameObject.tag = "BoardSquare";
        }
    }
    
    /// <summary>
    /// Xử lý khi click vào ô
    /// </summary>
    void OnMouseDown()
    {
        Debug.Log($"Clicked on square [{col}, {row}]");
        
        // Tìm GameManager
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        bool handled = false;
        
        if (gameManager != null)
        {
            // Tìm quân chess đang được chọn
            GameObject selectedChess = FindSelectedChessPlayer();
            
            if (selectedChess != null)
            {
                // Thử di chuyển quân chess đến ô này
                TryMoveChessToThisSquare(selectedChess);
                handled = true;
            }
            else
            {
                // Không có quân chess nào được chọn, thử chọn quân chess trên ô này
                if (occupyingPiece != null)
                {
                    MonoBehaviour chessComponent = occupyingPiece.GetComponent<MonoBehaviour>();
                    if (chessComponent != null && chessComponent.GetType().Name == "ChessPlayer")
                    {
                        var selectMethod = chessComponent.GetType().GetMethod("SelectPiece");
                        if (selectMethod != null)
                        {
                            selectMethod.Invoke(chessComponent, null);
                            handled = true;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager not found in BoardSquare.OnMouseDown()");
        }
        
        // Fallback - highlight ô được click
        if (!handled)
        {
            ToggleHighlight();
            Debug.Log($"Square [{row}, {col}] highlighted - GameManager not found or no valid action");
        }
    }
    
    /// <summary>
    /// Tìm quân chess đang được chọn (sử dụng reflection để tránh dependency)
    /// </summary>
    private GameObject FindSelectedChessPlayer()
    {
        // Tìm tất cả GameObject có component tên "ChessPlayer"
        MonoBehaviour[] allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        
        foreach (MonoBehaviour obj in allObjects)
        {
            if (obj.GetType().Name == "ChessPlayer")
            {
                // Kiểm tra property "isSelected" bằng reflection
                var isSelectedProperty = obj.GetType().GetField("isSelected");
                if (isSelectedProperty != null)
                {
                    bool isSelected = (bool)isSelectedProperty.GetValue(obj);
                    if (isSelected)
                    {
                        return obj.gameObject;
                    }
                }
            }
        }
        return null;
    }
    
    /// <summary>
    /// Thử di chuyển quân chess đến ô này
    /// </summary>
    private void TryMoveChessToThisSquare(GameObject selectedChess)
    {
        if (selectedChess == null) return;
        
        // Lấy component ChessPlayer
        MonoBehaviour chessComponent = selectedChess.GetComponent<MonoBehaviour>();
        if (chessComponent == null || chessComponent.GetType().Name != "ChessPlayer") return;
        
        // Lấy vị trí hiện tại của chess
        var rowField = chessComponent.GetType().GetField("row");
        var colField = chessComponent.GetType().GetField("col");
        
        if (rowField == null || colField == null) return;
        
        int chessRow = (int)rowField.GetValue(chessComponent);
        int chessCol = (int)colField.GetValue(chessComponent);
        
        // Kiểm tra ô này có trống và có thể di chuyển đến không
        Debug.Log($"🎯 BoardSquare.TryMoveChessToThisSquare called: target=[{row}, {col}], chess=[{chessCol}, {chessRow}]");
        Debug.Log($"   IsEmpty: {IsEmpty()}, IsValidMove: {IsValidMoveFor(chessCol, chessRow)}");
        
        if (IsEmpty() && IsValidMoveFor(chessCol, chessRow))
        {
            Debug.Log($"🎯 Moving chess to square [{row}, {col}]");
            
            // Xóa quân chess khỏi ô cũ
            if (boardManager != null)
            {
                GameObject oldSquare = boardManager.GetSquareAt(chessRow, chessCol);
                if (oldSquare != null)
                {
                    BoardSquare oldBoardSquare = oldSquare.GetComponent<BoardSquare>();
                    if (oldBoardSquare != null)
                    {
                        oldBoardSquare.ClearOccupyingPiece();
                    }
                }
            }
            
            // Gọi method MoveTo của ChessPlayer
            var moveToMethod = chessComponent.GetType().GetMethod("MoveTo");
            if (moveToMethod != null)
            {
                moveToMethod.Invoke(chessComponent, new object[] { col, row });
            }
            
            // Đặt quân chess lên ô mới
            SetOccupyingPiece(selectedChess);
        }
        else
        {
            Debug.LogWarning($"❌ Cannot move to square [{row}, {col}] - Square occupied or invalid move");
            
            // Bỏ chọn quân chess nếu click vào ô không hợp lệ
            var deselectMethod = chessComponent.GetType().GetMethod("DeselectPiece");
            if (deselectMethod != null)
            {
                deselectMethod.Invoke(chessComponent, null);
            }
        }
    }
    
    /// <summary>
    /// Kiểm tra ô này có phải nước đi hợp lệ cho quân chess không
    /// </summary>
    private bool IsValidMoveFor(int chessCol, int chessRow)
    {
        // Kiểm tra ô này có trong danh sách 4 ô xung quanh quân chess không
        int deltaCol = Mathf.Abs(col - chessCol);
        int deltaRow = Mathf.Abs(row - chessRow);
        
        // Chỉ cho phép di chuyển 1 ô theo 4 hướng (không đường chéo)
        return (deltaCol == 1 && deltaRow == 0) || (deltaCol == 0 && deltaRow == 1);
    }
    
    /// <summary>
    /// Highlight ô vuông
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        
        if (squareRenderer != null)
        {
            if (highlight && highlightMaterial != null)
            {
                squareRenderer.material = highlightMaterial;
            }
            else if (highlight)
            {
                // Màu highlight mặc định
                squareRenderer.material.color = Color.yellow;
            }
            else
            {
                // Trở về màu gốc
                if (originalMaterial != null)
                {
                    squareRenderer.material = originalMaterial;
                }
                else
                {
                    ResetToDefaultColor();
                }
            }
        }
    }
    
    /// <summary>
    /// Toggle highlight
    /// </summary>
    public void ToggleHighlight()
    {
        SetHighlight(!isHighlighted);
    }
    
    /// <summary>
    /// Đặt quân cờ lên ô
    /// </summary>
    public void SetOccupyingPiece(GameObject piece)
    {
        occupyingPiece = piece;
        isOccupied = (piece != null);
    }
    
    /// <summary>
    /// Xóa quân cờ khỏi ô
    /// </summary>
    public void ClearOccupyingPiece()
    {
        occupyingPiece = null;
        isOccupied = false;
    }
    
    /// <summary>
    /// Kiểm tra ô có trống không
    /// </summary>
    public bool IsEmpty()
    {
        return !isOccupied && occupyingPiece == null;
    }
    
    /// <summary>
    /// Reset về màu mặc định dựa trên vị trí
    /// </summary>
    private void ResetToDefaultColor()
    {
        if (squareRenderer != null)
        {
            // Pattern cờ vua
            bool isLightSquare = (row + col) % 2 == 0;
            Color defaultColor = isLightSquare ? 
                new Color(0.9f, 0.9f, 0.8f) : // Màu sáng
                new Color(0.4f, 0.3f, 0.2f);  // Màu tối
            
            squareRenderer.material.color = defaultColor;
        }
    }
    
    /// <summary>
    /// Lấy vị trí world của ô
    /// </summary>
    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }
    
    /// <summary>
    /// Lấy vị trí grid của ô
    /// </summary>
    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(col, row);
    }
    
    /// <summary>
    /// Kiểm tra ô có thể di chuyển đến không
    /// </summary>
    public bool IsValidMoveTarget()
    {
        return IsEmpty(); // Chỉ có thể di chuyển đến ô trống
    }
    
    /// <summary>
    /// Mouse hover effect
    /// </summary>
    void OnMouseEnter()
    {
        if (!isHighlighted && squareRenderer != null)
        {
            // Hiệu ứng hover nhẹ
            Color hoverColor = squareRenderer.material.color * 1.1f;
            squareRenderer.material.color = hoverColor;
        }
    }
    
    /// <summary>
    /// Mouse exit effect
    /// </summary>
    void OnMouseExit()
    {
        if (!isHighlighted)
        {
            ResetToDefaultColor();
        }
    }
    
    /// <summary>
    /// Debug info
    /// </summary>
    public override string ToString()
    {
        return $"BoardSquare[{row},{col}] - Occupied: {isOccupied}, Highlighted: {isHighlighted}";
    }
}
