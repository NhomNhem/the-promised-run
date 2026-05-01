using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using ThePromisedRun.UI;
using System.Threading;
using System;

namespace ThePromisedRun.UI {
public class MainMenuController : MonoBehaviour {
    [Header("Scene Config")]
    [SerializeField] private string _gameplaySceneName = "Scene_GamePlay";
    [SerializeField] private string _hudSceneName = "Scene_HUD";
    [SerializeField] private string _defaultLevelSceneName = "Level_01_WelcomeHall";

    [Header("UI Settings")]
    [SerializeField] private float _lateAdviceDelay = 1.5f;
    [SerializeField] private float _stressLevel = 42f;

    [Header("Loading Screen")]
    [SerializeField] private float _loadingFadeInDuration      = 0.3f;
    [SerializeField] private float _loadingFadeOutDuration     = 0.4f;
    [SerializeField] private float _loadingMinDisplayTime      = 0.8f;  // prevent flash on fast loads
    [SerializeField] private float _loadingBarCompleteDuration = 0.5f;  // animate bar to 100% after load
    [SerializeField] private float _loadingCompleteHoldTime    = 0.4f;  // hold at 100% before fade-out

    // Tracks the current displayed progress (0-1) for smooth bar animation
    private float _displayedProgress = 0f;

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

    private SettingsPanelController _settingsPanel;

    private VisualElement _loadingOverlay;
    private VisualElement _loadingBarFill;
    private Label _loadingLabel;

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

        _settingsPanel = GetComponent<SettingsPanelController>();
        _settingsPanel?.Initialize(_root);
        
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

        _loadingOverlay = _root.Q<VisualElement>("LoadingOverlay");
        _loadingBarFill = _root.Q<VisualElement>("LoadingBarFill");
        _loadingLabel   = _root.Q<Label>("LoadingLabel");
        if (_loadingOverlay != null) {
            _loadingOverlay.style.display = DisplayStyle.None;
            _loadingOverlay.style.opacity = 0f;
        }

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
        if (_btnSettings != null) _btnSettings.clicked += () => _settingsPanel?.Open();
        if (_btnInfo != null) _btnInfo.clicked += () => _ = TriggerLateAdviceAsync("Hệ thống: Thông tin? Bạn có chắc muốn biết không?");
        if (_btnQuit != null) _btnQuit.clicked += HandleQuit;
    }

    private async void OnStartClick() {
        if (_isLoading) return;
        _isLoading = true;

        Debug.Log($"[MainMenu] Loading: {_hudSceneName} → {_gameplaySceneName} → {_defaultLevelSceneName}");

        if (SceneLoadManager.Instance == null) {
            Debug.LogError("[MainMenu] SceneLoadManager.Instance is null. Aborting.");
            _isLoading = false;
            return;
        }

        // Step 1: Fade-in overlay — wait until fully visible before loading
        await ShowLoadingOverlayAsync();

        SceneLoadManager.Instance.OnProgressChanged += OnLoadingProgress;

        // Step 2: Load gameplay scenes only (NOT HUD — it would render over loading screen)
        await SceneLoadManager.Instance.LoadSceneAdditiveAsync(_gameplaySceneName);
        await SceneLoadManager.Instance.LoadSceneAdditiveAsync(_defaultLevelSceneName);

        // Set Level as active scene (for lighting, physics)
        var levelScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(_defaultLevelSceneName);
        if (levelScene.IsValid())
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(levelScene);

        SceneLoadManager.Instance.OnProgressChanged -= OnLoadingProgress;

        // Step 3: Animate bar to 100% — gives "completion" feel even if load was instant
        await AnimateProgressToCompleteAsync();

        // Step 4: Hold at 100% briefly so user sees the completed state
        await Awaitable.WaitForSecondsAsync(_loadingCompleteHoldTime);

        // Step 5: Fade-out overlay
        await HideLoadingOverlayAsync();

        // Step 6: Load HUD scene NOW — after loading screen is gone, so it never overlaps
        await SceneLoadManager.Instance.LoadSceneAdditiveAsync(_hudSceneName);

        // Step 7: Fade-in HUD with animation
        HUDManager hudManager = FindFirstObjectByType<HUDManager>();
        hudManager?.FadeIn();

        // Step 8: Unload MainMenu last (after all gameplay scenes are ready)
        await SceneLoadManager.Instance.UnloadSceneAsync("Scene_MainMenu");

        _isLoading = false;
    }

    /// <summary>Fade-in the loading overlay using DOTween.</summary>
    private async Awaitable ShowLoadingOverlayAsync() {
        if (_loadingOverlay == null) return;

        _displayedProgress = 0f;
        _loadingOverlay.style.opacity = 0f;
        _loadingOverlay.style.display = DisplayStyle.Flex;

        if (_loadingBarFill != null)
            _loadingBarFill.style.width = new Length(0f, LengthUnit.Percent);
        if (_loadingLabel != null)
            _loadingLabel.text = "LOADING... 0%";

        // Animate opacity 0 → 1 and wait for completion
        bool done = false;
        DOTween.To(
            () => _loadingOverlay.style.opacity.value,
            x  => _loadingOverlay.style.opacity = x,
            1f, _loadingFadeInDuration
        ).SetEase(Ease.OutCubic)
         .SetUpdate(true)
         .OnComplete(() => done = true);

        while (!done)
            await Awaitable.NextFrameAsync();
    }

    /// <summary>
    /// Animate progress bar from current displayed value to 100%.
    /// Skipped if already at 100% to avoid double-completion flash.
    /// </summary>
    private async Awaitable AnimateProgressToCompleteAsync() {
        if (_loadingBarFill == null) return;

        // Already at 100% — no need to animate again
        if (_displayedProgress >= 1f) return;

        float startProgress = _displayedProgress;
        bool done = false;

        DOTween.To(
            () => startProgress,
            x  => {
                startProgress = x;
                if (_loadingBarFill != null)
                    _loadingBarFill.style.width = new Length(x * 100f, LengthUnit.Percent);
                if (_loadingLabel != null)
                    _loadingLabel.text = $"LOADING... {Mathf.RoundToInt(x * 100)}%";
            },
            1f, _loadingBarCompleteDuration
        ).SetEase(Ease.OutQuart)
         .SetUpdate(true)
         .OnComplete(() => done = true);

        while (!done)
            await Awaitable.NextFrameAsync();
    }

    /// <summary>Fade-out the loading overlay using DOTween, then hide it.</summary>
    private async Awaitable HideLoadingOverlayAsync() {
        if (_loadingOverlay == null) return;

        bool done = false;
        DOTween.To(
            () => _loadingOverlay.style.opacity.value,
            x  => _loadingOverlay.style.opacity = x,
            0f, _loadingFadeOutDuration
        ).SetEase(Ease.InCubic)
         .SetUpdate(true)
         .OnComplete(() => done = true);

        while (!done)
            await Awaitable.NextFrameAsync();

        _loadingOverlay.style.display = DisplayStyle.None;
    }

    private void OnLoadingProgress(float progress) {
        // Update stress bar (decorative)
        if (_stressBarInner != null) {
            float bgWidth = _stressBarBg?.resolvedStyle.width ?? 200f;
            _stressBarInner.style.width = new Length(progress * bgWidth / 2f, LengthUnit.Pixel);
        }
        if (_stressLabel != null)
            _stressLabel.text = $"CPU STRESS: {Mathf.RoundToInt(progress * 100)}%";

        // Track real progress — AnimateProgressToCompleteAsync will pick up from here
        _displayedProgress = progress;
        if (_loadingBarFill != null)
            _loadingBarFill.style.width = new Length(progress * 100f, LengthUnit.Percent);
        if (_loadingLabel != null)
            _loadingLabel.text = $"LOADING... {Mathf.RoundToInt(progress * 100)}%";
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
}