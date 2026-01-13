using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class recordLogData
{
    public List<string> records = new List<string>();

    public recordLogData(List<string> recordData)
    {
        if(recordData != null)
        {
            records = recordData;
        }
    }

}