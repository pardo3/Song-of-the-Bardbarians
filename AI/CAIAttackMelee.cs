using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAIAttackMelee : CAIAttack
{
    // Start is called before the first frame update
    [SerializeField]    List<float> m_ListGlimmerStart      = new List<float>();
    [SerializeField]    List<float> m_ListGlimmerEnd        = new List<float>();
    [SerializeField]    float       m_fDashForce            = 1000.0f;
    CGlimmer                        m_Glimmer;
    CAI.AIType                      m_AIType;
    

    CAI.AIType GetAIType()  {   return m_AIType;    }
    CGlimmer GetGlimmer()   {   return m_Glimmer;   }

    public override void Start()
    {
        base.Start();
        if ( GetComponent<CGlimmer>() )
        {
            m_Glimmer = GetComponent<CGlimmer>();
        }
        m_AIType = GetComponent<CAI>().GetAIType();
    }

    // Update is called once per frame
    void Update()
    {
        if ( GetCanAttack() == false )
        {
            if (GetIsAttacking() == false)
            {
                if ( m_fAttackTimer < m_fAttackFrequency )
                {
                    m_fAttackTimer += Time.deltaTime;
                }
                else
                {
                    SetCanAttack(true);
                }
            }
        }
    }

    public override void Attack( int iAttackIndex )
    {
        if ( iAttackIndex <= m_ListWeaponsGO.Count )
        {
            SetAttackIndex( iAttackIndex );
        }
        else 
        {
            SetAttackIndex(0);
        }
        MeleeAttack_ColliderEnable();

        OnAttackStarted();
    }

    void MeleeAttack_ColliderEnable()
    {
        m_ListWeaponsGO[ GetAttackIndex() ].gameObject.SetActive(true);
        //if has debugvfx == true
        if ( m_bShowDebugVFX == true )
        {
            m_ListWeaponsGO[ GetAttackIndex() ].GetComponent<MeshRenderer>().material = m_ListAttackMaterials[ 0 ];
        }
        
        int iAttackIndex = GetAttackIndex();
        GetGlimmer().StartGlimmerAfterDuration( m_ListGlimmerStart[iAttackIndex], m_ListGlimmerEnd[iAttackIndex]);
        
        if ( m_ListWeaponsGO[ iAttackIndex ].name == "HitRectangle_MeleeGoblin" ) //Dash attack
        {
            Invoke("MeleeAttack_DealDamageAndDash", m_ListAttackBuildUpTime[ iAttackIndex ]);
        }
        else Invoke("MeleeAttack_DealDamage", m_ListAttackBuildUpTime[ iAttackIndex ]);
    }

    void MeleeAttack_DealDamageAndDash()
    {
        m_StatusEffects.Dash(transform.forward, m_fDashForce);
        m_ListWeaponsGO[ GetAttackIndex() ].GetComponent<CWeapon>().EnableWeapon();
        //if debugvfx == true
        if ( m_bShowDebugVFX == true ) 
        {
            m_ListWeaponsGO[ GetAttackIndex() ].GetComponent<MeshRenderer>().material = m_ListAttackMaterials[1];
        }
        Invoke("Attack_ColliderDisable", m_ListAttackDamageTime[ GetAttackIndex() ]);

        CSoundBank.Instance.GoblinMeleeAttack_Hit(gameObject);
    }

    void MeleeAttack_DealDamage()
    {
        m_ListWeaponsGO[ GetAttackIndex() ].GetComponent<CWeapon>().EnableWeapon();
        //if debugvfx == true
        if ( m_bShowDebugVFX == true ) 
        {
            m_ListWeaponsGO[ GetAttackIndex() ].GetComponent<MeshRenderer>().material = m_ListAttackMaterials[1];
        }
        
        Invoke("Attack_ColliderDisable", m_ListAttackDamageTime[ GetAttackIndex() ]);
        //VFX
        if ( m_ListVFXPrefabGO.Count >= ( GetAttackIndex() + 1 ) )
        {
            EnableVFX( GetAttackIndex(), m_ListWeaponsGO[ GetAttackIndex() ].transform.position, transform.rotation );
        }

        CSoundBank.Instance.AIMelee_Hit(m_AIType, GetAttackIndex(), gameObject);
    }
}
