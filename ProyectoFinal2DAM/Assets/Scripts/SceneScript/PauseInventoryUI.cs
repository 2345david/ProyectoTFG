using TMPro;              // Nos deja usar textos bonitos en la pantalla (TextMeshPro)
using UnityEngine;
using UnityEngine.UI;     // Nos deja usar imágenes, botones y cajas que ordenan los elementos

/// <summary>
/// Crea con código la pantalla del inventario que aparece en el menú de pausa.
/// Muestra los objetos (poción, poción de maná, flechas) con su cantidad,
/// unos controles de volumen y una ventanita para equipar flechas.
/// Las cantidades y los dibujos siempre se mantienen al día con el inventario del jugador.
/// </summary>
public class PauseInventoryUI : MonoBehaviour
{
    [SerializeField] Sprite potionIcon;       // Dibujo de la poción de vida
    [SerializeField] Sprite arrowIcon;        // Dibujo de las flechas
    [SerializeField] Sprite manaPotionIcon;   // Dibujo de la poción de maná

    Image _potionSlotIcon;                 // La imagen del dibujo en la casilla de la poción
    Image _arrowSlotIcon;                  // La imagen del dibujo en la casilla de las flechas
    Image _manaPotionSlotIcon;             // La imagen del dibujo en la casilla de la poción de maná
    TextMeshProUGUI _potionAmount;         // Texto que muestra cuántas pociones tienes
    TextMeshProUGUI _arrowAmount;          // Texto que muestra las flechas equipadas (y las de reserva)
    TextMeshProUGUI _manaPotionAmount;     // Texto que muestra cuántas pociones de maná tienes
    bool _built;                           // Vale "sí" cuando la pantalla ya está creada (para no crearla dos veces)

    GameObject _transferPanel;             // La ventanita para equipar flechas
    TextMeshProUGUI _transferText;         // Texto que muestra cuántas flechas vas a equipar
    int _currentTransferAmount;            // Cuántas flechas tienes elegidas ahora mismo para equipar

    // Recibe del menú de pausa los dibujos de los objetos y los guarda. Si la pantalla ya existe, los vuelve a mostrar.
    public void Configure(Sprite potion, Sprite arrow, Sprite mana)
    {
        // Solo cambia cada dibujo si nos pasan uno de verdad (no vacío)
        if (potion != null)
            potionIcon = potion;
        if (arrow != null)
            arrowIcon = arrow;
        if (mana != null)
            manaPotionIcon = mana;
        // Si falta algún dibujo, intenta poner uno por defecto
        EnsureDefaultSlotSprites();
        // Si la pantalla ya está creada, vuelve a colocar los dibujos en las casillas
        if (_built)
            ApplyIcons();
    }

    // Cuando se enciende el panel: crea la pantalla (si hace falta) y se "apunta" a los avisos del inventario
    // para que los números se actualicen solos cuando cambia algo.
    void OnEnable()
    {
        // La primera vez que se abre el panel, crea toda la pantalla
        if (!_built)
            Build();
        // Pide al inventario que le avise cada vez que cambie
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnChanged += RefreshLabels;
        // Pide que le avisen cuando cambie la cantidad de flechas
        SubItems.OnAmountChanged += RefreshLabels;
        // Pone los números al día ahora mismo
        RefreshLabels();
    }

    // Cuando se apaga el panel: deja de escuchar esos avisos, para no gastar memoria ni llamar a algo apagado.
    void OnDisable()
    {
        var inv = PlayerInventory.Instance;
        if (inv != null)
            inv.OnChanged -= RefreshLabels;
        SubItems.OnAmountChanged -= RefreshLabels;
    }

    // Crea con código toda la pantalla del inventario: el título "PAUSA", la zona con las casillas de objetos,
    // el panel de volumen y la ventanita de equipar flechas. Solo se hace una vez.
    void Build()
    {
        // Evita reconstruir la UI si ya fue creada
        if (_built)
            return;
        _built = true;

        // Estira el RectTransform raíz para ocupar todo el panel padre
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
        contentH.childForceExpandWidth = false; // Sin expansión forzada para que PanelVolumen pueda ser estrecho
        contentH.childForceExpandHeight = true;

        // 2a. Panel de inventario (izquierda - más grande)
        var panelInvRt = CreatePanelInParent("PanelInventario", contentGo.transform, new Color(0.12f, 0.12f, 0.15f, 0.9f));
        var invLe = panelInvRt.gameObject.AddComponent<LayoutElement>();
        invLe.flexibleWidth = 1f; // Ocupa todo el espacio disponible

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

        // 3. Panel emergente para equipar flechas (posicionamiento absoluto, superpuesto)
        BuildTransferUI();

        // 4. Botón "Ir al menú" (definido en la escena): el VerticalLayoutGroup de este
        //    panel lo aplastaría a altura 0 y el Content se dibujaría encima. Lo sacamos
        //    del layout (ignoreLayout) y lo ponemos como último hijo para que quede
        //    visible y por encima de todo, conservando su posición/anclaje de la escena.
        EnsureGoToMenuButtonOnTop();
    }

    /// <summary>
    /// Asegura que el botón "Ir al menú" (añadido en la escena) no sea gestionado por el
    /// VerticalLayoutGroup del panel y se dibuje por encima del resto del contenido.
    /// </summary>
    void EnsureGoToMenuButtonOnTop()
    {
        var goMenuBtn = transform.Find("GoToMenuButton");
        if (goMenuBtn == null) return;

        var le = goMenuBtn.GetComponent<LayoutElement>();
        if (le == null) le = goMenuBtn.gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;          // El layout group lo ignora (conserva tamaño/posición)

        goMenuBtn.SetAsLastSibling();    // Se dibuja por encima del contenido del inventario
    }

    // Crea el título "INVENTARIO" y la cuadrícula de casillas: poción, poción de maná, flechas y casillas vacías.
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

        // Slot de poción de maná: al pulsarlo intenta usar una poción de maná
        (_manaPotionSlotIcon, _manaPotionAmount) = CreateItemSlot(gridGo.transform, "SlotMana", manaPotionIcon, new Color(0.2f, 0.2f, 0.8f), () => {
            if (PlayerInventory.Instance != null) PlayerInventory.Instance.TryUseManaPotion();
        });

        // Slot de flechas: al pulsarlo abre el panel para equipar flechas
        (_arrowSlotIcon, _arrowAmount) = CreateItemSlot(gridGo.transform, "SlotFlecha", arrowIcon, new Color(0.2f, 0.8f, 0.2f), () => {
            ShowTransferPanel();
        });

        // Rellena hasta completar 20 slots con slots vacíos
        for(int i=0; i<17; i++) {
            CreateItemSlot(gridGo.transform, "SlotEmpty", null, new Color(0.2f, 0.2f, 0.2f, 0.5f), null);
        }
}

        // Crea la ventanita para equipar flechas: un título, un número y botones (-10, +10, MAX, CANCELAR, EQUIPAR). Empieza escondida.
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

        // Crea un botón dentro de la ventanita: le pone su texto y le dice qué hacer al pulsarlo.
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

        /// <summary>
        /// Muestra el panel emergente de equipar flechas, reiniciando la cantidad a 0
        /// y trayéndolo al frente. No hace nada si no existe SubItems.
        /// </summary>
        void ShowTransferPanel()
        {
        if (SubItems.Instance == null) return;
        _currentTransferAmount = 0;
        UpdateTransferDisplay();
        _transferPanel.SetActive(true);
        _transferPanel.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Modifica la cantidad seleccionada a equipar, limitándola entre 0 y las flechas en reserva.
        /// </summary>
        void ChangeTransfer(int delta)
        {
        if (SubItems.Instance == null) return;
        _currentTransferAmount = Mathf.Clamp(_currentTransferAmount + delta, 0, SubItems.Instance.reserveArrows);
        UpdateTransferDisplay();
        }

        /// <summary>
        /// Actualiza el texto del contador del panel emergente con la cantidad seleccionada.
        /// </summary>
        void UpdateTransferDisplay()
        {
        if (_transferText != null)
            _transferText.text = _currentTransferAmount.ToString();
        }

        /// <summary>
        /// Confirma la operación: equipa la cantidad seleccionada de flechas (si es > 0),
        /// cierra el panel emergente y refresca las etiquetas del inventario.
        /// </summary>
        void ConfirmTransfer()
        {
        if (SubItems.Instance != null && _currentTransferAmount > 0)
        {
            SubItems.Instance.EquipArrows(_currentTransferAmount);
        }
        _transferPanel.SetActive(false);
        RefreshLabels();
        }

    // Crea una casilla del inventario: el fondo, el dibujo del objeto y el texto de la cantidad.
    // Devuelve la imagen y el texto para poder cambiarlos después. Si recibe una acción, la casilla se vuelve un botón.
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

    /// <summary>
    /// Mueve los sliders de música y efectos existentes en la jerarquía al panel de
    /// volumen, agrupándolos con su etiqueta correspondiente.
    /// </summary>
    void ReparentVolumeControls(RectTransform panelVolRt)
    {
        var music = transform.Find("MusicSlider");
        if (music != null) ReparentControl(panelVolRt, music, "VOLUMEN MÚSICA");

        var effect = transform.Find("EffectSlider");
        if (effect != null) ReparentControl(panelVolRt, effect, "VOLUMEN EFECTOS");
    }

    /// <summary>
    /// Crea un grupo (etiqueta + control) dentro del panel indicado y reubica el
    /// control de volumen dentro de él ajustando su tamaño en el layout.
    /// </summary>
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

    // NOTA: Se eliminó el método privado 'CreatePanelRoot' por ser código muerto
    // (estaba definido pero nunca se llamaba desde ningún sitio).

    /// <summary>
    /// Crea un panel básico (GameObject con RectTransform e Image de fondo) como hijo
    /// del padre indicado y devuelve su RectTransform.
    /// </summary>
    RectTransform CreatePanelInParent(string name, Transform parent, Color background)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = background;
        return go.GetComponent<RectTransform>();
    }


    /// <summary>
    /// Asigna iconos por defecto (poción y flecha) cargándolos desde los assets del
    /// proyecto solo en el Editor, en caso de que no se hayan asignado en el Inspector.
    /// </summary>
    void EnsureDefaultSlotSprites()
    {
#if UNITY_EDITOR
        if (potionIcon == null)
            potionIcon = LoadSpriteFromTexturePath("Assets/Art/Items/pocion.png", "pocion_0");
        if (arrowIcon == null)
            arrowIcon = LoadSpriteFromTexturePath("Assets/Art/Items/flecha.png", "flecha_0");
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// (Solo Editor) Carga un Sprite concreto por nombre desde una textura/atlas en la
    /// ruta indicada usando AssetDatabase. Devuelve null si no lo encuentra.
    /// </summary>
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

    /// <summary>
    /// Aplica los sprites actuales a los iconos de cada slot, poniéndolos en blanco
    /// (visibles) cuando hay sprite asignado.
    /// </summary>
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

    // Pone al día los números y los dibujos de cada casilla según lo que tiene el jugador.
    // Si tienes 0 de algo, el texto se deja vacío y el dibujo se ve más apagado.
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

    /// <summary>
    /// Aplica la fuente por defecto de TextMeshPro al texto indicado, si está configurada.
    /// </summary>
    static void ApplyDefaultFont(TextMeshProUGUI tmp)
    {
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
    }
}
