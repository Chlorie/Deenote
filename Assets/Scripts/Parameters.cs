using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Parameters
{
    //Note moving
    public static float noteSpeed1FallTime = 18.0f;
    public static float maximumNoteRange = 240.0f;
    public static float alpha1NoteRange = 120.0f;
    public static float maximumNoteWidth = 7.5f;
    public static float slowScrollSpeed = 2.5f;
    public static float fastScrollSpeed = 10.0f;
    public static float NoteFallTime(int chartPlaySpeed) //Maybe I'll change this into something more precise later...
    {
        float speed;
        switch (chartPlaySpeed)
        {
            case 1: speed = 10.0f; break;
            case 2: speed = 8.5f; break;
            case 3: speed = 7.0f; break;
            case 4: speed = 5.7f; break;
            case 5: speed = 4.6f; break;
            case 6: speed = 3.8f; break;
            case 7: speed = 3.2f; break;
            case 8: speed = 2.7f; break;
            case 9: speed = 2.2f; break;
            case 10: speed = 1.7f; break;
            case 11: speed = 1.3f; break;
            case 12: speed = 0.9f; break;
            case 13: speed = 0.6f; break;
            case 14: speed = 0.45f; break;
            case 15: speed = 0.35f; break;
            case 16: speed = 0.3f; break;
            case 17: speed = 0.25f; break;
            case 18: speed = 0.2f; break;
            case 19: speed = 0.15f; break;
            default: speed = 1.7f; break;
        }
        return speed / 180.0f * maximumNoteRange;
    }
    //Note displaying
    public static float noteReturnTime = 5.0f;
    public static float frameSpeed = 0.025f;
    public static float circleSize = 6.0f;
    public static float circleTime = 0.5f;
    public static float waveSize = 4.0f;
    public static float waveHeight = 6.0f;
    public static float lightSize = 30.0f;
    public static float lightHeight = 40.0f;
    public static float waveIncTime = 0.05f;
    public static float waveDecTime = 0.5f;
    public static float lightIncTime = 8.0f / 60;
    public static float lightDecTime = 40.0f / 60;
    public static Color linkLineColor = new Color(1.0f, 233 / 255.0f, 135 / 255.0f);
    public static float minAlphaDif = 0.05f;
    //Judge line
    public static float jlEffectDecTime = 1.0f;
    public static Vector3 jlOffsetVector = new Vector3(0.0f, 0.0f, 32.0f);
    //Combo effect
    public static float noNumberFrameLength = 1 / 30.0f;
    public static float numberWhiteToBlackTime = 0.5f;
    public static float shadowMaxTime = 0.4f;
    public static float shadowMinAlpha = 0.1f;
    public static float charmingIncTime = 0.075f;
    public static float charmingDecTime = 0.225f;
    public static float shockWaveMaxTime = 0.15f;
    public static float strikeDisappearTime = 0.15f;
    //T grid
    public static float minBeatLength = 0.05f;
    //Controlling
    public static int placeButton = 1;
    public static int selectButton = 0;
    public static int scrollButton = 2;
    //Mouse Actions with Notes
    public static float tAxisRange = 0.02f;
    public static float xAxisRange = 0.50f;
    //Line Renderers
    public static float lineWidth = 1.0f;
}
