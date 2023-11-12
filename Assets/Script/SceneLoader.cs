using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class SceneLoader{
    public enum Scene
    {
        MainMenuScene,
        LobbyScene,
        CharacterSelectScene,
        GameScene,
        LoadingScene,
    }

    private static Scene targetScene;

    //Desc : Use while there is no network related componnte in current scene
    public static void Load(Scene targetScene)
    {
        SceneLoader.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    } 

    //Desc : Once USe NEtwork Mangaer ,it need to use the SceneManger build in of the network manager,
    //else the network object will not automatically spawned at the beginning of the scene
    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
