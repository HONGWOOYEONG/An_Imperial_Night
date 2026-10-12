using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private enum MenuEntryMode
    {
        Main,
        GameOver,
        Victory
    }

    private const string MainMenuSceneName = "Build_MainMenu";
    private static MenuEntryMode entryMode = MenuEntryMode.Main;

    [SerializeField] private Button startButton;
    [SerializeField] private Button endButton;
    [SerializeField] private string gameSceneName = "Build_Game";
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button gameOverQuitButton;
    [SerializeField, Min(0.1f)] private float gameOverColorDuration = 2f;

    private Button selectedButton;
    private bool stickNavigationHeld;
    private Coroutine gameOverColorCoroutine;

    private void Awake()
    {
        ConfigureHitArea(startButton);
        ConfigureHitArea(endButton);
        startButton.onClick.AddListener(StartGame);
        endButton.onClick.AddListener(QuitGame);

        ConfigureHitArea(mainMenuButton);
        ConfigureHitArea(gameOverQuitButton);
        mainMenuButton.onClick.AddListener(ShowMainMenu);
        gameOverQuitButton.onClick.AddListener(QuitGame);
        MenuEntryMode requestedMode = entryMode;
        entryMode = MenuEntryMode.Main;
        if (requestedMode == MenuEntryMode.GameOver)
        {
            ShowGameOver(); // * 패배로 씬에 진입한 경우 Start 대신 GameOver 화면부터 표시
        }
        else if (requestedMode == MenuEntryMode.Victory)
        {
            ShowVictory(); // * 모든 승리 증강 선택을 마친 경우 Victory 화면부터 표시
        }
        else
        {
            ShowMainMenu(); // * 일반 실행으로 메인메뉴에 진입하면 Start 화면을 기본 활성화
        }
    }

    private void Start()
    {
        SelectButton((gameOverPanel != null && gameOverPanel.activeSelf) ||
            (victoryPanel != null && victoryPanel.activeSelf)
            ? mainMenuButton
            : startButton); // * 현재 열려 있는 화면의 첫 번째 버튼을 키보드·패드 기본 선택으로 지정
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            Button eventSelectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            if (eventSelectedButton != null)
                selectedButton = eventSelectedButton;
        }

        bool moveUp = Keyboard.current != null &&
            (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame);
        bool moveDown = Keyboard.current != null &&
            (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame);

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            moveUp |= gamepad.dpad.up.wasPressedThisFrame;
            moveDown |= gamepad.dpad.down.wasPressedThisFrame;

            float stickY = gamepad.leftStick.ReadValue().y;
            if (!stickNavigationHeld)
            {
                moveUp |= stickY > 0.5f;
                moveDown |= stickY < -0.5f;
            }
            stickNavigationHeld = Mathf.Abs(stickY) > 0.5f;
        }
        else
        {
            stickNavigationHeld = false;
        }

        bool resultScreenActive = (gameOverPanel != null && gameOverPanel.activeSelf) ||
            (victoryPanel != null && victoryPanel.activeSelf);
        Button upperButton = resultScreenActive
            ? mainMenuButton
            : startButton;
        Button lowerButton = resultScreenActive
            ? gameOverQuitButton
            : endButton;

        if (moveUp)
            SelectButton(upperButton);
        else if (moveDown)
            SelectButton(lowerButton);

        bool submit = Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame);
        submit |= gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;

        if (submit && selectedButton != null)
            selectedButton.onClick.Invoke();

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            SelectButton(selectedButton != null ? selectedButton : startButton);
    }

    private void SelectButton(Button button)
    {
        if (button == null) return;
        selectedButton = button;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    private static void ConfigureHitArea(Button button)
    {
        button.image.alphaHitTestMinimumThreshold = 0.1f;

        Graphic[] childGraphics = button.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in childGraphics)
        {
            if (graphic != button.targetGraphic)
                graphic.raycastTarget = false;
        }
    }

    public void ShowMainMenu()
    {
        if (gameOverColorCoroutine != null)
        {
            StopCoroutine(gameOverColorCoroutine);
            gameOverColorCoroutine = null;
        }

        startPanel?.SetActive(true);
        gameOverPanel?.SetActive(false);
        victoryPanel?.SetActive(false);
        mainMenuButton?.gameObject.SetActive(false);
        gameOverQuitButton?.gameObject.SetActive(false);
        SelectButton(startButton);
    }

    public void ShowGameOver()
    {
        if (gameOverText != null)
            gameOverText.text = "GAME OVER";
        startPanel?.SetActive(false);
        gameOverPanel?.SetActive(true);
        victoryPanel?.SetActive(false);
        mainMenuButton?.gameObject.SetActive(true);
        gameOverQuitButton?.gameObject.SetActive(true);

        if (gameOverColorCoroutine != null)
            StopCoroutine(gameOverColorCoroutine);
        gameOverColorCoroutine = StartCoroutine(AnimateResultColor(gameOverText, Color.red));
        SelectButton(mainMenuButton);
    }

    public void ShowVictory()
    {
        if (victoryText != null)
            victoryText.text = "VICTORY";
        startPanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
        victoryPanel?.SetActive(true);
        mainMenuButton?.gameObject.SetActive(true);
        gameOverQuitButton?.gameObject.SetActive(true);

        if (gameOverColorCoroutine != null)
            StopCoroutine(gameOverColorCoroutine);
        gameOverColorCoroutine = StartCoroutine(AnimateResultColor(victoryText, new Color(1f, 0.72f, 0.12f)));
        SelectButton(mainMenuButton);
    }

    public static void LoadGameOverScene()
    {
        entryMode = MenuEntryMode.GameOver; // * 씬 전환 뒤 Awake가 패배 화면을 선택하도록 진입 원인을 보관
        SceneManager.LoadScene(MainMenuSceneName);
    }

    public static void LoadVictoryScene()
    {
        entryMode = MenuEntryMode.Victory; // * 씬 전환 뒤 Awake가 승리 화면을 선택하도록 진입 원인을 보관
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private IEnumerator AnimateResultColor(TMP_Text resultText, Color targetColor)
    {
        if (resultText == null) yield break;

        resultText.color = Color.black;
        float elapsed = 0f;
        while (elapsed < gameOverColorDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            resultText.color = Color.Lerp(Color.black, targetColor,
                Mathf.Clamp01(elapsed / gameOverColorDuration));
            yield return null;
        }

        resultText.color = targetColor;
        gameOverColorCoroutine = null;
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(StartGame);
        endButton.onClick.RemoveListener(QuitGame);
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(ShowMainMenu);
        if (gameOverQuitButton != null)
            gameOverQuitButton.onClick.RemoveListener(QuitGame);
    }

    private void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
