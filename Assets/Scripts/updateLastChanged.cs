using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class updateLastChanged
{
    public static bool isUpdated = true;
    static List<string> records = new List<string>();
    static int recordCount = 0;
    
    public static void record(int present = 0, int type = 1, string name = "label")
    {
        recordLogData data = LoadRecordData();
        if(data != null)
        {
            records = data.records;
            recordCount = records.Count;
        }
        DateTime time = DateTime.Now;

        string hour = "";
        string minute = LeadingZero(time.Minute);
        string second = LeadingZero(time.Second);
        string AMPM = time.Hour > 12 ? "PM" : "AM";

        string day = LeadingZero(time.Day);
        string month = LeadingZero(time.Month);
        string year = LeadingZero(time.Year);
        string weekday = time.DayOfWeek.ToString();

        if (time.Hour > 12)
        {
            hour = LeadingZero(time.Hour - 12);
        }
        else
        {
            hour = LeadingZero(time.Hour);
        }

        PlayerPrefs.SetString("Date", day + "/" + month + "/" + year);
        PlayerPrefs.SetString("Time", hour + ":" + minute);
        PlayerPrefs.SetString("AMPM", AMPM);
        PlayerPrefs.SetString("Weekday", weekday);
        
        string recordLog = "_";
        string dateAndTime = "<color=#525252>" + PlayerPrefs.GetString("Date") + " " + PlayerPrefs.GetString("Time") + " " + PlayerPrefs.GetString("AMPM") + " " + PlayerPrefs.GetString("Weekday") + "</color>";

        string PresentAbsent = present == 1 ? "<color=#FF2B00>Present</color><color=#525252> --></color> " : "<color=#FF2B00>Absent </color><color=#525252> --></color> ";
        int maxCharacters = 15;

        if (name.Length > maxCharacters)
        {
            name = name.Substring(0, maxCharacters);
            name = name + "...";
        }

        switch (type)
        {
            case 1:
                recordLog = PresentAbsent + name + " lecture \n" + dateAndTime;   
                break;
            case 2:
                recordLog = PresentAbsent + name + " lab \n" + dateAndTime;  
                break;
            case 3:
                recordLog = PresentAbsent + name + " tutorial \n" + dateAndTime;  
                break;
            default:
                Debug.Log("Invalid type selected.");
                break;
        }
        if(present == -1)
        {
            recordLog = "Deleted -> " + name + "\n" + dateAndTime;
        }
        if(present == -2)
        {
            recordLog = "Undo" + "\n" + dateAndTime;
        }
        Debug.Log(recordLog);
        records.Add(recordLog);
        if(records.Count > 15)
        {
            records.RemoveAt(0);
        }
        isUpdated = true;
        SaveData();
    }

    private static string LeadingZero(int n)
    {
        return n.ToString().PadLeft(2, '0');
    }

    static void SaveData()
    {
        List<string> dict = new List<string>();
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + "logData" + ".lol";
        FileStream stream = new FileStream(path, FileMode.Create);
        recordLogData data = new recordLogData(records);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    static recordLogData LoadRecordData()
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

