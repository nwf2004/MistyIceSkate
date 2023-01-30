using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("AssetPreview");
    }
    public void PlayCredits()
    {
        SceneManager.LoadScene("Credits");
    }
    public void PlayMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void PlayManual()
    {
        SceneManager.LoadScene("Manual");
    }
}
