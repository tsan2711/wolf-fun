using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    [Header("Scene Names")]
    [SerializeField] private string menuScene = "MainMenu";
    [SerializeField] private string loadingScene = "LoadingScene";
    [SerializeField] private string gameScene = "GameScene";

    [Header("Settings")]
    [SerializeField] private bool useLoading = true;

    // Properties
    public bool IsLoading { get; private set; }
    public string CurrentScene => SceneManager.GetActiveScene().name;


    public void LoadScene(string sceneName)
    {
        if (IsLoading) return;

        if (useLoading && sceneName != loadingScene)
        {
            PlayerPrefs.SetString("TargetScene", sceneName);
            LoadDirect(loadingScene);
        }
        else
        {
            LoadDirect(sceneName);
        }
    }

    public void LoadDirect(string sceneName)
    {
        if (IsLoading) return;
        StartCoroutine(LoadCoroutine(sceneName));

    }

    private IEnumerator LoadCoroutine(string sceneName)
    {
        IsLoading = true;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        IsLoading = false;
    }


    public void ToMenu() => LoadScene(menuScene);
    public void ToGame() => LoadScene(gameScene);
    public void ToLoading() => LoadDirect(loadingScene);
    public void Restart() => LoadDirect(CurrentScene);

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    public static void Menu() => Instance?.ToMenu();
    public static void Loading() => Instance?.ToLoading();
    public static void Game() => Instance?.ToGame();
    public static void Load(string scene) => Instance?.LoadScene(scene);
    public static void Direct(string scene) => Instance?.LoadDirect(scene);
    public static void Exit() => Instance?.Quit();
}
