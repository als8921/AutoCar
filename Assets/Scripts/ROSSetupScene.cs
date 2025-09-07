using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ROSSetupScene : MonoBehaviour
{
    [Header("Settings")]
    public string simulatorSceneName = "SimulatorScene";
    
    [Header("Default Values")]
    public string defaultIP = "127.0.0.1";
    public int defaultPort = 10000;
    
    // UI 컴포넌트들 (동적 생성)
    private Canvas canvas;
    private TMP_InputField ipInputField;
    private TMP_InputField portInputField;
    private Button connectButton;
    private Button exitButton;
    
    void Start()
    {
        CreateUI();
        InitializeSettings();
    }
    
    void CreateUI()
    {
        // Canvas 생성
        GameObject canvasGO = new GameObject("Setup Canvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Canvas Scaler
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Graphic Raycaster
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // 메인 패널
        GameObject panel = new GameObject("Main Panel");
        panel.transform.SetParent(canvasGO.transform, false);
        
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.3f);
        panelRect.anchorMax = new Vector2(0.8f, 0.7f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Layout Group
        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(50, 50, 50, 50);
        layout.spacing = 30;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        
        // 제목
        CreateTitle(panel);
        
        // IP 입력
        CreateIPField(panel);
        
        // Port 입력
        CreatePortField(panel);
        
        // 버튼들
        CreateButtons(panel);
    }
    
    void CreateTitle(GameObject parent)
    {
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(parent.transform, false);
        
        TMP_Text title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text = "ROS Connection Setup";
        title.fontSize = 36;
        title.fontStyle = FontStyles.Bold;
        title.color = Color.white;
        title.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = titleGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 60);
    }
    
    void CreateIPField(GameObject parent)
    {
        // 라벨
        GameObject labelGO = new GameObject("IP Label");
        labelGO.transform.SetParent(parent.transform, false);
        
        TMP_Text label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = "ROS IP Address:";
        label.fontSize = 20;
        label.color = Color.white;
        
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(0, 30);
        
        // 입력 필드
        GameObject inputGO = new GameObject("IP Input");
        inputGO.transform.SetParent(parent.transform, false);
        
        Image inputBg = inputGO.AddComponent<Image>();
        inputBg.color = Color.white;
        
        ipInputField = inputGO.AddComponent<TMP_InputField>();
        
        // 텍스트 컴포넌트
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(inputGO.transform, false);
        TMP_Text inputText = textGO.AddComponent<TextMeshProUGUI>();
        inputText.color = Color.black;
        inputText.fontSize = 18;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        ipInputField.textComponent = inputText;
        ipInputField.text = defaultIP;
        
        RectTransform inputRect = inputGO.GetComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(0, 50);
    }
    
    void CreatePortField(GameObject parent)
    {
        // 라벨
        GameObject labelGO = new GameObject("Port Label");
        labelGO.transform.SetParent(parent.transform, false);
        
        TMP_Text label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = "ROS Port:";
        label.fontSize = 20;
        label.color = Color.white;
        
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(0, 30);
        
        // 입력 필드
        GameObject inputGO = new GameObject("Port Input");
        inputGO.transform.SetParent(parent.transform, false);
        
        Image inputBg = inputGO.AddComponent<Image>();
        inputBg.color = Color.white;
        
        portInputField = inputGO.AddComponent<TMP_InputField>();
        portInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        
        // 텍스트 컴포넌트
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(inputGO.transform, false);
        TMP_Text inputText = textGO.AddComponent<TextMeshProUGUI>();
        inputText.color = Color.black;
        inputText.fontSize = 18;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        portInputField.textComponent = inputText;
        portInputField.text = defaultPort.ToString();
        
        RectTransform inputRect = inputGO.GetComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(0, 50);
    }
    
    void CreateButtons(GameObject parent)
    {
        // 버튼 컨테이너
        GameObject buttonContainer = new GameObject("Button Container");
        buttonContainer.transform.SetParent(parent.transform, false);
        
        HorizontalLayoutGroup buttonLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 30;
        buttonLayout.childControlWidth = true;
        buttonLayout.childForceExpandWidth = true;
        
        RectTransform containerRect = buttonContainer.GetComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(0, 60);
        
        // Connect 버튼
        connectButton = CreateButton(buttonContainer, "Connect & Start", Color.green);
        connectButton.onClick.AddListener(OnConnectClick);
        
        // Exit 버튼
        exitButton = CreateButton(buttonContainer, "Exit", Color.red);
        exitButton.onClick.AddListener(OnExitClick);
    }
    
    Button CreateButton(GameObject parent, string text, Color color)
    {
        GameObject buttonGO = new GameObject(text + " Button");
        buttonGO.transform.SetParent(parent.transform, false);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = color;
        
        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        // 버튼 텍스트
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        TMP_Text buttonText = textGO.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 18;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
    
    void InitializeSettings()
    {
        // 이전에 저장된 설정이 있다면 로드
        ROSSettingsManager.Instance.LoadROSSettings();
        
        if (ipInputField != null)
            ipInputField.text = ROSSettingsManager.Instance.rosIPAddress;
            
        if (portInputField != null)
            portInputField.text = ROSSettingsManager.Instance.rosPort.ToString();
    }
    
    void OnConnectClick()
    {
        string ip = ipInputField.text;
        if (string.IsNullOrEmpty(ip))
            ip = defaultIP;
            
        if (!int.TryParse(portInputField.text, out int port))
            port = defaultPort;
            
        // 설정 저장
        ROSSettingsManager.Instance.SetROSSettings(ip, port);
        
        // 시뮬레이터 씬으로 이동
        SceneManager.LoadScene(simulatorSceneName);
    }
    
    void OnExitClick()
    {
        Application.Quit();
    }
    
    void Update()
    {
        // Enter 키로 연결
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnConnectClick();
        }
        
        // ESC 키로 종료
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnExitClick();
        }
    }
}