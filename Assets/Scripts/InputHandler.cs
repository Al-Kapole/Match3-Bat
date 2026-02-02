using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private float minSwipeDistance = 0.3f;
    [SerializeField] private Board board;

    private Camera cam;
    private Vector3 dragStartWorld;
    private Tile selectedTile;
    private bool isDragging;
    private GameManager gameManager;

    private void Start()
    {
        cam = Camera.main;
        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        if (gameManager.State != GameManager.GameState.Playing) return;
        if (board.IsBusy) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)        
            BeginDrag(mouse.position.ReadValue());
        else if (mouse.leftButton.wasReleasedThisFrame && isDragging)        
            EndDrag(mouse.position.ReadValue());
        

        // Also support touch
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            TouchControl touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
                BeginDrag(touch.position.ReadValue());
        }
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame && isDragging)
            EndDrag(Touchscreen.current.primaryTouch.position.ReadValue());
    }

    private void BeginDrag(Vector2 screenPos)
    {
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        worldPos.z = 0f;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                selectedTile = tile;
                dragStartWorld = worldPos;
                isDragging = true;
            }
        }
    }

    private void EndDrag(Vector2 screenPos)
    {
        isDragging = false;
        if (selectedTile == null) return;

        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        worldPos.z = 0f;

        Vector3 delta = worldPos - dragStartWorld;
        if (delta.magnitude < minSwipeDistance)
        {
            selectedTile = null;
            return;
        }

        int dirCol = 0, dirRow = 0;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            dirCol = delta.x > 0 ? 1 : -1;
        else
            dirRow = delta.y > 0 ? 1 : -1;

        int targetCol = selectedTile.Col + dirCol;
        int targetRow = selectedTile.Row + dirRow;

        board.TrySwap(selectedTile.Col, selectedTile.Row, targetCol, targetRow);
        selectedTile = null;
    }
}
