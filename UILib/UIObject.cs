using SFKMod.UILib;
using SuperFantasyKingdom.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

public class UIObject
{
    public GameObject Object {  get; private set; }
    public RectTransform Transform { get; private set; }

    // Enum to make anchor positioning more intuitive
    public enum ScreenAnchor
    {
        Center,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public T AddComponent<T>() where T : Component
    {
        return Object.AddComponent<T>();
    }

    public static Vector2 getRandomScreenPosition()
    {
        // Get the current screen resolution
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Generate random x and y coordinates
        float randomX = Random.Range(0f, screenWidth);
        float randomY = Random.Range(0f, screenHeight);

        return new Vector2(randomX, randomY);
    }
    /// <summary>
    /// Create a rectangle at a random location
    /// </summary>
    /// <param name="name"></param>
    /// <param name="color"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static UIObject TestCreateRandomRect(string name, Color color, Transform parent = null)
    {

        var randObj = new UIObject(name, parent)
            .SetBounds(100, 100, absolutePos: getRandomScreenPosition())
            .AddBackground(color);

        return randObj;
    }
    /// <summary>
    /// Create text with a background at a random location
    /// </summary>
    /// <param name="name"></param>
    /// <param name="text"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static UIObject TestCreateRandomTextRect(string name, string text, Transform parent = null)
    {
        var randTextObj = new UIObject(name, parent)
            .SetBounds(100,100, absolutePos: getRandomScreenPosition())
            .AddBackground(Color.black)
            .AddText(text);

        return randTextObj;
    }
    /// <summary>
    /// Test creating a menu like button at a random location
    /// </summary>
    /// <param name="name"></param>
    /// <param name="text"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static UIObject TestCreateRandomMenuButton(string name, string text, Transform parent = null)
    {
        var randButton = new UIObject(name, parent)
            .SetBounds(200, 50, absolutePos: getRandomScreenPosition())
            .AddBackground(new Color(0, 0, 0, 0.5f))
            .AddText(text)
            .MakeClickable(true, () => { Debug.Log("Hello World"); });

        return randButton;
    }
    /// <summary>
    /// Test creating a menu like button
    /// </summary>
    /// <param name="name"></param>
    /// <param name="text"></param>
    /// <param name="onClick"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static UIObject TestCreateMenuButton(string name, string text, UnityAction onClick = null, Transform parent = null)
    {

        var menuButton = new UIObject(name, parent)
            .SetBounds(250, 42)
            .AddBackground(new Color(0, 0, 0, 0.5f), rounded: true)
            .AddText(text)
            .MakeClickable(true, onClick);

        return menuButton;
    }

    public UIObject(string name, Transform parent = null)
    {
        Object = new GameObject(name);
        if (parent != null)
        {
            Object.transform.SetParent(parent, false);
        }
        else
        {
            try
            {
                // Try to set parent default to the main canvas
                var mainCanvas = GameObject.Find("Canvas");
                Object.transform.SetParent(mainCanvas.transform, false);
            } catch
            {
                Debug.Log("Failed to set parent object.");
                throw;
            }
        }

            Transform = Object.AddComponent<RectTransform>();
    }
    /// <summary>
    /// Adds a background, which essentially just fills your bounds with a color.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="rounded"></param>
    /// <returns></returns>
    public UIObject AddBackground(Color color, bool rounded = false)
    {
        // Check if Image component already exists
        Image bg = Object.GetComponent<Image>();

        if (bg == null)
        {
            bg = Object.AddComponent<Image>();
        }

        // TODO: Seperate this out
        Sprite roundedCorners = AssetManager.Instance.GetSpriteByName("UI_27");

        if (roundedCorners != null)
        {
            bg.sprite = roundedCorners;
            bg.type = Image.Type.Sliced;
            bg.fillCenter = true;
        } else
        {
            Debug.LogWarning("Sprite UI_27 not found.");
        }

        bg.color = color;

        return this;
    }
    /// <summary>
    /// Add a text component. If you're adding it to something like a button you should set createChild to true
    /// </summary>
    /// <param name="text"></param>
    /// <param name="createChild"></param>
    /// <returns></returns>
    public UIObject AddText(string text, bool createChild = true)
    {
        TextMeshProUGUI textMesh;
        
        if (createChild)
        {
            // Mirror games component heirarchy
            GameObject textChild = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textChild.transform.SetParent(Object.transform, false);

            // Set up the text child's RectTransform to stretch
            RectTransform textRectTransform = textChild.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            textRectTransform.anchoredPosition = Vector2.zero;

            textMesh = textChild.GetComponent<TextMeshProUGUI>();
        } else
        {
            textMesh = Object.AddComponent<TextMeshProUGUI>();
        }

        textMesh.SetText(text);
        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
        // TODO: Parameterize
        TMP_FontAsset menuFont = AssetManager.Instance.GetFontByName("Compass", exact: false);
        
        textMesh.font = menuFont;

        return this;
    }

    /// <summary>
    /// Make the object clickable
    /// </summary>
    /// <param name="withShake"></param>
    /// <param name="onClick"></param>
    /// <returns></returns>
    public UIObject MakeClickable(bool withShake = false, UnityAction onClick = null)
    {
        Button btn = Object.AddComponent<Button>();
        btn.onClick = new ButtonClickedEvent();
        btn.onClick.AddListener(onClick);

        if (withShake)
        {
            UIClickable sfkClickable = Object.AddComponent<UIClickable>();
            sfkClickable.buttonThatMustBeEnabledAndInteractable = btn;
        }

        return this;
    }

    /// <summary>
    /// This sets your newly created object in a position relative to an existing object. 
    /// Useful for adding your element to existing layouts.
    /// Offset works like objVector + offsetVector e.g (0,-50) moves it down 50 pixels.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="offset"></param>
    /// <returns>'this' UIObject</returns>
    public UIObject RelativeTo(GameObject obj, Vector2 offset)
    {
        if (obj == null)
        {
            Debug.Log($"RelativeTo(): Could not find game object");
            return this;
        }

        var objTransform = obj.transform;
        var position = objTransform.position;

        Vector2 newPos = Vector2.zero;
        if (position != null)
        {
            newPos = position;
            newPos.x += offset.x;
            newPos.y += offset.y;
        }

        Transform.position = newPos;

        return this;
    }
    /// <summary>
    /// Creates a vertical layout with size and spacing you can use in your own code
    /// </summary>
    /// <param name="size"></param>
    /// <param name="padding"></param>
    /// <param name="spacing"></param>
    /// <returns></returns>
    public static GameObject CreateVerticalLayout(Vector2 size, Vector2 location, Color background, int padding= 10, int spacing = 5)
    {
        GameObject canvas = GameObject.Find("Canvas");
        Canvas canvasComp = canvas.GetComponent<Canvas>();
        
        GameObject panel = new GameObject("mPanel");
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.SetParent(canvas.transform, false);

        // Set anchors to top-left corner
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1); 
        panelRect.pivot = new Vector2(0, 1); 


        panelRect.anchoredPosition = location;
        panelRect.sizeDelta = size;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = background;

        VerticalLayoutGroup vLayout = panel.AddComponent<VerticalLayoutGroup>();
        vLayout.padding = new RectOffset(padding, padding, padding, padding);
        vLayout.spacing = spacing;
        vLayout.childAlignment = TextAnchor.UpperCenter;
        vLayout.childControlHeight = false;
        vLayout.childForceExpandHeight = false;
        

        return panel;
    }

    // TODO: Update this
    /// <summary>
    /// Set the size and location of your object
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="screenAnchor"></param>
    /// <param name="offset"></param>
    /// <param name="absolutePos"></param>
    /// <param name="parentCanvas"></param>
    /// <returns></returns>
    public UIObject SetBounds(
        // Size
        int width,
        int height,
        // Location
        ScreenAnchor screenAnchor = ScreenAnchor.Center,
        Vector2? offset = null,
        Vector2? absolutePos = null,
        Canvas parentCanvas = null
        )
    {
        Transform.sizeDelta = new Vector2(width, height);

        if (absolutePos.HasValue)
        {
            Canvas canvas = parentCanvas ?? Object.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("No Canvas found. Unable to convert absolute screen position.");
                return this;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                absolutePos.Value,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint
            );

            Transform.anchorMin = new Vector2(0.5f, 0.5f);
            Transform.anchorMax = new Vector2(0.5f, 0.5f);

            Transform.anchoredPosition = localPoint;
            return this;
        }
        else
        {

            // Handle anchor-based positioning
            switch (screenAnchor)
            {
                case ScreenAnchor.Center:
                    Transform.anchorMin = new Vector2(0.5f, 0.5f);
                    Transform.anchorMax = new Vector2(0.5f, 0.5f);
                    Transform.anchoredPosition = Vector2.zero;
                    break;
                case ScreenAnchor.TopLeft:
                    Transform.anchorMin = new Vector2(0, 1);
                    Transform.anchorMax = new Vector2(0, 1);
                    Transform.anchoredPosition = new Vector2(width / 2, -height / 2);
                    break;
                case ScreenAnchor.TopRight:
                    Transform.anchorMin = new Vector2(1, 1);
                    Transform.anchorMax = new Vector2(1, 1);
                    Transform.anchoredPosition = new Vector2(-width / 2, -height / 2);
                    break;
                case ScreenAnchor.BottomLeft:
                    Transform.anchorMin = new Vector2(0, 0);
                    Transform.anchorMax = new Vector2(0, 0);
                    Transform.anchoredPosition = new Vector2(width / 2, height / 2);
                    break;
                case ScreenAnchor.BottomRight:
                    Transform.anchorMin = new Vector2(1, 0);
                    Transform.anchorMax = new Vector2(1, 0);
                    Transform.anchoredPosition = new Vector2(-width / 2, height / 2);
                    break;
            }
        }

        if (offset.HasValue)
        {
            Transform.anchoredPosition += offset.Value;
        }

        return this;
    }


}