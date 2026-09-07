using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class sceneChanger : MonoBehaviour
{
    [SerializeField] private Button attendanceButton;
    [SerializeField] private Button cgpaButton;
    [SerializeField] private Button eventManagerButton;

    [SerializeField] private sidebarAnimation sidebarAnimation;


    private void Start()
    {
        sidebarAnimation = GetComponent<sidebarAnimation>();
        attendanceButton.onClick.AddListener(() => { ChangeScene(1); StartCoroutine(toggleSidebar()); });
        cgpaButton.onClick.AddListener(() => { ChangeScene(2); StartCoroutine(toggleSidebar()); });
        eventManagerButton.onClick.AddListener(() => { ChangeScene(3); StartCoroutine(toggleSidebar()); });
        int lastSceneIndex = PlayerPrefs.GetInt("LastScene", 0); // Get the last scene index or default to 0
        ChangeScene(lastSceneIndex); // Change to the last scene on start
    }

    IEnumerator toggleSidebar()
    {
        yield return new WaitForSeconds(0.1f); // Wait for the sidebar animation to complete
        sidebarAnimation.ToggleSidebar();
    }

    void ChangeScene(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("Invalid scene index: " + sceneIndex);
            return;
        }
        if (SceneManager.GetActiveScene().buildIndex == sceneIndex)
        {
            Debug.Log("Already in the requested scene.");
            return;
        }
        PlayerPrefs.SetInt("LastScene", sceneIndex); // Save the last scene index
        SceneManager.LoadScene(sceneIndex);
    }
}
