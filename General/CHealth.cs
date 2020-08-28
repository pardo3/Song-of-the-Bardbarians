using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class CHealth : MonoBehaviour
{
    [HideInInspector]   public int      m_iCurrentHP;
    [HideInInspector]   public bool     m_bDead;

    [SerializeField]    ParticleSystem  m_BloodVFX                  = null;
    public int                          m_iMaxHP;
    [HideInInspector]   public bool     m_bInvincibility;
    [SerializeField]    float           m_fDamageFlashingDuration   = 0.15f;

    bool                                m_bIsAI                     = false;
    CAI.AIType                          m_eAIType;
    CStatusEffects                      m_StatusEffects;
    CGlimmer                            m_Glimmer;
    Animator                            m_Animator;
    Collider                            m_Collider;
    Rigidbody                           m_Rigidbody;

    private Rigidbody                   GetRigidbody()                          {   return m_Rigidbody;                 }
    private Collider                    GetCollider()                           {   return m_Collider;                  }
    private Animator                    GetAnimator()                           {   return m_Animator;                  }
    private CGlimmer                    GetGlimmer()                            {   return m_Glimmer;                   }
    public CStatusEffects               GetStatusEffects()                      {   return m_StatusEffects;             }
    public bool                         GetIsAlive()                            {   return !m_bDead;                    }
    public int                          GetCurrentHP()                          {   return m_iCurrentHP;                }  

    public bool                         GetInvincibility()                      {   return m_bInvincibility;            }
    public void                         SetInvincibilityTrue()                  {   m_bInvincibility = true;            }
    public void                         SetInvincibilityFalse()                 {   m_bInvincibility = false;           }
    public void                         SetInvincibility( bool invincibility )  {   m_bInvincibility = invincibility;   }

    private void Start()
    {
       Initialize();
    }

    void Initialize()
    {
        m_bInvincibility = false;
        m_iCurrentHP = m_iMaxHP;
        if ( GetComponent<CAI>() )
        {
            m_bIsAI = true;
            m_eAIType = GetComponent<CAI>().GetAIType();
        }
        if (GetComponent<CGlimmer>())
        {
            m_Glimmer = GetComponent<CGlimmer>();
        }
        m_StatusEffects = GetComponent<CStatusEffects>();
        m_Animator = GetComponent<Animator>();
        m_Collider = GetComponent<Collider>();
        m_Rigidbody = GetComponent<Rigidbody>();

        Shader.SetGlobalFloat("_CurrentHP", m_iCurrentHP / m_iMaxHP);
    }

    public void TakeDamage( int iAmount )
    {
        //Make sure player if player is alive or not to prevent complete
        //earrape when they aren't
        if ( m_bInvincibility == false && m_bDead == false )
        {
            m_iCurrentHP -= iAmount;
            DamageTakenEffect();
            
            if ( GetGlimmer() != null )
            {
                GetGlimmer().OnDamageTakenFlash( m_fDamageFlashingDuration );
            }
            if ( m_iCurrentHP <= 0 )
            {   
                Death();
            }
            //Update health UI
            else
            {
                if ( m_bIsAI == false )
                {
                    CSoundBank.Instance.PlayerTakeDamage( gameObject );
                    CUIManager.UpdateHealthUI();
                    CAmbienceController.Instance.SetHeartbeatParameter((float)m_iCurrentHP / m_iMaxHP);
                    Shader.SetGlobalFloat("_CurrentHP", m_iCurrentHP / m_iMaxHP);
                }
                else
                {
                    CSoundBank.Instance.AITakeDamage(m_eAIType, gameObject);
                }
            }
        }
        //print(gameObject.name + " took " + iAmount + " damage. " + m_iCurrentHP + " HP remaining.");
    }

    public void Resurrect()
    {
        GetCollider().enabled = true;
        GetRigidbody().isKinematic = false;
        GetRigidbody().useGravity = true;
        
        m_bDead = false;
        m_iCurrentHP = m_iMaxHP;
        GetAnimator().SetBool("Dead", m_bDead);
        CUIManager.UpdateHealthUI();
        CAmbienceController.Instance.SetHeartbeatParameter((float)m_iCurrentHP / m_iMaxHP);
        Shader.SetGlobalFloat("_CurrentHP", m_iCurrentHP / m_iMaxHP);
        GetStatusEffects().SetMoveSpeedMultiplier(1.0f);
        //print(gameObject.name + " was Resurrected");
    }
    public void Heal(int iAmount)
    {
        if ( m_bDead == false && m_bIsAI == false)
        {
            m_iCurrentHP += iAmount;
            if (m_iCurrentHP > m_iMaxHP)
                m_iCurrentHP = m_iMaxHP;

            CUIManager.UpdateHealthUI();
            CAmbienceController.Instance.SetHeartbeatParameter((float)m_iCurrentHP / m_iMaxHP);
            Shader.SetGlobalFloat("_CurrentHP", m_iCurrentHP / m_iMaxHP);
        }
    }
    void DamageTakenEffect()
    {
        if ( m_BloodVFX != null )
        {
            m_BloodVFX.Play();
        }
    }
    void Death()
    {
        if (m_bIsAI == false)
        {
            //GetRigidbody().isKinematic = true;
            GetRigidbody().useGravity = false;
            GetRigidbody().isKinematic = true;
            GetCollider().enabled = false;
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Death");
        }
        //GetRigidbody().isKinematic = true;
        //GetCollider().enabled = false;

        m_iCurrentHP = 0;
        m_bDead = true;
        GetAnimator().SetBool( "Dead", m_bDead );
        CTriggerZoneManager.RemoveFromAllTriggerZones( gameObject );
        if (m_bIsAI == false)
        {
            CSoundBank.Instance.PlayerDeath( gameObject) ;
        }
        else
        {
            CSoundBank.Instance.AIDeath( m_eAIType, gameObject );
        }
    }
}
