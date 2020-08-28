using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLayers : MonoBehaviour
{
    static LayerMask m_Layer_Wall;
    static LayerMask m_Layer_Ground;
    static LayerMask m_Layer_Enemy;
    static LayerMask m_Layer_Player;
    static LayerMask m_Layer_Weapon;
    static LayerMask m_Layer_EnemyAndWeapon;

    static public LayerMask GetLayerMask_Wall()                 {   return m_Layer_Wall;            }  
    static public LayerMask GetLayerMask_Ground()               {   return m_Layer_Ground;          }  
    static public LayerMask GetLayerMask_Enemy()                {   return m_Layer_Enemy;           }
    static public LayerMask GetLayerMask_Player()               {   return m_Layer_Player;          }
    static public LayerMask GetLayerMask_Weapon()               {   return m_Layer_Weapon;          }
    static public LayerMask GetLayerMask_EnemyAndWeapon()       {   return m_Layer_EnemyAndWeapon;  }

    private void Awake()
    {
        m_Layer_Enemy               =   1 << LayerMask.NameToLayer( "Enemy"  );
        m_Layer_Player              =   1 << LayerMask.NameToLayer( "Player" );
        m_Layer_Ground              =   1 << LayerMask.NameToLayer( "Ground" );
        m_Layer_Weapon              =   1 << LayerMask.NameToLayer( "Weapon" );
        m_Layer_EnemyAndWeapon      =   1 << LayerMask.NameToLayer( "Enemy"  ) | ( 1 << LayerMask.NameToLayer( "Weapon" ) ); 
    }

}
