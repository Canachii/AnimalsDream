using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class LobbyViewController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private LobbyManager lobbyManager;

    private VisualElement root;
    private VisualElement selectModeContainer;
    private VisualElement hostModeContainer;
    private VisualElement joinModeContainer;
    private VisualElement roomCodeContainer;

    private Button hostButton;
    private Button joinButton;
    private TextField joinTextField;
    
    private Label roomCodeText;
    private Button copyButton;
    
    private Button startButton;
    private Button exitButton;
    private Button optionButton;

    private ListView hostListView;
    private ListView joinListView;

    private string currentJoinCode;

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (lobbyManager == null) lobbyManager = FindObjectOfType<LobbyManager>();

        root = uiDocument.rootVisualElement;

        // Containers
        selectModeContainer = root.Q<VisualElement>("SelectMode");
        hostModeContainer = root.Q<VisualElement>("HostMode");
        joinModeContainer = root.Q<VisualElement>("JoinMode");
        roomCodeContainer = root.Q<VisualElement>("RoomCodeContainer");

        // Buttons & Inputs
        hostButton = root.Q<Button>("HostButton");
        joinButton = root.Q<Button>("JoinButton");
        joinTextField = root.Q<TextField>("JoinTextField");
        
        // HostMode Elements
        hostListView = hostModeContainer.Q<ListView>("ListView");
        startButton = hostModeContainer.Q<Button>(); // Assumes the only button in HostMode container is the Start button
        
        // JoinMode Elements
        joinListView = joinModeContainer.Q<ListView>("ListView");

        // RoomCode Elements
        roomCodeText = root.Q<Label>("RoomCodeText");
        copyButton = roomCodeContainer.Q<Button>(); 

        // Header Buttons
        exitButton = root.Q<Button>("ExitButton");
        optionButton = root.Q<Button>("OptionButton");

        // Event Binding
        if (hostButton != null) hostButton.clicked += OnHostButtonClicked;
        if (joinButton != null) joinButton.clicked += OnJoinButtonClicked;
        if (copyButton != null) copyButton.clicked += OnCopyButtonClicked;
        if (startButton != null) startButton.clicked += OnStartButtonClicked;
        if (exitButton != null) exitButton.clicked += OnExitButtonClicked;

        // Initialize ListViews
        SetupListView(hostListView);
        SetupListView(joinListView);

        // Initial State
        ShowSelectMode();

        // LobbyManager Events
        if (lobbyManager != null)
        {
             lobbyManager.Players.OnListChanged += OnPlayerListChanged;
        }
    }

    private void OnDisable()
    {
        if (hostButton != null) hostButton.clicked -= OnHostButtonClicked;
        if (joinButton != null) joinButton.clicked -= OnJoinButtonClicked;
        if (copyButton != null) copyButton.clicked -= OnCopyButtonClicked;
        if (startButton != null) startButton.clicked -= OnStartButtonClicked;
        if (exitButton != null) exitButton.clicked -= OnExitButtonClicked;

        if (lobbyManager != null && lobbyManager.Players != null)
        {
            lobbyManager.Players.OnListChanged -= OnPlayerListChanged;
        }
    }

    private void SetupListView(ListView listView)
    {
        if (listView == null) return;
        
        // item-template is already set in UXML, so makeItem is handled.
        // We just need bindItem.
        listView.bindItem = (element, index) =>
        {
            var playerData = (PlayerData)listView.itemsSource[index];
            var nameLabel = element.Q<Label>("PlayerNameLabel");
            if (nameLabel != null)
            {
                nameLabel.text = playerData.PlayerName.ToString();
            }
        };
    }

    private async void OnHostButtonClicked()
    {
        if (RelayManager.Instance == null) return;
        
        // Show loading or disable button?
        hostButton.SetEnabled(false);

        string code = await RelayManager.Instance.StartHostWithLobby();
        
        hostButton.SetEnabled(true);

        if (!string.IsNullOrEmpty(code))
        {
            currentJoinCode = code;
            roomCodeText.text = $"초대 코드: {code}";
            ShowHostMode();
            UpdatePlayerList();
        }
    }

    private async void OnJoinButtonClicked()
    {
        if (RelayManager.Instance == null) return;

        string code = joinTextField.value.Trim();
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogError("코드를 입력하세요.");
            return;
        }

        joinButton.SetEnabled(false);
        
        try 
        {
            await RelayManager.Instance.StartClient(code);
            currentJoinCode = code;
            roomCodeText.text = $"초대 코드: {code}"; // Client also sees code? Usually yes in lobby.
            ShowJoinMode();
            UpdatePlayerList();
        }
        catch(System.Exception e)
        {
            Debug.LogError($"Join failed: {e.Message}");
        }
        finally
        {
             joinButton.SetEnabled(true);
        }
    }

    private void OnCopyButtonClicked()
    {
        if (!string.IsNullOrEmpty(currentJoinCode))
        {
            GUIUtility.systemCopyBuffer = currentJoinCode;
            copyButton.text = "복사됨!";
            Invoke(nameof(ResetCopyButton), 1.5f);
        }
    }

    private void ResetCopyButton()
    {
        if(copyButton != null) copyButton.text = "복사";
    }

    private void OnStartButtonClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void OnExitButtonClicked()
    {
        //    ٽ κ  ȭ 
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        ShowSelectMode();
    }

    private void OnPlayerListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        if (lobbyManager == null) return;

        // Convert NetworkList to List for UI
        var playerList = new List<PlayerData>();
        foreach (var player in lobbyManager.Players)
        {
            playerList.Add(player);
        }

        if (hostListView != null)
        {
            hostListView.itemsSource = playerList;
            hostListView.Rebuild();
        }

        if (joinListView != null)
        {
            joinListView.itemsSource = playerList;
            joinListView.Rebuild();
        }
    }

    private void ShowSelectMode()
    {
        if (exitButton != null) exitButton.style.display = DisplayStyle.None;

        selectModeContainer.style.display = DisplayStyle.Flex;
        hostModeContainer.style.display = DisplayStyle.None;
        joinModeContainer.style.display = DisplayStyle.None;
        roomCodeContainer.style.display = DisplayStyle.None;
    }

    private void ShowHostMode()
    {
        if (exitButton != null) exitButton.style.display = DisplayStyle.Flex;

        selectModeContainer.style.display = DisplayStyle.None;
        hostModeContainer.style.display = DisplayStyle.Flex;
        joinModeContainer.style.display = DisplayStyle.None;
        roomCodeContainer.style.display = DisplayStyle.Flex;
    }

    private void ShowJoinMode()
    {
        if (exitButton != null) exitButton.style.display = DisplayStyle.Flex;

        selectModeContainer.style.display = DisplayStyle.None;
        hostModeContainer.style.display = DisplayStyle.None;
        joinModeContainer.style.display = DisplayStyle.Flex;
        roomCodeContainer.style.display = DisplayStyle.Flex;
    }
}
