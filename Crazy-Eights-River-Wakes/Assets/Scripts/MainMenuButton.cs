using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{

    public void OnPlayClicked()
    {
        SceneManager.LoadScene("playthroughenvironment");
    }
}