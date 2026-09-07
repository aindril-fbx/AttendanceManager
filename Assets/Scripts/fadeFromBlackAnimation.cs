using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FadeFromBlackAnimation : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float delay = 0.3f;

    IEnumerator Start()
    {
        if (PlayerPrefs.GetFloat("startup", 1f) == 0f)
        {
            Destroy(canvasGroup.gameObject);
            yield break;
        }

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        yield return new WaitForSeconds(delay);
        LeanTween.alphaCanvas(canvasGroup, 0f, duration)
                 .setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
                 {
                     Destroy(canvasGroup.gameObject);
                 });
    }
}
