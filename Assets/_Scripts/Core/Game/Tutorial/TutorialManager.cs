using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarWriter.Core;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using UnityEngine.UI;
using StarWriter.Utility.Singleton;

public class TutorialManager : Singleton<TutorialManager>
{
    public List<GameObject> tutorialPanels;

    public List<TutorialStage> tutorialStages; //SO Assests

    public Dictionary<string, bool> TutorialTests = new Dictionary<string, bool>();

    [SerializeField]
    private TextMeshProUGUI dialogueText;
    public Sprite dialogueBox; 

    private float dialogueReadTime = 3f;
    private float dialogueFailReadTime = 2f;

    private string retryLine003 = "Woah there partner.I saved your skin there but if that were a real flight you�d be toast.Now try again.";
    private string failLine003 = "I did mention danger, right? ...How about we keep moving.";
    private string failLine004 = "Always look for mutons, blah, blah, blah, lets move on...";

    [SerializeField]
    private int index = 0;

    //public bool hasCompletedTutorial = false;
    public bool isTextBoxActive = false;

    public int Index { get => index; private set => index = value; }

    public delegate void OnTutorialIndexChangeEvent(int idx);
    public event OnTutorialIndexChangeEvent OnTutorialIndexChange;

    // Start is called before the first frame update
    void Start()
    {
        OnTutorialIndexChange?.Invoke(index); //initialize all tutorial classes indexes

        InitializeTutorialStages();
        InitializeTutorialTests();
        
        tutorialStages[index].Begin();
        UpdateDialogueTextBox();
        
    }
    /// <summary>
    /// Adds Panels to tutorialStages and sets IsStarted and IsCompleted to false
    /// </summary>
    private void InitializeTutorialStages()
    {
        int idx = 0;
        //Ref panels in scene to the SO Assest and clear bools
        foreach (TutorialStage stage in tutorialStages)
        {
            tutorialStages[idx].UiPanel = tutorialPanels[idx];
            tutorialStages[idx].IsStarted = tutorialStages[idx].HasCompleted = false;
            idx++;
        }
    }
    /// <summary>
    /// Adds tutorial stages names to dictionary TutorialTest and sets all bools false
    /// </summary>
    private void InitializeTutorialTests() 
    {
        int idx = 0;
        foreach(TutorialStage stage in tutorialStages)
        {
            TutorialTests.Add(tutorialStages[idx].StageName, false);
            idx++;
        }
    }

    private void Update()
    {
        if(index >= tutorialStages.Count)
        {
            if (TutorialTests.ContainsValue(true))
            {
                CompleteTutorial();
            }    
        }  
        else if (tutorialStages[index].IsStarted)
        {
            //playerController.controlLevels[tutorialStages[0].StageName] = true;        
            CheckCurrentTutorialStagePassed();
        }
    }
    /// <summary>
    /// Checks for tutorial stage completion and begins the next
    /// </summary>
    public void CheckCurrentTutorialStagePassed()
    {
        if (!tutorialStages[index].HasMuton)  //no muton = dialolue times out to control stage
        {
            if (isTextBoxActive) { return; }
            else if (index == 3)
            {
                dialogueText.text = retryLine003;
                StartCoroutine(DelayFadeOfTextBox(dialogueFailReadTime));
                IncrementToNextStage();
            }
            else if(!isTextBoxActive)     
            {
                dialogueText.text = tutorialStages[index].LineOne.ToString();
                StartCoroutine(DelayFadeOfTextBox(tutorialStages[index].LineOneDisplayTime));
                IncrementToNextStage();
            }
                      
        }
        else  //requires muton to hit to close stage
        {
            if (tutorialStages[index].HasActiveMuton) { return; }

            TutorialTests.TryGetValue(tutorialStages[index].StageName, out bool value);
            if (value == true)
            {
                if (!tutorialStages[index].HasAnotherAttempt)
                {
                    if (index == 3)
                    {
                        dialogueText.text = failLine003;
                        StartCoroutine(DelayFadeOfTextBox(dialogueFailReadTime));
                    }
                    else if (index == 4)
                    {
                        dialogueText.text = failLine004;
                        StartCoroutine(DelayFadeOfTextBox(dialogueFailReadTime));
                    }
                    

                    if (!isTextBoxActive) //coroutine timer is finished
                    {
                        IncrementToNextStage();
                    }
                }
            }
            if (tutorialStages[index].HasAnotherAttempt)
            {
                tutorialStages[index].RetryOnce();
                return;
            }
            else
            {
                IncrementToNextStage();
            }
        }
        
    }

    IEnumerator DelayFadeOfTextBox(float time)
    {
        isTextBoxActive = true;
        yield return new WaitForSeconds(time);
        dialogueText.enabled = false;
        isTextBoxActive = false;
    }
    /// <summary>
    /// Ends one Tutorial Stage and starts the next
    /// </summary>
    private void IncrementToNextStage()
    {
        StopAllCoroutines();
        tutorialStages[index].End();  //End last stage

        Debug.Log("Passed tutorial test " + tutorialStages[index].StageName);

        index++;
        if (index >= tutorialStages.Count) { return; }
        OnTutorialIndexChange?.Invoke(index); //Increment tutorial index

        tutorialStages[index].Begin(); //Begin next stage
        UpdateDialogueTextBox();
    }
    /// <summary>
    /// Updates the text in the Dialogue Text Box
    /// </summary>
    private void UpdateDialogueTextBox()
    {
        dialogueText.enabled = true;
        dialogueText.text = tutorialStages[index].LineOne.Text;
        dialogueReadTime = tutorialStages[index].LineOneDisplayTime;
        StartCoroutine(DelayFadeOfTextBox(dialogueReadTime));
    }

    /// <summary>
    /// Tells GameSettings and GameManager that Tutorial has been completed
    /// </summary>
    public void CompleteTutorial()
    {
        //hasCompletedTutorial = true;
        GameSetting setting = GameSetting.Instance;
        setting.TutorialHasBeenCompleted = true;
        SceneManager.LoadScene(0);
    }
}
