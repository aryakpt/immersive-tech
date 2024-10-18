using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts;
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
    private GameObject personCubePrefab;
    [SerializeField]
   private GameObject HatPrefab; // Prefab untuk Hat

    [SerializeField]
    private Canvas canvas; // Referensi untuk Canvas

    [SerializeField]
    private Button hatDetectedButton; // Button untuk "hat" deteksi
    [SerializeField]
    private Text displayText; // Teks yang ditampilkan saat button diklik
    [SerializeField]
    private AudioSource audioSource; // Audio yang dimainkan saat button diklik

    private GameObject instantiatedCube;
    private bool personDetected = false;
    private bool hatDetected = false;

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

    private void OnEnable()
    {
        nn = new NNHandler(ModelFile);
        yolo = new YOLOv8Segmentation(nn);

        textureProvider = GetTextureProvider(nn.model);
        textureProvider.Start();

        // Pastikan button dan teks disembunyikan pada awalnya
        hatDetectedButton.gameObject.SetActive(false);
        displayText.gameObject.SetActive(false);

        // Menambahkan listener ke button
        hatDetectedButton.onClick.AddListener(OnHatButtonClick);
    }

    private void Update()
    {
        YOLOv8OutputReader.DiscardThreshold = MinBoxConfidence;
        Texture2D texture = GetNextTexture();

        var boxes = yolo.Run(texture);
        DrawResults(boxes, texture);
        ImageUI.texture = texture;

        // Periksa jika 'person' atau 'hat' terdeteksi
        CheckForPerson(boxes);
        CheckForHat(boxes);
    }

    private void CheckForPerson(List<ResultBox> boxes)
    {
        personDetected = false;

        foreach (var box in boxes)
        {
            if (classNames[box.bestClassIndex] == "book")
            {
                personDetected = true;
                break;
            }
        }

        if (personDetected && instantiatedCube == null)
        {
            // Mendapatkan posisi di depan kamera
            Vector3 cameraForwardPosition = Camera.main.transform.position + Camera.main.transform.forward * 2;
            Vector3 offsetPosition = new Vector3(0, 0, 2);

            instantiatedCube = Instantiate(personCubePrefab);
            instantiatedCube.transform.position = cameraForwardPosition + offsetPosition;
            instantiatedCube.transform.localScale = new Vector3(1, 1, 1);

            Animator hatAnimator = instantiatedCube.GetComponent<Animator>();
            if (hatAnimator != null)
            {
                hatAnimator.SetTrigger("StartAnimation");
            }

            Debug.Log("Instantiating animated Cube at world position: " + (cameraForwardPosition + offsetPosition));
        }
    }

    private void CheckForHat(List<ResultBox> boxes)
    {
        hatDetected = false;

        foreach (var box in boxes)
        {
            if (classNames[box.bestClassIndex] == "hat")
            {
                hatDetected = true;
                break;
            }
        }

        // Tampilkan atau sembunyikan button berdasarkan deteksi "hat"
        if (hatDetected)
        {
            hatDetectedButton.gameObject.SetActive(true);
        }
        else
        {
            hatDetectedButton.gameObject.SetActive(false);
        }
    }

    private void OnHatButtonClick()
    {
        // Tampilkan teks dan mainkan audio ketika button diklik
        displayText.text = "Hat Detected!";
        displayText.gameObject.SetActive(true);
        Vector3 cameraForwardPosition = Camera.main.transform.position + Camera.main.transform.forward * 2;
        Vector3 offsetPosition = new Vector3(0, 0, 2);

        instantiatedCube = Instantiate(HatPrefab);
        instantiatedCube.transform.position = cameraForwardPosition + offsetPosition;
        instantiatedCube.transform.localScale = new Vector3(1, 1, 1);

        Animator hatAnimator = instantiatedCube.GetComponent<Animator>();
        if (hatAnimator != null)
        {
            hatAnimator.SetTrigger("StartAnimation");
        }

        Debug.Log("Instantiating animated Cube at world position: " + (cameraForwardPosition + offsetPosition));
    
        if (audioSource != null)
        {
            audioSource.Play();
        }

        Debug.Log("Hat button clicked: Text displayed and audio played.");
    }

    private void OnDisable()
    {
        if (instantiatedCube != null)
        {
            Destroy(instantiatedCube);
        }
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
        return provider;
    }

    protected Texture2D GetNextTexture()
    {
        return textureProvider.GetTexture();
    }

    protected void DrawResults(IEnumerable<ResultBox> results, Texture2D img)
    {
        boundingBoxes.Clear();
        results.ForEach(box =>
        {
            DrawBox(box, img);
            string className = box.bestClassIndex >= 0 && box.bestClassIndex < classNames.Length ? classNames[box.bestClassIndex] : "Unknown";
            boundingBoxes.Add((box.rect, className));
        });
    }

    protected virtual void DrawBox(ResultBox box, Texture2D img)
    {
        Color boxColor = colorArray[box.bestClassIndex % colorArray.Length];
        int boxWidth = (int)(box.score / MinBoxConfidence);
        TextureTools.DrawRectOutline(img, box.rect, boxColor, boxWidth, rectIsNormalized: false, revertY: true);
    }

    private void OnGUI()
    {
        foreach (var (rect, className) in boundingBoxes)
        {
            Vector2 labelPosition = new Vector2(rect.xMin, rect.yMin);
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 50;

            GUI.Label(new Rect(labelPosition.x, Screen.height - labelPosition.y, 100, 20), className, style);
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
}
