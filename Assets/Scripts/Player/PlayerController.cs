using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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
    private bool parentSet;
    private float baseHealth = 0;

    void Start()
    {
        baseHealth = health;
        gameController = GameObject.Find("Game").GetComponent<GameController>();
        soldierList = new List<SoldierController>();
        knightList = new List<KnightController>();
        giantList = new List<GiantController>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!parentSet)
        {
            if (GameObject.Find("PlayerTile(Clone)") != null)
            {
                gameObject.transform.parent = GameObject.Find("PlayerTile(Clone)").transform;
                parentSet = true;
            }
        }


        //UpdatePlayerEconomy(rateOfEconomy);
    }

    public void CreateSoldier()
    {
        
        // Instantiate
        if (gameController.GetPlayerResources() >= 150)
        {
            //GameObject tempSoldier = Instantiate(soldierPrefab, gameController.GetPlayerBase().transform);
            mSoldier = Instantiate(soldierPrefab, gameController.GetPlayerBase().Position, Quaternion.identity);
            mSoldier.transform.SetParent(gameObject.transform);
            mSoldier.CurrentPosition = gameController.GetPlayerBase();
            //List<EnvironmentTile> newRoute = gameController.routeToEnemy;
            //newRoute.RemoveAt(newRoute.Count - 1);
            //mSoldier.GoTo(newRoute);
            mSoldier.GoTo(gameController.routeToEnemy);
            soldierList.Add(mSoldier);
            gameController.UpdateResources(-150);
        }
        
    }

    public void CreateKnight()
    {
        // Instantiate
        if (gameController.GetPlayerResources() >= 300)
        {
            //GameObject tempSoldier = Instantiate(soldierPrefab, gameController.GetPlayerBase().transform);
            mKnight = Instantiate(knightPrefab, gameController.GetPlayerBase().Position, Quaternion.identity);
            mKnight.transform.SetParent(gameObject.transform);
            mKnight.CurrentPosition = gameController.GetPlayerBase();
            //List<EnvironmentTile> newRoute = gameController.routeToEnemy;
            //newRoute.RemoveAt(newRoute.Count - 1);
            //mSoldier.GoTo(newRoute);
            mKnight.GoTo(gameController.routeToEnemy);
            knightList.Add(mKnight);
            gameController.UpdateResources(-300);
        }
        
    }

    public void CreateGiant()
    {
        if (gameController.GetPlayerResources() >= 900)
        {
            //GameObject tempSoldier = Instantiate(soldierPrefab, gameController.GetPlayerBase().transform);
            mGiant = Instantiate(giantPrefab, gameController.GetPlayerBase().Position, Quaternion.identity);
            mGiant.transform.SetParent(gameObject.transform);
            mGiant.CurrentPosition = gameController.GetPlayerBase();
            //List<EnvironmentTile> newRoute = gameController.routeToEnemy;
            //newRoute.RemoveAt(newRoute.Count - 1);
            //mSoldier.GoTo(newRoute);
            mGiant.GoTo(gameController.routeToEnemy);
            giantList.Add(mGiant);
            gameController.UpdateResources(-900);
        }
        
    }

    public List<SoldierController> GetSoldier()
    {
        return soldierList;
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
