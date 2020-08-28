using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CStatusEffects : MonoBehaviour
{    
    
    float               m_fPushRecoveryGroundcheck          = 0.9f;
    bool                m_bIsAI                             = false;
    float               m_fMoveSpeedMultiplier              = 1.0f;
    float               m_fPushedTimer;
    bool                m_bPushed;
    bool                m_bGrounded;
    LayerMask           m_LayerMaskGround;
    float               m_fOrigDrag;
    float               m_fOrigMass;
    float               m_fMassNonKnockback                 = 15.0f;
    

    //Components
    CAI                 m_AI;
    Animator            m_Animator;
    Rigidbody           m_Rigidbody;

    public bool         GetIsSlowed()               {   if ( GetMoveSpeedMultiplier() < 1.0f )  { return true; } else { return false ;} }  
    public bool         GetIsPushed()               {   return m_bPushed;               }

    public Rigidbody    GetRigidbody()              {   return m_Rigidbody;             }
    Animator            GetAnimator()               {   return m_Animator;              }
    public bool         GetGrounded()               {   return m_bGrounded;             }  
    public CAI          GetAI()                     {   return m_AI;                    }   

    public float        GetMoveSpeedMultiplier()    {   return m_fMoveSpeedMultiplier;  }
    public void         ResetMoveSpeedMultiplier()  {   SetMoveSpeedMultiplier( 1.0f ); }
   
    public void         SetMoveSpeedMultiplier( float newMultiplier )    
    {
        m_fMoveSpeedMultiplier = newMultiplier;

        if ( m_bIsAI == true )
        {
            GetAI().ResetSpeed();
            //m_AI.ResetSpeed();
            GetAI().GetAgent().speed *= m_fMoveSpeedMultiplier;
            GetAnimator().SetFloat( "SpeedMultiplier", GetMoveSpeedMultiplier() );
        }
    }

    private void Start()
    {
        Initialize();
    }

    public void SetPushed(bool bNewPushed )        
    {   
        m_bPushed = bNewPushed;
        if ( m_bIsAI == true )
        {
            if ( m_bPushed == true )
            {
                GetRigidbody().mass = m_fOrigMass;
                GetAI().TryResetPath();
                GetAI().GetAgent().enabled = false;
                GetRigidbody().constraints = RigidbodyConstraints.FreezeRotation;
            }
            else
            {
                GetRigidbody().mass = m_fMassNonKnockback;
                if (GetAI().GetAgent() != null )
                {
                    GetAI().GetAgent().enabled = true;
                }
                GetRigidbody().constraints = RigidbodyConstraints.FreezeAll;
            }
            GetAnimator().SetBool("Pushed", bNewPushed );
        }
    }

    void Initialize()
    {
        m_LayerMaskGround = CLayers.GetLayerMask_Ground();
        m_bGrounded = Physics.CheckSphere( transform.position, 1.5f, m_LayerMaskGround );
        if ( m_bGrounded == false )
        {
            print("Warning: " + gameObject.name + " isn't on the ground");
        }
        /////////RIGIDBODY/////////
        m_Rigidbody = GetComponent<Rigidbody>();
        m_fOrigDrag = GetRigidbody().drag;
        m_fOrigMass = GetRigidbody().mass;

        ///////////AI///////////
        m_Animator = GetComponent<Animator>();
        if ( GetComponent<CAI>() )
        {
            m_bIsAI = true;
            m_AI = GetComponent<CAI>();
            GetAnimator().SetFloat( "SpeedMultiplier", 1 );
            GetRigidbody().mass = m_fMassNonKnockback;
        }
        else m_bIsAI = false;

    }

    private void Update()
    {
        //print( gameObject.name + "grounded = " + m_bGrounded );
        //always update groundcheck for player
        if (m_bIsAI == false)
        {
            m_bGrounded = Physics.CheckSphere( transform.position, 0.5f, m_LayerMaskGround );
        }

        if ( m_bPushed == true )
        {
            if ( m_bIsAI == true ) //Groundcheck AI only if m_bPushed == true
            {
                m_bGrounded = Physics.CheckSphere( transform.position, 0.5f, m_LayerMaskGround );
            }
            m_fPushedTimer += Time.deltaTime;
            if ( m_fPushedTimer > m_fPushRecoveryGroundcheck )
            {
                if ( m_bGrounded == true )
                {
                    m_fPushedTimer = 0.0f;
                    SetPushed( false );
                    GetRigidbody().drag = m_fOrigDrag;
                }
                if ( m_fPushedTimer > 3.5f )
                {
                    GetComponent<CHealth>().TakeDamage(9999);
                }
            }
        }

        if ( m_bGrounded == false )
        {
            GetRigidbody().drag = 0.0f;
            if ( GetRigidbody().drag < 0.0f )
            {
                GetRigidbody().drag = 0.0f;
            }
        }
        else
        {
            if ( m_bIsAI == false )
            {
                GetRigidbody().drag = m_fOrigDrag;
            }
        }
    }

    public void Knockback( Vector3 vDirection, float fForce )
    {
        //Play knockback animation if available
        SetPushed( true );
        m_Rigidbody.AddForce( vDirection.normalized * ( fForce * 100.0f ) );
        //print( gameObject.name + " was pushed with " + fForce + " force." );
    }
    
    public void Dash( Vector3 vDirection, float fForce )
    {
        //Same as "Knockback" function except play different animation
        SetPushed( true );
        m_Rigidbody.AddForce( vDirection.normalized * ( fForce * 100.0f ) );
        //print( gameObject.name + " was pushed with " + fForce + " force." );
    }
}
