using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorNormal;
    [SerializeField] private Texture2D cursorShoot;
    [SerializeField] private Texture2D cursorReload;
    private Vector2 hotspot = new Vector2(16, 16);

    [SerializeField] private Pistol pistol;

    private bool wasReloading = false; // nhớ trạng thái reload trước đó

    void Start()
    {
        if (pistol == null)
        {
            Debug.LogError("Pistol is not assigned!");
            return;
        }
        Cursor.SetCursor(cursorNormal, hotspot, CursorMode.Auto);
    }

    void Update()
    {
        if (pistol == null) return;

        // Nếu đang reload
        if (pistol.isReloading)
        {
            Cursor.SetCursor(cursorReload, hotspot, CursorMode.Auto);
            wasReloading = true;
            return;
        }

        // Nếu vừa kết thúc reload thì reset về cursorNormal
        if (wasReloading && !pistol.isReloading)
        {
            Cursor.SetCursor(cursorNormal, hotspot, CursorMode.Auto);
            wasReloading = false;
        }

        // Bình thường: đổi theo click chuột
        if (Input.GetMouseButtonDown(0))
            Cursor.SetCursor(cursorShoot, hotspot, CursorMode.Auto);
        else if (Input.GetMouseButtonUp(0))
            Cursor.SetCursor(cursorNormal, hotspot, CursorMode.Auto);
    }
}
