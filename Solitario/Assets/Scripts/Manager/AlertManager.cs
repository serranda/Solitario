using System;
using System.Collections;
using System.Collections.Generic;
using Controller;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using Util;

namespace Manager
{
    public class AlertManager : Singleton<AlertManager>
    {
        [SerializeField] private AssetReference alertReference;
        [SerializeField] private Canvas mainCanvas;

        private AlertController activeAlert;

        private void OpenAlertPanel(string mainText, bool buttonEnabled = false, string resumeText = null, string exitText = null, UnityAction resumeAction = null, UnityAction exitAction = null)
        {
            GameManager.Instance.currentState = GameManager.GameState.Pause;

            StartCoroutine(OpenAlertPanelRoutine(mainText, buttonEnabled, resumeText, exitText, resumeAction, exitAction));
        }

        private IEnumerator OpenAlertPanelRoutine(string mainText, bool buttonEnabled, string resumeText, string exitText, UnityAction resumeAction, UnityAction exitAction)
        {
            AsyncOperationHandle<GameObject> handleAlertPanel = alertReference.LoadAssetAsync<GameObject>();
            yield return handleAlertPanel;
            GameObject newAlertPrefab = handleAlertPanel.Result;

            //instantiate new card GameObject
            GameObject newAlertGameObject = Instantiate(newAlertPrefab, mainCanvas.transform);

            AlertController newAlertController = newAlertGameObject.GetComponent<AlertController>();

            newAlertController.InitializeAlertPanel(mainText, buttonEnabled, resumeText, exitText, resumeAction, exitAction);

            activeAlert = newAlertController;
        }

        public void CloseAlertPanel()
        {
            activeAlert.DestroyPanel();

            GameManager.Instance.currentState = GameManager.GameState.Playing;
        }

        public void SpawnWaitShufflePanel()
        {
            OpenAlertPanel("in attesa che il mazzo venga mischiato...");
        }

        public void SpawnWinPanel()
        {
            OpenAlertPanel("congratulazioni, hai vinto!!! vuoi giocare una nuova partita?", true, "nuova partita", "menù iniziale", StartNewGame, GoToStartMenu);
        }

        public void SpawnOptionPanel()
        {
            OpenAlertPanel("vuoi iniziare una nuova partita?", true, "riprendi", "nuova partita", CloseAlertPanel, StartNewGame);
        }

        public void SpawnHomePanel()
        {
            OpenAlertPanel("proseguendo, interromperai la partita ed i progressi andranno persi. vuoi tornare al menù iniziale?", true, "riprendi", "menù iniziale", CloseAlertPanel, GoToStartMenu);
        }

        private void StartNewGame()
        {

        }

        private void GoToStartMenu()
        {
            PanelManager.Instance.SwapToMenuPanel();
            CloseAlertPanel();
        }

    }
}

