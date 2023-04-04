using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [SerializeField] string firstLevel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startGame() {
        SceneManager.LoadScene(firstLevel);
    }

    public void openControlsMenu() { 
    
    }

    public void closeControlsMenu() { 
    
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
