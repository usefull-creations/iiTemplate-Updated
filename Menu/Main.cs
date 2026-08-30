using System;
using System.Linq;
using BepInEx;
using GorillaLocomotion;
using HarmonyLib;
using StupidTemplate.Classes;
using StupidTemplate.Notifications;
using StupidTemplate.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using static StupidTemplate.Menu.Settings;
using static StupidTemplate.Menu.Buttons;
using Button = StupidTemplate.Classes.Button;
using Random = UnityEngine.Random;

/*
 * Hello, current and future developers!
 * This is ii's Stupid Template, a base mod menu template for Gorilla Tag.
 *
 * Comments are placed around the code showing you how certain classes work, such as the settings, buttons, and notifications.
 *
 * If you need help with the template, you may join the Discord server https://discord.gg/iidk or contact @zlothyy on Discord
 * It's full of talented developers that can show you the way and how things work.
 *
 * Thank you, and enjoy the template!
 */

namespace StupidTemplate.Menu;

[HarmonyPatch(typeof(GTPlayer), nameof(GTPlayer.LateUpdate))]
public class Main : MonoBehaviour
{

    private const  float TextRenderScale   = 0.001f;
    private const  float TextSurfaceOffset = 0.0035f;
    private static int?  noInvisLayerMask;

    public static bool  gunLocked;
    public static VRRig lockTarget;

    // Variables
    // Important
    // Objects
    public static GameObject menu;
    public static GameObject menuBackground;
    public static GameObject canvasObject;

    private static Texture2D watermarkTexture;
    private static Material  watermarkMaterial;

    private static readonly Vector3 MenuScale = new(
            0.1f,
            0.3f,
            0.3825f);

    private static GameObject searchRoot;

    private static GameObject leftReference;
    private static GameObject rightReference;

    public static SphereCollider leftButtonCollider;
    public static SphereCollider rightButtonCollider;

    private static bool menuButtonWasPressed;
    private static bool menuToggledOpen;

    private static bool menuClosing;
    private static bool menuShouldDropOnClose;

    private static GameObject   searchKeyboard;
    private static string       searchQuery   = string.Empty;
    private static ButtonInfo[] searchResults = Array.Empty<ButtonInfo>();
    private static bool         searchKeyboardFollowing;

    private static bool searchUsingDesktopKeyboard;

    public static Camera TPC;
    public static Text   fpsObject;

    private static GameObject   GunPointer;
    private static LineRenderer GunLine;

    // Data
    public static int pageNumber;

    private static ButtonCategory currentCategory;

    private static readonly ButtonInfo previousPageButton = new()
    {
            buttonText  = "PreviousPage",
            displayText = () => "< Previous Page",
            method      = PreviousPage,
            mode        = ButtonMode.Action,
            toolTip     = "Moves to the previous page.",
    };

    private static readonly ButtonInfo nextPageButton = new()
    {
            buttonText  = "NextPage",
            displayText = () => "Next Page >",
            method      = NextPage,
            mode        = ButtonMode.Action,
            toolTip     = "Moves to the next page.",
    };

    private static readonly ButtonInfo searchButton = new()
    {
            buttonText = "Search",

            displayText = () =>
                                  IsSearching
                                          ? $"Search [{(string.IsNullOrEmpty(searchQuery) ? "..." : searchQuery)}]"
                                          : "Search",

            method  = ToggleSearch,
            mode    = ButtonMode.Action,
            toolTip = "Searches for buttons in the menu.",
    };
    public static bool IsSearching { get; private set; }

    public static ButtonCategory CurrentCategory =>
            currentCategory ??= GetCategory("Main");

    public static void SetCategory(string categoryName)
    {
        ButtonCategory category = GetCategory(categoryName);

        if (category == null)
        {
            Debug.LogError($"{Constants.Name} // Category {categoryName} does not exist.");

            return;
        }

        currentCategory = category;
        pageNumber      = 0;

        if (IsSearching)
            CloseSearch(false);
    }

    // Constant
    public static void Prefix()
    {
        try
        {
            bool menuButtonPressed =
                    !rightHanded &&
                    ControllerInputPoller.instance.leftControllerSecondaryButton ||
                    rightHanded &&
                    ControllerInputPoller.instance.rightControllerSecondaryButton;

            bool keyboardOpen =
                    UnityInput.Current.GetKey(keyboardButton);

            if (IsSearching)
            {
                if (searchUsingDesktopKeyboard)
                    HandleDesktopSearchInput();
                else
                    UpdateSearchLayout();
            }

            if (menu != null)
                UpdateMenuAnimation();

            bool keepOpen =
                    menuButtonPressed ||
                    keyboardOpen      ||
                    IsSearching;

            bool keyboardMode =
                    keyboardOpen ||
                    IsSearching &&
                    searchUsingDesktopKeyboard;

            if (menu == null)
            {
                if (keepOpen)
                {
                    CreateMenu();
                    CreateReferences();

                    RecenterMenu(
                            rightHanded,
                            keyboardMode);
                }
            }
            else if (keepOpen)
            {
                if (menuClosing)
                    CancelMenuClose();

                CreateReferences();

                RecenterMenu(
                        rightHanded,
                        keyboardMode);
            }
            else if (!menuClosing)
            {
                BeginCloseMenu(
                        dropMenu &&
                        !keyboardOpen);
            }
        }
        catch (Exception exc)
        {
            Debug.LogError(
                    $"{Constants.Name} // Error initializing at {exc.StackTrace}: {exc.Message}");
        }

        try
        {
            if (GunPointer != null)
            {
                if (!GunPointer.activeSelf)
                    Destroy(GunPointer);
                else
                    GunPointer.SetActive(false);
            }

            if (GunLine != null)
            {
                if (!GunLine.gameObject.activeSelf)
                {
                    Destroy(GunLine.gameObject);
                    GunLine = null;
                }
                else
                    GunLine.gameObject.SetActive(false);
            }
        }
        catch { }

        try
        {
            if (fpsObject != null)
            {
                fpsObject.text =
                        "FPS: " +
                        Mathf.Ceil(
                                1f /
                                Time.unscaledDeltaTime);
            }

            ButtonInfo[] allButtons =
                    AllButtons;

            foreach (ButtonInfo button in allButtons)
            {
                if (button.mode != ButtonMode.Toggle ||
                    !button.enabled                  ||
                    button.method == null)
                {
                    continue;
                }

                try
                {
                    button.method.Invoke();
                }
                catch (Exception exc)
                {
                    Debug.LogError(
                            $"{Constants.Name} // Error with mod {button.buttonText} at {exc.StackTrace}: {exc.Message}");
                }
            }
        }
        catch (Exception exc)
        {
            Debug.LogError(
                    $"{Constants.Name} // Error with executing mods at {exc.StackTrace}: {exc.Message}");
        }
    }

    // Functions
    public static void CreateMenu(bool playOpenAnimation = true)
    {
        // Menu Holder
        menu = GameObject.CreatePrimitive(PrimitiveType.Cube);

        Destroy(menu.GetComponent<Rigidbody>());
        Destroy(menu.GetComponent<BoxCollider>());
        Destroy(menu.GetComponent<Renderer>());

        menu.transform.localScale = MenuScale;

        menuClosing = false;

        // Menu Background
        menuBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);

        Destroy(menuBackground.GetComponent<Rigidbody>());
        Destroy(menuBackground.GetComponent<BoxCollider>());

        menuBackground.transform.SetParent(
                menu.transform,
                false);

        menuBackground.transform.localRotation =
                Quaternion.identity;

        menuBackground.transform.localPosition =
                new Vector3(
                        0.5f,
                        0f,
                        0f);

        if (roundedButtons)
        {
            RoundedButtonMesh.Apply(
                    menuBackground,
                    menuSize,
                    menuCornerRadius,
                    buttonCornerSegments);
        }
        else
        {
            menuBackground.transform.localScale =
                    menuSize;
        }

        CreateOutline(
                menu.transform,
                menuBackground.transform.localPosition,
                menuSize,
                menuCornerRadius,
                roundedButtons);

        ColorChanger colorChanger =
                menuBackground.AddComponent<ColorChanger>();

        colorChanger.colors =
                backgroundColor;

        colorChanger.spatialGradient =
                buttonGradients;

        colorChanger.verticalGradient =
                verticalButtonGradients;

        CreateWatermark();

        // Canvas
        canvasObject =
                new GameObject("Menu Canvas");

        canvasObject.transform.SetParent(
                menu.transform,
                true);

        Canvas canvas =
                canvasObject.AddComponent<Canvas>();

        CanvasScaler canvasScaler =
                canvasObject.AddComponent<CanvasScaler>();

        canvasObject.AddComponent<GraphicRaycaster>();

        canvas.renderMode =
                RenderMode.WorldSpace;

        canvasScaler.dynamicPixelsPerUnit =
                1500f;

        // Title
        Text titleText = new GameObject
        {
                transform =
                {
                        parent = canvasObject.transform,
                },
        }.AddComponent<Text>();

        titleText.font =
                currentFont;

        titleText.text =
                Constants.Name                        +
                " <color=grey>[</color><color=white>" +
                (pageNumber + 1)                      +
                "</color><color=grey>]</color>";

        titleText.fontSize             = 1;
        titleText.color                = textColors[0];
        titleText.supportRichText      = true;
        titleText.fontStyle            = FontStyle.Italic;
        titleText.alignment            = TextAnchor.MiddleCenter;
        titleText.resizeTextForBestFit = true;
        titleText.resizeTextMinSize    = 0;

        RectTransform titleTransform =
                titleText.GetComponent<RectTransform>();

        titleTransform.localPosition =
                Vector3.zero;

        titleTransform.sizeDelta =
                new Vector2(
                        0.28f,
                        0.05f);

        titleTransform.position =
                new Vector3(
                        0.06f,
                        0f,
                        0.165f);

        titleTransform.rotation =
                Quaternion.Euler(
                        180f,
                        90f,
                        90f);

        // FPS
        if (fpsCounter)
        {
            fpsObject = new GameObject
            {
                    transform =
                    {
                            parent = canvasObject.transform,
                    },
            }.AddComponent<Text>();

            fpsObject.font =
                    currentFont;

            fpsObject.text =
                    "FPS: " +
                    Mathf.Ceil(
                            1f /
                            Time.unscaledDeltaTime);

            fpsObject.color =
                    textColors[0];

            fpsObject.fontSize =
                    1;

            fpsObject.supportRichText =
                    true;

            fpsObject.fontStyle =
                    FontStyle.Italic;

            fpsObject.alignment =
                    TextAnchor.MiddleCenter;

            fpsObject.horizontalOverflow =
                    HorizontalWrapMode.Overflow;

            fpsObject.resizeTextForBestFit =
                    true;

            fpsObject.resizeTextMinSize =
                    0;

            RectTransform fpsTransform =
                    fpsObject.GetComponent<RectTransform>();

            fpsTransform.localPosition =
                    Vector3.zero;

            fpsTransform.sizeDelta =
                    new Vector2(
                            0.28f,
                            0.02f);

            fpsTransform.position =
                    new Vector3(
                            0.06f,
                            0f,
                            0.135f);

            fpsTransform.rotation =
                    Quaternion.Euler(
                            180f,
                            90f,
                            90f);
        }

        // Disconnect
        if (disconnectButton)
        {
            GameObject disconnectbutton =
                    GameObject.CreatePrimitive(
                            PrimitiveType.Cube);

            if (!UnityInput.Current.GetKey(keyboardButton))
                disconnectbutton.layer = 2;

            Destroy(
                    disconnectbutton.GetComponent<Rigidbody>());

            BoxCollider disconnectCollider =
                    disconnectbutton.GetComponent<BoxCollider>();

            disconnectCollider.isTrigger =
                    true;

            disconnectbutton.transform.SetParent(
                    menu.transform,
                    false);

            disconnectbutton.transform.localRotation =
                    Quaternion.identity;

            disconnectbutton.transform.localPosition =
                    new Vector3(
                            0.56f,
                            0f,
                            0.6f);

            Vector3 disconnectSize =
                    buttonSize;

            if (roundedButtons)
            {
                RoundedButtonMesh.Apply(
                        disconnectbutton,
                        disconnectSize,
                        buttonCornerRadius,
                        buttonCornerSegments);
            }
            else
            {
                disconnectbutton.transform.localScale =
                        disconnectSize;
            }

            Button disconnectPress =
                    disconnectbutton.AddComponent<Button>();

            disconnectPress.relatedButton =
                    GetIndex("Disconnect");

            CreateOutline(
                    menu.transform,
                    disconnectbutton.transform.localPosition,
                    disconnectSize,
                    buttonCornerRadius,
                    roundedButtons);

            colorChanger =
                    disconnectbutton.AddComponent<ColorChanger>();

            colorChanger.colors =
                    buttonColors[0];

            colorChanger.spatialGradient =
                    buttonGradients;

            colorChanger.verticalGradient =
                    verticalButtonGradients;

            Text disconnectText = new GameObject
            {
                    transform =
                    {
                            parent = canvasObject.transform,
                    },
            }.AddComponent<Text>();

            disconnectText.text =
                    "Disconnect";

            disconnectText.font =
                    currentFont;

            disconnectText.fontSize =
                    1;

            disconnectText.color =
                    textColors[0];

            disconnectText.alignment =
                    TextAnchor.MiddleCenter;

            disconnectText.resizeTextForBestFit =
                    true;

            disconnectText.resizeTextMinSize =
                    0;

            RectTransform disconnectTransform =
                    disconnectText.GetComponent<RectTransform>();

            disconnectTransform.localPosition =
                    new Vector3(
                            0.064f,
                            0f,
                            0.23f);

            disconnectTransform.sizeDelta =
                    new Vector2(
                            0.2f,
                            0.03f);

            disconnectTransform.rotation =
                    Quaternion.Euler(
                            180f,
                            90f,
                            90f);
        }

        CreateMenuButtons();

        if (animateMenu &&
            playOpenAnimation)
        {
            menu.transform.localScale =
                    MenuScale *
                    0.01f;
        }
    }

    private static void CreateWatermark()
    {
        const string WatermarkName = "watermark.png";

        watermarkTexture ??=
                StupidAssetUtils.LoadTexture2D(WatermarkName);

        if (watermarkTexture == null)
        {
            Debug.LogError(
                    $"{Constants.Name} // Failed to load {WatermarkName}.");

            return;
        }

        if (watermarkMaterial == null)
        {
            Shader shader =
                    Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Debug.LogError(
                        $"{Constants.Name} // Failed to find watermark shader.");

                return;
            }

            watermarkMaterial =
                    new Material(shader)
                    {
                            mainTexture = watermarkTexture,
                            color       = Color.white,
                    };
        }

        GameObject watermark =
                GameObject.CreatePrimitive(
                        PrimitiveType.Quad);

        watermark.name =
                $"{Constants.Name}_Watermark";

        Destroy(
                watermark.GetComponent<Collider>());

        watermark.transform.SetParent(
                menu.transform,
                false);

        float backSurface =
                menuBackground.transform.localPosition.x -
                menuSize.x / 2f                          -
                0.002f;

        watermark.transform.localPosition =
                new Vector3(
                        backSurface,
                        0f,
                        0f);

        watermark.transform.localRotation =
                Quaternion.Euler(
                        0f,
                        -90f,
                        0f) *
                Quaternion.Euler(
                        0f,
                        0f,
                        -90f);

        float aspect =
                watermarkTexture.width /
                (float)watermarkTexture.height;

        const float Height = 0.55f;

        float correctedWidth =
                Height *
                aspect *
                (MenuScale.z / MenuScale.y);

        watermark.transform.localScale =
                new Vector3(
                        correctedWidth,
                        Height,
                        1f);

        Renderer renderer =
                watermark.GetComponent<Renderer>();

        renderer.sharedMaterial =
                watermarkMaterial;
    }

    private static ButtonInfo[] GetVisibleButtons() =>
            IsSearching
                    ? searchResults
                    : CurrentCategory.buttons;

    private static int GetContentButtonsPerPage()
    {
        if (!pageButtonsAtTop)
            return Mathf.Max(1, buttonsPerPage);

        return Mathf.Max(1, buttonsPerPage - 2);
    }

    private static int GetLastPage()
    {
        ButtonInfo[] visibleButtons = GetVisibleButtons();

        if (visibleButtons.Length == 0)
            return 0;

        int pageSize = GetContentButtonsPerPage();

        return (visibleButtons.Length - 1) / pageSize;
    }

    private static void PreviousPage()
    {
        int lastPage = GetLastPage();

        pageNumber--;

        if (pageNumber < 0)
            pageNumber = lastPage;
    }

    private static void NextPage()
    {
        int lastPage = GetLastPage();

        pageNumber++;

        if (pageNumber > lastPage)
            pageNumber = 0;
    }

    private static void CreateMenuButtons()
    {
        ButtonInfo[] visibleButtons = GetVisibleButtons();

        int pageSize  = GetContentButtonsPerPage();
        int rowOffset = 0;

        if (pageButtonsAtTop)
        {
            CreateButton(0f,            previousPageButton);
            CreateButton(buttonSpacing, nextPageButton);

            rowOffset = 2;
        }
        else
            CreateSidePageButtons();

        int lastPage = GetLastPage();

        if (pageNumber > lastPage)
            pageNumber = lastPage;

        int startIndex = pageNumber * pageSize;

        int visibleCount = Math.Min(
                pageSize,
                Math.Max(0, visibleButtons.Length - startIndex));

        for (int i = 0; i < visibleCount; i++)
        {
            CreateButton(
                    (i + rowOffset) * buttonSpacing,
                    visibleButtons[startIndex + i]);
        }

        CreateButton(
                buttonsPerPage * buttonSpacing + 0.04f,
                searchButton);
    }

    private static void CreateSidePageButtons()
    {
        CreateSidePageButton(
                previousPageButton,
                "<",
                0.65f,
                0.195f);

        CreateSidePageButton(
                nextPageButton,
                ">",
                -0.65f,
                -0.195f);
    }

    private static void CreateSidePageButton(
            ButtonInfo button,
            string     textValue,
            float      buttonPosition,
            float      textPosition)
    {
        CreateButtonObject(
                button,
                new Vector3(
                        buttonSize.x,
                        0.2f,
                        0.9f),
                new Vector3(
                        0.56f,
                        buttonPosition,
                        0f),
                true,
                false,
                false);

        Text text = new GameObject
        {
                transform =
                {
                        parent = canvasObject.transform,
                },
        }.AddComponent<Text>();

        text.font =
                currentFont;

        text.text =
                textValue;

        text.fontSize =
                1;

        text.color =
                textColors[0];

        text.alignment =
                TextAnchor.MiddleCenter;

        text.resizeTextForBestFit =
                true;

        text.resizeTextMinSize =
                0;

        RectTransform component =
                text.GetComponent<RectTransform>();

        component.localPosition =
                new Vector3(
                        0.064f,
                        textPosition,
                        0f);

        component.sizeDelta =
                new Vector2(
                        0.2f,
                        0.03f);

        component.rotation =
                Quaternion.Euler(
                        180f,
                        90f,
                        90f);
    }

    private static void CreateSearchKeyboard()
    {
        if (searchUsingDesktopKeyboard ||
            searchKeyboard != null)
        {
            return;
        }

        EnsureSearchRoot();

        searchKeyboard =
                new GameObject(
                        $"{Constants.Name}_SearchKeyboard");

        searchKeyboard.transform.SetParent(
                searchRoot.transform,
                false);

        GameObject background =
                GameObject.CreatePrimitive(
                        PrimitiveType.Cube);

        Destroy(background.GetComponent<Rigidbody>());
        Destroy(background.GetComponent<BoxCollider>());

        background.transform.SetParent(
                searchKeyboard.transform,
                false);

        Vector3 backgroundSize =
                new(
                        0.025f,
                        0.68f,
                        0.28f);

        if (roundedButtons)
        {
            RoundedButtonMesh.Apply(
                    background,
                    backgroundSize,
                    0.025f,
                    buttonCornerSegments);
        }
        else
        {
            background.transform.localScale =
                    backgroundSize;
        }

        ColorChanger backgroundColorChanger =
                background.AddComponent<ColorChanger>();

        backgroundColorChanger.colors =
                backgroundColor;

        backgroundColorChanger.spatialGradient =
                buttonGradients;

        backgroundColorChanger.verticalGradient =
                verticalButtonGradients;

        GameObject keyboardCanvas = new("Canvas");

        keyboardCanvas.transform.SetParent(
                searchKeyboard.transform,
                false);

        Canvas canvas =
                keyboardCanvas.AddComponent<Canvas>();

        CanvasScaler scaler =
                keyboardCanvas.AddComponent<CanvasScaler>();

        canvas.renderMode =
                RenderMode.WorldSpace;

        scaler.dynamicPixelsPerUnit =
                2000f;

        CreateSearchKeyboardRow(
                keyboardCanvas,
                "QWERTYUIOP",
                0.085f);

        CreateSearchKeyboardRow(
                keyboardCanvas,
                "ASDFGHJKL",
                0.03f);

        CreateSearchKeyboardRow(
                keyboardCanvas,
                "ZXCVBNM",
                -0.025f);

        const float BottomZ =
                -0.092f;

        CreateSearchKey(
                keyboardCanvas,
                "SPACE",
                "SPACE",
                -0.185f,
                BottomZ,
                0.25f);

        CreateSearchKey(
                keyboardCanvas,
                "BACK",
                "BACK",
                0.008f,
                BottomZ,
                0.12f);

        CreateSearchKey(
                keyboardCanvas,
                "CLEAR",
                "CLEAR",
                0.131f,
                BottomZ,
                0.11f);

        CreateSearchKey(
                keyboardCanvas,
                "X",
                "CLOSE",
                0.249f,
                BottomZ,
                0.11f);

        ApplySearchLayout();
    }

    private static void CreateSearchKeyboardRow(
            GameObject keyboardCanvas,
            string     characters,
            float      verticalPosition)
    {
        const float KeyWidth = 0.055f;
        const float KeyGap   = 0.008f;

        float totalWidth =
                characters.Length       * KeyWidth +
                (characters.Length - 1) * KeyGap;

        float startPosition =
                -totalWidth / 2f +
                KeyWidth    / 2f;

        for (int i = 0; i < characters.Length; i++)
        {
            string character = characters[i].ToString();

            float horizontalPosition =
                    startPosition +
                    i * (KeyWidth + KeyGap);

            CreateSearchKey(
                    keyboardCanvas,
                    character,
                    character,
                    horizontalPosition,
                    verticalPosition,
                    KeyWidth);
        }
    }

    private static void CreateSearchKey(
            GameObject keyboardCanvas,
            string     label,
            string     value,
            float      horizontalPosition,
            float      verticalPosition,
            float      width)
    {
        const float KeyDepth  = 0.023f;
        const float KeyHeight = 0.045f;

        float localHorizontalPosition =
                -horizontalPosition;

        GameObject key =
                GameObject.CreatePrimitive(
                        PrimitiveType.Cube);

        key.layer = 2;

        Destroy(key.GetComponent<Rigidbody>());

        key.transform.SetParent(
                searchKeyboard.transform,
                false);

        key.transform.localPosition = new Vector3(
                0.025f,
                localHorizontalPosition,
                verticalPosition);

        Vector3 keySize = new(
                KeyDepth,
                width,
                KeyHeight);

        BoxCollider collider =
                key.GetComponent<BoxCollider>();

        collider.isTrigger = true;

        SearchKeyButton keyButton =
                key.AddComponent<SearchKeyButton>();

        keyButton.value = value;

        if (roundedButtons)
        {
            RoundedButtonMesh.Apply(
                    key,
                    keySize,
                    0.01f,
                    buttonCornerSegments);
        }
        else
            key.transform.localScale = keySize;

        ColorChanger colorChanger =
                key.AddComponent<ColorChanger>();

        colorChanger.colors           = buttonColors[0];
        colorChanger.spatialGradient  = buttonGradients;
        colorChanger.verticalGradient = verticalButtonGradients;

        Text text = new GameObject
        {
                transform =
                {
                        parent = keyboardCanvas.transform,
                },
        }.AddComponent<Text>();

        text.font                 = currentFont;
        text.text                 = label;
        text.fontSize             = 1;
        text.color                = textColors[0];
        text.alignment            = TextAnchor.MiddleCenter;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize    = 0;

        RectTransform component =
                text.GetComponent<RectTransform>();

        component.localPosition = new Vector3(
                0.04f,
                localHorizontalPosition,
                verticalPosition);

        component.sizeDelta = new Vector2(
                width * 0.85f,
                0.034f);

        component.rotation =
                Quaternion.Euler(
                        180f,
                        90f,
                        90f);
    }

    public static void CreateButton(
            float      offset,
            ButtonInfo method)
    {
        float zPosition =
                0.28f -
                offset;

        if (method.mode == ButtonMode.Incremental &&
            incrementalButtons)
        {
            const float CenterWidth     = 0.52f;
            const float IncrementWidth  = 0.17f;
            const float IncrementOffset = 0.365f;
            const float TextScale       = 0.3f;

            CreateButtonObject(
                    method,
                    new Vector3(
                            buttonSize.x,
                            IncrementWidth,
                            buttonSize.z),
                    new Vector3(
                            0.56f,
                            IncrementOffset,
                            zPosition),
                    true,
                    true,
                    false);

            CreateButtonObject(
                    method,
                    new Vector3(
                            buttonSize.x,
                            CenterWidth,
                            buttonSize.z),
                    new Vector3(
                            0.56f,
                            0f,
                            zPosition),
                    false,
                    false,
                    false);

            CreateButtonObject(
                    method,
                    new Vector3(
                            buttonSize.x,
                            IncrementWidth,
                            buttonSize.z),
                    new Vector3(
                            0.56f,
                            -IncrementOffset,
                            zPosition),
                    true,
                    true,
                    true);

            CreateButtonText(
                    "-",
                    IncrementOffset * TextScale,
                    offset,
                    0.04f,
                    textColors[0]);

            CreateButtonText(
                    method.GetDisplayText(),
                    0f,
                    offset,
                    0.12f,
                    textColors[0]);

            CreateButtonText(
                    "+",
                    -IncrementOffset * TextScale,
                    offset,
                    0.04f,
                    textColors[0]);

            return;
        }

        CreateButtonObject(
                method,
                buttonSize,
                new Vector3(
                        0.56f,
                        0f,
                        zPosition),
                true,
                false,
                false);

        CreateButtonText(
                method.GetDisplayText(),
                0f,
                offset,
                0.2f,
                method.enabled
                        ? textColors[1]
                        : textColors[0]);
    }

    private static GameObject CreateButtonObject(
            ButtonInfo method,
            Vector3    size,
            Vector3    localPosition,
            bool       clickable,
            bool       incremental,
            bool       positive)
    {
        GameObject buttonObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

        if (!UnityInput.Current.GetKey(keyboardButton))
            buttonObject.layer = 2;

        Destroy(buttonObject.GetComponent<Rigidbody>());

        buttonObject.transform.parent        = menu.transform;
        buttonObject.transform.rotation      = Quaternion.identity;
        buttonObject.transform.localPosition = localPosition;

        BoxCollider collider = buttonObject.GetComponent<BoxCollider>();

        if (clickable)
        {
            collider.isTrigger = true;

            Button button = buttonObject.AddComponent<Button>();
            button.relatedButton = method;
            button.incremental   = incremental;
            button.positive      = positive;
        }
        else
            Destroy(collider);

        bool useRounded = method.useRounded ?? roundedButtons;

        if (useRounded)
            RoundedButtonMesh.Apply(buttonObject, size, buttonCornerRadius, buttonCornerSegments);
        else
            buttonObject.transform.localScale = size;

        CreateOutline(
                menu.transform,
                localPosition,
                size,
                buttonCornerRadius,
                useRounded);

        ExtGradient colors = method.enabled
                                     ? method.enabledColor  ?? buttonColors[1]
                                     : method.disabledColor ?? buttonColors[0];

        ColorChanger colorChanger = buttonObject.AddComponent<ColorChanger>();
        colorChanger.colors           = colors;
        colorChanger.spatialGradient  = method.useGradient ?? buttonGradients;
        colorChanger.verticalGradient = verticalButtonGradients;

        return buttonObject;
    }

    private static void CreateButtonText(
            string value,
            float  localY,
            float  offset,
            float  width,
            Color  color)
    {
        Text text = new GameObject
        {
                transform =
                {
                        parent = canvasObject.transform,
                },
        }.AddComponent<Text>();

        text.font                 = currentFont;
        text.text                 = value;
        text.supportRichText      = true;
        text.fontSize             = 1;
        text.color                = color;
        text.alignment            = TextAnchor.MiddleCenter;
        text.fontStyle            = FontStyle.Italic;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize    = 0;

        RectTransform component =
                text.GetComponent<RectTransform>();

        component.localPosition =
                new Vector3(
                        0.064f,
                        localY,
                        0.111f -
                        offset / 2.6f);

        component.sizeDelta =
                new Vector2(
                        width,
                        0.03f);

        component.rotation =
                Quaternion.Euler(
                        180f,
                        90f,
                        90f);
    }

    private static void CreateOutline(
            Transform parent,
            Vector3   localPosition,
            Vector3   size,
            float     cornerRadius,
            bool      rounded)
    {
        if (!outlines)
            return;

        GameObject outline = GameObject.CreatePrimitive(PrimitiveType.Cube);

        outline.name  = "UI Outline";
        outline.layer = 2;

        Destroy(outline.GetComponent<Rigidbody>());
        Destroy(outline.GetComponent<BoxCollider>());

        outline.transform.SetParent(parent, false);
        outline.transform.localPosition = localPosition;
        outline.transform.localRotation = Quaternion.identity;

        Vector3 outlineSize = new(
                size.x * 0.8f,
                size.y + outlineThickness,
                size.z + outlineThickness);

        if (rounded)
        {
            RoundedButtonMesh.Apply(
                    outline,
                    outlineSize,
                    cornerRadius + outlineThickness / 2f,
                    buttonCornerSegments);
        }
        else
            outline.transform.localScale = outlineSize;

        ColorChanger colorChanger = outline.AddComponent<ColorChanger>();
        colorChanger.colors = outlineColor;
    }

    public static void RecreateMenu()
    {
        if (menu == null)
            return;

        Transform previousParent =
                menu.transform.parent;

        Vector3 previousPosition =
                menu.transform.position;

        Quaternion previousRotation =
                menu.transform.rotation;

        Vector3 previousLocalPosition =
                menu.transform.localPosition;

        Quaternion previousLocalRotation =
                menu.transform.localRotation;

        Destroy(menu);

        menu =
                null;

        CreateMenu(false);

        if (previousParent != null)
        {
            menu.transform.SetParent(
                    previousParent,
                    false);

            menu.transform.localPosition =
                    previousLocalPosition;

            menu.transform.localRotation =
                    previousLocalRotation;
        }
        else
        {
            menu.transform.position =
                    previousPosition;

            menu.transform.rotation =
                    previousRotation;
        }

        menu.transform.localScale =
                MenuScale;

        if (IsSearching &&
            !searchUsingDesktopKeyboard)
        {
            ApplySearchLayout();
        }
    }

    public static void RecenterMenu(
            bool isRightHanded,
            bool isKeyboardCondition)
    {
        if (menu == null)
            return;

        if (IsSearching &&
            !searchUsingDesktopKeyboard)
        {
            UpdateSearchLayout();

            return;
        }

        if (!isKeyboardCondition)
        {
            if (menu.transform.parent != null)
            {
                menu.transform.SetParent(
                        null,
                        true);
            }

            if (!isRightHanded)
            {
                menu.transform.position =
                        GorillaTagger.Instance
                                     .leftHandTransform
                                     .position;

                menu.transform.rotation =
                        GorillaTagger.Instance
                                     .leftHandTransform
                                     .rotation;
            }
            else
            {
                menu.transform.position =
                        GorillaTagger.Instance
                                     .rightHandTransform
                                     .position;

                Vector3 rotation =
                        GorillaTagger.Instance
                                     .rightHandTransform
                                     .rotation
                                     .eulerAngles;

                rotation +=
                        new Vector3(
                                0f,
                                0f,
                                180f);

                menu.transform.rotation =
                        Quaternion.Euler(
                                rotation);
            }

            return;
        }

        try
        {
            TPC =
                    GameObject.Find(
                                       "Player Objects/Third Person Camera/Shoulder Camera")
                             ?.GetComponent<Camera>();
        }
        catch
        {
            // ignored
        }

        GameObject.Find("Shoulder Camera")
                 ?.transform
                  .Find("CM vcam1")
                 ?.gameObject
                  .SetActive(false);

        if (TPC == null)
            return;

        TPC.transform.position =
                new Vector3(
                        -999f,
                        -999f,
                        -999f);

        TPC.transform.rotation =
                Quaternion.identity;

        GameObject bg =
                GameObject.CreatePrimitive(
                        PrimitiveType.Cube);

        bg.transform.localScale =
                new Vector3(
                        10f,
                        10f,
                        0.01f);

        bg.transform.position =
                TPC.transform.position +
                TPC.transform.forward;

        Color realColor =
                backgroundColor.GetCurrentColor();

        bg.GetComponent<Renderer>()
          .material
          .color =
                new Color32(
                        (byte)(realColor.r * 50),
                        (byte)(realColor.g * 50),
                        (byte)(realColor.b * 50),
                        255);

        Destroy(
                bg,
                0.05f);

        menu.transform.SetParent(
                TPC.transform,
                true);

        menu.transform.position =
                TPC.transform.position       +
                TPC.transform.forward * 0.5f +
                TPC.transform.up      * -0.02f;

        menu.transform.rotation =
                TPC.transform.rotation *
                Quaternion.Euler(
                        -90f,
                        90f,
                        0f);

        if (!Mouse.current.leftButton.isPressed)
            return;

        Ray ray =
                TPC.ScreenPointToRay(
                        Mouse.current.position.ReadValue());

        if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    100f))
            return;

        Button collide =
                hit.transform
                   .gameObject
                   .GetComponent<Button>();

        SphereCollider simulatedCollider =
                rightButtonCollider != null
                        ? rightButtonCollider
                        : leftButtonCollider;

        if (simulatedCollider != null)
            collide?.OnTriggerEnter(simulatedCollider);
    }

    private static void UpdateMenuAnimation()
    {
        if (menu == null)
            return;

        Vector3 targetScale =
                menuClosing
                        ? Vector3.zero
                        : MenuScale;

        if (!animateMenu)
        {
            menu.transform.localScale =
                    targetScale;

            if (menuClosing)
                FinishCloseMenu();

            return;
        }

        float interpolation =
                1f -
                Mathf.Exp(
                        -menuAnimationSpeed *
                        Time.unscaledDeltaTime);

        menu.transform.localScale =
                Vector3.Lerp(
                        menu.transform.localScale,
                        targetScale,
                        interpolation);

        if (!menuClosing)
            return;

        if (menu.transform.localScale.sqrMagnitude >
            0.00001f)
            return;

        FinishCloseMenu();
    }

    private static void BeginCloseMenu(bool shouldDrop)
    {
        if (menu == null ||
            menuClosing)
            return;

        menuClosing =
                true;

        menuShouldDropOnClose =
                shouldDrop;

        if (searchRoot != null &&
            menu.transform.parent ==
            searchRoot.transform)
        {
            menu.transform.SetParent(
                    null,
                    true);
        }

        if (shouldDrop)
        {
            Rigidbody rigidbody =
                    menu.GetComponent<Rigidbody>();

            if (rigidbody == null)
                rigidbody = menu.AddComponent<Rigidbody>();

            rigidbody.linearVelocity =
                    (rightHanded
                             ? GTPlayer.Instance.RightHand.velocityTracker
                             : GTPlayer.Instance.LeftHand.velocityTracker)
                   .GetAverageVelocity(
                            true,
                            0);
        }

        if (!animateMenu)
            FinishCloseMenu();
    }

    private static void CancelMenuClose()
    {
        if (menu == null)
            return;

        menuClosing =
                false;

        menuShouldDropOnClose =
                false;

        Rigidbody rigidbody =
                menu.GetComponent<Rigidbody>();

        if (rigidbody != null)
            Destroy(rigidbody);
    }

    private static void FinishCloseMenu()
    {
        if (menu == null)
            return;

        GameObject oldMenu = menu;

        menu        = null;
        menuClosing = false;
        bool dropped = menuShouldDropOnClose;
        menuShouldDropOnClose = false;

        DestroyReferences();

        GameObject shoulderCamera =
                GameObject.Find(
                        "Shoulder Camera");

        shoulderCamera
              ?.transform
               .Find("CM vcam1")
              ?.gameObject
               .SetActive(true);

        if (dropped && !animateMenu)
            Destroy(oldMenu, 2f);
        else
            Destroy(oldMenu);
    }

    private static void CreateReferences()
    {
        if (leftReference == null)
        {
            leftReference = CreateReference(
                    GorillaTagger.Instance.leftHandTransform,
                    out leftButtonCollider);
        }

        if (rightReference == null)
        {
            rightReference = CreateReference(
                    GorillaTagger.Instance.rightHandTransform,
                    out rightButtonCollider);
        }

        bool searchKeyboardOpen =
                IsSearching                 &&
                !searchUsingDesktopKeyboard &&
                searchKeyboard != null;

        bool leftActive =
                searchKeyboardOpen ||
                rightHanded;

        bool rightActive =
                searchKeyboardOpen ||
                !rightHanded;

        leftReference.SetActive(
                leftActive);

        rightReference.SetActive(
                rightActive);
    }

    private static GameObject CreateReference(
            Transform          hand,
            out SphereCollider collider)
    {
        GameObject handReference =
                GameObject.CreatePrimitive(
                        PrimitiveType.Sphere);

        handReference.name =
                $"{Constants.Name}_ButtonReference";

        handReference.transform.SetParent(
                hand,
                false);

        handReference.transform.localPosition =
                new Vector3(
                        0f,
                        -0.1f,
                        0f);

        handReference.transform.localRotation =
                Quaternion.identity;

        handReference.transform.localScale =
                new Vector3(
                        0.01f,
                        0.01f,
                        0.01f);

        collider =
                handReference.GetComponent<SphereCollider>();

        ColorChanger colorChanger =
                handReference.AddComponent<ColorChanger>();

        colorChanger.colors =
                backgroundColor;

        colorChanger.spatialGradient =
                false;

        return handReference;
    }

    private static void DestroyReferences()
    {
        if (leftReference != null)
            Destroy(leftReference);

        if (rightReference != null)
            Destroy(rightReference);

        leftReference  = null;
        rightReference = null;

        leftButtonCollider  = null;
        rightButtonCollider = null;
    }

    public static bool TryGetReferenceHand(
            Collider collider,
            out bool rightHand)
    {
        if (collider != null &&
            collider == rightButtonCollider)
        {
            rightHand = true;

            return true;
        }

        if (collider != null &&
            collider == leftButtonCollider)
        {
            rightHand = false;

            return true;
        }

        rightHand = false;

        return false;
    }

    // This is an overload, it allows you to do two different methods of toggling a mod, either by name, or by a ButtonInfo.
    public static void Toggle(string buttonText, bool? incrementDirection = null)
    {
        ButtonInfo button = GetIndex(buttonText);

        if (button == null)
        {
            Debug.LogError($"{Constants.Name} // Button {buttonText} does not exist.");

            return;
        }

        Toggle(button, incrementDirection);
    }

    public static void Toggle(ButtonInfo target, bool? incrementDirection = null)
    {
        switch (target.mode)
        {
            case ButtonMode.Toggle:
            {
                target.enabled = !target.enabled;

                if (target.enabled)
                {
                    NotifiLib.SendNotification(
                            "<color=grey>[</color><color=green>ENABLE</color><color=grey>]</color> " +
                            target.toolTip);

                    if (target.enableMethod != null)
                    {
                        try
                        {
                            target.enableMethod.Invoke();
                        }
                        catch (Exception exc)
                        {
                            Debug.LogError($"{Constants.Name} // Error enabling {target.buttonText}: {exc}");
                        }
                    }
                }
                else
                {
                    NotifiLib.SendNotification(
                            "<color=grey>[</color><color=red>DISABLE</color><color=grey>]</color> " +
                            target.toolTip);

                    if (target.disableMethod != null)
                    {
                        try
                        {
                            target.disableMethod.Invoke();
                        }
                        catch (Exception exc)
                        {
                            Debug.LogError($"{Constants.Name} // Error disabling {target.buttonText}: {exc}");
                        }
                    }
                }

                break;
            }

            case ButtonMode.Action:
            {
                if (target.method != null)
                {
                    try
                    {
                        target.method.Invoke();
                    }
                    catch (Exception exc)
                    {
                        Debug.LogError($"{Constants.Name} // Error invoking {target.buttonText}: {exc}");
                    }
                }

                break;
            }

            case ButtonMode.Incremental:
            {
                bool positive = incrementDirection ?? true;

                if (target.incrementMethod != null)
                {
                    try
                    {
                        target.incrementMethod.Invoke(positive);
                    }
                    catch (Exception exc)
                    {
                        Debug.LogError($"{Constants.Name} // Error changing {target.buttonText}: {exc}");
                    }
                }

                break;
            }
        }

        Preferences.Save();

        RecreateMenu();
    }

    private static void ToggleSearch()
    {
        if (IsSearching)
        {
            CloseSearch(false);

            return;
        }

        IsSearching =
                true;

        searchQuery =
                string.Empty;

        pageNumber =
                0;

        searchUsingDesktopKeyboard =
                UnityInput.Current.GetKey(keyboardButton) ||
                menu != null &&
                TPC  != null &&
                menu.transform.parent ==
                TPC.transform;

        RefreshSearchResults();

        if (!searchUsingDesktopKeyboard)
        {
            EnsureSearchRoot();
            CreateSearchKeyboard();

            UpdateSearchLayout(true);
        }
    }

    private static void CloseSearch(bool recreate)
    {
        IsSearching =
                false;

        searchQuery =
                string.Empty;

        searchResults =
                Array.Empty<ButtonInfo>();

        if (menu       != null &&
            searchRoot != null &&
            menu.transform.parent ==
            searchRoot.transform)
        {
            menu.transform.SetParent(
                    null,
                    true);
        }

        if (searchKeyboard != null)
        {
            Destroy(searchKeyboard);

            searchKeyboard =
                    null;
        }

        if (searchRoot != null)
        {
            Destroy(searchRoot);

            searchRoot =
                    null;
        }

        searchKeyboardFollowing =
                false;

        searchUsingDesktopKeyboard =
                false;

        pageNumber =
                0;

        if (recreate)
            RecreateMenu();
    }

    private static void EnsureSearchRoot()
    {
        if (searchRoot != null)
            return;

        searchRoot =
                new GameObject(
                        $"{Constants.Name}_SearchRoot");
    }

    private static void UpdateSearchLayout(
            bool snap = false)
    {
        if (searchUsingDesktopKeyboard)
            return;

        EnsureSearchRoot();

        Camera headCamera =
                Camera.main;

        if (headCamera == null)
            return;

        Transform head =
                headCamera.transform;

        Vector3 forward =
                Vector3.ProjectOnPlane(
                        head.forward,
                        Vector3.up);

        if (forward.sqrMagnitude < 0.001f)
            forward = head.forward;

        forward.Normalize();

        Vector3 targetPosition =
                head.position;

        Quaternion targetRotation =
                Quaternion.LookRotation(
                        forward,
                        Vector3.up);

        if (snap)
        {
            searchRoot.transform.position =
                    targetPosition;

            searchRoot.transform.rotation =
                    targetRotation;
        }
        else
        {
            float interpolation =
                    1f -
                    Mathf.Exp(
                            -searchKeyboardFollowSpeed *
                            Time.unscaledDeltaTime);

            searchRoot.transform.position =
                    Vector3.Lerp(
                            searchRoot.transform.position,
                            targetPosition,
                            interpolation);

            searchRoot.transform.rotation =
                    Quaternion.Slerp(
                            searchRoot.transform.rotation,
                            targetRotation,
                            interpolation);
        }

        ApplySearchLayout();
    }

    private static void ApplySearchLayout()
    {
        if (searchRoot == null)
            return;

        if (menu != null)
        {
            if (menu.transform.parent !=
                searchRoot.transform)
            {
                menu.transform.SetParent(
                        searchRoot.transform,
                        false);
            }

            menu.transform.localPosition =
                    new Vector3(
                            0f,
                            0.02f,
                            0.70f);

            menu.transform.rotation =
                    searchRoot.transform.rotation *
                    Quaternion.Euler(
                            -90f,
                            90f,
                            0f);
        }

        if (searchKeyboard == null)
            return;

        if (searchKeyboard.transform.parent !=
            searchRoot.transform)
        {
            searchKeyboard.transform.SetParent(
                    searchRoot.transform,
                    false);
        }

        searchKeyboard.transform.localPosition =
                new Vector3(
                        0f,
                        -0.4f,
                        0.56f);

        Vector3 keyboardUp =
                (Vector3.up +
                 searchRoot.transform.forward)
               .normalized;

        searchKeyboard.transform.rotation =
                Quaternion.LookRotation(
                        keyboardUp,
                        -searchRoot.transform.right);
    }

    private static void HandleDesktopSearchInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        foreach (Key keyCode in from key in keyboard.allKeys where key is { wasPressedThisFrame: true, } select key.keyCode)
        {
                switch (keyCode)
                {
                        case Key.Backspace:
                        {
                                HandleSearchKey("BACK");

                                return;
                        }

                        case Key.Space:
                        {
                                HandleSearchKey("SPACE");

                                return;
                        }

                        case Key.Delete:
                        {
                                HandleSearchKey("CLEAR");

                                return;
                        }

                        case Key.Escape:
                        {
                                HandleSearchKey("CLOSE");

                                return;
                        }
                }

                if (keyCode >= Key.A &&
                    keyCode <= Key.Z)
                {
                        HandleSearchKey(
                                        keyCode.ToString());

                        return;
                }

                if (keyCode >= Key.Digit1 &&
                    keyCode <= Key.Digit9)
                {
                        string keyName =
                                        keyCode.ToString();

                        HandleSearchKey(
                                        keyName.Substring(
                                                        keyName.Length - 1));

                        return;
                }

                if (keyCode == Key.Digit0)
                {
                        HandleSearchKey("0");

                        return;
                }
        }
    }

    private static void RefreshSearchResults()
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            searchResults = [];

            return;
        }

        ButtonInfo[] allButtons = AllButtons;

        searchResults = (from button in allButtons let displayText = button.GetDisplayText() let nameMatch = displayText.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 let toolTipMatch = !string.IsNullOrEmpty(button.toolTip) && button.toolTip.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 where nameMatch || toolTipMatch select button).ToArray();
    }

    public static void HandleSearchKey(string value)
    {
        if (!IsSearching)
            return;

        switch (value)
        {
            case "BACK":
            {
                if (searchQuery.Length > 0)
                    searchQuery = searchQuery.Substring(0, searchQuery.Length - 1);

                break;
            }

            case "SPACE":
            {
                if (searchQuery.Length < 32)
                    searchQuery += " ";

                break;
            }

            case "CLEAR":
            {
                searchQuery = string.Empty;

                break;
            }

            case "CLOSE":
            {
                CloseSearch(true);

                return;
            }

            default:
            {
                if (searchQuery.Length < 32)
                    searchQuery += value;

                break;
            }
        }

        pageNumber = 0;

        RefreshSearchResults();
        RecreateMenu();
    }

    public static Vector3 RandomVector3(float range = 1f) =>
            new(Random.Range(-range,     range),
                    Random.Range(-range, range),
                    Random.Range(-range, range));

    public static Quaternion RandomQuaternion(float range = 360f) =>
            Quaternion.Euler(Random.Range(0f, range),
                    Random.Range(0f,          range),
                    Random.Range(0f,          range));

    public static Color RandomColor(byte range = 255, byte alpha = 255) =>
            new Color32((byte)Random.Range(0, range),
                    (byte)Random.Range(0,     range),
                    (byte)Random.Range(0,     range),
                    alpha);

    public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueLeftHand()
    {
        Quaternion rot = GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handRotOffset;

        return (GorillaTagger.Instance.leftHandTransform.position + GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
    }

    public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueRightHand()
    {
        Quaternion rot = GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handRotOffset;

        return (GorillaTagger.Instance.rightHandTransform.position + GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
    }

    public static void WorldScale(GameObject obj, Vector3 targetWorldScale)
    {
        Vector3 parentScale = obj.transform.parent.lossyScale;
        obj.transform.localScale = new Vector3(
                targetWorldScale.x / parentScale.x,
                targetWorldScale.y / parentScale.y,
                targetWorldScale.z / parentScale.z
        );
    }

    public static void FixStickyColliders(GameObject platform)
    {
        Vector3[] localPositions =
        [
                        new(0, 1f, 0),
                new(0, -1f, 0),
                new(1f, 0, 0),
                new(-1f, 0, 0),
                new(0, 0, 1f),
                new(0, 0, -1f),
        ];

        Quaternion[] localRotations =
        [
                        Quaternion.Euler(90,  0,   0),
                Quaternion.Euler(-90, 0,   0),
                Quaternion.Euler(0,   -90, 0),
                Quaternion.Euler(0,   90,  0),
                Quaternion.identity,
                Quaternion.Euler(0, 180, 0),
        ];

        for (int i = 0; i < localPositions.Length; i++)
        {
            GameObject side = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                if (platform.GetComponent<GorillaSurfaceOverride>() != null)
                {
                    side.AddComponent<GorillaSurfaceOverride>().overrideIndex = platform.GetComponent<GorillaSurfaceOverride>().overrideIndex;
                }
            }
            catch { }

            const float Size = 0.025f;
            side.transform.SetParent(platform.transform);
            side.transform.position = localPositions[i] * (Size / 2);
            side.transform.rotation = localRotations[i];
            WorldScale(side, new Vector3(Size, Size, 0.01f));
            side.GetComponent<Renderer>().enabled = false;
        }
    }

    public static (RaycastHit Ray, GameObject NewPointer) RenderGun(
            int?        overrideLayerMask = null,
            GunSettings settings          = null)
    {
        GunSettings currentSettings = settings ?? gunSettings;

        Transform gunTransform = currentSettings.hand == XRNode.LeftHand
                                         ? GorillaTagger.Instance.leftHandTransform
                                         : GorillaTagger.Instance.rightHandTransform;

        Vector3 startPosition = gunTransform.position;
        Vector3 direction     = gunTransform.forward;

        Vector3 rayOrigin = startPosition + direction / 4f;

        bool didHit;

        RaycastHit ray;

        if (overrideLayerMask.HasValue)
        {
                didHit = Physics.Raycast(
                                rayOrigin,
                                direction,
                                out ray,
                                currentSettings.maxDistance,
                                overrideLayerMask.Value,
                                QueryTriggerInteraction.Ignore);
        }
        else
        {
                didHit = GunRaycast(
                                rayOrigin,
                                direction,
                                currentSettings.maxDistance,
                                out ray);
        }

        Vector3 endPosition;

        if (gunLocked && lockTarget != null)
            endPosition = lockTarget.transform.position;
        else if (didHit)
            endPosition = ray.point;
        else
            endPosition = startPosition + direction * currentSettings.maxDistance;

        if (GunPointer == null)
        {
            GunPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            Destroy(GunPointer.GetComponent<Collider>());

            Renderer pointerRenderer = GunPointer.GetComponent<Renderer>();
            pointerRenderer.material = new Material(Shader.Find("GUI/Text Shader"));
        }

        GunPointer.SetActive(true);
        GunPointer.transform.position   = endPosition;
        GunPointer.transform.localScale = Vector3.one * currentSettings.pointerSize;

        Renderer currentPointerRenderer = GunPointer.GetComponent<Renderer>();
        currentPointerRenderer.enabled = currentSettings.showPointer;

        bool pressed =
                gunLocked ||
                ControllerInputPoller.TriggerFloat(currentSettings.hand) > 0.5f;

        ExtGradient pointerColors = pressed
                                            ? currentSettings.pointerActiveColor ?? buttonColors[1]
                                            : currentSettings.pointerIdleColor   ?? buttonColors[0];

        currentPointerRenderer.material.color = pointerColors.GetCurrentColor();

        if (GunLine == null)
        {
            GameObject lineObject = new($"{Constants.Name}_GunLine");

            GunLine                   = lineObject.AddComponent<LineRenderer>();
            GunLine.useWorldSpace     = true;
            GunLine.numCapVertices    = 4;
            GunLine.numCornerVertices = 4;
            GunLine.material          = new Material(Shader.Find("GUI/Text Shader"));
        }

        GunLine.gameObject.SetActive(true);

        ExtGradient lineColors = currentSettings.lineColor ?? backgroundColor;

        GunLine.startColor = lineColors.GetCurrentColor();
        GunLine.endColor   = lineColors.GetCurrentColor(0.5f);

        GunLine.startWidth = currentSettings.lineWidth;
        GunLine.endWidth   = currentSettings.lineWidth;

        RenderGunLine(
                startPosition,
                endPosition,
                gunTransform,
                currentSettings);

        return (ray, GunPointer);
    }
    
    private static bool GunRaycast(
                    Vector3        origin,
                    Vector3        direction,
                    float          maxDistance,
                    out RaycastHit hit)
    {
            Ray ray = new(origin, direction);

            RaycastHit[] hits = Physics.RaycastAll(
                            ray,
                            maxDistance,
                            ~0,
                            QueryTriggerInteraction.Ignore);

            hit = default(RaycastHit);

            float closestDistance = float.MaxValue;

            foreach (RaycastHit candidate in hits)
            {
                    Collider collider = candidate.collider;

                    if (collider == null)
                    {
                            continue;
                    }

                    int layerMask = 1 << collider.gameObject.layer;

                    bool isInteractable =
                                    (layerMask & GTPlayer.Instance.locomotionEnabledLayers) != 0;

                    VRRig rig = collider.GetComponentInParent<VRRig>();

                    bool isRig =
                                    rig != null &&
                                    rig != VRRig.LocalRig;

                    if (!isInteractable && !isRig)
                    {
                            continue;
                    }

                    if (candidate.distance >= closestDistance)
                    {
                            continue;
                    }

                    closestDistance = candidate.distance;
                    hit             = candidate;
            }

            return hit.collider != null;
    }

    private static void RenderGunLine(
            Vector3     startPosition,
            Vector3     endPosition,
            Transform   gunTransform,
            GunSettings settings)
    {
        if (settings.lineStyle == GunLineStyle.Straight)
        {
            GunLine.positionCount = 2;

            GunLine.SetPosition(0, startPosition);
            GunLine.SetPosition(1, endPosition);

            return;
        }

        int quality = Mathf.Clamp(settings.lineQuality, 4, 64);

        GunLine.positionCount = quality;

        if (settings.lineStyle == GunLineStyle.Curve)
        {
            Vector3 controlPoint =
                    Vector3.Lerp(startPosition, endPosition, 0.5f) +
                    gunTransform.up * settings.curveAmount;

            for (int i = 0; i < quality; i++)
            {
                float t        = i / (float)(quality - 1);
                float inverseT = 1f - t;

                Vector3 position =
                        inverseT * inverseT * startPosition    +
                        2f       * inverseT * t * controlPoint +
                        t        * t        * endPosition;

                GunLine.SetPosition(i, position);
            }

            return;
        }

        Vector3 lineDirection = endPosition - startPosition;

        Vector3 waveAxis = Vector3.Cross(
                lineDirection.normalized,
                gunTransform.up);

        if (waveAxis.sqrMagnitude < 0.001f)
            waveAxis = gunTransform.right;

        waveAxis.Normalize();

        for (int i = 0; i < quality; i++)
        {
            float t = i / (float)(quality - 1);

            Vector3 position = Vector3.Lerp(
                    startPosition,
                    endPosition,
                    t);

            float edgeFade = Mathf.Sin(t * Mathf.PI);

            float wave =
                    Mathf.Sin(
                            t         * Mathf.PI * 2f * settings.waveFrequency +
                            Time.time * 8f) *
                    settings.waveAmount     *
                    edgeFade;

            position += waveAxis * wave;

            GunLine.SetPosition(i, position);
        }
    }
}