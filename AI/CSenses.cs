using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSenses : MonoBehaviour
{
    CPlayerControlls                m_Player;
    [SerializeField] Transform      m_Head;
    CAI                             m_AI;
    [SerializeField] float          m_fDetectAfterSeconds   = 0.25f;
    [SerializeField] float          m_fTimeUntilLoseSight   = 2.0f;
    float                           m_fSightDuration        = 0.0f;
    float                           m_fNormalSightCone      = 0.5f;
    float                           m_fCombatSightCone      = 0.4f;
    float                           m_fCurrentSightCone     = 0.0f;
    [SerializeField] float          m_fSightRange           = 20.0f;
    bool                            m_bSeesPlayer           = false;
    LayerMask                       m_LayerMask;

    public bool     GetSeesPlayer()                     {   return m_bSeesPlayer;                   }
    public float    GetTimeUntilLoseSight()             {   return m_fTimeUntilLoseSight;           }
    Vector3         GetHeadPos()                        {   return m_Head.transform.position;       }
    Transform       GetHeadTransform()                  {   return m_Head;                          }
    public float    GetNormalSightCone()                {   return m_fNormalSightCone;              }
    public float    GetCombatSightCone()                {   return m_fCombatSightCone;              }
    public void     SetSightCone( float fNewSightCone ) {   m_fCurrentSightCone = fNewSightCone;    }
    public void     SetSightDuration(float fNewSightDur){   m_fSightDuration = fNewSightDur;        }

    //Update senses every few frames instead of every frame
    //add Time.deltaTime to a float every frame even if i don't update that frame. 
    //This is to accumulate the correct values that will be used to multiply with, instead of Time.deltaTime
    void Start()
    {   
        m_LayerMask = ~CLayers.GetLayerMask_EnemyAndWeapon(); //layermask to ignore collisions with EnemyLayer and Weapon Layer
        m_AI = GetComponent<CAI>();
        m_Player = CPlayerControlls.GetPlayer();
        m_fCurrentSightCone = m_fNormalSightCone;
        if (m_Head == null)
        {
            print( "Error: Variable m_Head of " + gameObject.name + " prefab was NULL!" );
            m_Head = transform;
        }
    }

    void Update()
    {
        m_bSeesPlayer = false;
        Vector3 vDirToPlayer = m_Player.GetPlayerHeadPos() - GetHeadPos();
        //Debug.DrawRay(GetHeadPos(), vDirToPlayer);
        if ( Vector3.Distance( GetHeadPos(), m_Player.GetPlayerHeadPos() ) < m_fSightRange )
        {
            if ( Vector3.Dot( GetHeadTransform().transform.forward, vDirToPlayer.normalized ) >= m_fCurrentSightCone )
            {
                if ( RayToPlayer() == true )
                {
                    m_bSeesPlayer = true;
                    m_AI.SetLoseSightTimer( GetTimeUntilLoseSight() );
                    m_AI.SetPOI( m_Player.transform.position );
                }
            }
        }

        if ( GetSeesPlayer() == true )
        {
            if ( m_fSightDuration < m_fDetectAfterSeconds )
            {
                m_fSightDuration += Time.deltaTime;
                if ( m_fSightDuration >= m_fDetectAfterSeconds )
                {   
                    m_AI.SetCombat(true);
                    //CAIManager.AddToCombatList(m_AI);
                }
            }
        }
        else //m_bSeesPlayer == false
        {
            if ( m_fSightDuration > 0.0f && m_fSightDuration < m_fDetectAfterSeconds )
            {   
                m_fSightDuration -= Time.deltaTime;
                if (m_fSightDuration <= 0.0f)
                {
                    m_fSightDuration = 0.0f;
                }
                m_AI.SetPOI( m_Player.transform.position );
            }
            if ( m_AI.GetCombat() == true )
            {
                if ( m_AI.LoseCombatSlowly() == true ) // returns true if Combat was turned false
                {
                    m_fSightDuration = 0.0f;
                }
            }
        }
    }

    public bool RayToPlayer()
    {
        Vector3 vDirToPlayer = m_Player.GetPlayerHeadPos() - GetHeadPos();
        Ray ray = new Ray( GetHeadPos(), vDirToPlayer);
        RaycastHit hit;
        if ( Physics.Raycast( ray, out hit, m_fSightRange, m_LayerMask, QueryTriggerInteraction.Ignore ) )
        {
            if ( hit.transform.gameObject.tag == "Player" )
            {
                return true;
            }
        }
        return false;
    }

    public void Listen( Vector3 noisePos )
    {
        m_AI.SetPOI( noisePos );
        /*if ( m_AI.GetCombat() == false )
        {
            m_AI.SetPOI( noisePos );
            //print( gameObject.name + " heard something." );
        }*/
    }
}
