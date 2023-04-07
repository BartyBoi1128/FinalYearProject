using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelperScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI helperText;
    [TextArea] public string helpingText;
    [SerializeField] Canvas helperCanvas;
    [SerializeField] Canvas mainCanvas;
    void Start()
    {
   
    }

    void Update()
    {
     
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            updateHelpingText();
            helperCanvas.gameObject.SetActive(true);
            mainCanvas.gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        helperCanvas.gameObject.SetActive(false);
        mainCanvas.gameObject.SetActive(true);
    }

    public void updateHelpingText() {
        helperText.text = helpingText;
    }
}
