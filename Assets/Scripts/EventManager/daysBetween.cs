using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

public class daysBetween : MonoBehaviour
{
    [SerializeField] private RectTransform rt;
    [SerializeField] private TextMeshProUGUI days_text;

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        UpdateDaysBetween();
    }

    public void UpdateDaysBetween()
    {
        int index = gameObject.transform.GetSiblingIndex();
        Transform previousEvent = transform.parent.GetChild(index - 1);
        Transform nextEvent = transform.parent.GetChild(index + 1);
        Debug.Log(index);
        DateTime date_prev = previousEvent.GetComponent<eventCard>().eventDateTime;
        DateTime date_next = nextEvent.GetComponent<eventCard>().eventDateTime;

        TimeSpan difference = date_next - date_prev;
        double daysBetween = difference.TotalDays;
        int daysBetweenI = daysBetween < 0.2 ? 0 : (int)Math.Ceiling(daysBetween);



        if (daysBetweenI <= 0)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, 0f);
            days_text.text = "";
        }
        else
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, (float)daysBetweenI * 50f);
            days_text.text = "> " + daysBetweenI.ToString() + " Days";
        }
    }

}


#if UNITY_EDITOR

[CustomEditor(typeof(daysBetween))]
public class daysBetweenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        daysBetween myScript = (daysBetween)target;
        if (GUILayout.Button("Update Days Between"))
        {
            myScript.UpdateDaysBetween();
        }
    }
}

#endif

