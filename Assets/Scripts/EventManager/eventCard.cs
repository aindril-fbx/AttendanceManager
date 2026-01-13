using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;

public class eventCard : MonoBehaviour
{
    public string eventTitle;
    public DateTime eventDateTime;
    public string eventDate;
    public string eventDescription;

    [SerializeField] private bool currentDayCard;

    [SerializeField] private TextMeshProUGUI eventTitle_text;
    [SerializeField] private TextMeshProUGUI eventDate_text;
    [SerializeField] private TextMeshProUGUI eventDescription_text;

    private bool detailsVisible = false;
    [SerializeField] private Button detailsButton;
    [SerializeField] private RectTransform cardRT;


    private void Start()
    {
        UpdateCard();
        detailsButton = GetComponentInChildren<Button>();
        if(currentDayCard) return;
        cardRT = GetComponent<RectTransform>();
        detailsButton.onClick.AddListener(() =>
        {
            detailsVisible = !detailsVisible;
            cardRT.sizeDelta = detailsVisible ? new Vector2(cardRT.sizeDelta.x, 400f) : new Vector2(cardRT.sizeDelta.x, 215.56f);
            
        });
    }

    private void FixedUpdate() {
        string prev_date = "";
        if (eventDate == prev_date)
        {
            return;
        }
        prev_date = eventDate;
        UpdateCard();
    }

    public void UpdateCard()
    {
        if (currentDayCard)
        {
            eventDateTime = DateTime.Now;
            eventDate_text.text = eventDateTime.ToString("dd/MM/yyyy");
        }
        else
        {
            if(DateTime.TryParse(eventDate, out DateTime parsedDate))
            {
                eventDateTime = parsedDate;
            }
            else
            {
                Debug.LogError("Invalid date format: " + eventDate);
            }
            eventTitle_text.text = eventTitle;
            eventDate_text.text = eventDateTime.ToString("dd/MM/yyyy");
            eventDescription_text.text = eventDescription;
        }
    }
}