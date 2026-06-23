using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;

public class SemesterManager : MonoBehaviour
{
    [SerializeField] private GameObject semesterCardPrefab;
    [SerializeField] private Transform semesterCardParent;

    [SerializeField] private Button addSemesterButton;

    [SerializeField] private TextMeshProUGUI CGPAText;
    public Dictionary<int, Dictionary<string, float[]>> mainData = new Dictionary<int, Dictionary<string, float[]>>();
    [SerializeField] private List<semesterCard> semesterCards = new List<semesterCard>();

    public int currentEditingSemesterIndex = -1; // Index of the semester currently being edited

    [SerializeField] private TextMeshProUGUI indicatorText;
    public int indicatorValue = 0;

    private void Start()
    {
        addSemesterButton.onClick.AddListener(OnAddSemesterButtonClick);
        semesterData data = LoadData(); // Load the semester data
        if (data != null)
        {
            mainData = data.data; // Initialize mainData with loaded data
            Dictionary<int, Dictionary<string, float[]>> tempData = new Dictionary<int, Dictionary<string, float[]>>(mainData);
            foreach (var semester in tempData)
            {
                GameObject newSemesterCard = Instantiate(semesterCardPrefab, semesterCardParent);
                semesterCard semesterCardComponent = newSemesterCard.GetComponent<semesterCard>();
                semesterCards.Add(semesterCardComponent);
                if (semesterCardComponent != null)
                {
                    semesterCardComponent.SM = this; // Set the reference to this SemesterManager
                    semesterCardComponent.semesterIndex = semester.Key; // Set the index based on the key
                    semesterCardComponent.subjectInfo = semester.Value; // Set the subject info
                    semesterCardComponent.updateSemesterIndex(); // Update the index display
                    semesterCardComponent.updateSubjects(); // Update the subjects in the semester card
                }
            }
            tempData.Clear(); // Clear the temporary data to free memory
        }
        else
        {
            Debug.LogWarning("No data found, starting with an empty state.");
        }
    }

    bool holding = false;
    private void Update()
    {
        for (int i = 0; i < semesterCardParent.childCount; i++)
        {
            semesterCard card = semesterCardParent.GetChild(i).GetComponent<semesterCard>();
            if (card.isHolding)
            {
                Debug.Log("Holding semester card: " + card.semesterIndex);
                holding = true;
                break; // Exit loop once we find a holding card
            }
        }
        if (indicatorValue > 5 && holding)
        {
            indicatorText.text = "Deleting: |" + new string('>', indicatorValue) + new string(' ', 31 - indicatorValue) + "|"; // Update the indicator bar text
        }
        else
        {
            indicatorText.text = ""; // Clear the indicator text if not holding
        }
        holding = false; // Reset holding state for the next frame
    }

    private void OnAddSemesterButtonClick()
    {
        Vibrator.Vibrate(75); // Vibrate for 50 milliseconds
        int numberofSemesters = semesterCardParent.childCount; // Increment the number of semesters
        if (numberofSemesters >= 24) // Limit to 8 semesters
        {
            Debug.LogWarning("Maximum number of semesters reached.");
            return;
        }
        GameObject newSemesterCard = Instantiate(semesterCardPrefab, semesterCardParent);
        semesterCard semesterCardComponent = newSemesterCard.GetComponent<semesterCard>();
        semesterCards.Add(semesterCardComponent);
        if (semesterCardComponent != null)
        {
            semesterCardComponent.SM = this; // Set the reference to this SemesterManager
            semesterCardComponent.semesterIndex = semesterCardParent.childCount; // Set the index based on the number of existing cards
            mainData[semesterCardComponent.semesterIndex] = semesterCardComponent.subjectInfo; // Initialize the subject info for the new semester
        }
        SaveData(); // Save data before adding a new semester
    }

    private void OnLostFocus() {
        SaveData();
    }

    private void OnFocus() {
        SaveData();
    }

    private void OnApplicationQuit() {
        SaveData();
    }

    public void RemoveSemesterCard(int semesterIndex)
    {
        if (mainData.ContainsKey(semesterIndex))
        {
            mainData.Clear(); // Clear the main data dictionary
            Destroy(semesterCardParent.GetChild(semesterIndex - 1).gameObject); // Destroy the semester card game object
            semesterCards.RemoveAt(semesterIndex-1);
            for (int i = 0; i < semesterCardParent.childCount; i++)
            {
                semesterCard card = semesterCardParent.GetChild(i).GetComponent<semesterCard>();
                if (card.semesterIndex > semesterIndex)
                {
                    card.semesterIndex--; // Adjust the index of subsequent cards
                }
                card.updateSemesterIndex(); // Update the index display
                mainData[card.semesterIndex] = card.subjectInfo; // Reinitialize the main data with the remaining cards
            }
            Debug.Log("Removed semester card at index: " + semesterIndex);
            UpdateCGPA(); // Recalculate CGPA after removing a semester
            SaveData(); // Save data after removing a semester
            logSemesters(); // Log the current semesters for debugging
        }
    }

    public void logSemesters()
    {
        foreach (var semester in mainData)
        {
            Debug.Log($"Semester {semester.Key}:");
            foreach (var subject in semester.Value)
            {
                Debug.Log($"  Subject: {subject.Key}, Credits: {subject.Value[0]}, Grade Points: {subject.Value[1]}");
            }
        }
    }

    public void updateSemesterData(int semesterIndex, Dictionary<string, float[]> subjects)
    {
        if (mainData.ContainsKey(semesterIndex))
        {
            mainData[semesterIndex] = subjects; // Update the subject info for the specified semester
        }
        else
        {
            mainData.Add(semesterIndex, subjects); // Add a new entry if it doesn't exist
        }
        UpdateCGPA(); // Recalculate CGPA after updating semester data
    }

    public void UpdateCGPA()
    {
        float totalCredits = 0f;
        float totalPoints = 0f;

        /*
        foreach (var semester in mainData)
        {
            foreach (var subject in semester.Value)
            {
                float[] subjectData = subject.Value;
                if(subjectData[1] < 0) continue; // Skip invalid grades
                totalCredits += subjectData[0]; // Credits
                totalPoints += subjectData[0] * subjectData[1]; // Credits * Grade Points
            }
        }
        */

        for(int i = 0; i<semesterCards.Count; i++){
            if(semesterCards[i].credits <= 0f){
                continue;
            }
            totalCredits += semesterCards[i].credits;
            totalPoints += semesterCards[i].creditsEarned;
        }

        if (totalCredits > 0)
        {
            float cgpa = totalPoints / totalCredits;
            cgpa = Mathf.Floor(cgpa * 100f) / 100f; // Truncate to 2 decimals

            CGPAText.text = cgpa.ToString("F2");
        }
        else
        {
            CGPAText.text = "#.##";
        }

        for (int i = 0; i < semesterCardParent.childCount; i++)
        {
            semesterCard semesterCardComponent = semesterCardParent.GetChild(i).GetComponent<semesterCard>();
            if (currentEditingSemesterIndex - 1 != i)
            {
                semesterCardComponent.isEditing = false;
            }
        }
    }

    public void SaveData()
    {
        Debug.Log("Saving semester data...");
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + "CGPAdata" + ".lol";
        FileStream stream = new FileStream(path, FileMode.Create);
        semesterData data = new semesterData(mainData);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    semesterData LoadData()
    {
        string path = Application.persistentDataPath + "/" + "CGPAdata" + ".lol";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            semesterData data = formatter.Deserialize(stream) as semesterData;
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
