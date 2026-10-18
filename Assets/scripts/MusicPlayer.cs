using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class MusicPlayer : MonoBehaviour
{
    public string musicFolderPath = "Music";
    
    public float fadeDuration = 3f;
    public float masterVolume = 0.8f;

    public bool isRandomMusic = false;
    public bool repeatCurrentTrack = false;
    public bool loopPlaylist = true;

    private AudioClip[] tracks;
    private AudioSource sourceA;
    private AudioSource sourceB;
    
    private int currentTrackIndex = 0;
    private bool isSourceAActive = true;
    private bool isFading = false;

    private void Start()
    {
        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();
        
        sourceA.playOnAwake = false;
        sourceB.playOnAwake = false;
        sourceA.loop = false;
        sourceB.loop = false;

        StartCoroutine(LoadMusicAndPlay());
    }

    private IEnumerator LoadMusicAndPlay()
    {
        string basePath = Path.Combine(Application.streamingAssetsPath, musicFolderPath);
        
        if (!Directory.Exists(basePath))
        {
            yield break;
        }

        string[] files = Directory.GetFiles(basePath, "*.*");
        
        System.Collections.Generic.List<string> audioFiles = new System.Collections.Generic.List<string>();
        foreach (string file in files)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (ext == ".ogg" || ext == ".mp3" || ext == ".wav")
            {
                audioFiles.Add(file);
            }
        }

        if (audioFiles.Count == 0)
        {
            //Debug.LogError("111");
            yield break;
        }

        tracks = new AudioClip[audioFiles.Count];
        for (int i = 0; i < audioFiles.Count; i++)
        {
            string filePath = audioFiles[i];
            string url = "file://" + filePath;
            
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                tracks[i] = DownloadHandlerAudioClip.GetContent(www);
                tracks[i].name = Path.GetFileNameWithoutExtension(filePath);
            }
            else
            {
                //Debug.LogError("123");
            }
        }

        System.Collections.Generic.List<AudioClip> validTracks = new System.Collections.Generic.List<AudioClip>();
        foreach (var track in tracks)
        {
            if (track != null)
                validTracks.Add(track);
        }
        tracks = validTracks.ToArray();

        if (tracks.Length == 0)
        {
            yield break;
        }

        if (repeatCurrentTrack || tracks.Length == 1)
        {
            PlayTrackOnSource(sourceA, currentTrackIndex);
            sourceA.loop = true;
            yield break;
        }

        PlayTrackOnSource(sourceA, currentTrackIndex);
    }

    private void Update()
    {
        if (tracks == null || tracks.Length <= 1 || isFading) return;
        
        if (repeatCurrentTrack) return;

        AudioSource activeSource = isSourceAActive ? sourceA : sourceB;

        if (activeSource.isPlaying && activeSource.clip != null)
        {
            if (activeSource.time >= activeSource.clip.length - fadeDuration)
            {
                StartCoroutine(CrossFadeToNextTrack());
            }
        }
        else if (!activeSource.isPlaying && activeSource.clip != null)
        {
            StartCoroutine(CrossFadeToNextTrack());
        }
    }

    private void PlayTrackOnSource(AudioSource source, int trackIndex)
    {
        source.clip = tracks[trackIndex];
        source.volume = 0f;
        source.Play();
        
        if (!isFading && source == sourceA && trackIndex == 0)
        {
            source.volume = masterVolume;
        }
    }

    private IEnumerator CrossFadeToNextTrack()
    {
        isFading = true;

        AudioSource fadingOutSource = isSourceAActive ? sourceA : sourceB;
        AudioSource fadingInSource = isSourceAActive ? sourceB : sourceA;

        int nextTrackIndex = GetNextTrackIndex();
        PlayTrackOnSource(fadingInSource, nextTrackIndex);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            fadingOutSource.volume = Mathf.Lerp(masterVolume, 0f, easedProgress);
            fadingInSource.volume = Mathf.Lerp(0f, masterVolume, easedProgress);

            yield return null;
        }

        fadingOutSource.Stop();
        fadingOutSource.volume = 0f;
        fadingInSource.volume = masterVolume;

        isSourceAActive = !isSourceAActive;
        currentTrackIndex = nextTrackIndex;
        
        isFading = false;
    }

    private int GetNextTrackIndex()
    {
        if (isRandomMusic)
        {
            int nextIndex = Random.Range(0, tracks.Length);
            if (nextIndex == currentTrackIndex && tracks.Length > 1)
            {
                nextIndex = (nextIndex + 1) % tracks.Length;
            }
            return nextIndex;
        }
        else
        {
            int nextIndex = currentTrackIndex + 1;
            
            if (nextIndex >= tracks.Length)
            {
                if (loopPlaylist)
                {
                    nextIndex = 0;
                }
                else
                {
                    nextIndex = tracks.Length - 1;
                }
            }
            
            return nextIndex;
        }
    }

    public void NextTrack()
    {
        if (tracks == null || tracks.Length <= 1 || isFading) return;
        StopAllCoroutines();
        isFading = false;
        StartCoroutine(CrossFadeToNextTrack());
    }

    public void SetVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (!isFading && tracks != null)
        {
            AudioSource activeSource = isSourceAActive ? sourceA : sourceB;
            activeSource.volume = masterVolume;
        }
    }
}