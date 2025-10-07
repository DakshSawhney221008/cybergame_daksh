using PurrNet;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : NetworkBehaviour
{
    [PurrScene][SerializeField] string sceneName;

    public void LoadScene()
    {
        networkManager.sceneModule.LoadSceneAsync(sceneName);

        foreach (var x in GameData.Instance.nicknames)
        {
            Debug.Log($"ID: {x.Key}, Nickname: {x.Value}");
        }
    }
}
