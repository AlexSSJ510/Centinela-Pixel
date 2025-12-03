// MainMenuController.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject controlsPanel;
    public GameObject creditsPanel;

    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button controlsButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Control Buttons")]
    public Button backFromControlsButton;
    public Button backFromCreditsButton;

    [Header("Scene to Load")]
    public string gameSceneName = "GameScene"; // Cambia esto al nombre de tu escena de juego

    void Start()
    {
        // Inicializar todos los paneles
        ShowMainMenu();

        // Configurar botones del menú principal
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(ShowControls);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCredits);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Configurar botones de volver
        if (backFromControlsButton != null)
            backFromControlsButton.onClick.AddListener(ShowMainMenu);

        if (backFromCreditsButton != null)
            backFromCreditsButton.onClick.AddListener(ShowMainMenu);

        // Configurar teclas de atajo
        SetupKeyboardShortcuts();
    }

    void SetupKeyboardShortcuts()
    {
        // Opcional: atajos de teclado
    }

    void Update()
    {
        // Tecla Escape para volver al menú principal
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!mainMenuPanel.activeSelf)
            {
                ShowMainMenu();
            }
        }

        // Tecla Enter/Return para iniciar juego
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (mainMenuPanel.activeSelf)
            {
                StartGame();
            }
        }
    }

    public void StartGame()
    {
        Debug.Log("Iniciando juego...");

        // Efecto de fade out (opcional)
        // StartCoroutine(LoadGameScene());

        // Cargar escena directamente
        SceneManager.LoadScene(gameSceneName);
    }

    System.Collections.IEnumerator LoadGameScene()
    {
        // Aquí podrías añadir un efecto de fade
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(controlsPanel, false);
        SetPanelActive(creditsPanel, false);
    }

    public void ShowControls()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(controlsPanel, true);
        SetPanelActive(creditsPanel, false);
    }

    public void ShowCredits()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(controlsPanel, false);
        SetPanelActive(creditsPanel, true);
    }

    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}