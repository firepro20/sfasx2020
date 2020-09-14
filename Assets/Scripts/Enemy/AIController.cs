using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public SoldierController soldierPrefab;
    public KnightController knightPrefab;
    public GiantController giantPrefab;
    // Start is called before the first frame update

    public GameController gameController;

    private List<SoldierController> soldierList;
    private List<KnightController> knightList;
    private List<GiantController> giantList;

    private SoldierController mSoldier;
    private KnightController mKnight;
    private GiantController mGiant;

    public float health = 200f;
    private float timer = 0f;
    private bool invoked;
    private float baseHealth = 0f;
    void Start()
    {
        gameController = GameObject.Find("Game").GetComponent<GameController>();
        soldierList = new List<SoldierController>();
        knightList = new List<KnightController>();
        giantList = new List<GiantController>();
        invoked = false;
        baseHealth = health;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.IsPlaying())
        {
            if (!invoked)
            {
                InvokeRepeating("CreateSoldier", 15f, 12f);
                InvokeRepeating("CreateKnight", 100f, 40f);
                InvokeRepeating("CreateGiant", 200f, 100f);
                invoked = true;
            }
            timer += Time.deltaTime;
            if (timer > 100f)
            {
                CancelInvoke("CreateSoldier");
            }
            if (timer > 300f)
            {
                CancelInvoke("CreateKnight");
            }
            if (timer > 400f)
            {
                CancelInvoke("CreateGiant");
                timer = 0f;
            }
        }
        
        // call unit creation over time with modulus, spawnInterval // similar to calculations of player Resources
    }

    public void CreateSoldier()
    {

        // Instantiate
        
        //GameObject tempSoldier = Instantiate(soldierPrefab, gameController.GetPlayerBase().transform);
        mSoldier = Instantiate(soldierPrefab, gameController.GetEnemyBase().Position, Quaternion.identity);
        mSoldier.transform.SetParent(gameObject.transform);
        mSoldier.CurrentPosition = gameController.GetEnemyBase();
        //List<EnvironmentTile> newRoute = gameController.routeToEnemy;
        //newRoute.RemoveAt(newRoute.Count - 1);
        //mSoldier.GoTo(newRoute);
        mSoldier.GoTo(gameController.routeToPlayer);
        soldierList.Add(mSoldier);
        
    }

    public void CreateKnight()
    {
        // Instantiate

        //GameObject tempSoldier = Instantiate(soldierPrefab, gameController.GetPlayerBase().transform);
        mKnight = Instantiate(knightPrefab, gameController.GetEnemyBase().Position, Quaternion.identity);
        mKnight.transform.SetParent(gameObject.transform);
        mKnight.CurrentPosition = gameController.GetEnemyBase();
        //List<EnvironmentTile> newRoute = gameController.routeToEnemy;
        //newRoute.RemoveAt(newRoute.Count - 1);
        //mSoldier.GoTo(newRoute);
        mKnight.GoTo(gameController.routeToPlayer);
        knightList.Add(mKnight);
    }

    public void CreateGiant()
    {
        // Instantiate

        //GameObject tempSoldier = Instantiate(soldierPrefab, gameController.GetPlayerBase().transform);
        mGiant = Instantiate(giantPrefab, gameController.GetEnemyBase().Position, Quaternion.identity);
        mGiant.transform.SetParent(gameObject.transform);
        mGiant.CurrentPosition = gameController.GetEnemyBase();
        //List<EnvironmentTile> newRoute = gameController.routeToEnemy;
        //newRoute.RemoveAt(newRoute.Count - 1);
        //mSoldier.GoTo(newRoute);
        mGiant.GoTo(gameController.routeToPlayer);
        giantList.Add(mGiant);
    }

    public void TakeDamage(float amount)
    {
        baseHealth -= amount;
        if (baseHealth <= 0)
        {
            baseHealth = 0;
        }
    }
    
    public float GetHealth()
    {
        return baseHealth;
    }
}
