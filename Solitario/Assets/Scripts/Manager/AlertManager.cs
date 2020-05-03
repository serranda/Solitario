using System;
using System.Collections;
using System.Collections.Generic;
using Controller;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Util;

namespace Manager
{
    public class AlertManager : Singleton<AlertManager>
    {
        [SerializeField] private AssetReference alertReference;
        [SerializeField] private Canvas mainCanvas;

        public delegate void ResumeDelegate();

        public void SpawnAlertPanel(string mainText, bool buttonEnabled, string resumeText, string exitText)
        {
            StartCoroutine(SpawnAlertPanelRoutine(mainText, buttonEnabled, resumeText, exitText));
        }

        private IEnumerator SpawnAlertPanelRoutine(string mainText, bool buttonEnabled, string resumeText, string exitText)
        {
            AsyncOperationHandle<GameObject> handleAlertPanel = alertReference.LoadAssetAsync<GameObject>();
            yield return handleAlertPanel;
            GameObject newAlertPrefab = handleAlertPanel.Result;

            //instantiate new card GameObject
            GameObject newAlertGameObject = Instantiate(newAlertPrefab, mainCanvas.transform);

            AlertController newAlertController = newAlertGameObject.GetComponent<AlertController>();

            newAlertController.InitializeAlertPanel(mainText, buttonEnabled, resumeText, exitText);
        }

        public AlertController GetActiveAlertController()
        {
            return FindObjectOfType<AlertController>();
        }

        public void resumeHandle(ResumeDelegate resumeDelegate)
        {

        }
    }
}

