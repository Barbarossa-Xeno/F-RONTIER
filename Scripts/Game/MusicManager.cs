using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Utility;

public class MusicManager : UtilityClass
{
    private AudioSource audioSource;
    private AudioClip music;
    private AudioClip metronome;
    public int bpm = 0;
    private int songID;
    private string songName;
    public bool musicPlayed = false;
    private bool isCalledOnce = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (GameManager.instance.gameScene)
        {
            case Reference.GameScenes.Menu:
                audioSource.loop = true;
                break;
            case Reference.GameScenes.Game:
                if (GameManager.instance.gamePlayState == GameManager.GamePlayState.Starting && !musicPlayed && !isCalledOnce && bpm != 0)
                {
                    StartCoroutine(StartingRoutine());
                    isCalledOnce = true;
                }
                if (GameManager.instance.start && audioSource.clip != music)
                {
                    audioSource.clip = music;
                    audioSource.Play();
                    musicPlayed = true;
                }
                if (GameManager.instance.start && GameManager.instance.gamePlayState == GameManager.GamePlayState.Playing && !audioSource.isPlaying && audioSource.clip == music)
                {
                    audioSource.Stop();
                    musicPlayed = false;
                    GameManager.instance.gamePlayState = GameManager.GamePlayState.Finishing;
                    StartCoroutine(EndingRoutine());
                }
                break;
        }
    }

    public override void OnSceneLoaded(Reference.GameScenes scene)
    {
        switch (scene)
        {
            case Reference.GameScenes.Menu:
                break;
            case Reference.GameScenes.Game:
                music = (AudioClip)Resources.Load<AudioClip>($"Data/{GameManager.instance.songID}/song");
                musicPlayed = false;
                isCalledOnce = false;
                break;
        }
    }

    private IEnumerator StartingRoutine()
    {
        StartCoroutine(MetronomeBeat());
        yield return new WaitForSeconds(3f);
        GameManager.instance.start = true;
        GameManager.instance.startTime = Time.time;
    }
    private IEnumerator EndingRoutine()
    {
        yield return new WaitForSeconds(3f);
        GameManager.instance.start = false;
    }

    private IEnumerator MetronomeBeat()
    {
        metronome = (AudioClip)Resources.Load<AudioClip>($"SE/bpm_{bpm}_wb");
        yield return new WaitForSeconds(0f);
        audioSource.clip = metronome;
        audioSource.Play();
    }
}
