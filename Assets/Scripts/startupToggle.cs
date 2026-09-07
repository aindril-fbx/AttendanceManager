using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class startupToggle : MonoBehaviour, IPointerClickHandler
{
    public bool currentSliderValue { get; private set; }

    [SerializeField] private Slider startupAnimationToggle;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve sliderEase =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine animationCoroutine;

    private void Start()
    {
        startupAnimationToggle = GetComponent<Slider>();
        startupAnimationToggle.interactable = false;
        startupAnimationToggle.transition = Selectable.Transition.None;

        startupAnimationToggle.value =
            PlayerPrefs.GetFloat("startup", 1f);

        currentSliderValue = startupAnimationToggle.value >= 0.5f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

    private void Toggle()
    {
        currentSliderValue = !currentSliderValue;

        PlayerPrefs.SetFloat(
            "startup",
            currentSliderValue ? 1f : 0f
        );

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        float targetValue = currentSliderValue ? 1f : 0f;

        animationCoroutine = StartCoroutine(
            AnimateSlider(targetValue)
        );
    }

    private IEnumerator AnimateSlider(float targetValue)
    {
        float startingValue = startupAnimationToggle.value;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / animationDuration;
            t = sliderEase.Evaluate(t);

            startupAnimationToggle.value =
                Mathf.Lerp(startingValue, targetValue, t);

            yield return null;
        }
        startupAnimationToggle.value = targetValue;

        animationCoroutine = null;
    }
}
