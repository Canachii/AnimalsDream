using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private Button serverBtn;
    [SerializeField] private InputField joinCodeInput;
    [SerializeField] private Text joinCodeText;

    private void Start()
    {
        hostBtn.onClick.AddListener(async () =>
        {
            Debug.Log("호스트 버튼 클릭됨");
            string code = await RelayManager.Instance.StartHostWithLobby();
            joinCodeText.text = code;
            joinCodeInput.text = code;
        });

        serverBtn.onClick.AddListener(async () =>
        {
            Debug.Log("퀵 조인 시도중 ...");
            try
            {
                await RelayManager.Instance.QuickJoin();

            }
            catch
            {
                Debug.Log("방이 없어서 새로운 호스트로 시작합니다..");
                string code = await RelayManager.Instance.StartHostWithLobby();
                joinCodeText.text = code;
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
    }
}
