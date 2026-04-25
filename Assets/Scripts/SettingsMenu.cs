using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel;

    void Start()
    {
        settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf)
                Kapat();
            else
                Ac();
        }
    }

    public void Ac()
    {
        settingsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Kapat()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void AnaMenuyeDon()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}