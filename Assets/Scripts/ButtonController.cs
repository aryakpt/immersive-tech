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

public class ButtonController : MonoBehaviour
{
    public Button yourButton; // Button yang akan diklik
    public Text displayText;  // Teks yang akan ditampilkan
    public AudioSource audioSource; // Audio yang akan diputar
    public string message = "Hat Detected!"; // Pesan yang akan ditampilkan

    // Tambahkan referensi ke prefab dan posisi kamera
    [SerializeField]
    public GameObject HatPrefab; // Prefab untuk ditampilkan setelah button diklik
    private GameObject instantiatedPrefab;
    private GameObject instantiatedCube;
   
    void Start()
    {
        // Menyembunyikan teks pada awalnya
        displayText.gameObject.SetActive(false);

        // Menambahkan listener untuk button
        yourButton.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        // Tampilkan teks
        displayText.text = message;
        displayText.gameObject.SetActive(true);
        Vector3 cameraForwardPosition = Camera.main.transform.position + Camera.main.transform.forward * 2;
        Vector3 offsetPosition = new Vector3(0, 0, 2);

        instantiatedCube = Instantiate(HatPrefab);
        instantiatedCube.transform.position = cameraForwardPosition + offsetPosition;
        instantiatedCube.transform.localScale = new Vector3(3, 3, 3);

        Animator hatAnimator = instantiatedCube.GetComponent<Animator>();
        if (hatAnimator != null)
        {
            hatAnimator.SetTrigger("StartAnimation");
        }

        Debug.Log("Instantiating animated Cube at world position: " + (cameraForwardPosition + offsetPosition));
    

        // Mainkan audio
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // Instansiasi prefab di depan kamera jika belum ada
       
           
        

        Debug.Log("Button clicked: " + message);
    }

    // Fungsi untuk menginstansiasi prefab di depan kamera
    //void InstantiatePrefabInFrontOfCamera()
    //{
    //    // Mendapatkan posisi di depan kamera
    //    Vector3 cameraForwardPosition = Camera.main.transform.position + Camera.main.transform.forward * 2;
    //    Vector3 offsetPosition = new Vector3(0, 0, 2);

    //    instantiatedCube = Instantiate(HatPrefab);
    //    instantiatedCube.transform.position = cameraForwardPosition + offsetPosition;
    //    instantiatedCube.transform.localScale = new Vector3(3, 3, 3);

    //    Animator hatAnimator = instantiatedCube.GetComponent<Animator>();
    //    if (hatAnimator != null)
    //    {
    //        hatAnimator.SetTrigger("StartAnimation");
    //    }

    //    Debug.Log("Instantiating animated Cube at world position: " + (cameraForwardPosition + offsetPosition));
    //}
}
