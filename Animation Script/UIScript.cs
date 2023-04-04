using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIScript : MonoBehaviour
{   
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] TextMeshProUGUI milkCartonsText;
    [SerializeField] TextMeshProUGUI grapplingText;
    private float countdown = 1f;

    private GameControl gc;
    private Grappling gp;
    void Start()
    {
        gc = GameObject.Find("Player").GetComponent<GameControl>();
        gp = GameObject.Find("Player").GetComponent<Grappling>();
    }

    
    void Update()
    {
        scoreText.text = gc.getCurrentScore().ToString();
        livesText.text = "Lives Remaining: " + gc.getLives().ToString();
        milkCartonsText.text = "Milk Cartons: " + gc.getMilkCartonsCollected().ToString() +"/9";
        if (gp.getActivated())
        {
            grapplingText.text = "Grappling Mode";
        }
        else {
            grapplingText.text = "";
        }
    }
}
