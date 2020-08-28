using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAIAttackRanged : CAIAttack
{
    float                                   m_fRangedAnimationDuration  = 1.0f;
    [SerializeField]    int                 m_fProjectileSpeed          = 75;
    [SerializeField]    float               m_fProjectileSpawnOffset    = 0;
    
    float                                   m_fProjectileTimestamp;
    [SerializeField]    Transform           m_ProjectileSpawnTransform  = null;   
    Vector3                                 m_TargetPos;

    bool                                    m_bExplosionTriggered       = false;
    bool                                    m_bReverseDir               = false;
    bool                                    m_bRequestedCleanup         = false;
    //VFX0 = Projectile
    //VFX1 = Explosion
    //VFX2 = Collider Marker


    public override void Start()
    {
        base.Start();
        if ( m_ProjectileSpawnTransform == null )
        {
            m_ProjectileSpawnTransform = transform;
        }
    }

    void Update()
    {
        if ( GetCanAttack() == false )
        {
            if ( GetIsAttacking() == true )
            {
                if ( m_ListVFXTempGO[ 0 ] != null )
                {
                    Vector3 pos = m_ListVFXTempGO[0].transform.position;
                    Vector3 tempProjectilePos;
                    if (m_bReverseDir == true )
                    {   
                        tempProjectilePos = new Vector3(pos.x, pos.y -= m_fProjectileSpeed * Time.deltaTime, pos.z);
                        if ( tempProjectilePos.y < m_TargetPos.y )
                        {
                            if (m_bExplosionTriggered == false)
                            {
                                RangedAttack_Explode();
                            }
                        }
                    }
                    else 
                    {
                        tempProjectilePos = new Vector3(pos.x, pos.y += m_fProjectileSpeed * Time.deltaTime, pos.z);
                        if ( Time.time > m_fProjectileTimestamp + ( m_ListAttackBuildUpTime[ GetAttackIndex() ] / 2 ) + m_fRangedAnimationDuration)
                        {
                            m_bReverseDir = true;
                            tempProjectilePos = new Vector3(m_TargetPos.x, tempProjectilePos.y, m_TargetPos.z);
                        }
                    }
                    m_ListVFXTempGO[0].transform.position = tempProjectilePos;
                }
            }
            else
            {
                if ( m_fAttackTimer < m_fAttackFrequency )
                {
                    m_fAttackTimer += Time.deltaTime;
                }
                else
                {
                    SetCanAttack( true );
                }
            }
        }
    }

    public override void Attack( Vector3 targetPos )
    {
        m_TargetPos = targetPos;
        SetAttackIndex(0);
        GetAnimator().SetInteger("AttackIndex", 1); //ranged attack is index 1 in animator
        //m_ListWeaponsGO[ 0 ].transform.position = m_TargetPos;
        //m_ListWeaponsGO[ 0 ].transform.SetParent( null );
        ///Test
        for (int i = 0; i < m_ListWeaponsGO.Count; i++)
        {
            m_ListWeaponsGO[ i ].transform.position = m_TargetPos;
            m_ListWeaponsGO[ i ].transform.SetParent( null );
        }
        m_fProjectileTimestamp = Time.time;
        m_bExplosionTriggered = false;

        RangedAttack_ColliderEnable();
        OnAttackStarted();

        //CSoundBank.Instance.GoblinRangedAttack(gameObject);
    }

    public override void Cleanup()
    {
        if (GetIsAttacking() == false)
        {
            //m_ListWeaponsGO[ 0 ].transform.SetParent( transform );
            ///Test
            for (int i = 0; i < m_ListWeaponsGO.Count; i++)
            {
                m_ListWeaponsGO[i].transform.SetParent(transform);
            }
            base.Cleanup();
        }
        else
        {
            m_bRequestedCleanup = true;
        }
    }

    void RangedAttack_Explode()
    {
        Vector3 explosionPos = m_TargetPos;
        explosionPos.y += 0.05f;
        //Destroy( m_ListVFXTempGO[ 2 ]);
        EnableVFX( 1, explosionPos, Quaternion.identity );
        
        m_bExplosionTriggered = true;

        DisableVFX(0);
        DisableVFX(2);

        CSoundBank.Instance.GoblinRangedExplosion(explosionPos);

        if (m_bRequestedCleanup == true)
        {
            ParticleSystem ExplosionTempRef = m_ListVFXTempGO[1].GetComponentInChildren<ParticleSystem>();
            float ExplosionRemainingTime = ExplosionTempRef.main.duration - ExplosionTempRef.time;
            Invoke("OnExplosionVFXEnd", ExplosionRemainingTime);
            m_bRequestedCleanup = false;
        }

        
    }

    void RangedAttack_ColliderEnable()
    {
        EnableVFX(2, m_TargetPos, Quaternion.identity);
        m_ListWeaponsGO[ GetAttackIndex() ].gameObject.SetActive( true );
        if ( m_bShowDebugVFX == true ) 
        {
            m_ListWeaponsGO[ GetAttackIndex() ].GetComponent<MeshRenderer>().material = m_ListAttackMaterials[ 0 ];
        }

        m_bReverseDir = false;
        Invoke( "RangedAttack_InstantiateProjectile", m_fRangedAnimationDuration + m_fProjectileSpawnOffset );
        Invoke( "RangedAttack_DealDamage", m_ListAttackBuildUpTime[ GetAttackIndex() ] + m_fRangedAnimationDuration );
    }

    void RangedAttack_DealDamage()
    {
        m_ListWeaponsGO[ GetAttackIndex() ].GetComponent<CWeapon>().EnableWeapon();
        if ( m_bShowDebugVFX == true )
        {
            m_ListWeaponsGO[ GetAttackIndex() ].GetComponent<MeshRenderer>().material = m_ListAttackMaterials[ 1 ];
        }
        Invoke( "Attack_ColliderDisable", m_ListAttackDamageTime[ GetAttackIndex() ] );
    }

    void RangedAttack_InstantiateProjectile()
    {
        EnableVFX(0, m_ProjectileSpawnTransform.position, Quaternion.identity);
    }

    void OnExplosionVFXEnd()
    {
        if (GetIsAttacking() == false)
        {
            Cleanup();
            
        }
    }
    
}
