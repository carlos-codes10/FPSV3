using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public enum enemyStates
{
    WANDER,
    PURSUE,
    ATTACK,
    RECOVERY
}
public class Enemy : MonoBehaviour
{

    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform target;

    enemyStates states;

    float wanderRange = 8;
    Vector3 startingLocation;
    float playerSightRange = 6;
    float playerAttackRange = 1;
    float currentStateElapsed;
    float RecoveryTime = 3f;
    Vector3 randomLocation;

    public Rigidbody Rigidbody { get; private set; }
    Vector3 origin;
    
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        origin = transform.position;
        startingLocation = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        switch (states)
        {
            case enemyStates.WANDER:
                Wander();
                break;
            case enemyStates.PURSUE:
                Pursue();
                break;
            case enemyStates.ATTACK:
                Attack();
                break;
            case enemyStates.RECOVERY:
                Recovery();
                break;
        }
    }

    void Wander()
    {
        Debug.Log("Wander Message Caled!: ENEMY IS LOOKING FOR YOU!");
        agent.isStopped = false;
        float distance = Vector3.Distance(transform.position, target.position);

        currentStateElapsed += Time.deltaTime;
        if (currentStateElapsed >= 5)
        {
            Debug.Log("Random Location Set!");
            SetLocation();

            if (distance > wanderRange && !(distance < playerSightRange))
            {

                agent.SetDestination(randomLocation);
            }
            else
                states = enemyStates.PURSUE;

            currentStateElapsed = 0;
        }

    }


    void SetLocation()
    {
        float x = Random.Range(0, target.position.x);
        float z = Random.Range(0, target.position.z);
        randomLocation = new Vector3(x, 0, z);
    }

    void Pursue()
    {
        Debug.Log("Pusue Message Caled!: ENEMY IS PURSUING YOU!");
        agent.isStopped = false;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > playerSightRange && distance > wanderRange)
        {
            states = enemyStates.WANDER;
        }
        else if (distance > playerSightRange)
        {
            agent.SetDestination(target.position);
        }
        else if (distance > playerAttackRange)
        {
            Debug.Log("ENEMY IS GETTING CLOSER!!!");
            agent.SetDestination(target.position);
        } 
        else
            states = enemyStates.ATTACK;
   
    }

    void Attack()
    {
        Debug.Log("Attack Message Called!:");
        agent.isStopped = false;
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance < playerAttackRange)
        {
            Debug.Log("Player has been hit!");
            
        }
        else
            states = enemyStates.PURSUE;

    }

    void Recovery()
    {
        Debug.Log("Recovery Message Called!: ENEMY HIT!");
        agent.isStopped = true;
        currentStateElapsed += Time.deltaTime;

        if (currentStateElapsed >= RecoveryTime)
        {
            states = enemyStates.PURSUE;
            currentStateElapsed = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Projectile>() != null)
        {
            Debug.Log("TIGGER COLLISION ON ENEMY");
            states = enemyStates.RECOVERY;
        }
        
    }


    public void ApplyKnockback(Vector3 knockback)
    {
        GetComponent<Rigidbody>().AddForce(knockback, ForceMode.Impulse); 
    }

    public void Respawn()
    {
        transform.position = origin;
    }
}
