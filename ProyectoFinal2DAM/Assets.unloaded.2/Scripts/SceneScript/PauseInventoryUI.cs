using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseInventoryUI : MonoBehaviour
{
    [SerializeField] Sprite potionIcon;
    [SerializeField] Sprite arrowIcon;
    [SerializeField] Sprite manaPotionIcon;

    Image _potionSlotIcon;
    Image _arrowSlotIcon;
    Image _manaPotionSlotIcon;
    TextMeshProUGUI _potionAmount;
    TextMeshProUGUI _arrowAmount;
    TextMeshProUGUI _manaPotionAmount;
    bool _built;

    GameObject _transferPanel;
    TextMeshProUGUI _transferText;
    int _currentTransferAmount;

    public void Configure(Sprite potion, Sprite arrow, Sprite mana)
    {
        if (potion != null)
            potionIcon = potion;
        if (arrow != null)
            arrowIcon = arrow;
        if (mana != null)
            manaPotionIcon = mana;
        EnsureDefaultSlotSprites();
        if (_built)
            ApplyIcons();
    }

    void OnEnable()
    {
        if (!_built)
            Build();
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnChanged += RefreshLabels;
        SubItems.OnAmountChanged += RefreshLabels;
        RefreshLabels();
    }

    void OnDisable()
    {
        var inv = PlayerInventory.Instance;
        if (inv != null)
            inv.OnChanged -= RefreshLabels;
        SubItems.OnAmountChanged -= RefreshLabels;
    }

    void Build()
    {
        if (_built)
            return;
        _built = true;

        // Reset Root RectTransform
        var rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Root Layout (Vertical)
        var vRoot = gameObject.GetComponent<VerticalLayoutGroup>();
        if (vRoot == null) vRoot = gameObject.AddComponent<VerticalLayoutGroup>();
        vRoot.padding = new RectOffset(60, 60, 40, 60);
        vRoot.spacing = 30;
        vRoot.childAlignment = TextAnchor.UpperCenter;
        vRoot.childControlWidth = true;
        vRoot.childControlHeight = true;
        vRoot.childForceExpandWidth = true;
        vRoot.childForceExpandHeight = false;

        // 1. PAUSA Text - Arriba del todo
        var pauseText = transform.Find("Text");
        if (pauseText != null)
        {
            pauseText.SetParent(transform, false);
            var tmp = pauseText.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                var legacyText = pauseText.GetComponent<Text>();
                if (legacyText != null) DestroyImmediate(legacyText);
                tmp = pauseText.gameObject.AddComponent<TextMeshProUGUI>();
            }
            tmp.text = "PAUSA";
            tmp.fontSize = 80;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            ApplyDefaultFont(tmp);
            
            var textLe = pauseText.GetComponent<LayoutElement>();
            if (textLe == null) textLe = pauseText.gameObject.AddComponent<LayoutElement>();
            textLe.preferredHeight = 100f;
            textLe.flexibleHeight = 0f;
        }

        // 2. Main Content Area (Horizontal)
        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        contentGo.transform.SetParent(transform, false);
        var contentLe = contentGo.AddComponent<LayoutElement>();
        contentLe.flexibleHeight = 1f; // Fill remaining vertical space

        var contentH = contentGo.GetComponent<HorizontalLayoutGroup>();
        contentH.spacing = 50;
        contentH.childControlWidth = true;
        contentH.childControlHeight = true;
        contentH.childForceExpandWidth = false; // Disable force expand to allow PanelVolumen to be narrow
        contentH.childForceExpandHeight = true;

        // 2a. Inventory Panel (Left - BIGGER)
        var panelInvRt = CreatePanelInParent("PanelInventario", contentGo.transform, new Color(0.12f, 0.12f, 0.15f, 0.9f));
        var invLe = panelInvRt.gameObject.AddComponent<LayoutElement>();
        invLe.flexibleWidth = 1f; // Takes all available space

        var vInv = panelInvRt.gameObject.AddComponent<VerticalLayoutGroup>();
        vInv.padding = new RectOffset(35, 35, 35, 35);
        vInv.spacing = 25;
        vInv.childAlignment = TextAnchor.UpperCenter;
        vInv.childControlWidth = true;
        vInv.childControlHeight = true;
        vInv.childForceExpandWidth = true;
        vInv.childForceExpandHeight = false;

        EnsureDefaultSlotSprites();
        BuildInventoryGrid(panelInvRt);

        // 2b. Settings Panel (Right - SMALLER/NARROWER)
        var panelVolRt = CreatePanelInParent("PanelVolumen", contentGo.transform, new Color(0.12f, 0.12f, 0.15f, 0.9f));
        var volLe = panelVolRt.gameObject.AddComponent<LayoutElement>();
        volLe.preferredWidth = 450f; // Limit width to keep sliders small
        volLe.flexibleWidth = 0f;

        var vVol = panelVolRt.gameObject.AddComponent<VerticalLayoutGroup>();
        vVol.padding = new RectOffset(40, 40, 40, 40);
        vVol.spacing = 50;
        vVol.childAlignment = TextAnchor.MiddleCenter;
        vVol.childControlWidth = true;
        vVol.childControlHeight = true;
        vVol.childForceExpandWidth = true;
        vVol.childForceExpandHeight = false;

        ReparentVolumeControls(panelVolRt);

        // 3. Transfer Overlay (Absolute Positioning)
        BuildTransferUI();
    }

    void BuildInventoryGrid(RectTransform parent)
    {
        var titleGo = new GameObject("Titulo", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(parent, false);
        var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
        titleTmp.text = "INVENTARIO";
        titleTmp.fontSize = 36;
        titleTmp.alignment = TextAlignmentOptions.Center;
        ApplyDefaultFont(titleTmp);

        var gridGo = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(parent, false);
        var grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(120, 120);
        grid.spacing = new Vector2(20, 20);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5; // Mas cuadraditos (5 columnas)

        var gridLe = gridGo.AddComponent<LayoutElement>();
        gridLe.flexibleHeight = 1f;

        (_potionSlotIcon, _potionAmount) = CreateItemSlot(gridGo.transform, "SlotPocion", potionIcon, new Color(0.8f, 0.2f, 0.2f), () => {
            if (PlayerInventory.Instance != null) PlayerInventory.Instance.TryUsePotion();
        });

        (_manaPotionSlotIcon, _manaPotionAmount) = CreateItemSlot(gridGo.transform, "SlotMana", manaPotionIcon, new Color(0.2f, 0.2f, 0.8f), () => {
            if (PlayerInventory.Instance != null) PlayerInventory.Instance.TryUseManaPotion();
        });

        (_arrowSlotIcon, _arrowAmount) = CreateItemSlot(gridGo.transform, "SlotFlecha", arrowIcon, new Color(0.2f, 0.8f, 0.2f), () => {
            ShowTransferPanel();
        });

        // Completar hasta 20 slots
        for(int i=0; i<17; i++) {
            CreateItemSlot(gridGo.transform, "SlotEmpty", null, new Color(0.2f, 0.2f, 0.2f, 0.5f), null);
        }
}

        void BuildTransferUI()
        {
        _transferPanel = new GameObject("TransferPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        _transferPanel.transform.SetParent(transform, false);
        
        var le = _transferPanel.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        var rt = _transferPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 320);
        rt.anchoredPosition = Vector2.zero;
        
        var img = _transferPanel.GetComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.12f, 0.98f);
        img.raycastTarget = true;

        var outline = _transferPanel.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(2, -2);

        var v = _transferPanel.GetComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(30, 30, 30, 30);
        v.spacing = 20;
        v.childAlignment = TextAnchor.MiddleCenter;
        v.childControlWidth = true;
        v.childControlHeight = true;

        var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(_transferPanel.transform, false);
        var lTmp = label.GetComponent<TextMeshProUGUI>();
        lTmp.text = "EQUIPAR FLECHAS";
        lTmp.fontSize = 28;
        lTmp.alignment = TextAlignmentOptions.Center;
        lTmp.raycastTarget = false;
        ApplyDefaultFont(lTmp);

        var countGo = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
        countGo.transform.SetParent(_transferPanel.transform, false);
        _transferText = countGo.GetComponent<TextMeshProUGUI>();
        _transferText.text = "0";
        _transferText.fontSize = 48;
        _transferText.alignment = TextAlignmentOptions.Center;
        _transferText.raycastTarget = false;
        ApplyDefaultFont(_transferText);

        var buttonsRow = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        buttonsRow.transform.SetParent(_transferPanel.transform, false);
        var h = buttonsRow.GetComponent<HorizontalLayoutGroup>();
        h.spacing = 15;
        h.childControlWidth = true;
        h.childControlHeight = true;

        CreateTransferBtn(buttonsRow.transform, "-10", () => ChangeTransfer(-10));
        CreateTransferBtn(buttonsRow.transform, "+10", () => ChangeTransfer(10));
        CreateTransferBtn(buttonsRow.transform, "MAX", () => {
if (SubItems.Instance != null) {
                _currentTransferAmount = SubItems.Instance.reserveArrows;
                UpdateTransferDisplay();
            }
        });

        var actionsRow = new GameObject("Actions", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        actionsRow.transform.SetParent(_transferPanel.transform, false);
        var hA = actionsRow.GetComponent<HorizontalLayoutGroup>();
        hA.spacing = 15;
        hA.childControlWidth = true;
        hA.childControlHeight = true;

        CreateTransferBtn(actionsRow.transform, "CANCELAR", () => _transferPanel.SetActive(false));
        CreateTransferBtn(actionsRow.transform, "EQUIPAR", ConfirmTransfer);

        _transferPanel.SetActive(false);
        }

        void CreateTransferBtn(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
        var btnGo = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(parent, false);
        btnGo.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.35f, 1f);
        btnGo.GetComponent<Button>().onClick.AddListener(action);
        var le = btnGo.AddComponent<LayoutElement>();
        le.preferredHeight = 50f;

        var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        var tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        ApplyDefaultFont(tmp);
        
        var rt = txtGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        void ShowTransferPanel()
        {
        if (SubItems.Instance == null) return;
        _currentTransferAmount = 0;
        UpdateTransferDisplay();
        _transferPanel.SetActive(true);
        _transferPanel.transform.SetAsLastSibling();
        }

        void ChangeTransfer(int delta)
        {
        if (SubItems.Instance == null) return;
        _currentTransferAmount = Mathf.Clamp(_currentTransferAmount + delta, 0, SubItems.Instance.reserveArrows);
        UpdateTransferDisplay();
        }

        void UpdateTransferDisplay()
        {
        if (_transferText != null)
            _transferText.text = _currentTransferAmount.ToString();
        }

        void ConfirmTransfer()
        {
        if (SubItems.Instance != null && _currentTransferAmount > 0)
        {
            SubItems.Instance.EquipArrows(_currentTransferAmount);
        }
        _transferPanel.SetActive(false);
        RefreshLabels();
        }

    (Image, TextMeshProUGUI) CreateItemSlot(Transform parent, string name, Sprite icon, Color bgColor, UnityEngine.Events.UnityAction onClick)
    {
        var slot = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        slot.transform.SetParent(parent, false);
        slot.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);
        
        if (onClick != null)
            slot.GetComponent<Button>().onClick.AddListener(onClick);

        var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
        inner.transform.SetParent(slot.transform, false);
        var innerRt = inner.GetComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero; innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(6,6); innerRt.offsetMax = new Vector2(-6,-6);
        inner.GetComponent<Image>().color = new Color(0.28f, 0.28f, 0.35f, 1f);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(slot.transform, false);
        var iconImg = iconGo.GetComponent<Image>();
        iconImg.sprite = icon;
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;
        iconImg.color = icon != null ? Color.white : new Color(0,0,0,0);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.2f, 0.2f); iconRt.anchorMax = new Vector2(0.8f, 0.8f);
        iconRt.offsetMin = Vector2.zero; iconRt.offsetMax = Vector2.zero;

        var amountGo = new GameObject("Amount", typeof(RectTransform), typeof(TextMeshProUGUI));
        amountGo.transform.SetParent(slot.transform, false);
        var tmp = amountGo.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.BottomRight;
        ApplyDefaultFont(tmp);
        var amountRt = amountGo.GetComponent<RectTransform>();
        amountRt.anchorMin = new Vector2(0.5f, 0); amountRt.anchorMax = new Vector2(1, 0.5f);
        amountRt.offsetMin = new Vector2(0, 5); amountRt.offsetMax = new Vector2(-8, 0);

        return (iconImg, tmp);
    }

    void ReparentVolumeControls(RectTransform panelVolRt)
    {
        var music = transform.Find("MusicSlider");
        if (music != null) ReparentControl(panelVolRt, music, "VOLUMEN MÚSICA");

        var effect = transform.Find("EffectSlider");
        if (effect != null) ReparentControl(panelVolRt, effect, "VOLUMEN EFECTOS");
    }

    void ReparentControl(Transform panel, Transform control, string label)
    {
        var container = new GameObject(control.name + "_Group", typeof(RectTransform), typeof(VerticalLayoutGroup));
        container.transform.SetParent(panel, false);
        var v = container.GetComponent<VerticalLayoutGroup>();
        v.spacing = 15;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        
        var leContainer = container.AddComponent<LayoutElement>();
        leContainer.preferredHeight = 100f;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(container.transform, false);
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.95f, 0.95f, 0.95f);
        ApplyDefaultFont(tmp);

        control.SetParent(container.transform, false);
        var controlRt = control as RectTransform;
        controlRt.offsetMin = Vector2.zero;
        controlRt.offsetMax = Vector2.zero;
        
        var leControl = control.GetComponent<LayoutElement>();
        if (leControl == null) leControl = control.gameObject.AddComponent<LayoutElement>();
        leControl.preferredHeight = 25f; // Slider bar thickness
        leControl.minHeight = 20f;
    }

    RectTransform CreatePanelRoot(string name, Color background)
    {
        return CreatePanelInParent(name, transform, background);
    }

    RectTransform CreatePanelInParent(string name, Transform parent, Color background)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = background;
        return go.GetComponent<RectTransform>();
    }


    void EnsureDefaultSlotSprites()
    {
#if UNITY_EDITOR
        if (potionIcon == null)
            potionIcon = LoadSpriteFromTexturePath("Assets/Art/pocion.png", "pocion_0");
        if (arrowIcon == null)
            arrowIcon = LoadSpriteFromTexturePath("Assets/Art/flecha.png", "flecha_0");
#endif
    }

#if UNITY_EDITOR
    static Sprite LoadSpriteFromTexturePath(string assetPath, string spriteName)
    {
        foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath))
        {
            if (obj is Sprite s && s.name == spriteName)
                return s;
        }
        return null;
    }
#endif

    void ApplyIcons()
    {
        if (_potionSlotIcon != null) {
            _potionSlotIcon.sprite = potionIcon;
            if (potionIcon != null) _potionSlotIcon.color = Color.white;
        }
        if (_arrowSlotIcon != null) {
            _arrowSlotIcon.sprite = arrowIcon;
            if (arrowIcon != null) _arrowSlotIcon.color = Color.white;
        }
        if (_manaPotionSlotIcon != null) {
            _manaPotionSlotIcon.sprite = manaPotionIcon;
            if (manaPotionIcon != null) _manaPotionSlotIcon.color = Color.white;
        }
    }

    void RefreshLabels()
    {
        var pCount = PlayerInventory.Instance != null ? PlayerInventory.Instance.PotionCount : 0;
        var mCount = PlayerInventory.Instance != null ? PlayerInventory.Instance.ManaPotionCount : 0;
        var aEquipped = SubItems.Instance != null ? SubItems.Instance.subItemsAmount : 0;
        var aReserve = SubItems.Instance != null ? SubItems.Instance.reserveArrows : 0;

        if (_potionAmount != null) _potionAmount.text = pCount > 0 ? pCount.ToString() : "";
        if (_manaPotionAmount != null) _manaPotionAmount.text = mCount > 0 ? mCount.ToString() : "";
        
        // Muestra Equipadas (Reserva)
        if (_arrowAmount != null) 
            _arrowAmount.text = aEquipped > 0 || aReserve > 0 ? $"{aEquipped} ({aReserve})" : "";

        if (_potionSlotIcon != null) _potionSlotIcon.color = pCount > 0 ? Color.white : new Color(1,1,1,0.2f);
        if (_manaPotionSlotIcon != null) _manaPotionSlotIcon.color = mCount > 0 ? Color.white : new Color(1,1,1,0.2f);
        if (_arrowSlotIcon != null) _arrowSlotIcon.color = aEquipped > 0 || aReserve > 0 ? Color.white : new Color(1,1,1,0.2f);
    }

    static void ApplyDefaultFont(TextMeshProUGUI tmp)
    {
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
    }
}
