using UnityEngine;
using System;
using System.IO;
using System.Data;
using ExcelDataReader;

public class TimeTable : MonoBehaviour
{
    void Start()
    {
        ReadTodaysFirstClass();
    }

    void ReadTodaysFirstClass()
    {
        string filePath = Path.Combine(
            Application.streamingAssetsPath,
            "TIME TABLE JULYTODEC2026.xlsx"
        );

        if (!File.Exists(filePath))
        {
            Debug.LogError("Excel file not found: " + filePath);
            return;
        }

        using (FileStream stream = File.Open(
            filePath,
            FileMode.Open,
            FileAccess.Read))
        {
            using (IExcelDataReader reader =
                   ExcelReaderFactory.CreateReader(stream))
            {
                DataSet dataSet = reader.AsDataSet();

                // Get 3rd Year A sheet
                DataTable sheet = dataSet.Tables["3RD YEAR A"];

                if (sheet == null)
                {
                    Debug.LogError("Could not find '3RD YEAR A' sheet.");
                    return;
                }

                Debug.Log("Excel loaded successfully!");

                // Find the 3F32 column
                int sectionColumn = Find3F32Column(sheet);

                if (sectionColumn == -1)
                {
                    Debug.LogError("Could not find 3F32!");
                    return;
                }

                Debug.Log("3F32 found at column " + sectionColumn);

                // For now, print today's day
                string today = DateTime.Now.DayOfWeek.ToString();

                Debug.Log("Today is: " + today);

                // Search today's timetable
                FindFirstClass(sheet, sectionColumn, today);
            }
        }
    }

    int Find3F32Column(DataTable sheet)
    {
        for (int column = 0; column < sheet.Columns.Count; column++)
        {
            for (int row = 0; row < Mathf.Min(10, sheet.Rows.Count); row++)
            {
                string value = sheet.Rows[row][column]?.ToString().Trim();

                if (value == "3F32")
                {
                    return column;
                }
            }
        }

        return -1;
    }

    void FindFirstClass(
        DataTable sheet,
        int sectionColumn,
        string today)
    {
        Debug.Log("========== FINDING TODAY'S FIRST CLASS ==========");

        // The timetable uses two rows per period:
        //
        // Row A = subject
        // Row B = room/faculty/details
        //
        // The day is represented by letters in the first columns:
        // M O N D A Y
        //
        // We convert today's name into the letters we need.

        string dayLetters = today.ToUpper();

        // We only want the first occurrence of the day.
        // For Monday this will search for M O N D A Y.

        int dayLetterIndex = 0;

        for (int row = 0; row < sheet.Rows.Count; row++)
        {
            // Check the first column for the day letter
            string dayCell = GetCell(sheet, row, 0);

            if (string.IsNullOrEmpty(dayCell))
                continue;

            dayCell = dayCell.ToUpper();

            // Is this the next letter of today's day?
            if (dayLetterIndex < dayLetters.Length &&
                dayCell == dayLetters[dayLetterIndex].ToString())
            {
                dayLetterIndex++;

                Debug.Log(
                    "Found " + dayLetters[dayLetterIndex - 1] +
                    " for " + today +
                    " at row " + row
                );

                // The actual class is on the row immediately ABOVE
                int classRow = row - 1;

                string subject =
                    GetCell(sheet, classRow, sectionColumn);

                // Ignore empty periods
                if (!string.IsNullOrEmpty(subject))
                {
                    string room =
                        GetCell(sheet, row, sectionColumn);

                    string faculty =
                        GetCell(sheet, row + 2, sectionColumn);

                    Debug.Log("==============================");
                    Debug.Log("TODAY'S FIRST CLASS");
                    Debug.Log("==============================");
                    Debug.Log("Day: " + today);
                    Debug.Log("Subject: " + subject);
                    Debug.Log("Room: " + room);
                    Debug.Log("Faculty: " + faculty);
                    Debug.Log("==============================");

                    return;
                }
            }
        }

        Debug.Log("No class found for " + today);
    }


    string GetCell(DataTable sheet, int row, int column)
    {
        if (row < 0 ||
            row >= sheet.Rows.Count ||
            column < 0 ||
            column >= sheet.Columns.Count)
        {
            return "";
        }

        object value = sheet.Rows[row][column];

        if (value == null)
            return "";

        return value.ToString().Trim();
    }
}
