using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIButtonSfx : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    [Header("Sound")]
    [SerializeField] private SoundId hoverSound = SoundId.UI_ButtonHover;
    [SerializeField] private SoundId clickSound = SoundId.UI_ButtonClick;

    // 중복 등록 방지용(씬 전환/패널 재오픈에서 콜백 중복 방지)
    private readonly List<Button> boundButtons = new();

    private void Awake()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (!uiDocument)
        {
            Debug.LogError("[UIToolkitButtonSfxBinder] UIDocument가 없습니다.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[UIToolkitButtonSfxBinder] rootVisualElement가 null 입니다.");
            return;
        }

        BindAllButtons(root);
    }

    private void OnDisable()
    {
        UnbindAllButtons();
    }

    private void BindAllButtons(VisualElement root)
    {
        UnbindAllButtons(); // 혹시 모를 중복 방지

        // UXML 안의 모든 Button을 찾아서 연결
        root.Query<Button>().ForEach(btn =>
        {
            // Hover: 마우스/포인터가 버튼 위로 들어올 때
            btn.RegisterCallback<PointerEnterEvent>(OnHover);

            // Click: UI Toolkit의 Button.clicked는 마우스 클릭 + 키보드/패드 Submit까지 포함
            btn.clicked += OnClicked;

            boundButtons.Add(btn);
        });

        Debug.Log($"[UIToolkitButtonSfxBinder] 버튼 {boundButtons.Count}개에 Hover/Click SFX 바인딩 완료");
    }

    private void UnbindAllButtons()
    {
        if (boundButtons.Count == 0) return;

        foreach (var btn in boundButtons)
        {
            // 등록한 것과 동일한 메서드로 해제해야 중복 호출을 막을 수 있음
            btn.UnregisterCallback<PointerEnterEvent>(OnHover);
            btn.clicked -= OnClicked;
        }

        boundButtons.Clear();
    }

    private void OnHover(PointerEnterEvent evt)
    {
        AudioManager.Instance?.Play(hoverSound);
    }

    private void OnClicked()
    {
        AudioManager.Instance?.Play(clickSound);
    }
}
