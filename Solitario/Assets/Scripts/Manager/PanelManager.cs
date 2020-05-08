using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;

namespace Manager
{
    public class PanelManager : Singleton<PanelManager>
    {
        public GameObject MenuPanel;
        public GameObject GamePanel;

        public void SwapToMenuPanel()
        {
            MenuPanel.SetActive(true);
            GamePanel.SetActive(false);
        }

        public void SwapToGamePanel()
        {
            MenuPanel.SetActive(false);
            GamePanel.SetActive(true);

            AlertManager.Instance.SpawnWaitShufflePanel();
        }
    }
}

