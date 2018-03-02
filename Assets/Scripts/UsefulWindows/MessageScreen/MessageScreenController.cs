using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void MessageScreenCallback();

public class MessageScreenController : MonoBehaviour
{
    public static MessageScreenController Instance { get; internal set; }
    [SerializeField] private GameObject messageScreen;
    [SerializeField] private GameObject[] buttonGroups;
    [SerializeField] private LocalizedText[] buttonTexts1;
    [SerializeField] private LocalizedText[] buttonTexts2;
    [SerializeField] private LocalizedText[] buttonTexts3;
    [SerializeField] private LocalizedText buttonTexts4;
    [SerializeField] private LocalizedText title;
    [SerializeField] private LocalizedText notice;
    private MessageScreenCallback[] callbacks = new MessageScreenCallback[4];
    private int activatedButtonGroup = 0;
    public void Activate(string[] title, string[] notice,
        MessageScreenCallback callback1, string[] buttonText1,
        MessageScreenCallback callback2, string[] buttonText2,
        MessageScreenCallback callback3, string[] buttonText3,
        MessageScreenCallback callback4, string[] buttonText4)
    {
        this.title.SetStrings(title);
        this.notice.SetStrings(notice);
        callbacks[0] = callback1; callbacks[1] = callback2; callbacks[2] = callback3; callbacks[3] = callback4;
        for (int i = 0; i < 4; i++) buttonTexts1[i].SetStrings(buttonText1);
        for (int i = 0; i < 3; i++) buttonTexts2[i].SetStrings(buttonText2);
        for (int i = 0; i < 2; i++) buttonTexts3[i].SetStrings(buttonText3);
        buttonTexts4.SetStrings(buttonText4);
        if (buttonText4 != null)
            activatedButtonGroup = 3;
        else if (buttonText3 != null)
            activatedButtonGroup = 2;
        else if (buttonText2 != null)
            activatedButtonGroup = 1;
        else
            activatedButtonGroup = 0;
        messageScreen.SetActive(true);
        buttonGroups[activatedButtonGroup].SetActive(true);
        CurrentState.ignoreAllInput = true;
    }
    public void Callback(int index)
    {
        messageScreen.SetActive(false);
        buttonGroups[activatedButtonGroup].SetActive(false);
        if (callbacks[index] != null) callbacks[index]();
        CurrentState.ignoreAllInput = false;
    }
    private void Awake()
    {
        Instance = this;
    }
}

public class MessageScreen // Static methods for using message screen
{
    public static void Activate(string[] title, string[] notice,
        string[] buttonText1, MessageScreenCallback callback1 = null,
        string[] buttonText2 = null, MessageScreenCallback callback2 = null,
        string[] buttonText3 = null, MessageScreenCallback callback3 = null,
        string[] buttonText4 = null, MessageScreenCallback callback4 = null) // Activate message screen
    {
        if (callback1 != null)
            MessageScreenController.Instance.Activate(
                title,
                notice,
                callback1, buttonText1,
                callback2, buttonText2,
                callback3, buttonText3,
                callback4, buttonText4);
        else
            return;
    }
}
