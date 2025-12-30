using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using AnimalsDream.UI;
using System.Collections.Generic;

public class GameHUDController : MonoBehaviour
{
    [SerializeField] private ResultUIController resultUI; // 인스펙터로 연결 추천
    private bool switchedToResult;

    private UIDocument uiDocument;
    private PlayerSkillController localPlayerSkills;
    private PlayerRaceProgress localPlayerProgress;
    private PlayerController localPlayerController;
    private GameFlow gameFlow;
    private RaceManager raceManager;

    // UI Elements
    private VisualElement playerIconElement;
    private Label currentRankText;
    private Label lastRankText;
    private Label leftTimeText;
    private RadialProgressElement skill1Overlay;
    private Label skill1Text;
    private RadialProgressElement skill2Overlay;
    private Label skill2Text;
    private VisualElement skill1Icon;
    private VisualElement skill2Icon;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        gameFlow = FindAnyObjectByType<GameFlow>();
        raceManager = FindAnyObjectByType<RaceManager>();

        // ResultUI는 "비활성 오브젝트"일 수 있으니 인스펙터 연결이 가장 안전
        // (FindAnyObjectByType는 비활성 오브젝트를 못 찾는 버전/환경이 많음)
        if (resultUI != null)
            resultUI.gameObject.SetActive(false);

        switchedToResult = false;

        if (gameFlow != null)
        {
            gameFlow.OnMatchFinished += SwitchToResultUI;

            // 혹시 이미 Finished 상태로 들어온 경우(늦게 진입/리플레이 등) 즉시 반영
            if (gameFlow.State == MatchState.Finished)
                SwitchToResultUI();
        }
    }
    private void OnDisable()
    {
        if (gameFlow != null)
            gameFlow.OnMatchFinished -= SwitchToResultUI;
    }

    private void SwitchToResultUI()
    {
        if (switchedToResult) return;
        switchedToResult = true;

        if (resultUI != null)
        {
            var data = BuildRankingData();

            resultUI.SetRankingData(data);

            resultUI.gameObject.SetActive(true);
        }

        // HUD 끄기
        gameObject.SetActive(false);
    }

    private List<ResultUIController.RankingData> BuildRankingData()
    {
        var list = new List<ResultUIController.RankingData>();

        // 가장 안전: 씬에 있는 PlayerRaceProgress를 수집해서 결과용 스냅샷 만들기
        var progresses = FindObjectsByType<PlayerRaceProgress>(FindObjectsSortMode.None);

        foreach (var p in progresses)
        {
            // playerName은 너 프로젝트에 맞는 이름 필드로 교체해줘
            string name = p.name;
            var netObj = p.GetComponent<NetworkObject>();
            if (netObj != null) name = $"Player {netObj.OwnerClientId}";

            list.Add(new ResultUIController.RankingData
            {
                rank = p.Rank,
                playerName = name,
                finishTime = p.finishTime,   
                deathCount = p.DeathCount    
            });
        }

        list.Sort((a, b) => a.rank.CompareTo(b.rank));
        return list;
    }
    private void Update()
    {
        if (uiDocument == null)
        {
            uiDocument = FindAnyObjectByType<UIDocument>();
            if (uiDocument == null) return;
        }

        if (localPlayerSkills == null)
        {
            FindLocalPlayer();
            if (localPlayerSkills == null) return;
            
            // Initialize UI once player is found
            InitializeUI();
        }

        UpdateHUD();
        UpdateSkillUI(0, skill1Overlay, skill1Text, skill1Icon);
        UpdateSkillUI(1, skill2Overlay, skill2Text, skill2Icon);
    }

    private void FindLocalPlayer()
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p.IsOwner)
            {
                localPlayerController = p;
                localPlayerSkills = p.GetComponent<PlayerSkillController>();
                localPlayerProgress = p.GetComponent<PlayerRaceProgress>();
                Debug.Log("GameHUDController: Local Player Found");
                break;
            }
        }
    }

    private void InitializeUI()
    {
        var root = uiDocument.rootVisualElement;
        if (root == null) return;
        
        playerIconElement = root.Q<VisualElement>("PlayerIcon");
        currentRankText = root.Q<Label>("CurrentRankText");
        lastRankText = root.Q<Label>("LastRankText");
        leftTimeText = root.Q<Label>("LeftTimeText");

        skill1Overlay = root.Q<RadialProgressElement>("Skill1Overlay");
        skill1Text = root.Q<Label>("Skill1CooldownText");
        skill1Icon = root.Q<VisualElement>("Skill1");

        skill2Overlay = root.Q<RadialProgressElement>("Skill2Overlay");
        skill2Text = root.Q<Label>("Skill2CooldownText");
        skill2Icon = root.Q<VisualElement>("Skill2");

        if (localPlayerController != null && localPlayerController.playerIcon != null)
        {
            playerIconElement.style.backgroundImage = new StyleBackground(localPlayerController.playerIcon);
        }

        UpdateSkillIcon(0, skill1Icon);
        UpdateSkillIcon(1, skill2Icon);
    }

    private void UpdateHUD()
    {
        if (localPlayerProgress != null)
        {
            if (currentRankText != null) currentRankText.text = localPlayerProgress.Rank.ToString();
            if (lastRankText != null && raceManager != null) lastRankText.text = "/" + raceManager.PlayerCount.ToString();
        }

        if (gameFlow != null && leftTimeText != null)
        {
            float remaining = gameFlow.RemainingTime;
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            leftTimeText.text = string.Format("{0}:{1:00}", minutes, seconds);
        }
    }

    private void UpdateSkillIcon(int index, VisualElement icon)
    {
        if (icon == null) return;
        var skill = localPlayerSkills.GetSkill(index);
        if (skill != null && skill.icon != null)
        {
            icon.style.backgroundImage = new StyleBackground(skill.icon);
        }
    }

    private void UpdateSkillUI(int index, RadialProgressElement overlay, Label text, VisualElement icon)
    {
        if (overlay == null || text == null) return;

        var skill = localPlayerSkills.GetSkill(index);
        if (skill == null) return;

        // Ensure icon is correct (in case skills change or init order)
        if (icon.style.backgroundImage.value.sprite != skill.icon)
        {
             if (skill.icon != null) icon.style.backgroundImage = new StyleBackground(skill.icon);
        }
        
        float remaining = skill.RemainingCooldown;
        float ratio = skill.CooldownRatio;

        overlay.Progress = ratio; // 1.0 (Dark) -> 0.0 (Clear)

        if (remaining > 0)
        {
            text.style.display = DisplayStyle.Flex;
            text.text = Mathf.CeilToInt(remaining).ToString();
        }
        else
        {
            text.style.display = DisplayStyle.None;
        }
    }
}
