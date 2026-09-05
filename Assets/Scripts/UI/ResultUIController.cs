using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ResultUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset rankingItemTemplate;

    private ListView rankingListView;
    private List<RankingData> currentRankingData;

    [System.Serializable]
    public struct RankingData
    {
        public int rank;
        public string playerName;
        public float finishTime;
        public int deathCount;
    }

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            rankingListView = root.Q<ListView>();

            if (rankingListView != null)
            {
                ConfigureListView();
            }
            else
            {
                Debug.LogError("[ResultUIController] ListView not found in UI Document.");
            }

            // Find the button (assuming it's the only button or we search by text/hierarchy if needed)
            var lobbyButton = root.Q<Button>(); 
            if (lobbyButton != null)
            {
                lobbyButton.clicked += OnLobbyButtonClicked;
            }
        }
    }

    private void ConfigureListView()
    {
        if (rankingItemTemplate == null)
        {
            Debug.LogError("[ResultUIController] Ranking Item Template is not assigned.");
            return;
        }

        rankingListView.makeItem = () => rankingItemTemplate.Instantiate();

        rankingListView.bindItem = (element, index) =>
        {
            if (currentRankingData == null || index >= currentRankingData.Count) return;

            var data = currentRankingData[index];

            var rankLabel = element.Q<Label>("Rank");
            var nameLabel = element.Q<Label>("Name");
            var timeLabel = element.Q<Label>("Time");
            var deathLabel = element.Q<Label>("Death");

            if (rankLabel != null) rankLabel.text = data.rank.ToString();
            if (nameLabel != null) nameLabel.text = data.playerName;
            if (timeLabel != null) timeLabel.text = FormatTime(data.finishTime);
            if (deathLabel != null) deathLabel.text = data.deathCount.ToString();
        };

        rankingListView.itemsSource = currentRankingData;
        rankingListView.fixedItemHeight = 50; 
    }

    public void SetRankingData(List<RankingData> data)
    {
        currentRankingData = data;
        if (rankingListView != null)
        {
            rankingListView.itemsSource = currentRankingData;
            rankingListView.RefreshItems();
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnLobbyButtonClicked()
    {
        Debug.Log("Return to Lobby requested.");
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        AudioManager.Instance?.StopAllNonBgm();
        SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }
}
