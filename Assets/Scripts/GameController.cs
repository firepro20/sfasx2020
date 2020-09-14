using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Character Character;
    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas GenUI;
    [SerializeField] private Canvas InGameUI;
    [SerializeField] private Canvas PauseUI;
    [SerializeField] private Canvas StatusUI;
    [SerializeField] private Transform CharacterStart; // character position in UI

    public AudioSource backgroundMusic;

    // Class Instances

    public CameraController cameraController; // We need to get reference to this in Inspector, from MainCamera
    public PlayerController playerController;
    public AudioController audioController;
    private AIController aiController;
    // Internal Variables

    private RaycastHit[] mRaycastHits;
    private Character mCharacter;
    private EnvironmentController mMap;
    private float cameraMoveSpeed = 20.0f;
    private Vector3 cameraStartPosition;
    private Vector3 cameraCurrentPosition;
    private bool gameStarted;
    private bool isPlaying;
    private bool playerBaseSet;
    private bool playerBaseInitialised;

    private readonly int NumberOfRaycastHits = 1;

    // Path finding for units

    private EnvironmentTile playerBase;
    private EnvironmentTile enemyBase;

    public List<EnvironmentTile> routeToEnemy;
    public List<EnvironmentTile> routeToPlayer;

    // Economy

    public int rateOfEconomy = 5;
    private TextMeshProUGUI resourceBar;
    float resources = 0f;
    float timer = 0;
    

    // Game State
    private bool victory;

    private static GameController instance;
    public static GameController Instance { get { return instance; } }

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

        cameraStartPosition = MainCamera.transform.position;
        backgroundMusic.Play();
    }

    void Start()
    {
        isPlaying = false;
        playerBaseSet = false;
        playerBaseInitialised = false;
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mMap = GetComponentInChildren<EnvironmentController>();
        //mCharacter = Instantiate(Character, transform); // create character
        Debug.Log("The mCharacter value is - " + mCharacter);
        ShowMenu(true);

        // Economy
        
        resourceBar = transform.Find("UI/IngameUI/ResourcesBar/ResourceCounter").GetComponent<TextMeshProUGUI>();
        resourceBar.SetText("0");
    }

    private void Update()
    {

        if (isPlaying)
        {
            if (!playerBaseSet)
            {
                PlayerBasePlacement();
                if (GameObject.Find("EnemyTile(Clone)") != null)
                {
                    aiController = GameObject.Find("EnemyTile(Clone)").GetComponent<AIController>();
                }
            }
            else
            {
                if (GameObject.Find("PlayerTile(Clone)") != null) // can be improved by adding flag to check once
                {
                    playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
                }
                // Start incrementing Economy
                timer += Time.deltaTime;
                
                if (timer > 0.25f)
                {
                   resources = float.Parse(resourceBar.text);
                   resources += rateOfEconomy;
                    
                   resourceBar.text = String.Format("{0:F0}", resources);

                   timer = 0f;
                }
                if (resources > 9999)
                {
                    resources = 9999;
                }
                else if (resources < 0)
                {
                    resources = 0;
                }

                if (playerController.GetHealth() <= 0f || aiController.GetHealth() <= 0f)
                {
                    isPlaying = false;
                    Time.timeScale = 0f;
                    backgroundMusic.Stop();
                    StatusUI.gameObject.SetActive(true);
                    TextMeshProUGUI status = StatusUI.GetComponentInChildren<TextMeshProUGUI>();
                    Debug.Log("GameOver! Player Health - " + playerController.GetHealth() + " Enemy Health - " + aiController.GetHealth());
                    if (playerController.GetHealth() != 0)
                    {
                        status.text = "Victory!";
                        audioController.PlayVictory();
                        // Play Victory Music Display Victory by editing text
                    }
                    else
                    {
                        status.text = "Defeat!";
                        audioController.PlayDefeat();
                        // Play Defeat Nusic Display Defeat by editing text
                    }
                }

            }
            /*
            If (playerbaseset)
            kick off economy // economy should be placed in game controller for both AI and player?
            */


            // 

            // This needs to be modified in order to determine a start and end tile for the enemy AI / player generated units

            // Check to see if the player has clicked a tile and if they have, try to find a path to that 
            // tile. If we find a path then the character will move along it to the clicked tile. 
            
            // Character Code

            /*
            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit[] temp = new RaycastHit[NumberOfRaycastHits];
                Ray screenClick = MainCamera.ScreenPointToRay(Input.mousePosition);
                int hits = Physics.RaycastNonAlloc(screenClick, temp);
                if (hits > 0)
                {
                    EnvironmentTile tile = temp[0].transform.GetComponent<EnvironmentTile>();
                    if (tile != null)
                    {
                        // The route from playerBase to enemyBase and viceverse
                        List<EnvironmentTile> route = mMap.Solve(mCharacter.CurrentPosition, tile);
                        mCharacter.GoTo(route);
                    }
                }
            }     
            */
            

        }

        if (Input.GetKeyDown(KeyCode.Escape) && gameStarted)
        {
            if (isPlaying)
            {
                PauseMenu(isPlaying);
            }
            else
            {
                PauseMenu(false); 
            }
        }
        
    }

    private void LateUpdate()
    {
        if (isPlaying) 
        {
            cameraController.ToggleRotate(false);
            MoveCamera();
            /*
            /* Removed Camera Controls as I was not happy with the result, and started focusing on game mechanics instead 
            MoveCamera();
            /* Might re-enable just to allow WASD camera movement with limits 
            */
        }
    }

    public void UpdateResources(float amount)
    {
        resources += amount;
        resourceBar.text = String.Format("{0:F0}", resources);
    }

    public float GetPlayerResources()
    {
        return resources;
    }

    public EnvironmentTile GetPlayerBase()
    {
        return playerBase;
    }

    public EnvironmentTile GetEnemyBase()
    {
        return enemyBase;
    }
    // Toggle transparent cubes on tile depending if accessible or not, mostly this will be green
    // We can toggle red if exceeding half the tile map
    // Since !accessible tiles do not have a collider, this will not be an issue to toggle red cube for misplacement
    private void PlayerBasePlacement()
    {
        EnvironmentTile playerBasePrefab = mMap.BaseTiles[1];
        
        // Instantiate Once
        if (!playerBaseInitialised)
        {
            // Instantiate off map then bring back on map / modify
            playerBase = Instantiate(playerBasePrefab, new Vector3(100.0f, 100.0f, 100.0f), Quaternion.AngleAxis(180.0f, transform.up), transform);
            playerBaseInitialised = true;
        }
        
        Ray mousePointerHover = MainCamera.ScreenPointToRay(Input.mousePosition);
        
        int hits = Physics.RaycastNonAlloc(mousePointerHover, mRaycastHits);
        if (hits > 0)
        {
            GameObject gameObjectHit = mRaycastHits[0].collider.gameObject;
            if (gameObjectHit.GetComponent<EnvironmentTile>())
            {
                
                // NOTE - might need to get x y values of tile
                // Note - Selected tile will still be shown as the last one before going out of the collider into an inaccessible tile with no collider
                EnvironmentTile selectedTile = gameObjectHit.GetComponent<EnvironmentTile>();
                //Debug.LogWarning("This is the selected tiles position in mMap space - " + selectedTile.MapPosition);
                
                if (selectedTile.MapPosition.x < mMap.SizeOfMap().x / 2) // no need to check for enemyBase tile as minimum x for it is other half of mMap width
                {
                    playerBase.gameObject.transform.position = selectedTile.gameObject.transform.position;
                    // Instantiate playerBasePrefab on tile with green light enabled 
                    //EnvironmentTile tile = Instantiate(playerBasePrefab, selectedTile.gameObject.transform.position, Quaternion.AngleAxis(180.0f, transform.up), transform);
                    //tile.transform.SetParent(children[0].gameObject.transform);
                    playerBase.Position = selectedTile.Position;
                    playerBase.MapPosition = selectedTile.MapPosition;
                }

                if (Input.GetMouseButtonDown(0)) // selectedTile.gameobject.name != enemybase or contains it
                {
                    playerBase = selectedTile;
                    enemyBase = mMap.enemyBase.gameObject.transform.parent.GetComponentInParent<EnvironmentTile>();

                    //Debug.Log("Tile place at -" + selectedTile + " and has been placed permanently!");
                    //selectedTile = playerBasePrefab;
                    // mMap.SetPlayerBase(selectedTile);
                    // mMap is a reference to EnvironmentController
                    Debug.Log("Playerbase gameObject name - " + playerBase.gameObject.name);
                    Debug.Log("Enemybase is - " + mMap.enemyBase.gameObject.name);
                    Debug.Log("Parent of enemy base is - " + enemyBase.gameObject.name);
                    
                    // Solve once, keep using it, ensures that paths are the same
                    
                    routeToEnemy = mMap.Solve(playerBase, enemyBase);
                    List<EnvironmentTile> result = new List<EnvironmentTile>();
                    for (int i = 0; i < routeToEnemy.Count; i++)
                    {
                        result.Add(routeToEnemy[i]);
                    }
                    result.Reverse();
                    routeToPlayer = result;
                    //List<EnvironmentTile> routeToPlayer = mMap.Solve(mMap.enemyBase, playerBase);
                    Debug.LogWarning("routeToEnemy - " + routeToEnemy[routeToEnemy.Count - 1].gameObject.name);
                    Debug.LogWarning("routeToPlayer - " + routeToPlayer[0].gameObject.name);
                    //mCharacter.GoTo(route);
                    playerBaseSet = true;
                }

                // Just toggle UI permanently, this can be listed as a fix / enhancement
                /*
                if (Input.GetMouseButtonDown(0) && gameObjectHit == playerBase.gameObject)
                {
                    Debug.LogError("Turn on UI!");
                }
                */
            }
        }
        
    }
  
    private void MoveCamera()
    {
        
        //WASD to move X and Z .. Check camera specs for strategy game, orientation and perspective - orthographic or isometric
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            MainCamera.transform.localPosition += transform.forward * cameraMoveSpeed * Time.deltaTime; 
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) 
        {
            MainCamera.transform.localPosition += transform.forward * -cameraMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            MainCamera.transform.localPosition += transform.right * cameraMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            MainCamera.transform.localPosition += transform.right * -cameraMoveSpeed * Time.deltaTime;
        }

        
        if (MainCamera.transform.position.x <= -30.0f)
        {
            MainCamera.transform.position = new Vector3(-30.0f, MainCamera.transform.position.y, MainCamera.transform.position.z);
        }
        else if (MainCamera.transform.position.x >= 30.0f)
        {
            MainCamera.transform.position = new Vector3(30.0f, MainCamera.transform.position.y, MainCamera.transform.position.z);
        }
        if (MainCamera.transform.position.z <= -160.0f)
        {
            MainCamera.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, -160.0f);
        }
        else if (MainCamera.transform.position.z >= -100.0f)
        {
            MainCamera.transform.position = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, -100.0f);
        }
       
    }

    public void ShowMenu(bool show)
    {
        if (Menu != null && GenUI != null)
        {
            Menu.gameObject.SetActive(show);
            GenUI.gameObject.SetActive(!show);

            if (show)
            {
                MainCamera.transform.position = cameraStartPosition; 
                //mCharacter.transform.position = mCharacter.CurrentPosition.Position;
                //mCharacter.transform.position = CharacterStart.position;
                //mCharacter.transform.rotation = CharacterStart.rotation;
                isPlaying = false;
            }
            else{
                //mCharacter.transform.position = mMap.Start.Position; // The Start tile of the map, the position assigned from Generate method in Environment
                //mCharacter.transform.rotation = Quaternion.identity;
                //mCharacter.CurrentPosition = mMap.Start;
            }
            // Old Show Flag
            /*
            if( show )
            {
                MainCamera.transform.position = cameraStartPosition;
                // This will only work if the map exists
                //mCharacter.transform.position = mCharacter.CurrentPosition.Position;
                mCharacter.transform.position = CharacterStart.position;
                mCharacter.transform.rotation = CharacterStart.rotation;
                // Cleans up the world each time Menu is shown
                // This has been commented as we are now cleaning on generate, if we press play or clean on display it will result in a fatal error
                //mMap.CleanUpWorld(); 
                isPlaying = false;
            }
            else
            {
                mCharacter.transform.position = mMap.Start.Position; // The Start tile of the map, the position assigned from Generate method in Environment
                mCharacter.transform.rotation = Quaternion.identity;
                mCharacter.CurrentPosition = mMap.Start;
                //isPlaying = true;
            }
            */
        }
    }

    public void PauseMenu(bool showPause)
    {
        PauseUI.gameObject.SetActive(showPause);
        InGameUI.gameObject.SetActive(!showPause);
        isPlaying = !showPause;
        Time.timeScale = showPause ? 0 : 1; // stops time while paused
    }
    public void ConfirmMap(bool showUIOptions)
    {

        GenUI.gameObject.SetActive(false);
        

        Transform pauseButton = gameObject.transform.Find("UI").Find("PauseButton");
        pauseButton.gameObject.SetActive(true);
        InGameUI.gameObject.SetActive(true);

        gameStarted = true;
        isPlaying = true; // place castle first then set playing to true, start off economy
        /*
        Transform display = gameObject.transform.Find("HUD");
        display.gameObject.SetActive(false);
        */
        // find HUD game object for icons and turn on
    }

    public void Restart()
    {
        StatusUI.gameObject.SetActive(false);
        isPlaying = false;
        SceneManager.LoadScene(0);
    }

    public void Generate() // called from button
    {
        if (Menu.enabled)
        {
            Menu.enabled = false;
            GenUI.gameObject.SetActive(true);
        }
        // Clean then create // check for empty is already in cleanup method
        if (!mMap.isEmpty())
        {
            mMap.CleanUpWorld();
        }
        
        mMap.GenerateWorld();

        /* Set character or unit on Map when generation */

        // This piece of code resets character position to start tile (playerbase tile) and allows the path finding to work

        // The Start tile of the map, the position assigned from Generate method in Environment
        
        //mCharacter.transform.position = mMap.Start.Position; 
        //mCharacter.transform.rotation = Quaternion.identity;
        //mCharacter.CurrentPosition = mMap.Start; // current position of tile
        
        //Debug.LogWarning("Character Position in game - " + mCharacter.transform.position);

        isPlaying = false;
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }

    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
