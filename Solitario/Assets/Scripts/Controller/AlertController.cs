using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Controller
{
    public class AlertController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mainText;
        [SerializeField] private TextMeshProUGUI resumeText;
        [SerializeField] private TextMeshProUGUI exitText;

        [SerializeField] private GameObject buttonPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button exitButton;
        // Start is called before the first frame update

        public void InitializeAlertPanel(string mainText, bool buttonEnabled, string resumeText, string exitText, UnityAction resumeAction, UnityAction exitAction)
        {
            this.mainText.text = mainText;

            buttonPanel.SetActive(buttonEnabled);
            if (buttonEnabled)
            {
                this.resumeText.text = resumeText;
                this.exitText.text = exitText;

                resumeButton.onClick.AddListener(resumeAction);
                exitButton.onClick.AddListener(exitAction);
            }

        }

        public void DestroyPanel()
        {
            Destroy(gameObject);
        }
    }
}

