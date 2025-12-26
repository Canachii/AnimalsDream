using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject roomPanel;

    [Header("Lobby UI")]
    [SerializeField] private Button createRoomBtn;
    [SerializeField] private InputField joinCodeInput;

    [Header("Room UI")]
    [SerializeField] private Text joinCodeText;
    [SerializeField] private Button joinBtn;
    [SerializeField] private Button copyBtn;
    [SerializeField] private Text playerListText;
    [SerializeField] private Button startBtn;

    private string _currentJoinCode;

    private void Start()
    {
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);

        createRoomBtn.onClick.AddListener(async () =>
        {
            string code = await RelayManager.Instance.StartHostWithLobby();
            if (!string.IsNullOrEmpty(code))
            {
                _currentJoinCode = code;
                joinCodeText.text = $"초대 코드 : {code}";
                Debug.Log($"방 생성 완료. 코드 :{code}");

                lobbyPanel.SetActive(false);
                roomPanel.SetActive(true);
            }
            joinCodeText.text = $"초대 코드 : {code}";
            joinCodeInput.text = code;
        });

        copyBtn.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(_currentJoinCode))
            {
                GUIUtility.systemCopyBuffer = _currentJoinCode;

                var btnText = copyBtn.GetComponentInChildren<Text>();
                string originalText = btnText.text;
                btnText.text = "복사됨!";
                Debug.Log("초대 코드가 복사되었습니다!");

                Invoke(nameof(ResetCopyButtonText), 1.5f);
            }
        });

        joinBtn.onClick.AddListener(async () =>
        {
            Debug.Log("조인 버튼 클릭됨");
            string code = joinCodeInput.text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                Debug.LogError("입력된 JoinCode가 없습니다.");
                return;
            }
            try
            {
                await RelayManager.Instance.StartClient(code);
                Debug.Log("클라이언트 시작 성공");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"클라이언트 시작 실패: {e.Message}");
            }
        });

        startBtn.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        });
    }

    private void Update()
    {
        if (roomPanel.activeSelf && NetworkManager.Singleton.IsServer)
        {
            playerListText.text = $"접속 중인 플레이어 : {NetworkManager.Singleton.ConnectedClients.Count}명";
        }
    }

    private void ResetCopyButtonText()
    {
        copyBtn.GetComponentInChildren<Text>().text = "복사";
    }
}
