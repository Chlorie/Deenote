using System.Collections.Generic;
using UnityEngine;

public class PianoSoundsLoader : MonoBehaviour
{
    private int[] keys = { 24, 38, 43, 48, 53, 57, 60, 64, 67, 71, 74, 77, 81, 84, 89, 95 };
    private int[] volumes = { 38, 63, 111, 127 };
    [SerializeField] private AudioClip[] clips;
    public AudioSource playerPrefab;
    private List<AudioSource> pianoSoundPlayers = new List<AudioSource>();
    private List<bool> playerAvailable = new List<bool>();
    private List<float?> noteLength = new List<float?>();
    private List<float> playerVolumes = new List<float>();
    private int initialPlayerAmount = 15;
    public Transform playerParent;
    public AudioClip GetPianoAudioClip(int key, int volume, out float pitch, out float volumeScale)
    {
        if (volume == 0)
        {
            pitch = 1.0f;
            volumeScale = 0.0f;
            return clips[0];
        }
        int diff = 128, nearest = 0;
        for (int i = 0; i < 16; i++)
        {
            int d = key - keys[i]; if (d < 0) d = -d;
            if (d < diff)
            {
                diff = d;
                nearest = i;
            }
        }
        float ratio = 128.0f;
        int nearestVol = 0;
        for (int i = 0; i < 4; i++)
        {
            float r = (float)volume / volumes[i]; if (r < 1.0f) r = 1.0f / r;
            if (r < ratio)
            {
                ratio = r;
                nearestVol = i;
            }
        }
        int index = nearest * 4 + nearestVol;
        pitch = Mathf.Pow(2.0f, (key - keys[nearest]) / 12.0f);
        volumeScale = (float)volume / volumes[nearestVol];
        return clips[index];
    }
    public void PlayNote(int key, int volume, float pitch, float? length = null, float delay = 0.0f)
    {
        if (volume == 0 || length == 0) return;
        float vol, pit;
        AudioClip clip = GetPianoAudioClip(key, volume, out pit, out vol);
        pit *= pitch;
        for (int i = 0; i < playerAvailable.Count; i++)
            if (playerAvailable[i])
            {
                AudioSource player = pianoSoundPlayers[i];
                playerAvailable[i] = false;
                noteLength[i] = length * pit;
                playerVolumes[i] = vol;
                player.clip = clip;
                player.pitch = pit;
                player.volume = vol;
                player.PlayDelayed(delay / pitch);
                return;
            }
        AudioSource newPlayer = Instantiate(playerPrefab, playerParent);
        noteLength.Add(length * pit);
        playerAvailable.Add(false);
        pianoSoundPlayers.Add(newPlayer);
        newPlayer.clip = clip;
        newPlayer.pitch = pit;
        newPlayer.volume = vol;
        playerVolumes.Add(vol);
        newPlayer.Play();
    }
    private void Start()
    {
        for (int i = 0; i < initialPlayerAmount; i++)
        {
            AudioSource source = Instantiate(playerPrefab, playerParent);
            playerAvailable.Add(true);
            noteLength.Add(null);
            playerVolumes.Add(0.0f);
            pianoSoundPlayers.Add(source);
        }
    }
    private void Update()
    {
        for (int i = 0; i < playerAvailable.Count; i++)
        {
            if (!playerAvailable[i])
                if (!pianoSoundPlayers[i].isPlaying)
                    playerAvailable[i] = true;
                else if (noteLength[i] != null && pianoSoundPlayers[i].time > noteLength[i])
                {
                    pianoSoundPlayers[i].volume = playerVolumes[i] * Mathf.Pow(1e-6f, (pianoSoundPlayers[i].time - (float)noteLength[i]));
                    if (pianoSoundPlayers[i].volume < 0.01f)
                    {
                        pianoSoundPlayers[i].Stop();
                        playerAvailable[i] = true;
                    }
                }
        }
    }
}
