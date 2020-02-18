﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DialogController : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] Image profileBox = null;
    [SerializeField] Image profile = null, dialogBox = null;
    [SerializeField]
    Sprite profileBoxAngela = null, dialogBoxAngela = null, profileBoxElenor = null, dialogBoxElenor = null;
    [SerializeField] Text dialogText = null;
    [SerializeField] AudioSource textSoundSource = null;

    IEnumerator dialogStarter = null;
    IEnumerator dialogWritter = null;

    Vector3 originalPos = Vector3.zero;
    [Header("Variables")]
    [SerializeField] int speedMultiplier = 3;
    [SerializeField] bool silenceWhenMultiplying = false;
    [SerializeField] bool ready = false;
    [SerializeField] bool scrollText = false;
    [SerializeField] KeyCode dialogKey = KeyCode.E;
    bool scrollTextDelta = false;
    float scrollSpeed = 30;

    int deltaSpeedMultiplier = 1;

    private void OnValidate()
    {
        if (!(profileBoxAngela && dialogBoxAngela && profileBoxElenor && dialogBoxElenor))
        {
            Debug.LogWarning("Dialog Controller is missing sprite refrences");

        }
        if (dialogBox && profileBox && profile && dialogBox && textSoundSource)
        {
            ready = true;
        }
        else
        {
            ready = false;
            Debug.LogWarning("Dialog Controller is missing refrences");
        }
    }
    private void Start()
    {
        OnValidate();
        originalPos = dialogText.rectTransform.localPosition;
    }

    public void StartDialog(Dialog[] dialogProfiles)
    {
        if (ready)
        {
            dialogStarter = StartDialogIEnumerator(dialogProfiles);
            StartCoroutine(dialogStarter);
        }
    }
    bool ienumerating = false;
    int rowCount = 1;

    Player_UserInput user = null;
    public IEnumerator StartDialogIEnumerator(Dialog[] dialogProfiles)
    {
        if (ienumerating == true)
            yield break;
        ienumerating = true;

        if (GameController.Get.GetActivePlayer != null)
        {
            user = GameController.Get.GetActivePlayer;
            user.enabled = false;
        }
        for (int i = 0; i < dialogProfiles.Length; i++)
        {
            dialogWritter = DialogIEnumerator(
                dialogProfiles[i].message,
                dialogProfiles[i].profile.profileImage,
                dialogProfiles[i].profile.color,
                dialogProfiles[i].profile.textSounds,
                dialogProfiles[i].textVolume,
                dialogProfiles[i].textWaitTime,
                dialogProfiles[i].textSpeed,
                dialogProfiles[i].playerInput
                );

            yield return StartCoroutine(dialogWritter);

            choice = 0;
            if (dialogProfiles[i].multipleChoice)
            {
                yield return new WaitWhile(() => choice == 0);
                ienumerating = false;
                if (choice == 1)
                    dialogProfiles[i].yes.CallDialog();
                if (choice == 2)
                    dialogProfiles[i].no.CallDialog();

                yield break;
            }
            else
            {
                do
                {
                    yield return null;
                } while (!Input.GetKeyDown(dialogKey));
            }
        }
        StopDialog();
    }

    [SerializeField] int choice = 0;

    public IEnumerator DialogIEnumerator(string message, Sprite profile, Color profileColor, AudioClip[] textSounds, float textVolume, float textWaitTime, float textSpeed, bool playerInput)
    {
        CheckPerspective();

        //Profile
        //profileBox.gameObject.SetActive(true);
        //this.profile.sprite = profile;
        //this.profile.color = profileColor;

        //Box
        dialogBox.gameObject.SetActive(true);
        string[] messages = message.Split('@');
        if (scrollText)
            messages = message.Split('@', '-');

        dialogText.text = "";
        rowCount = 1;
        for (int i = 0; i < messages.Length; i++)
        {
            int lowerWaitBy = 1;
            if (!scrollText)
            {
                dialogText.text = "";
            }
            for (int j = 0; j < messages[i].Length; j++)
            {
                if (messages[i][j] == '/')
                {
                    lowerWaitBy += 1;
                }
                else if (!(messages[i][j] == '-'))
                {
                    dialogText.text += messages[i][j];
                    if (textSoundSource != null && textSounds.Length > 0 && !(messages[i][j] == ' ' || messages[i][j] == '\n') && (!silenceWhenMultiplying || (silenceWhenMultiplying && deltaSpeedMultiplier == 1)))
                        this.textSoundSource.PlayOneShot(textSounds[Random.Range(0, textSounds.Length)], textVolume);
                    else if (messages[i][j] == '\n')
                    {
                        rowCount++;
                    }
                    yield return new WaitForSeconds(textSpeed == 0 ? 0.001f : textSpeed / deltaSpeedMultiplier);
                }
            }
            if (scrollText && (i + 1) < messages.Length)
            {
                nextPos = dialogText.rectTransform.localPosition + (Vector3.up * (24 + 5f));
                float distance = (nextPos - dialogText.rectTransform.localPosition).y;
                scrollTextDelta = true;
                yield return new WaitForSeconds(distance / scrollSpeed);
                scrollTextDelta = false;
            }
            else if (playerInput == true && i != messages.Length -1 )
            {
                do
                {
                    yield return null;
                } while (!Input.GetKeyDown(dialogKey));
            }
            else
                yield return new WaitForSeconds(textWaitTime / lowerWaitBy);
        }
    }

    Vector3 nextPos = Vector2.zero;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            choice = 1;
        if (Input.GetKeyDown(KeyCode.L))
            choice = 2;

        if (Input.GetKeyDown(KeyCode.C))
            scrollText = !scrollText;
        if (scrollTextDelta && rowCount > 2 && dialogText.rectTransform.localPosition.y < nextPos.y)
            dialogText.rectTransform.localPosition += Vector3.up * Time.deltaTime * scrollSpeed;
        if (Input.GetKeyDown(dialogKey))
            deltaSpeedMultiplier = speedMultiplier;
        else if (Input.GetKeyUp(dialogKey))
            deltaSpeedMultiplier = 1;

    }
    public void CheckPerspective()
    {
        if (GameController.Get.CurrentPerspective == Perspective.Angela && ready)
        {
            profileBox.sprite = profileBoxAngela;
            dialogBox.sprite = dialogBoxAngela;
        }
        else if (GameController.Get.CurrentPerspective == Perspective.Elenor && ready)
        {
            profileBox.sprite = profileBoxElenor;
            dialogBox.sprite = dialogBoxElenor;
        }
    }
    public void StopDialog()
    {
        if (ready)
        {
            dialogText.text = "";
            dialogBox.gameObject.SetActive(false);
            //profileBox.gameObject.SetActive(false);
            dialogText.rectTransform.localPosition = originalPos;
            ienumerating = false;
            if (user != null)
                user.enabled = true;
            if (dialogStarter != null)
                StopCoroutine(dialogStarter);
            if (dialogWritter != null)
                StopCoroutine(dialogWritter);
        }
    }
}