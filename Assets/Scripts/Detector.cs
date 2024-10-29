using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.TextureProviders;
using NN;

public class Detector : MonoBehaviour
{
    [Tooltip("File of YOLO model.")]
    [SerializeField]
    protected NNModel ModelFile;

    [Tooltip("RawImage component which will be used to draw results.")]
    [SerializeField]
    protected RawImage ImageUI;

    [SerializeField]
    private GameObject tableCanvasPrefab;

    [SerializeField]
    private GameObject tableObjectPrefab;

    [SerializeField]
    private GameObject chairCanvasPrefab;
    [SerializeField]
    private GameObject chairObjectPrefab;

    [SerializeField]
    private GameObject whiteboardCanvasPrefab;

    [SerializeField]
    private GameObject whiteboardObjectPrefab;

    [SerializeField]
    private GameObject bookshelfCanvasPrefab;
    [SerializeField]
    private GameObject bookshelfObjectPrefab;

    [SerializeField]
    private GameObject clockCanvasPrefab;
    [SerializeField]
    private GameObject clockObjectPrefab;

    [SerializeField]
    private GameObject wallMagazineCanvasPrefab;
    [SerializeField]
    private GameObject wallMagazineObjectPrefab;

    [SerializeField]
    private GameObject trashCanCanvasPrefab;
    [SerializeField]
    private GameObject trashCanObjectPrefab;

    [SerializeField]
    private GameObject eraserCanvasPrefab;
    [SerializeField]
    private GameObject eraserObjectPrefab;

    [SerializeField]
    private GameObject sharpenerCanvasPrefab;
    [SerializeField]
    private GameObject sharpenerObjectPrefab;

    [SerializeField]
    private GameObject penCanvasPrefab;
    [SerializeField]
    private GameObject penObjectPrefab;

    [SerializeField]
    private GameObject bookCanvasPrefab;
    [SerializeField]
    private GameObject bookObjectPrefab;

    [SerializeField]
    private GameObject rulerCanvasPrefab;
    [SerializeField]
    private GameObject rulerObjectPrefab;

    [SerializeField]
    private GameObject scissorCanvasPrefab;
    [SerializeField]
    private GameObject scissorObjectPrefab;

    [SerializeField]
    private GameObject fanCanvasPrefab;
    [SerializeField]
    private GameObject fanObjectPrefab;

    [SerializeField]
    private GameObject laptopCanvasPrefab;
    [SerializeField]
    private GameObject laptopObjectPrefab;

    [SerializeField]
    private GameObject remoteControlCanvasPrefab;
    [SerializeField]
    private GameObject remoteControlObjectPrefab;

    [SerializeField]
    private GameObject bagCanvasPrefab;
    [SerializeField]
    private GameObject bagObjectPrefab;

    [SerializeField]
    private GameObject pantsCanvasPrefab;
    [SerializeField]
    private GameObject pantsObjectPrefab;

    [SerializeField]
    private GameObject shoesCanvasPrefab;
    [SerializeField]
    private GameObject shoesObjectPrefab;

    [SerializeField]
    private GameObject hatCanvasPrefab;
    [SerializeField]
    private GameObject hatObjectPrefab;

    [SerializeField]
    private Button closeButton;

    [Range(0.0f, 1f)]
    [Tooltip("The minimum value of box confidence below which boxes won't be drawn.")]
    [SerializeField]
    protected float MinBoxConfidence = 0.1f;

    [SerializeField]
    protected TextureProviderType.ProviderType textureProviderType;

    [SerializeReference]
    protected TextureProvider textureProvider = null;

    protected NNHandler nn;
    protected Color[] colorArray = new Color[] { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow };

    YOLOv8 yolo;

    private readonly string[] classNames = new string[]
    {
        "table", "chair", "whiteboard", "bookshelf", "clock", "wall-magazine",
        "trash-can", "eraser", "sharpener", "pen", "book", "ruler", "scissor",
        "fan", "laptop", "remote-control", "bag", "pants", "shoes", "hat",
    };

    private List<(Rect, string)> boundingBoxes = new List<(Rect, string)>();

    [SerializeField]
    public Button buttonPrefab;


    private void OnEnable()
    {
        nn = new NNHandler(ModelFile);
        yolo = new YOLOv8Segmentation(nn);

        textureProvider = GetTextureProvider(nn.model);
        textureProvider.Start();

        tableCanvasPrefab.SetActive(false);
        chairCanvasPrefab.SetActive(false);
        whiteboardCanvasPrefab.SetActive(false);
        bookshelfCanvasPrefab.SetActive(false);
        clockCanvasPrefab.SetActive(false);
        wallMagazineCanvasPrefab.SetActive(false);
        trashCanCanvasPrefab.SetActive(false);
        eraserCanvasPrefab.SetActive(false);
        sharpenerCanvasPrefab.SetActive(false);
        penCanvasPrefab.SetActive(false);
        bookCanvasPrefab.SetActive(false);
        rulerCanvasPrefab.SetActive(false);
        scissorCanvasPrefab.SetActive(false);
        fanCanvasPrefab.SetActive(false);
        laptopCanvasPrefab.SetActive(false);
        remoteControlCanvasPrefab.SetActive(false);
        bagCanvasPrefab.SetActive(false);
        pantsCanvasPrefab.SetActive(false);
        shoesCanvasPrefab.SetActive(false);
        hatCanvasPrefab.SetActive(false);

        closeButton.gameObject.SetActive(false);

        StartCoroutine(CreateButtonsWithDelay());
    }

    private void Update()
    {
        YOLOv8OutputReader.DiscardThreshold = MinBoxConfidence;
        Texture2D texture = GetNextTexture();
        ImageUI.texture = texture;

        var boxes = yolo.Run(texture);
        DrawResults(boxes, texture);
    }

    private void OnDisable()
    {
        nn.Dispose();
        textureProvider.Stop();
    }


    protected TextureProvider GetTextureProvider(Model model)
    {
        var firstInput = model.inputs[0];
        int height = firstInput.shape[5];
        int width = firstInput.shape[6];

        TextureProvider provider;
        switch (textureProviderType)
        {
            case TextureProviderType.ProviderType.WebCam:
                provider = new WebCamTextureProvider(textureProvider as WebCamTextureProvider, width, height);
                break;
            case TextureProviderType.ProviderType.Video:
                provider = new VideoTextureProvider(textureProvider as VideoTextureProvider, width, height);
                break;
            default:
                throw new InvalidEnumArgumentException();
        }

        if (ImageUI != null)
        {
            ImageUI.rectTransform.localEulerAngles = new Vector3(0, 0, -90);
            // ImageUI.uvRect = new Rect(0, 0, 1, -1); // Flip the texture if necessary
        }

        return provider;
    }

    protected Texture2D GetNextTexture()
    {
        return textureProvider.GetTexture();
    }

    private IEnumerator CreateButtonsWithDelay()
    {
        while (true)
        {
            ClearExistingButtons();
            foreach (var box in boundingBoxes)
            {
                CreateButton(box.Item1, box.Item2);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    protected void DrawResults(IEnumerable<ResultBox> results, Texture2D img)
    {
        boundingBoxes.Clear();

        results.ForEach(box =>
        {
            string className = box.bestClassIndex >= 0 && box.bestClassIndex < classNames.Length ? classNames[box.bestClassIndex] : "Unknown";
            boundingBoxes.Add((box.rect, className));
            DrawBox(box, img);
        });
    }

    protected void DrawBox(ResultBox box, Texture2D img)
    {
        Color boxColor = colorArray[box.bestClassIndex % colorArray.Length];
        int boxWidth = (int)(box.score / MinBoxConfidence);
        TextureTools.DrawRectOutline(img, box.rect, boxColor, boxWidth, rectIsNormalized: false, revertY: true);
    }

    private void CreateButton(Rect rect, string className)
    {
        Button button = Instantiate(buttonPrefab, ImageUI.transform);
        button.GetComponentInChildren<Text>().text = className;

        RectTransform rectTransform = button.GetComponent<RectTransform>();

        // Set pivot to top-left to align with the bounding box
        rectTransform.pivot = new Vector2(0, 1);

        // Set anchorMin and anchorMax to match the top-left corner of the bounding box
        rectTransform.anchorMin = new Vector2(rect.xMin / ImageUI.texture.width, 1 - (rect.yMax / ImageUI.texture.height));
        rectTransform.anchorMax = new Vector2(rect.xMin / ImageUI.texture.width, 1 - (rect.yMax / ImageUI.texture.height));

        // Reset the anchored position to (0, 0) so that it's aligned with the anchor
        rectTransform.anchoredPosition = Vector2.zero;

        rectTransform.Rotate(0, 0, 90);

        button.gameObject.SetActive(true);
        button.onClick.AddListener(() => OnButtonClick(className));
    }

    private void ClearExistingButtons()
    {
        foreach (Transform child in ImageUI.transform)
        {
            if (child.gameObject != ImageUI.gameObject)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private static void HandleButtonAction(GameObject canvasPrefab, GameObject objectPrefab, Button closeButton, float y = 0.15f)
    {
        Vector3 cameraForwardPosition = Camera.main.transform.position + Camera.main.transform.forward * 2;
        Vector3 offsetPosition = new Vector3(0, y, 0);

        objectPrefab.transform.position = cameraForwardPosition + offsetPosition;

        canvasPrefab.SetActive(true);
        closeButton.gameObject.SetActive(true);
        closeButton.onClick.AddListener(() =>
        {
            canvasPrefab.SetActive(false);
            closeButton.gameObject.SetActive(false);
        });
    }

    private void OnButtonClick(string className)
    {
        switch (className)
        {
            case "table":
                HandleButtonAction(tableCanvasPrefab, tableObjectPrefab, closeButton);
                break;
            case "chair":
                HandleButtonAction(chairCanvasPrefab, chairObjectPrefab, closeButton, 0.05f);
                break;
            case "whiteboard":
                HandleButtonAction(whiteboardCanvasPrefab, whiteboardObjectPrefab, closeButton, 0.25f);
                break;
            case "bookshelf":
                HandleButtonAction(bookshelfCanvasPrefab, bookshelfObjectPrefab, closeButton, 0.035f);
                break;
            case "clock":
                HandleButtonAction(clockCanvasPrefab, clockObjectPrefab, closeButton);
                break;
            case "wall-magazine":
                HandleButtonAction(wallMagazineCanvasPrefab, wallMagazineObjectPrefab, closeButton);
                break;
            case "trash-can":
                HandleButtonAction(trashCanCanvasPrefab, trashCanObjectPrefab, closeButton);
                break;
            case "eraser":
                HandleButtonAction(eraserCanvasPrefab, eraserObjectPrefab, closeButton);
                break;
            case "sharpener":
                HandleButtonAction(sharpenerCanvasPrefab, sharpenerObjectPrefab, closeButton);
                break;
            case "pen":
                HandleButtonAction(penCanvasPrefab, penObjectPrefab, closeButton);
                break;
            case "book":
                HandleButtonAction(bookCanvasPrefab, bookObjectPrefab, closeButton, 0.05f);
                break;
            case "ruler":
                HandleButtonAction(rulerCanvasPrefab, rulerObjectPrefab, closeButton);
                break;
            case "scissor":
                HandleButtonAction(scissorCanvasPrefab, scissorObjectPrefab, closeButton);
                break;
            case "fan":
                HandleButtonAction(fanCanvasPrefab, fanObjectPrefab, closeButton, 0.05f);
                break;
            case "laptop":
                HandleButtonAction(laptopCanvasPrefab, laptopObjectPrefab, closeButton);
                break;
            case "remote-control":
                HandleButtonAction(remoteControlCanvasPrefab, remoteControlObjectPrefab, closeButton, 0.18f);
                break;
            case "bag":
                HandleButtonAction(bagCanvasPrefab, bagObjectPrefab, closeButton);
                break;
            case "pants":
                HandleButtonAction(pantsCanvasPrefab, pantsObjectPrefab, closeButton, 0.025f);
                break;
            case "shoes":
                HandleButtonAction(shoesCanvasPrefab, shoesObjectPrefab, closeButton, 0.05f);
                break;
            case "hat":

                HandleButtonAction(hatCanvasPrefab, hatObjectPrefab, closeButton);
                break;
            default:
                Debug.Log($"Button clicked for class: {className}");
                break;
        }
    }

    private void OnValidate()
    {
        Type t = TextureProviderType.GetProviderType(textureProviderType);
        if (textureProvider == null || t != textureProvider.GetType())
        {
            if (nn == null)
                textureProvider = RuntimeHelpers.GetUninitializedObject(t) as TextureProvider;
            else
            {
                textureProvider = GetTextureProvider(nn.model);
                textureProvider.Start();
            }
        }
    }
    public static Texture2D RotateTexture90Degrees(Texture2D originalTexture)
    {
        int width = originalTexture.width;
        int height = originalTexture.height;

        Texture2D rotatedTexture = new Texture2D(height, width);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Map each pixel for -90 degree rotation (counterclockwise)
                rotatedTexture.SetPixel(y, width - x - 1, originalTexture.GetPixel(x, y));
            }
        }

        rotatedTexture.Apply();
        return rotatedTexture;
    }
}