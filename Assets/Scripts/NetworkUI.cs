using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkUI : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();

        StartCoroutine(LoadSceneAfterFrame());
    }

    private IEnumerator LoadSceneAfterFrame()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton.IsListening);

        yield return null; // extra safety frame

        NetworkManager.Singleton.SceneManager.LoadScene(
            "GameScene",
            LoadSceneMode.Single
        );
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
