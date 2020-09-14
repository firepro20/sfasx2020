using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController instance;
    public static AudioController Instance { get { return instance; } }

    void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }

    }

    public AudioClip soldierHitClip;
    public AudioClip knightHitClip;
    public AudioClip giantHitClip;
    public AudioClip baseHitClip;
    public AudioClip underAttackClip;
    public AudioClip victoryClip;
    public AudioClip defeatClip;

    private AudioSource mainAudioSource;
    private float volumeLevel = 1f;
    // Start is called before the first frame update
    void Start()
    {
        mainAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySoldierHit()
    {
        mainAudioSource.PlayOneShot(soldierHitClip, volumeLevel);
    }

    public void PlayKnightHit()
    {
        mainAudioSource.PlayOneShot(knightHitClip, volumeLevel);
    }

    public void PlayGiantHit()
    {
        mainAudioSource.PlayOneShot(giantHitClip, volumeLevel);
    }
    public void PlayBaseHit()
    {
        mainAudioSource.PlayOneShot(baseHitClip, volumeLevel);
    }

    public void PlayUnderAttack()
    {
        mainAudioSource.PlayOneShot(underAttackClip, volumeLevel);
    }

    public void PlayVictory()
    {
        mainAudioSource.clip = victoryClip;
        mainAudioSource.Play();
    }

    public void PlayDefeat()
    {
        mainAudioSource.clip = defeatClip;
        mainAudioSource.Play();
    }

}
