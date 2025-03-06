using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class menuManager : MonoBehaviour
{
    [Header("Menu Objects")]
    [SerializeField] private GameObject _mainMenuCanvasGO;
    [SerializeField] private GameObject _settingsMenuCanvasGO;

    [Header("Player Scripts to Deactivate on Pause")]
    //[SerializeField] private GameObject _pieceMovement;            (does nothing rn need to implement so that pieces can't move while paused)


    [Header("First Selected Options")]
    [SerializeField] private GameObject _mainMenuFirst;
    [SerializeField] private GameObject _settingsMenuFirst;
    

    private bool isPaused;

    public void Start()
    {
         _mainMenuCanvasGO.SetActive(false);
         _settingsMenuCanvasGO.SetActive(false);
    }

    private void Update()
    {
        if (inputManager.instance.menuOpenCloseInput)
        {
            if (!isPaused)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }
    }




    #region Pause/Unpause functions

    public void Pause()
    {
        //_pieceMovement.SetActive(false);         (does nothing rn need to implement so that pieces can't move while paused)
        isPaused = true;
        Time.timeScale = 0;
        openMainMenu();
    }

    public void Unpause()
    {
        isPaused = false;
        Time.timeScale = 1;
        closeAllMenus();
    }

    #endregion



    #region canvas activations/deactivations

    private void openMainMenu()
    {
        _mainMenuCanvasGO.SetActive(true);
        _settingsMenuCanvasGO.SetActive(false);

        EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
    }

    private void openSettingsMenu()
    {
        _mainMenuCanvasGO.SetActive(false);
        _settingsMenuCanvasGO.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_settingsMenuFirst);
    }

    private void closeAllMenus()
    {
        _mainMenuCanvasGO.SetActive(false);
        _settingsMenuCanvasGO.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
    }

    #endregion



    #region Main Menu Button Functions


    public void onSettingsPress()
    {
        openSettingsMenu();
    }

    public void onResumePress()
    {
        Unpause();
    }
    
    #endregion



    #region Settings Menu Button Functions

    public void onSettingsBackPress()
    {
        openMainMenu();
    }

    #endregion

}