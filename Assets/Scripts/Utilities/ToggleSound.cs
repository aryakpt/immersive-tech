using UnityEngine;
using UnityEngine.UI;

public class ToggleSound : MonoBehaviour
{
  public AudioSource audioSource;
  public Button toggleButton;

  private bool isPlaying = false;

  void Start()
  {
    // Set initial button text
    UpdateButtonText();
    // Add listener to button
    toggleButton.onClick.AddListener(ToggleAudio);
  }

  private void ToggleAudio()
  {
    if (isPlaying)
    {
      audioSource.Stop();
    }
    else
    {
      audioSource.Play();
      StartCoroutine(WaitForAudioToFinish());
    }
    isPlaying = !isPlaying; // Toggle the playing state
    UpdateButtonText(); // Update button text based on current state
  }

  private void UpdateButtonText()
  {
    if (isPlaying)
    {
      toggleButton.GetComponentInChildren<Text>().text = "Stop"; // Change button text to Stop
    }
    else
    {
      toggleButton.GetComponentInChildren<Text>().text = "Play"; // Change button text to Play
    }
  }

  private System.Collections.IEnumerator WaitForAudioToFinish()
  {
    // Tunggu sampai audio selesai
    yield return new WaitWhile(() => audioSource.isPlaying);
    // Setelah audio selesai, set isPlaying menjadi false dan update teks tombol
    isPlaying = false;
    UpdateButtonText();
  }
}
