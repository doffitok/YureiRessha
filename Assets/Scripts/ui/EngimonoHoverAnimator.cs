using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;

[DisallowMultipleComponent]
public class EngimonoHoverAnimator : MonoBehaviour
{
    [Header("Animación Hover (pop-out)")]
    [SerializeField] private float hoverGrowScale = 1.12f;
    [SerializeField] private float hoverSettleScale = 1.05f;
    [SerializeField] private float hoverGrowSpeed = 0.08f;
    [SerializeField] private float hoverSettleSpeed = 0.08f;
    [SerializeField] private float hoverReturnSpeed = 0.08f;

    [Header("Animación de aparición inicial")]
    [SerializeField] private float spawnPhase1Speed = 0.08f;
    [SerializeField] private float spawnPhase2Speed = 0.08f;
    [SerializeField] private float spawnPhase1Scale = 1.40f;
    [SerializeField] private float spawnPhase2Scale = 0.80f;

    [Header("Depuración")]
    [SerializeField] private bool debugLogs = false;

    private GameObject currentHovered;
    private Dictionary<GameObject, Vector3> baseScales = new Dictionary<GameObject, Vector3>();

    private void Start()
    {
        // Animación inicial para todos los engimonos visibles
        foreach (var rect in FindObjectsOfType<RectTransform>())
        {
            if (rect.GetComponent<EngimonoShopItem>() || rect.GetComponent<InventoryItemUI>())
            {
                baseScales[rect.gameObject] = rect.localScale;
                StartCoroutine(SpawnPopInAnimation(rect));
            }
        }
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        GameObject hovered = null;
        foreach (var r in results)
        {
            if (r.gameObject.GetComponent<EngimonoShopItem>() ||
                r.gameObject.GetComponent<InventoryItemUI>())
            {
                hovered = r.gameObject;
                break;
            }
        }

        if (hovered != currentHovered)
        {
            if (currentHovered != null)
                StartCoroutine(AnimateReturn(currentHovered));

            if (hovered != null)
                StartCoroutine(AnimateHover(hovered));

            currentHovered = hovered;
        }
    }

    private IEnumerator AnimateHover(GameObject target)
    {
        var rect = target.GetComponent<RectTransform>();
        if (rect == null) yield break;

        if (!baseScales.ContainsKey(target))
            baseScales[target] = rect.localScale;

        Vector3 baseScale = baseScales[target];
        Vector3 grow = baseScale * hoverGrowScale;
        Vector3 settle = baseScale * hoverSettleScale;

        if (debugLogs) Debug.Log($"[HoverAnimator] Hover enter → {target.name}");

        yield return AnimateScale(rect, baseScale, grow, hoverGrowSpeed);
        yield return AnimateScale(rect, grow, settle, hoverSettleSpeed);
    }

    private IEnumerator AnimateReturn(GameObject target)
    {
        var rect = target != null ? target.GetComponent<RectTransform>() : null;
        if (rect == null) yield break;

        if (!baseScales.ContainsKey(target))
            baseScales[target] = rect.localScale;

        Vector3 baseScale = baseScales[target];
        yield return AnimateScale(rect, rect.localScale, baseScale, hoverReturnSpeed);

        if (debugLogs) Debug.Log($"[HoverAnimator] Hover exit → {target.name}");
    }

    private IEnumerator SpawnPopInAnimation(RectTransform rect)
    {
        if (rect == null) yield break;

        if (!baseScales.ContainsKey(rect.gameObject))
            baseScales[rect.gameObject] = rect.localScale;

        Vector3 baseScale = baseScales[rect.gameObject];
        rect.localScale = baseScale * spawnPhase1Scale;

        yield return AnimateScale(rect, baseScale * spawnPhase1Scale, baseScale * spawnPhase2Scale, spawnPhase1Speed);
        yield return AnimateScale(rect, baseScale * spawnPhase2Scale, baseScale, spawnPhase2Speed);
    }

    private IEnumerator AnimateScale(RectTransform target, Vector3 from, Vector3 to, float duration)
    {
        if (target == null || duration <= 0f) yield break;

        float t = 0f;
        while (t < 1f)
        {
            if (target == null) yield break;
            t += Time.deltaTime / duration;
            float s = Mathf.SmoothStep(0f, 1f, t);
            target.localScale = Vector3.Lerp(from, to, s);
            yield return null;
        }
        if (target != null)
            target.localScale = to;
    }
}