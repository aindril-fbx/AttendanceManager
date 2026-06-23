using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;


public class semesterCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int semesterIndex;
    [SerializeField]
    private Dictionary<int, string> romanNumerals = new Dictionary<int, string>
    {
        { 1, "I" },
        { 2, "II" },
        { 3, "III" },
        { 4, "IV" },
        { 5, "V" },
        { 6, "VI" },
        { 7, "VII" },
        { 8, "VIII" },
        { 9, "IX" },
        { 10, "X" },
        { 11, "XI" },
        { 12, "XII" },
        { 13, "XIII" },
        { 14, "XIV" },
        { 15, "XV" },
        { 16, "XVI" },
        { 17, "XVII" },
        { 18, "XVIII" },
        { 19, "XIX" },
        { 20, "XX" },
        { 21, "XXI" },
        { 22, "XXII" },
        { 23, "XXIII" },
        { 24, "XXIV" }
    };
    public float credits;
    public float creditsEarned;
    public float SGPA;
    public Dictionary<string, float[]> subjectInfo = new Dictionary<string, float[]>();
    [SerializeField] private int num_Subjects;
    private RectTransform rt;

    [SerializeField] private Button editButton;
    [SerializeField] private Button addButton;
    public bool isEditing = false;
    private float offset = 205f; // Offset for the Height of the semester card
    [SerializeField] private float heightOffset = 100f; // Height offset for each subject

    [SerializeField] private float interpolationSpeed = 0.1f; // Speed of height adjustment

    [SerializeField] private TMP_InputField newSubjectNameText;
    [SerializeField] private TMP_InputField newSubjectCreditText;

    [SerializeField] private TextMeshProUGUI semesterCardIndexText;
    [field: SerializeField]
    public TMP_InputField sgpaDisplayText { get; private set; }
    [field: SerializeField]
    public TextMeshProUGUI creditsDisplayText {get; private set;}
    [SerializeField] private TextMeshProUGUI pointsDisplayText;
    [SerializeField] private TextMeshProUGUI subjectCountText;

    [SerializeField] private GameObject subjectCardPrefab;
    [SerializeField] private GameObject subjectCardParent;

    public SemesterManager SM;

    void Start()
    {
        editButton.onClick.AddListener(OnEditButtonClick);
        addButton.onClick.AddListener(GetNewSubjectInput);
        sgpaDisplayText.onEndEdit.AddListener((value) => {
            if(float.TryParse(sgpaDisplayText.text, out float x)){
                SGPA = x;
                creditsEarned = x*credits;
                creditsDisplayText.text = credits.ToString("F1");
                pointsDisplayText.text = creditsEarned.ToString("F1");
            }
            SM.updateSemesterData(semesterIndex, subjectInfo);
            SM.SaveData();
        });
        rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, offset); // Set initial height of the semester card
        calculateSGPA(); // Calculate SGPA on start

        newSubjectNameText.onSubmit.AddListener((text) => newSubjectCreditText.ActivateInputField()); // Move focus to credit input field on subject name submit
        newSubjectCreditText.onSubmit.AddListener((text) => GetNewSubjectInput()); // Call GetNewSubjectInput on credit input field submit
    }

    void GetNewSubjectInput()
    {
        string subjectName = newSubjectNameText.text.Trim();
        string subjectCredit = newSubjectCreditText.text.Trim();
        if (!string.IsNullOrEmpty(subjectName) && !string.IsNullOrEmpty(subjectCredit) && float.TryParse(subjectCredit, out float creditValue))
        {
            AddSubject(subjectName, new float[] { creditValue, 0f });
        }
        Vibrator.Vibrate(50); // Vibrate for 50 milliseconds
    }

    [SerializeField]
    private Dictionary<float, string> gradeInfo = new Dictionary<float, string>
    {
        { 10, "A" },
        { 9, "A-" },
        { 8, "B" },
        { 7, "B-" },
        { 6, "C" },
        { 5, "C-" },
        { 2, "E" },
        { 0, "F" },
        { -1, "I" }, // I for incomplete
        { -2, "X" } // X for absent
    };

    public void updateSubjects()
    {
        foreach (var subject in subjectInfo)
        {
            GameObject newSubjectCard = Instantiate(subjectCardPrefab, subjectCardParent.transform);
            subjectCard newCard = newSubjectCard.GetComponent<subjectCard>();
            newCard.subjectName = subject.Key;
            newCard.subjectCredit = subject.Value[0];
            newCard.parentSemesterCard = this; // Set the parent semester card reference
            newCard.gradeInputField.text = gradeInfo[subject.Value[1]]; // Set the initial grade value
            newCard.OnEnable(); // Initialize the subject card
        }
        calculateSGPA(); // Recalculate SGPA after updating subjects
    }

    void OnEditButtonClick()
    {
        // Logic to handle editing the semester card
        // This could open a UI panel or allow the user to edit the credits and subjects
        num_Subjects = subjectInfo.Count;
        Debug.Log("Number of subjects: " + num_Subjects);
        isEditing = !isEditing; // Toggle editing state
        SM.currentEditingSemesterIndex = semesterIndex; // Update the current editing semester index in the SemesterManager

        SM.logSemesters(); // Log the current semesters for debugging
        SM.SaveData(); // Save the semester data when editing starts or stops
        Vibrator.Vibrate(75); // Vibrate for 50 milliseconds
    }

    [SerializeField] private float holdDuration = 1f; // Duration to hold the card before deletion
    private float holdTimer = 0f; // Timer to track hold duration
    public bool isHolding = false; // Flag to check if the card is being held

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isEditing) return; // Prevent holding if in editing mode
        isHolding = true;
        holdTimer = 0f;
        //Debug.Log("Pointer down on semester card: " + semesterIndex);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        holdTimer = 0f;
        SM.indicatorValue = 0; // Reset the indicator value when not holding
    }

    private void Update()
    {
        if (isEditing)
        {
            // Adjust the height of the semester card based on the number of subjects
            rt.sizeDelta = Vector2.Lerp(new Vector2(rt.sizeDelta.x, rt.sizeDelta.y), new Vector2(rt.sizeDelta.x, offset + ((num_Subjects + 1) * heightOffset)), interpolationSpeed); // Assuming each subject adds 50 units to the height
            calculateSGPA(); // Calculate SGPA whenever editing
        }
        else
        {
            rt.sizeDelta = Vector2.Lerp(new Vector2(rt.sizeDelta.x, rt.sizeDelta.y), new Vector2(rt.sizeDelta.x, offset), interpolationSpeed); // Reset to default height when not editing
        }

        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            SM.indicatorValue = returnHoldRatio(); // Update the indicator value in SemesterManager
            if (holdTimer >= holdDuration)
            {
                OnHoldComplete();
                holdTimer = 0f;
            }
        }
        string subjectName = newSubjectNameText.text;
        string subjectCredit = newSubjectCreditText.text;
        if (string.IsNullOrEmpty(subjectName) || string.IsNullOrEmpty(subjectCredit) || !float.TryParse(subjectCredit, out float creditValue))
        {
            addButton.interactable = false; // Disable the button if input is invalid
        }
        else
        {
            addButton.interactable = true; // Enable the button if input is valid
        }
    }

    private void OnHoldComplete()
    {
        SM.indicatorValue = 0; // Reset the indicator value when not holding
        SM.currentEditingSemesterIndex = -1; // Reset the current editing semester index in the SemesterManager
        SM.RemoveSemesterCard(semesterIndex); // Call the SemesterManager to remove this semester card
    }

    public int returnHoldRatio()
    {
        return (int)((holdTimer / holdDuration) * 32f);
    }

    void AddSubject(string subjectName, float[] subjectData)
    {
        Vibrator.Vibrate(50); // Vibrate for 50 milliseconds
        if (!subjectInfo.ContainsKey(subjectName))
        {
            subjectInfo[subjectName] = subjectData;
            num_Subjects++;
            Debug.Log("Added subject: " + subjectName);

            newSubjectNameText.text = ""; // Clear the input field after adding
            newSubjectCreditText.text = ""; // Clear the input field after adding

            GameObject newSubjectCard = Instantiate(subjectCardPrefab, subjectCardParent.transform);
            subjectCard newCard = newSubjectCard.GetComponent<subjectCard>();
            newCard.subjectName = subjectName;
            newCard.subjectCredit = subjectData[0];
            newCard.parentSemesterCard = this; // Set the parent semester card reference
        }
        else
        {
            Debug.LogWarning("Subject already exists: " + subjectName);
        }
        Debug.Log("Current subjects: " + string.Join(", ", subjectInfo.Keys));
    }

    public void RemoveSubject(string subjectName)
    {
        if (subjectInfo.ContainsKey(subjectName))
        {
            subjectInfo.Remove(subjectName);
            num_Subjects--;
            Debug.Log("Removed subject: " + subjectName);
        }
        else
        {
            Debug.LogWarning("Subject not found: " + subjectName);
        }
    }

    void calculateSGPA()
    {
        float totalCredits = 0f;
        float totalPoints = 0f;

        foreach (var subject in subjectInfo)
        {
            float credit = subject.Value[0];
            float grade = subject.Value[1];
            if (grade < 0) continue; // Skip invalid grades
            totalCredits += credit;
            totalPoints += credit * grade; // Assuming grade is a numeric value
        }

        if (totalCredits > 0)
        {
            float sgpa = totalPoints / totalCredits;
            sgpa = Mathf.Floor(sgpa * 100f) / 100f; // Round SGPA to 2 decimal places
            sgpaDisplayText.text = sgpa.ToString("F2");
        }
        else
        {
            sgpaDisplayText.text = "<color=#3D3D3D>--</color>"; // Display placeholder if no credits
        }

        credits = totalCredits; // Update the total credits for the semester card
        creditsEarned = totalPoints; // Update the total points earned for the semester card
        SGPA = totalCredits > 0 ? totalPoints / totalCredits : 0f; // Calculate SGPA if credits are greater than zero

        creditsDisplayText.text = credits.ToString("F1");
        pointsDisplayText.text = creditsEarned.ToString("F1");
        subjectCountText.text = subjectInfo.Count + ""; // Update the subjects count

        semesterCardIndexText.text = $"Semester <color=#FF0078>{romanNumerals[semesterIndex]}</color>";

        if (SM != null)
        {
            SM.updateSemesterData(semesterIndex, subjectInfo); // Update the semester data in the SemesterManager
        }
    }

    public void updateSemesterIndex()
    {
        semesterCardIndexText.text = $"Semester <color=#FF0078>{romanNumerals[semesterIndex]}</color>";
    }
}
