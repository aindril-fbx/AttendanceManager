using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;


public class cardsManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField newCardNameText;
    [SerializeField] private Button addCardButton;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardparent;

    [SerializeField] private TextMeshProUGUI indicatorBar;
    [SerializeField] private List<card> cardsList = new List<card>();

    [SerializeField] private GameObject dummyCard;
    int numberOfCards;

    private void Start()
    {
        addCardButton.onClick.AddListener(OnAddCardButtonClick);
        addCardButton.interactable = false; // Disable the button initially
        cardData cardStats = LoadData(); // Load the card data
        if (cardStats == null) return; // Check if data is null
        foreach (string key in cardStats.data.Keys)
        {
            Instantiate(cardPrefab, cardparent).GetComponent<card>().cardName = key;
        }
        cardsList = new List<card>(cardparent.GetComponentsInChildren<card>());
        newCardNameText.shouldHideMobileInput = true; // Hide the mobile keyboard input
        newCardNameText.onSubmit.AddListener((text) => OnAddCardButtonClick()); // Add listener for input submission
    }

    bool isHolding = false;
    bool isHolding_undo = false;
    private void Update() {
        numberOfCards = cardsList.Count;
        if(numberOfCards != 0 || PlayerPrefs.HasKey("DummyDelete"))
        {
            if(dummyCard != null)
            {
                PlayerPrefs.SetInt("DummyDelete",1);
                PlayerPrefs.Save();
                Destroy(dummyCard);
            }
        }
        isHolding = false;
        isHolding_undo = false;
        int ratioValue = 0; // Initialize ratioValue to 0
        int ratioValue_undo = 0; // Initialize ratioValue to 0
        for(int i = 0; i < cardsList.Count; i++) {
            if(cardsList[i] != null) {
                if(cardsList[i].returnHoldRatio() > 3){
                    isHolding = true;
                    ratioValue = cardsList[i].returnHoldRatio(); // Check if any card is being held
                } // Update the card name if needed
            }
            if(cardsList[i] != null) {
                if(cardsList[i].returnHoldRatio_undo() > 2){
                    isHolding_undo = true;
                    ratioValue_undo = cardsList[i].returnHoldRatio_undo(); // Check if any card is being held
                } // Update the card name if needed
            }
        }
        indicatorBar.text = ""; // Clear the indicator bar text
        if(isHolding && !isHolding_undo) {
            indicatorBar.color = Color.red; // Change the color to red if a card is being held
            indicatorBar.text = "Deleting: |" + new string('>',ratioValue) + new string(' ',31-ratioValue) + "|"; // Update the indicator bar text
        }

        if(isHolding_undo && !isHolding) {
            indicatorBar.color = Color.grey; // Change the color to red if a card is being held
            indicatorBar.text = "Undo: |" + new string('>',ratioValue_undo) + new string(' ',35-ratioValue_undo) + "|"; // Update the indicator bar text
        }
    }

    private void OnDestroy() {
        addCardButton.onClick.RemoveListener(OnAddCardButtonClick);
    }

    public void OnValueChanged() {
        // Handle value changes if needed
        if(!string.IsNullOrEmpty(newCardNameText.text)) {
            addCardButton.interactable = true; // Enable the button if the input is not empty
        } else {
            addCardButton.interactable = false; // Disable the button if the input is empty
        }
    }

    private void OnAddCardButtonClick() {
        string cardName = newCardNameText.text;
        foreach (card existingCard in cardsList)
        {
            if (existingCard.cardName == cardName)
            {
                Debug.Log("Card with this name already exists!");
                return; // Prevent adding a card with the same name
            }
        }
        if (!string.IsNullOrEmpty(cardName))
        {
            // Add the card to the list or perform any other action
            Debug.Log("Adding card: " + cardName);
            newCardNameText.text = ""; // Clear the input field after adding

            GameObject card = Instantiate(cardPrefab, cardparent);
            card.GetComponent<card>().cardName = cardName;
            cardsList.Add(card.GetComponent<card>()); // Add the new card to the list 
            Vibrator.Vibrate(75); // Vibrate for 50 milliseconds
            updateLastChanged.record(); // Record the last changed time
        }
        else
        {
            Debug.Log("Card name cannot be empty!");
        }
    }

    cardData LoadData(){
        string path = Application.persistentDataPath + "/" + "data" + ".lol";
        if(File.Exists(path)){
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path , FileMode.Open);

            cardData data = formatter.Deserialize(stream) as cardData;
            stream.Close();
            return data;

        }else{
            Debug.LogError("Save File not found in " + path);
            return null;
        }
    }

    
}