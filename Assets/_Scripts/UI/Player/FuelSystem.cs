using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelSystem : MonoBehaviour
{
    #region Events
    public delegate void OnFuelOverflow(string uuid, int amount);
    public static event OnFuelOverflow onPlayerFuelOverflow;

    public delegate void OnFuelChangeEvent(string uuid, float intensity);
    public static event OnFuelChangeEvent onFuelChange;

    public delegate void OnFuelZeroEvent();
    public static event OnFuelZeroEvent zeroFuel;

    #endregion
    #region Floats
    [Tooltip("Initial and Max fuel level from 0-1")]
    [SerializeField]
    [Range(0, 1)]
    static float maxFuel = 1f;
    [Tooltip("Current intensity level from 0-1")]
    [SerializeField]
    [Range(0, 1)]
    static float currentFuel;

    [SerializeField]
    float rateOfFuelChange = -0.02f;

    #endregion

    [SerializeField]
    string uuidOfPlayer = "";

    public static float CurrentFuel { get => currentFuel; }

    private void OnEnable()
    {
        Trail.OnTrailCollision += ChangeFuelAmount;
        MutonPopUp.OnMutonPopUpCollision += ChangeFuelAmount;
    }

    private void OnDisable()
    {
        Trail.OnTrailCollision -= ChangeFuelAmount;
        MutonPopUp.OnMutonPopUpCollision -= ChangeFuelAmount;
    }

    void Start()
    {
        currentFuel = maxFuel;
        StartCoroutine(CountDownCoroutine());
    }

    IEnumerator CountDownCoroutine() // intensity
    {
        while (currentFuel != 0)
        {
            yield return new WaitForSeconds(1);
            ChangeFuelAmount("admin", rateOfFuelChange); //Only effects current player
        }
    }

    public static void ResetFuel()
    {
        currentFuel = maxFuel;
    }

    private void ChangeFuelAmount(string uuid, float amount)
    {
        uuidOfPlayer = uuid;  //Recieves uuid of from Collision Events
        if (currentFuel != 0) { currentFuel += amount; }
        if (currentFuel > 1f)
        {
            int excessFuel = (int)(currentFuel - 1f);
            AddExcessFuelToScore(uuidOfPlayer, excessFuel);  //Sending excess to Score Manager
            currentFuel = 1;
        }
        if (currentFuel <= 0)
        {
            currentFuel = 0;
            UpdateCurrentFuelAmount(uuidOfPlayer, currentFuel);
            UpdateFuelBar(uuid, currentFuel);
            GameOver();
        }
        if (currentFuel != 0)
        {
            UpdateCurrentFuelAmount(uuidOfPlayer, currentFuel);
            UpdateFuelBar(uuid, currentFuel);
        }
    }

    private void AddExcessFuelToScore(string uuidOfPlayer, int excessFuel)
    {
        if (onPlayerFuelOverflow != null) { onPlayerFuelOverflow(uuidOfPlayer, excessFuel); }
    }

    private void UpdateFuelBar(string uuidOfPlayer, float currentFuel)
    {
        if (onFuelChange != null) { onFuelChange(uuidOfPlayer, currentFuel); }
    }

    private void UpdateCurrentFuelAmount(string uuid, float amount)
    {
        if (uuid == "admin") { currentFuel = amount; }
    }

    private void GameOver()
    {
        zeroFuel?.Invoke();
    }
}
