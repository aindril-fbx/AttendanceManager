using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TextMeshProUGUI nameText;
    [Header("Flag")]
    [SerializeField] private Shadow[] outline;
    [SerializeField] private Color[] outlineColors;
    [Space]
    [Header("Text Objects")]
    [SerializeField] private TextMeshProUGUI currentTypeText;
    [SerializeField] private string[] currentTypeTexts = { "Lecture", "Lab", "Tutorial" };
    [SerializeField] private TextMeshProUGUI lectAmtText;
    [SerializeField] private TextMeshProUGUI labAmtText;
    [SerializeField] private TextMeshProUGUI tutAmtText;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private TextMeshProUGUI lectureSkip;
    [Header("Emoji")]
    [SerializeField] private Image emojiImage;
    [SerializeField] private Sprite[] emojiSprites = new Sprite[3];
    [Space]
    [Header("Buttons")]
    [SerializeField] private Button[] typeButton;
    [SerializeField] private GameObject undo;
    [SerializeField] private Button presentButton;
    [SerializeField] private Button absentButton;
    [Space]
    [Header("Card Data")]
    public string cardName;
    public int lectAmt = 0, lectTotal = 0;
    public int labAmt = 0, labTotal = 0;
    public int tutAmt = 0, tutTotal = 0;
    [Space]
    [Header("Rollback")]
    [SerializeField] private Stack<int> cmds = new Stack<int>();
    [SerializeField] private Stack<int> actions = new Stack<int>();

    Vector2 labelPosition = new Vector2(0, 0);
    public buttonHold BH;

    [SerializeField] private bool dummyCard;

    private void OnEnable()
    {
        for (int i = 0; i < typeButton.Length; i++)
        {
            typeButton[i].onClick.AddListener(OnTypeButtonClick);
        }
        presentButton.onClick.AddListener(OnPresentButtonClick);
        absentButton.onClick.AddListener(OnAbsentButtonClick);

        labelPosition = nameText.transform.localPosition;
        StartCoroutine(StartRoutine());
    }

    IEnumerator StartRoutine()
    {
        yield return new WaitForSeconds(0.3f);
        cardData cardstats = LoadData();
        if (cardstats != null && cardstats.data.ContainsKey(cardName))
        {
            lectAmt = cardstats.data[cardName]["LectureA"];
            lectTotal = cardstats.data[cardName]["LectureT"];
            labAmt = cardstats.data[cardName]["LabA"];
            labTotal = cardstats.data[cardName]["LabT"];
            tutAmt = cardstats.data[cardName]["TutorialA"];
            tutTotal = cardstats.data[cardName]["TutorialT"];
        }
        updateState();
    }

    private void OnDisable()
    {
        //remove listeners
        for (int i = 0; i < typeButton.Length; i++)
        {
            typeButton[i].onClick.RemoveListener(OnTypeButtonClick);
        }
        presentButton.onClick.RemoveListener(OnPresentButtonClick);
        absentButton.onClick.RemoveListener(OnAbsentButtonClick);
    }


    float x = 50f;
    float maxTime = 2f;
    private void FixedUpdate()
    {
        // Update the card's state or perform any other actions here if needed
        if (nameText.text != cardName) nameText.text = cardName;
        if (isHolding)
        {
            x = Mathf.Lerp(x, 230f, Time.deltaTime * 15f);
            nameText.transform.localPosition += Random.Range(-x, x) * Vector3.right * Time.deltaTime;
            holdTimer += Time.deltaTime;
            if (holdTimer >= maxTime)
            {
                OnHoldComplete();
                holdTimer = 0f;
            }
        }
        else
        {
            x = 0f;
            nameText.transform.localPosition = labelPosition;
        }
    }

    int currentType = 0;
    int changeType()
    {
        currentType++;
        if (currentType > 3)
        {
            currentType = 0;
        }
        Debug.Log(currentType);
        updateState();

        return currentType;
    }

    [SerializeField] private float percentage = 0f;
    [SerializeField] private float totalPercentage = 76f;

    void CalculateSkips()
    {
        int currentLectures = lectAmt;
        int currentTotalLectures = lectTotal;
        float currentLectPerctenage = (currentLectures/ (float)currentTotalLectures) * 100f;
        if (currentLectPerctenage < 75f)
        {
            lectureSkip.text = "Lecture Skips: 0";
            return; // No skips if percentage is below 75%
        }

        while (currentLectPerctenage > 75f)
        {
            currentTotalLectures++;
            currentLectPerctenage = (currentLectures / (float)currentTotalLectures) * 100f;
            if (currentLectPerctenage < 75f)
            {
                currentTotalLectures--; // Decrease total lectures if percentage drops below 75%
                break; // Stop if percentage drops below 75%
            }
        }
        lectureSkip.text = $"Lecture Skips: <color=#FF2B00>{(currentTotalLectures - lectTotal).ToString()}</color>";
    }
    void updateState()
    {
        nameText.text = cardName;
        // Update the card's state based on the current values of lectAmt, labAmt, and tutAmt
        lectAmtText.text = lectAmt.ToString() + "/" + lectTotal.ToString();
        labAmtText.text = labAmt.ToString() + "/" + labTotal.ToString();
        tutAmtText.text = tutAmt.ToString() + "/" + tutTotal.ToString();

        // Update the outline color based on the current type
        for (int i = 0; i < outline.Length; i++)
        {
            outline[i].effectColor = outlineColors[currentType];
        }

        // Update the current type text based on the current type
        currentTypeText.text = currentTypeTexts[currentType];

        // Calculate the percentage and update the percentage text
        float lectPercentage = 0f;
        float labPercentage = 0f;
        float tutPercentage = 0f;

        if (lectTotal != 0) lectPercentage = (lectAmt / (float)lectTotal) * 100f;
        if (labTotal != 0) labPercentage = (labAmt / (float)labTotal) * 100f;
        if (tutTotal != 0) tutPercentage = (tutAmt / (float)tutTotal) * 100f;

        //float[] percentages = new float[3] { lectPercentage, labPercentage, tutPercentage };
        float[] totals = new float[3] { lectTotal, labTotal, tutTotal };
        int count = 0;
        for (int i = 0; i < totals.Length; i++)
        {
            if (totals[i] > 0f) count++;
        }
        totalPercentage = (lectPercentage + labPercentage + tutPercentage) / (float)count;
        // Calculate the total and amount based on the current type
        switch (currentType)
        {
            case 0:
                percentage = totalPercentage;
                break;
            case 1:
                percentage = lectPercentage;
                break;
            case 2:
                percentage = labPercentage;
                break;
            case 3:
                percentage = tutPercentage;
                break;
            default:
                Debug.Log("Invalid type selected.");
                break;
        }
        // Calculate the percentage
        if (percentage > 0)
        {
            percentageText.text = percentage.ToString("F2") + "%";
        }
        else
        {
            percentageText.text = "0%";
        }
        if (totalPercentage <= 20f && totalPercentage > 0f)
        {
            emojiImage.gameObject.SetActive(true);
            emojiImage.sprite = emojiSprites[1];
        }
        else if (totalPercentage > 20f && totalPercentage < 75f)
        {
            emojiImage.gameObject.SetActive(true);
            emojiImage.sprite = emojiSprites[0];
        }
        else
        {
            emojiImage.gameObject.SetActive(false);
        }

        if (currentType == 0)
        {
            presentButton.interactable = false;
            absentButton.interactable = false;
        }
        else
        {
            presentButton.interactable = true;
            absentButton.interactable = true;
        }

        if (cmds.Count > 0)
        {
            undo.SetActive(true);
        }
        else
        {
            undo.SetActive(false);
        }

        CalculateSkips();
        SaveData();
    }

    public void Rollback()
    {
        if (cmds.Count > 0)
        {
            int type = cmds.Pop();
            int action = actions.Pop();
            switch (action)
            {
                case 1:
                    switch (type)
                    {
                        case 1:
                            lectAmt--;
                            lectTotal--;
                            break;
                        case 2:
                            labAmt--;
                            labTotal--;
                            break;
                        case 3:
                            tutAmt--;
                            tutTotal--;
                            break;
                        default:
                            Debug.Log("Invalid type selected.");
                            break;
                    }
                    break;
                case 0:
                    switch (type)
                    {
                        case 1:
                            lectTotal--;
                            break;
                        case 2:
                            labTotal--;
                            break;
                        case 3:
                            tutTotal--;
                            break;
                        default:
                            Debug.Log("Invalid type selected.");
                            break;
                    }
                    break;
                default:
                    Debug.Log("Invalid action selected.");
                    break;
            }
            updateState();
            updateLastChanged.record(-2);
        }
    }
    public bool isHolding = false;
    private float holdTimer = 0f;

    public int returnHoldRatio()
    {
        return (int)((holdTimer / maxTime) * 32f);
    }
    public int returnHoldRatio_undo()
    {
        return (int)((BH.holdTimer / BH.holdTime) * 35f);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        holdTimer = 0f;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        holdTimer = 0f;
    }
    private void OnHoldComplete()
    {
        Debug.Log("Hold complete!");
        SaveData(true);
        Destroy(gameObject);
        updateLastChanged.record(-1,0,cardName);
    }
    private void OnTypeButtonClick()
    {
        // Handle type button click
        Debug.Log("Type button clicked for: " + nameText.text);
        changeType();
        Vibrator.Vibrate(50);
    }
    private void OnPresentButtonClick()
    {
        // Handle present button click
        Debug.Log("Present button clicked for: " + nameText.text);
        switch (currentType)
        {
            case 1:
                lectAmt++;
                lectTotal++;
                break;
            case 2:
                labAmt++;
                labTotal++;
                break;
            case 3:
                tutAmt++;
                tutTotal++;
                break;
            default:
                Debug.Log("Invalid type selected.");
                break;
        }
        Vibrator.Vibrate(50);
        cmds.Push(currentType);
        actions.Push(1);
        updateState();
        updateLastChanged.record(1, currentType, cardName);
    }
    private void OnAbsentButtonClick()
    {
        // Handle absent button click
        Debug.Log("Absent button clicked for: " + nameText.text);
        switch (currentType)
        {
            case 1:
                lectTotal++;
                break;
            case 2:
                labTotal++;
                break;
            case 3:
                tutTotal++;
                break;
            default:
                Debug.Log("Invalid type selected.");
                break;
        }
        Vibrator.Vibrate(100);
        cmds.Push(currentType);
        actions.Push(0);
        updateState();
        updateLastChanged.record(0, currentType, cardName);
    }
    void SaveData(bool removal = false)
    {
        Dictionary<string, Dictionary<string, int>> dict = new Dictionary<string, Dictionary<string, int>>();
        cardData cardStats = LoadData();
        if (cardStats != null)
        {
            dict = cardStats.data;
        }
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + "data" + ".lol";
        FileStream stream = new FileStream(path, FileMode.Create);
        cardData data = new cardData(this, dict, removal);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    cardData LoadData()
    {
        string path = Application.persistentDataPath + "/" + "data" + ".lol";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            cardData data = formatter.Deserialize(stream) as cardData;
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