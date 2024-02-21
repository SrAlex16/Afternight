using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPointMenu : MonoBehaviour
{
    public GameObject checkPointMenuUI;
    [SerializeField] private String sceneName;
    
    [Header("Sound params")]
    public AudioSource audioSource;
    public AudioClip menuClip;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            audioSource.PlayOneShot(menuClip);
            Pause();
        }
    }

    public void Pause()
    {
        checkPointMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }
    
    public void NextLevel()
    {
        SceneManager.LoadScene(sceneName);
    }
    
    /*
    public void Resume()
    {
        checkPointMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    */
    
    public void CloseApp()
    {
        Application.Quit();
    }
}
