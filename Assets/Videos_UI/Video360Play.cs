using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR;

[RequireComponent(typeof(AudioSource))]
public class Video360Play : MonoBehaviour
{
    VideoPlayer vp;
    public VideoClip[] vcList;
    
    public AudioClip blinkSFX;
    [Range(0f, 1f)] public float effectVolume = 0.5f; // [추가됨] 볼륨 조절 슬라이더 (기본 0.5)
    
    private AudioSource audioSource;
    private MeshRenderer meshRenderer;
    private Material targetMaterial;

    void Start()
    {
        vp = GetComponent<VideoPlayer>();
        meshRenderer = GetComponent<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (meshRenderer != null)
        {
            targetMaterial = meshRenderer.material;
            targetMaterial.color = Color.white;
        }

        if (vcList.Length > 0)
        {
            vp.clip = vcList[0];
            vp.Play();
        }
    }

    public void PlayBlinkEffect(Color blinkColor, float durationPerBlink, int loopCount)
    {
        if (targetMaterial != null)
        {
            StopAllCoroutines();
            StartCoroutine(BlinkRoutine(blinkColor, durationPerBlink, loopCount));
        }
    }

    IEnumerator BlinkRoutine(Color targetColor, float duration, int count)
    {
        float halfDuration = duration * 0.5f;

        // [수정됨] 소리 1회 재생 + 볼륨 적용
        if (blinkSFX != null && audioSource != null)
        {
            // 두 번째 인자가 볼륨 스케일
            audioSource.PlayOneShot(blinkSFX, effectVolume);
        }

        for (int i = 0; i < count; i++)
        {
            VibrateControllers(1.0f, duration);

            // 1. White -> Target
            float timer = 0f;
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                targetMaterial.color = Color.Lerp(Color.white, targetColor, timer / halfDuration);
                yield return null;
            }

            // 2. Target -> White
            timer = 0f;
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                targetMaterial.color = Color.Lerp(targetColor, Color.white, timer / halfDuration);
                yield return null;
            }
            
            targetMaterial.color = Color.white; 
        }
    }

    private void VibrateControllers(float amplitude, float duration)
    {
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, inputDevices);

        foreach (var device in inputDevices)
        {
            if (device.TryGetHapticCapabilities(out HapticCapabilities capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    device.SendHapticImpulse(0, amplitude, duration);
                }
            }
        }
    }

    
}