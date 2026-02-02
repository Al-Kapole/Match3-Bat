using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public int FruitType;
    public int Row;
    public int Col;

    public Coroutine MoveTo(Vector3 target, float duration)
    {
        return StartCoroutine(MoveCoroutine(target, duration));
    }

    private IEnumerator MoveCoroutine(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.position = target;
    }
}
