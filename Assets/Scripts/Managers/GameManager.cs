﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public float canvasScaleFactor;

    public GameGlobalParameters gameGlobalParameters;
    public GamePlayersParameters gamePlayersParameters;
    public GameSoundParameters gameSoundParameters;
    public GameInputParameters gameInputParameters;
    public GameObject fov;

    [Header("Minimap")]
    public Transform minimapAnchor;
    public Camera minimapCamera;
    public BoxCollider minimapFOVCollider;
    public Minimap minimapScript;
    public Collider mapWrapperCollider;
    public int terrainSize;

    [HideInInspector]
    public bool gameIsPaused;

    [HideInInspector]
    public float producingRate = 3f; // in seconds

    [HideInInspector]
    public bool waitingForInput;
    [HideInInspector]
    public string pressedKey;

    private void Awake()
    {
        canvasScaleFactor = GameObject.Find("Canvas").GetComponent<Canvas>().scaleFactor;

        DataHandler.LoadGameData();
        GetComponent<DayAndNightCycler>().enabled = gameGlobalParameters.enableDayAndNightCycle;

        Globals.InitializeGameResources(gamePlayersParameters.players.Length);

        Globals.NAV_MESH_SURFACE = GameObject.Find("Terrain").GetComponent<NavMeshSurface>();
        Globals.UpdateNavMeshSurface();

        // enable/disable FOV depending on game parameters
        fov.SetActive(gameGlobalParameters.enableFOV);

        _SetupMinimap();

        gameIsPaused = false;
    }

    public void Start()
    {
        instance = this;
    }

    private void _SetupMinimap()
    {
        Bounds b = GameObject.Find("Terrain").GetComponent<Terrain>().terrainData.bounds;

        terrainSize = (int) b.size.x;
        float p = terrainSize / 2;

        minimapAnchor.position = new Vector3(p, 0, p);
        minimapCamera.orthographicSize = p;
        minimapFOVCollider.center = new Vector3(0, b.center.y, 0);
        minimapFOVCollider.size = b.size;
        minimapScript.terrainSize = Vector2.one * terrainSize;
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (waitingForInput)
            {
                if (Input.GetMouseButtonDown(0))
                    pressedKey = "mouse 0";
                else if (Input.GetMouseButtonDown(1))
                    pressedKey = "mouse 1";
                else if (Input.GetMouseButtonDown(2))
                    pressedKey = "mouse 2";
                else
                    pressedKey = Input.inputString;
                waitingForInput = false;
            }
            else
                gameInputParameters.CheckForInput();
        }
    }

    private void OnEnable()
    {
        EventManager.AddListener("PauseGame", _OnPauseGame);
        EventManager.AddListener("ResumeGame", _OnResumeGame);

        EventManager.AddListener("UpdateGameParameter:enableDayAndNightCycle", _OnUpdateDayAndNightCycle);
        EventManager.AddListener("UpdateGameParameter:enableFOV", _OnUpdateFOV);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PauseGame", _OnPauseGame);
        EventManager.RemoveListener("ResumeGame", _OnResumeGame);

        EventManager.RemoveListener("UpdateGameParameter:enableDayAndNightCycle", _OnUpdateDayAndNightCycle);
        EventManager.RemoveListener("UpdateGameParameter:enableFOV", _OnUpdateFOV);
    }

    private void _OnPauseGame()
    {
        gameIsPaused = true;
    }

    private void _OnResumeGame()
    {
        gameIsPaused = false;
    }

    /* game parameters update */
    private void _OnUpdateDayAndNightCycle(object data)
    {
        bool dayAndNightIsOn = (bool)data;
        GetComponent<DayAndNightCycler>().enabled = dayAndNightIsOn;
    }
    private void _OnUpdateFOV(object data)
    {
        bool fovIsOn = (bool)data;
        fov.SetActive(fovIsOn);
    }

    private void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        DataHandler.SaveGameData();
#endif
    }
}
