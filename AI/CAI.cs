using FMOD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CAI : MonoBehaviour
{
    [HideInInspector]       public enum State           {   Idle, Wander, Investigate, Wait, Attack     }
    [HideInInspector]       public enum AIType          {   Orc, Goblin                                 }
    [HideInInspector]       public enum AttackStyle     {   Melee, Ranged                               } 
                            private State                   m_eState;
    [SerializeField]        private AIType                  m_eAIType;
    [SerializeField]        private AttackStyle             m_eAttackStyle;
    
    bool                    m_bInitialized                  = false;
    [SerializeField] bool   m_bStationary                   = false;
    bool                    m_bIsInvestigating              = false;
    bool                    m_bIsMoving                     = false;
    bool                    m_bCanMove                      = true;

    float                   m_fUpdateDestinationTime        = 0.0f;
    [SerializeField] float  m_fAlertNoise                   = 15.0f;
    float                   m_fTimeVariation                = 2.0f;
    float                   m_fDistanceVariation            = 2.0f;
    float                   m_fSecToGenerateDest            = 0.0f;
    float                   m_fSecToGenerateDestBase        = 8.0f;
    float                   m_fSpeedOriginal;
    float                   m_fLoseSightTimer;
    [SerializeField] float  m_fIdleTimer;
    [SerializeField] float  m_fInvestigateDuration          = 8.0f;
    public float            m_fInvestigateTimer             = 0.0f;

    [SerializeField] float  m_fDestroyAfterDuration         = 20.0f;
    float                   m_fDeathTimer                   = 0.0f;

    //private Animator        m_Animator;
    Vector3                 m_vPointOfInterest;
    Vector3                 m_vPositionPreviousFrame;
    //Combat
    bool                    m_bCombat                       = false;
    bool                    m_bCombatPreviousFrame          = false;
    bool                    m_bDeathTriggered               = false;
    
    //Component references
    Transform               m_PlayerTransform;
    NavMeshAgent            m_Agent;
    Animator                m_Animator;
    CHealth                 m_Health;
    CStatusEffects          m_StatusEffects;
    CSenses                 m_Senses;
    CAIAttack               m_AIAttack;
    CSmokeVFX               m_SmokeVFX;   

    public State            GetState()                  {   return m_eState;                    }
    public AIType           GetAIType()                 {   return m_eAIType;                   }
    public AttackStyle      GetAttackStyle()            {   return m_eAttackStyle;              }

    public CStatusEffects   GetStatusEffects()          {   return m_StatusEffects;             }
    private CAIAttack       GetAIAttack()               {   return m_AIAttack;                  }
    public NavMeshAgent     GetAgent()                  {   return m_Agent;                     }
    private CHealth         GetHealth()                 {   return m_Health;                    }
    private Animator        GetAnimator()               {   return m_Animator;                  }

    public Vector3          GetPOI()                    {   return m_vPointOfInterest;          }
    public Vector3          GetPreviousPosition()       {   return m_vPositionPreviousFrame;    } 

    public bool             GetCanMove()                {   return m_bCanMove;                  }
    public bool             GetIsMoving()               {   return m_bIsMoving;                 }
    public bool             GetIsInvestigating()        {   return m_bIsInvestigating;          }
    public bool             GetCombat()                 {   return m_bCombat;                   }
    public bool             GetCombatPreviousFrame()    {   return m_bCombatPreviousFrame;      }

    public void             SetIsInvestigating( bool bNewInvestigating)             {   m_bIsInvestigating = bNewInvestigating;     }
    public void             SetIsMoving( bool bNewIsMoving )                        {   m_bIsMoving = bNewIsMoving;                 }

    public void             SetPOI( Vector3 newPOI )                                {   m_vPointOfInterest = newPOI;                        }
    public void             SetCombatPreviousFrame( bool bNewCombatPreviousFrame)   {   m_bCombatPreviousFrame = bNewCombatPreviousFrame;   }
    public void             SetCombat( bool bNewCombat )                            {   m_bCombat = bNewCombat;                             }
    public void             SetCanMove( bool bNewCanMove )                          {   m_bCanMove = bNewCanMove;                           }
    public void             SetLoseSightTimer( float newTimer )                     {   m_fLoseSightTimer = newTimer;                       }
    //for setting combat to false slowly based on CSenses.m_fTimeToLoseSight variable
    public bool             LoseCombatSlowly( )
    {
        m_fLoseSightTimer -= Time.deltaTime;
        if ( m_fLoseSightTimer < 0.0f )
        {
            m_fLoseSightTimer = m_Senses.GetTimeUntilLoseSight();
            m_bCombat = false;
            CAIManager.RemoveFromCombatList(this);
            return true;
        } 
        return false;
    }

    private void Awake()
    {
        CAIManager.AddAI_AllAIList(this);
    }

    void Start()
    {
        Initialize();
        SetState(State.Idle);
    }

    public void Update()
    {
        //print(gameObject.name + "'s state :" + m_eInternalState);
        if (gameObject.activeInHierarchy == false )
        {
            //print( gameObject.name + " is not active and Triggered a failsafe here." );
            return;
        }
        
        if (GetHealth().GetIsAlive() == false)
        {
            //Death stuff, cleanups etc
            OnDeath();
            return;
        }

        if (GetAgent().enabled == false)
        {
            //print( gameObject.name + "'s agent is not enabled and Triggered a failsafe here." );
            return;
        }

        OnUpdateStarted();

        if ( GetAIAttack().GetIsAttacking() == true || m_bStationary == true ) 
        {
            m_bCanMove = false;
        }

        if ( GetAgent().hasPath == true )
        {
            if ( GetStatusEffects().GetIsPushed() == false )
            {
                if ( transform.position != GetPreviousPosition() )
                {
                    SetIsMoving(true);
                }
            } 
        }
        
        /////INVESTIGATE POI/////
        if (GetCombat() == false)
        {
            if ( GetCombatPreviousFrame() == true )
            {
                m_Senses.SetSightCone(m_Senses.GetNormalSightCone());
            }
            if ( HasPOI() == true )
            {
                SetState(State.Investigate);
            }
        }
        /////INVESTIGATE POI/////
        else //GetCombat == true
        {
            if ( GetCombatPreviousFrame() == false )
            {
                m_Senses.SetSightCone( m_Senses.GetCombatSightCone() );
                CAIManager.AddToCombatList(this);
                CSoundBank.Instance.AIEnterCombat(GetAIType(), gameObject);
                //Alert enemies nearby and Engage
                //print("ALERT CODE WAS RUN");
                CNoiseSystem.Alert(CPlayerControlls.GetPlayer().transform.position, m_fAlertNoise);
            }
            //Attack state
            SetState(State.Attack);
        }

        UpdateAnimations();

        switch (m_eState)
        {
            case State.Idle:
                {
                    //WANDER interaction
                    m_fIdleTimer += Time.deltaTime;
                    if ( m_fIdleTimer >= m_fSecToGenerateDest )
                    {
                        if ( m_bStationary == false )
                        {
                            SetState( State.Wander );
                        }
                        //else remain in idle
                    }
                }
                break;

            case State.Wander:
                {
                    //IDLE interaction
                    if ( DestinationReached() == true )
                    {
                        SetState( State.Idle );
                    }
                }
                break;

            case State.Investigate:
                {
                    //play investigation emote
                    //investigate walk?
                    if ( DestinationReached() == true )
                    {
                        if( RotateAIBool( GetPOI(), 0.90f ) == true )
                        {
                            TryResetPath();
                        }
                        //On reach destination, rotate head for a couple of seconds, then return?
                    }
                    if ( GetIsMoving() == false )
                    {
                        m_fInvestigateTimer += Time.deltaTime;
                        if ( m_fInvestigateTimer > m_fInvestigateDuration )
                        {
                            ///m_fInvestigateTimer = 0.0f;
                            ResetPOI();
                            if ( m_bStationary == true )
                            {
                                SetState(State.Idle);
                            }
                            else
                            {
                                SetState( State.Wander );
                            }
                        }
                    }
                    
                }
                break;

            case State.Wait:
                break;

            case State.Attack:
                {
                    if ( Vector3.Distance( transform.position, m_PlayerTransform.position ) <= GetAIAttack().GetAttackRange() )//m_fAttackRange )
                    {
                        TryResetPath();
                        if ( GetAIAttack().GetCanAttack() == true )
                        {
                            if ( GetAttackStyle() == AttackStyle.Melee )
                            {
                                if ( RotateAIBool( m_PlayerTransform.position, 0.90f ) == true )
                                {
                                    if ( GetAIType() == AIType.Orc )
                                    {
                                        int iRandom;
                                        iRandom = Random.Range(0, 2); //means 0 to 1
                                        GetAIAttack().Attack(iRandom);
                                        CSoundBank.Instance.AIMelee(m_eAIType, iRandom, gameObject);
                                    }
                                    else
                                    {
                                        GetAnimator().SetBool( "Collided", false );
                                        GetAIAttack().Attack( 0 );
                                        CSoundBank.Instance.GoblinMeleeAttack( gameObject );
                                    }
                                }
                            }
                            else
                            {
                                if (RotateAIBool( m_PlayerTransform.position, 0.3f ))
                                {
                                    GetAIAttack().Attack(m_PlayerTransform.position);
                                    //CSoundBank.Instance.GoblinRangedAttack(gameObject);
                                }
                            }
                        }
                        if ( GetAIAttack().GetIsAttacking() == false )
                        {
                            RotateAI( m_PlayerTransform.position );
                        }
                    }
                    else
                    {
                        TrySetDestination( m_PlayerTransform.position );
                        //print("Running toward player in Combat mode");
                        if ( m_bStationary == false )
                        {
                            TrySetDestination( GetPOI() );
                        }
                        else
                        {
                            RotateAI( GetPOI() );
                        }
                    }
                }
                break;

            default:
                break;
        }
        OnUpdateFinished();
    }

    void Initialize()
    {
        if ( m_bInitialized == false )
        {
            m_vPointOfInterest = Vector3.zero;
            m_fSecToGenerateDest = m_fSecToGenerateDestBase;
            
            m_Health = GetComponent<CHealth>();

            m_Agent = GetComponent<NavMeshAgent>();
            if ( GetAgent().stoppingDistance == 0 ) 
            { 
                GetAgent().stoppingDistance = 1.5f;
                m_fSpeedOriginal = GetAgent().speed;
            }
            
            //m_Animator = GetComponent<Animator>();
            m_StatusEffects = GetComponent<CStatusEffects>();

            m_Senses = GetComponent<CSenses>();

            //m_Animations = GetComponent<CAnimations>();

            m_fLoseSightTimer = m_Senses.GetTimeUntilLoseSight();

            m_AIAttack = GetComponent<CAIAttack>();

            if (GetComponentInChildren<CSmokeVFX>())
            {
                m_SmokeVFX = GetComponentInChildren<CSmokeVFX>();
            }
            m_PlayerTransform = CPlayerControlls.GetPlayer().transform;

            m_Animator = GetComponent<Animator>();

            GetAnimator().keepAnimatorControllerStateOnDisable = true;

            m_bInitialized = true;
        }
    }


	public void SetState( State newState )
	{
        //SetIsInvestigating( false );
        //print(gameObject.name + "'s state is " + newState);
		m_eState = newState;
        if ( m_eState == State.Idle )
        {
            //SetIdle( true );
            m_fIdleTimer = 0;

            m_fSecToGenerateDest = Random.Range( m_fSecToGenerateDestBase - m_fTimeVariation , m_fSecToGenerateDestBase + m_fTimeVariation );
        } 

        else if (m_eState == State.Wander)
        {
            float randomX, randomZ;
            randomX = Random.Range( -m_fDistanceVariation, m_fDistanceVariation );
            randomZ = Random.Range( -m_fDistanceVariation, m_fDistanceVariation );
            Vector3 wanderPos = new Vector3(transform.position.x + randomX + GetAgent().stoppingDistance, transform.position.y, transform.position.z + randomZ + GetAgent().stoppingDistance);
            if ( TrySetDestination( wanderPos ) == false )//( GetAgent().SetDestination( wanderPos ) == false )
            {
                //if agent was unable to set a path
                SetState(State.Idle);
            }

            CSoundBank.Instance.AIIdle(m_eAIType, gameObject);
        }
        else if( m_eState == State.Investigate )
        {
            SetIsInvestigating(true);
            if ( m_bStationary == false )
            {
                TrySetDestination(GetPOI());
            }
            else
            {
                RotateAI(GetPOI());
                //RotateAIBool(GetPOI(), 0.9f);
            }
            
            CAIManager.AddToInvestigateList(this);
        }

        if ( m_eState != State.Investigate )
        {
            m_fInvestigateTimer = 0.0f;
            SetIsInvestigating( false );
        }
	}

    bool DestinationReached()
    {
        if (GetPreviousPosition() != transform.position)
        {
            if (GetAgent().pathPending == false)
            {
                if (Vector3.Distance(transform.position, GetAgent().pathEndPosition) < GetAgent().stoppingDistance)
                {
                    //GetAgent().ResetPath();
                    TryResetPath();
                    return true;
                }
            }
        }
        return false;
    }

    public void ResetPOI()
    {
        m_fInvestigateTimer = 0.0f; ///
        m_vPointOfInterest = Vector3.zero;
        CAIManager.RemoveFromInvestigateList(this);
    }

    public bool HasPOI()
    {
        if (m_vPointOfInterest != Vector3.zero)
        {
            return true;
        }
        return false;
    }

    void OnUpdateStarted()
    {
        //SetIdle( false );
        //SetIsInvestigating( false );
        SetIsMoving( false );
        SetCanMove( true );
    }

    void OnUpdateFinished()
    {
        m_vPositionPreviousFrame = transform.position;
        m_bCombatPreviousFrame = m_bCombat;
    }

    void RotateAI( Vector3 vPos )
    {
        Vector3 direction = (vPos - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5.0f);
    }

    bool RotateAIBool( Vector3 vPos, float fDotProduct )
    {
        Vector3 direction = (vPos - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5.0f);
        if ( Vector3.Dot( transform.forward, direction ) > fDotProduct )
        {   
            //print("Dot product is = " + Vector3.Dot( transform.forward, direction ));
            //looking in direction
            return true;
        }
        return false;
    }
    
    public bool TryResetPath()
    {
        if (GetAgent().enabled == true)
        {
            GetAgent().ResetPath();
            return true;
        }
        return false;
    }

    bool TrySetDestination( Vector3 pos )
    {
        if ( GetAgent().enabled == true )
        {
            if ( m_bCanMove == true )
            {
                if (Time.time > m_fUpdateDestinationTime)
                {
                    m_fUpdateDestinationTime = Time.time + 0.25f;
                    return GetAgent().SetDestination( pos );
                }
                //return GetAgent().SetDestination( pos );
            }
        }
        return false;
    }

    public void ResetSpeed()
    {
        Initialize();
        GetAgent().speed = m_fSpeedOriginal;
    }

    public void UpdateAnimations()
    {
        GetAnimator().SetBool( "Moving", GetIsMoving()                  );
        GetAnimator().SetBool( "Investigate", GetIsInvestigating()      );
        GetAnimator().SetBool( "Attack", GetAIAttack().GetIsAttacking() );
        //additional stuff
    }

    private void OnCollisionEnter( Collision collision )
    {
        if ( GetAIAttack().GetIsAttacking() ) 
        {
            if ( GetAIType() == AIType.Goblin )
            {
                if ( collision.gameObject.layer == LayerMask.NameToLayer("Player") )
                {
                    //print( "collided with : " + collision.gameObject.name );
                    GetAnimator().SetBool( "Collided", true );
                }
            }
        }
    }

    public void AISetActive( bool bActive )
    {
        if ( m_bInitialized == false )
        {
            Initialize();
        }
        if ( m_SmokeVFX != null )
        {
            m_SmokeVFX.VFXSetActive( bActive );
        }

        gameObject.SetActive(bActive);
        /*if (bActive == true)
        {
            GetStatusEffects().SetMoveSpeedMultiplier(1.0f);
        }*/
    }

    private void OnDeath()
    {
        if (m_bDeathTriggered == false)
        {
            m_bDeathTriggered = true;
            GetAnimator().SetBool( "Investigate", false );
            GetAnimator().SetBool( "Attack", false);
            GetAnimator().SetBool( "Moving", false );
            GetAIAttack().Cleanup();
            if ( m_SmokeVFX != null )
            {
                m_SmokeVFX.Cleanup();
            }
            TryResetPath();
            if (GetAgent().enabled == true)
            {
                GetAgent().isStopped = true;
                GetAgent().enabled = false;
            }
            Destroy(GetAgent());
            
            CAIManager.RemoveAI_BothLists(this);
            CAIManager.RemoveFromCombatList(this);
            CAIManager.RemoveFromInvestigateList(this);
        }
        else
        {
            m_fDeathTimer += Time.deltaTime;
            if ( m_fDeathTimer > m_fDestroyAfterDuration )
            {
                Destroy(gameObject);
            }
        }
    }
}
