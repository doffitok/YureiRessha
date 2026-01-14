using UnityEngine;

public class FloatingEngimono : MonoBehaviour
{
    [Header("Movimiento flotante")]
    public float moveAmountX = 10f; // cuántos píxeles/unidades se mueve en X
    public float moveAmountY = 8f;  // cuántos píxeles/unidades se mueve en Y
    public float moveSpeedX = 1f;   // velocidad del movimiento en X
    public float moveSpeedY = 1.3f; // velocidad del movimiento en Y

    private Vector3 startPos;
    private RectTransform rectTransform;
    private bool isUI;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        isUI = rectTransform != null;
        startPos = isUI ? rectTransform.anchoredPosition : transform.localPosition;
    }

    void Update()
    {
        float offsetX = Mathf.Sin(Time.time * moveSpeedX) * moveAmountX;
        float offsetY = Mathf.Cos(Time.time * moveSpeedY) * moveAmountY;
        Vector3 offset = new Vector3(offsetX, offsetY, 0f);

        if (isUI)
            rectTransform.anchoredPosition = startPos + offset;
        else
            transform.localPosition = startPos + offset;
    }
}
