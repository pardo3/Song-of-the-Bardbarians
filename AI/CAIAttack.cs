using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAIAttack : MonoBehaviour
{
    public              List<GameObject>        m_ListVFXPrefabGO           = new List<GameObject>();   //Which vfx object to spawn
    public              List<Material>          m_ListAttackMaterials       = new List<Material>();     //Debug Bullshit
    public              List<GameObject>        m_ListWeaponsGO             = new List<GameObject>();   //Damage Trigger
    public              List<float>             m_ListAttackBuildUpTime     = new List<float>();        //Time until collider activation
    public              List<float>             m_ListAttackDamageTime      = new List<float>();        //Collider active duration
    [SerializeField]    List<int>               m_ListAttackDamage          = new List<int>();          //Attack damage
    [HideInInspector]   public List<GameObject> m_ListVFXTempGO             = new List<GameObject>();   //References to spawned vfx objects
    
    public bool                                 m_bShowDebugVFX             = true;
    bool                                        m_bIsAttacking              = false;
    bool                                        m_bCanAttack                = true;
    int                                         m_iCurrentAttackIndex       = 0;

    [SerializeField]    float                   m_fAttackRange              = 2.5f;
    public              float                   m_fAttackFrequency          = 2.0f;
    [HideInInspector]   public float            m_fAttackTimer              = 0.0f;
    [HideInInspector]   public  CStatusEffects  m_StatusEffects;
    Animator                                    m_Animator;

    public bool         GetIsAttacking()    {   return m_bIsAttacking;              }   
    public bool         GetCanAttack()      {   return m_bCanAttack;                }
    public float        GetAttackRange()    {   return m_fAttackRange;              }
    public int          GetAttackIndex()    {   return m_iCurrentAttackIndex;       } 
    public Animator     GetAnimator()       {   return m_Animator;                  }

    public void         SetCanAttack( bool bNewBool )       {   m_bCanAttack = bNewBool;    }
    public void         SetAttackIndex( int iNewIndex )     
    {   
        m_iCurrentAttackIndex = iNewIndex; 
        GetAnimator().SetInteger( "AttackIndex", iNewIndex );   
    }

    public virtual void Start()
    {
        //for (int i = 0; i < m_ListVFXPrefabGO.Count; i++)
        //{
        //    m_ListVFXTempGO.Add(null); // to fill the list
        //}
        for(int i = 0; i < m_ListVFXPrefabGO.Count; ++i)
        {
            m_ListVFXTempGO.Add(Instantiate(m_ListVFXPrefabGO[i]));
            DisableVFX(i);
        }

        m_bIsAttacking = false;
        m_bCanAttack = true;
        m_StatusEffects = GetComponent<CStatusEffects>();
        m_Animator = GetComponent<Animator>();
        for ( int i = 0; i < m_ListWeaponsGO.Count; ++i )
        {
            m_ListWeaponsGO[i].GetComponent<CWeapon>().SetDamage( m_ListAttackDamage[i] );
            if ( m_bShowDebugVFX == false )
            {
                m_ListWeaponsGO[i].GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    public virtual void Attack( int iAttackIndex )
    {
        //overridden
    }

    public virtual void Attack( Vector3 pos )
    {
        //overridden
    }

    public void Attack_ColliderDisable()
    {
        m_ListWeaponsGO[m_iCurrentAttackIndex].gameObject.SetActive( false );
        m_ListWeaponsGO[m_iCurrentAttackIndex].GetComponent<CWeapon>().DisableWeapon();
        m_bIsAttacking = false;
    }

    void InterruptAttacks()
    {
        for (int i = 0; i < m_ListWeaponsGO.Count; i++)
        {
            CWeapon weapon = m_ListWeaponsGO[m_iCurrentAttackIndex].GetComponent<CWeapon>();
            weapon.Initialize();
            weapon.DisableWeapon();
            m_ListWeaponsGO[m_iCurrentAttackIndex].gameObject.SetActive( false );
        }
        m_bIsAttacking = false;
        m_bCanAttack = true;
        //additional stuff if need be
    }

    public virtual void Cleanup()
    {
        //overridden
        InterruptAttacks();
        CancelInvoke();
        for (int i = 0; i < m_ListVFXTempGO.Count; i++)
        {
            if (m_ListVFXTempGO[i] != null)
            {
                //Destroy(m_ListVFXTempGO[i]);
                DisableVFX(i);
            }
        }
    }

    public void OnAttackStarted()
    {
        m_bIsAttacking = true;
        m_bCanAttack = false;
        m_fAttackTimer = 0;
    }

    public void EnableVFX( int iVFXIndex, Vector3 pos, Quaternion rot )
    {
        //bool to check if can instantiate particles
        //DestroyVFX( iVFXIndex );
        //m_ListVFXTempGO[ iVFXIndex ] = Instantiate( m_ListVFXPrefabGO[ iVFXIndex ], pos, rot );
        //GameObject test = Instantiate( m_ListVFXPrefabGO[ iVFXIndex ], pos, rot );
        if (m_ListVFXTempGO[iVFXIndex] != null)
        {

            m_ListVFXTempGO[iVFXIndex].transform.SetParent(null);
            m_ListVFXTempGO[iVFXIndex].transform.SetPositionAndRotation(pos, rot);

            if (m_ListVFXTempGO[iVFXIndex].GetComponentInChildren<ParticleSystem>() != null)
                m_ListVFXTempGO[iVFXIndex].GetComponentInChildren<ParticleSystem>().Play();

            m_ListVFXTempGO[iVFXIndex].SetActive(true);
            
        }

    }
    public void DisableVFX( int iVFXIndex )
    {
        if (m_ListVFXTempGO[iVFXIndex] != null)
        {
            m_ListVFXTempGO[iVFXIndex].SetActive(false);
            m_ListVFXTempGO[iVFXIndex].transform.SetParent(transform);
        }
    }
}
