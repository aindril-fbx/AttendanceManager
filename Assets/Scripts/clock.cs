using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class clock : MonoBehaviour
{
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private TMP_Text dateText;

    List<string> records = new List<string>();
    [SerializeField] private RectTransform logRt;
    [SerializeField] private Button expandButton;
    [SerializeField] private float offset = 205f;
    [SerializeField] private float heightOffset = 100f;
    [SerializeField] private float interpolationSpeed = 0.1f;
    bool viewing = false;

    private void OnEnable() {
        ChangeDate();
        expandButton.onClick.AddListener(changeViewingMode);
    }

    void changeViewingMode()
    {
        viewing = !viewing;
    }
    private void Update()
    {
        if (updateLastChanged.isUpdated)
        {
            updateLastChanged.isUpdated = false;
            ChangeDate();
        }
        if (viewing)
        {
            logRt.sizeDelta = Vector2.Lerp(new Vector2(logRt.sizeDelta.x, logRt.sizeDelta.y), new Vector2(logRt.sizeDelta.x, offset + ((records.Count + 1) * heightOffset)), interpolationSpeed);
        }
        else
        {
            logRt.sizeDelta = Vector2.Lerp(new Vector2(logRt.sizeDelta.x, logRt.sizeDelta.y), new Vector2(logRt.sizeDelta.x, offset), interpolationSpeed);
        }
    }


    private void ChangeDate()
    {
        recordLogData recordData = LoadRecordData();
        if(recordData != null)
        {
            records = recordData.records;
        }

        DateTime time = DateTime.Now;
        //clockText.text = hour + ":" + minute + "  " + AMPM;
        dateText.text = "";
        for (int i = 0; i < records.Count; i++)
        {
            dateText.text = records[i] + "\n\n" + dateText.text;
        }
    }

    string LeadingZero(int n)
    {
        return n.ToString().PadLeft(2, '0');
    }

    recordLogData LoadRecordData()
    {
        string path = Application.persistentDataPath + "/" + "logData" + ".lol";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            recordLogData data = formatter.Deserialize(stream) as recordLogData;
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("Save File not found in " + path);
            return null;
        }
    }
}