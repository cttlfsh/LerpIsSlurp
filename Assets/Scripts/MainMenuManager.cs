using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    #region Singleton
    public static MainMenuManager Instance;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }
    #endregion


    [HideInInspector] public GameObject optionsMenu;
    [HideInInspector] public bool isGamePaused = false;

    public void LoadGame()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        SceneManager.LoadScene("Game");
    }
}
