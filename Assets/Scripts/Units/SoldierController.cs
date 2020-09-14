using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierController : MonoBehaviour, IUnit
{
    // Keep this class for movement and actions of entity only, similar to Character 
    public EnvironmentTile CurrentPosition { get; set; }
    private GameController gameController;
    private AudioController audioController;
    
    [SerializeField] private float SingleNodeMoveTime = 0.5f;

    private Animator animator;

    public float health = 20f;
    public float damage = 5f; // Knight 20f // Giant 40f

    private float soldierHealth = 0f;

    private void Start()
    {
        soldierHealth = health;
        animator = gameObject.GetComponent<Animator>();
        gameController = GameObject.Find("Game").GetComponent<GameController>();
        audioController = GameObject.Find("AudioController").GetComponent<AudioController>();
    }
    private void Update()
    {
        if (soldierHealth <= 0f)
        {
            transform.position = CurrentPosition.Position;
            StartCoroutine(PerformDeath(2f));
        }
        else
        {
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(PerformAttack());
        if (other.gameObject.CompareTag("AI") && !gameObject.tag.Contains("AI")) // Player Units
        {
            
            if (other.gameObject.GetComponent<SoldierController>()) // Knight / Giant
            {
                other.gameObject.GetComponent<SoldierController>().TakeDamage(damage);
                audioController.PlaySoldierHit();
                //CurrentPosition = other.gameObject.GetComponent<SoldierController>().CurrentPosition;
            }
            if (other.gameObject.GetComponent<KnightController>())
            {
                other.gameObject.GetComponent<KnightController>().TakeDamage(damage);
                audioController.PlayKnightHit();
            }
            if (other.gameObject.GetComponent<GiantController>())
            {
                other.gameObject.GetComponent<GiantController>().TakeDamage(damage);
                audioController.PlayGiantHit();
            }
            if (other.gameObject.GetComponent<AIController>())
            {
                other.gameObject.GetComponent<AIController>().TakeDamage(damage);
                audioController.PlayBaseHit();
            }
            //transform.position = CurrentPosition.Position;

            // Animation

            //if (other.gameObject.GetComponent<GruntController>())
            //{

            //}
        }
        if (other.gameObject.CompareTag("Player") && !gameObject.tag.Contains("Player")) // AI Units
        {
            Debug.Log("Other Game Object AI - " + other.gameObject.name);
            if (other.gameObject.GetComponent<SoldierController>()) // Knight / Giant
            {
                other.gameObject.GetComponent<SoldierController>().TakeDamage(damage);
                audioController.PlaySoldierHit();
                //CurrentPosition = other.gameObject.GetComponent<SoldierController>().CurrentPosition;
            }
            if (other.gameObject.GetComponent<KnightController>())
            {
                other.gameObject.GetComponent<KnightController>().TakeDamage(damage);
                audioController.PlayKnightHit();
            }
            if (other.gameObject.GetComponent<GiantController>())
            {
                other.gameObject.GetComponent<GiantController>().TakeDamage(damage);
                audioController.PlayGiantHit();
            }
            if (other.gameObject.GetComponent<AIController>())
            {
                other.gameObject.GetComponent<AIController>().TakeDamage(damage);
                audioController.PlayBaseHit();
            }
            if (other.gameObject.GetComponentInChildren<PlayerController>())
            {
                other.gameObject.GetComponentInChildren<PlayerController>().TakeDamage(damage);
                audioController.PlayUnderAttack();
            }
            //transform.position = CurrentPosition.Position;

            // Animation

            //if (other.gameObject.GetComponent<GruntController>())
            //{

            //}
        }

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("I am player!");
        }
        if (other == null) {
            Debug.Log("No colldier hit, calculating new route");
            //List<EnvironmentTile> newRoute = environmentController.Solve(CurrentPosition, environmentController.enemyBase);
            //newRoute.RemoveAt(newRoute.Count - 1);
            //GoTo(newRoute);
        }
    }

    private IEnumerator DoMove(Vector3 position, Vector3 destination)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination)
        {
            transform.rotation = Quaternion.LookRotation(destination - position, Vector3.up);

            Vector3 p = transform.position;
            float t = 0.0f;

            while (t < SingleNodeMoveTime)
            {
                t += Time.deltaTime;
                p = Vector3.Lerp(position, destination, t / SingleNodeMoveTime);
                transform.position = p;
                yield return null;
            }
        }
    }

    private IEnumerator DoGoTo(List<EnvironmentTile> route)
    {
        // Move through each tile in the given route
        if (route != null)
        {
            Vector3 position = CurrentPosition.Position;
            for (int count = 0; count < route.Count; ++count)
            {
                Vector3 next = route[count].Position;
                yield return DoMove(position, next);
                CurrentPosition = route[count];
                position = next;
            }
        }
    }

    public void GoTo(List<EnvironmentTile> route)
    {
        // Clear all coroutines before starting the new route so 
        // that clicks can interupt any current route animation
        StopAllCoroutines();
        StartCoroutine(DoGoTo(route));
    }

    public IEnumerator PerformAttack()
    {
        // isWalking false, performs fighting
        if (animator.GetBool("IsWalking"))
        {
            animator.SetBool("IsWalking", false);
        }

        animator.speed = 4f;
        yield return new WaitForSeconds(0.5f);
        animator.speed = 1f;
        
        animator.SetBool("IsWalking", true);
        
    }

    public IEnumerator PerformDeath(float waitTime)
    {
        SingleNodeMoveTime = 50f; // larger value slower movement
        gameObject.GetComponent<BoxCollider>().enabled = false; // prevent further triggers
        // combat death
        animator.SetBool("Combat", true);
        animator.speed = 2f;
        yield return new WaitForSeconds(waitTime);
        animator.speed = 1f;
        Destroy(gameObject);
    }

    public void PerformMove()
    {
        throw new System.NotImplementedException();
    }

    public void PerformVictory()
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(float amount)
    {
        soldierHealth -= amount;
        if (soldierHealth <= 0)
        {
            soldierHealth = 0;
        }
    }

}
