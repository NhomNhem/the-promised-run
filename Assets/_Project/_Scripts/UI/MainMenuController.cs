using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using ThePromisedRun.UI;
using System.Threading;
using System;

public class MainMenuController : MonoBehaviour {
    [Header("Scene Config")]
    [SerializeField] private string _gameplaySceneName = "Scene_GamePlay";
    [SerializeField] private string _hudSceneName = "Scene_HUD";
    [SerializeField] private string _defaultLevelSceneName = "Level_01_WelcomeHall";

    [Header("UI Settings")]
    [SerializeField] private float _lateAdviceDelay = 1.5f;
    [SerializeField] private float _stressLevel = 42f;

    private UIDocument _uiDocument;
    private VisualElement _root;
    private VisualElement _popupContainer;
    private VisualElement _blueScreen;
    private Label _titleLabel;
    private VisualElement _stressBarInner;
    private VisualElement _stressBarBg;
    private Label _stressLabel;

    private Button _btnStart;
    private Button _btnSettings;
    private Button _btnInfo;
    private Button _btnQuit;

    private Material _glitchMaterial;
    private CancellationTokenSource _cts;
    private bool _isLoading = false;

    private static readonly int GlitchIntensityId = Shader.PropertyToID("_GlitchIntensity");

    private void Awake() {
        _uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable() {
        if (_uiDocument == null) return;
        
        _root = _uiDocument.rootVisualElement;
        if (_root == null) return;

        _cts = new CancellationTokenSource();

        InitializeElements();
        SetupButtonCallbacks();
        
        _blueScreen.style.display = DisplayStyle.None;
        _blueScreen.style.opacity = 0f;

        AnimateStressBar(_stressLevel);
    }

    private void OnDisable() {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    private void InitializeElements() {
        _popupContainer = _root.Q<VisualElement>("PopupLayer");
        _blueScreen = _root.Q<VisualElement>("BlueScreen");
        _titleLabel = _root.Q<Label>("GameTitle");
        _stressBarInner = _root.Q<VisualElement>("StressBarInner");
        _stressBarBg = _root.Q<VisualElement>("StressBarBackground");
        _stressLabel = _root.Q<Label>("StressLabel");

        _btnStart = _root.Q<Button>("BtnStart");
        _btnSettings = _root.Q<Button>("BtnSettings");
        _btnInfo = _root.Q<Button>("BtnInfo");
        _btnQuit = _root.Q<Button>("BtnQuit");

        if (_titleLabel != null) {
            _titleLabel.RegisterCallback<GeometryChangedEvent>(OnTitleGeometryChanged);
        }
    }

    private void SetupButtonCallbacks() {
        RegisterButtonEffects(_btnStart);
        RegisterButtonEffects(_btnSettings);
        RegisterButtonEffects(_btnInfo);
        RegisterButtonEffects(_btnQuit);

        if (_btnStart != null) _btnStart.clicked += OnStartClick;
        if (_btnSettings != null) _btnSettings.clicked += () => _ = TriggerLateAdviceAsync("Hệ thống: Cài đặt này chỉ là ảo giác của bạn.");
        if (_btnInfo != null) _btnInfo.clicked += () => _ = TriggerLateAdviceAsync("Hệ thống: Thông tin? Bạn có chắc muốn biết không?");
        if (_btnQuit != null) _btnQuit.clicked += HandleQuit;
    }

    private async void OnStartClick() {
        if (_isLoading) return;
        _isLoading = true;

        Debug.Log($"[MainMenu] Loading: {_hudSceneName} → {_gameplaySceneName} → {_defaultLevelSceneName}");

        SceneLoadManager.Instance.OnProgressChanged += OnLoadingProgress;

        await SceneLoadManager.Instance.LoadSceneAdditiveAsync(_hudSceneName);
        await SceneLoadManager.Instance.LoadSceneAsync(_gameplaySceneName);
        await SceneLoadManager.Instance.LoadSceneAsync(_defaultLevelSceneName);

        SceneLoadManager.Instance.OnProgressChanged -= OnLoadingProgress;

        _isLoading = false;
    }

    private void OnLoadingProgress(float progress) {
        if (_stressBarInner != null) {
            float bgWidth = _stressBarBg?.resolvedStyle.width ?? 200f;
            _stressBarInner.style.width = new Length(progress * bgWidth / 2f, LengthUnit.Pixel);
        }
        if (_stressLabel != null) {
            _stressLabel.text = $"CPU STRESS: {Mathf.RoundToInt(progress * 100)}%";
        }
    }

    private void RegisterButtonEffects(Button btn) {
        if (btn == null) return;
        
        btn.RegisterCallback<MouseEnterEvent>(evt => OnButtonHoverEnter(btn));
        btn.RegisterCallback<MouseLeaveEvent>(evt => OnButtonHoverLeave(btn));
        btn.RegisterCallback<MouseDownEvent>(evt => OnButtonPress(btn));
        btn.RegisterCallback<MouseUpEvent>(evt => OnButtonRelease(btn));
        
        btn.style.scale = new StyleScale(Vector2.one);
    }

    private Tween _buttonTween;
    
    private void OnButtonHoverEnter(Button btn) {
        KillButtonTween();
        
        _buttonTween = DOTween.To(() => 1f, x => {
            btn.style.scale = new StyleScale(new Vector2(x * 1.08f, x * 1.08f));
        }, 1f, 0.15f).SetEase(Ease.OutBack);
        
        btn.style.borderBottomWidth = 6f;
        btn.style.borderRightWidth = 6f;
    }

    private void OnButtonHoverLeave(Button btn) {
        KillButtonTween();
        
        btn.style.borderBottomWidth = 4f;
        btn.style.borderRightWidth = 4f;
        
        _buttonTween = DOTween.To(() => 1f, x => {
            btn.style.scale = new StyleScale(new Vector2(x, x));
        }, 1f, 0.1f).SetEase(Ease.OutQuad)
        .OnComplete(() => btn.style.scale = new StyleScale(Vector2.one));
    }

    private void OnButtonPress(Button btn) {
        KillButtonTween();
        
        _buttonTween = DOTween.To(() => 1f, x => {
            btn.style.scale = new StyleScale(new Vector2(x * 0.92f, x * 0.92f));
        }, 1f, 0.05f).SetEase(Ease.InQuad);
    }

    private void OnButtonRelease(Button btn) {
        KillButtonTween();
        
        _buttonTween = DOTween.To(() => 0.92f, x => {
            btn.style.scale = new StyleScale(new Vector2(x, x));
        }, 1f, 0.1f).SetEase(Ease.OutBack)
        .OnComplete(() => btn.style.scale = new StyleScale(Vector2.one));
    }

    private void KillButtonTween() {
        if (_buttonTween != null && _buttonTween.IsActive()) {
            _buttonTween.Kill();
        }
        _buttonTween = null;
    }

    private void OnTitleGeometryChanged(GeometryChangedEvent evt) {
        if (_titleLabel == null) return;
        
        var materialDef = _titleLabel.resolvedStyle.unityMaterial;
        if (materialDef != null && _glitchMaterial == null) {
            _glitchMaterial = new Material(Shader.Find("UI/Default"));
            _titleLabel.style.unityMaterial = new MaterialDefinition(_glitchMaterial);
            _ = GlitchShaderLoopAsync(_cts.Token);
        }
    }

    private void AnimateStressBar(float targetValue) {
        if (_stressBarInner == null) return;
        
        DOTween.To(
            () => 0f,
            x => _stressBarInner.style.width = new Length(x, LengthUnit.Percent),
            targetValue, 2f
        ).SetEase(Ease.InOutQuart);
    }

    private async Awaitable GlitchShaderLoopAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            try {
                await Awaitable.WaitForSecondsAsync(UnityEngine.Random.Range(3f, 6f), token);
            } catch (OperationCanceledException) {
                break;
            }

            if (_glitchMaterial != null && !token.IsCancellationRequested) {
                DOTween.To(() => 0f, x => _glitchMaterial.SetFloat(GlitchIntensityId, x), 1f, 0.1f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.Flash);

                if (_titleLabel != null) {
                    float currentTranslate = 15f;
                    DOTween.To(() => currentTranslate, x => {
                        currentTranslate = x;
                        _titleLabel.style.translate = new StyleTranslate(new Translate(new Length(x, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
                    }, 0f, 0.1f).SetEase(Ease.InOutQuad);
                    
                    _titleLabel.style.color = new StyleColor(new Color(1f, 0f, 0.6f));
                    await Awaitable.WaitForSecondsAsync(0.2f, token);
                    
                    _titleLabel.style.translate = new StyleTranslate(new Translate(new Length(0, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
                    _titleLabel.style.color = new StyleColor(Color.white);
                }
            }
        }
    }

    private async Awaitable TriggerLateAdviceAsync(string message) {
        try {
            await Awaitable.WaitForSecondsAsync(_lateAdviceDelay, _cts.Token);
        } catch (OperationCanceledException) {
            return;
        }

        if (!_cts.Token.IsCancellationRequested) {
            CreatePopup(message);
        }
    }

    private void CreatePopup(string message) {
        if (_popupContainer == null) return;
        
        var popup = new VisualElement();
        popup.AddToClassList("popup-window");
        
        popup.style.left = new StyleLength(new Length(UnityEngine.Random.Range(15, 65), LengthUnit.Percent));
        popup.style.top = new StyleLength(new Length(UnityEngine.Random.Range(15, 65), LengthUnit.Percent));
        popup.style.opacity = 0f;
        popup.style.scale = new StyleScale(new Vector2(0.7f, 0.7f));

        var label = new Label(message);
        label.style.whiteSpace = WhiteSpace.Normal;
        
        var closeBtn = new Button(() => AnimateClosePopup(popup));
        closeBtn.text = "ĐÃ HIỂU (QUÁ MUỘN)";
        
        popup.Add(label);
        popup.Add(closeBtn);
        _popupContainer.Add(popup);

        DOTween.To(() => 0f, x => popup.style.opacity = x, 1f, 0.4f);
        
        float currentScale = 0.7f;
        DOTween.To(
            () => currentScale,
            x => {
                currentScale = x;
                popup.style.scale = new StyleScale(new Vector2(x, x));
            },
            1f, 0.5f
        ).SetEase(Ease.OutBack);
    }

    private void AnimateClosePopup(VisualElement popup) {
        float currentScale = 1f;
        DOTween.To(
            () => currentScale,
            x => {
                currentScale = x;
                popup.style.scale = new StyleScale(new Vector2(x, x));
            },
            0f, 0.2f
        ).OnComplete(() => popup.RemoveFromHierarchy());
    }

    private void HandleQuit() {
        if (_blueScreen == null) return;
        
        _blueScreen.style.display = DisplayStyle.Flex;
        DOTween.To(() => _blueScreen.style.opacity.value, x => _blueScreen.style.opacity = x, 1f, 1f)
            .SetEase(Ease.InCubic)
            .OnComplete(() => {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
    }
}