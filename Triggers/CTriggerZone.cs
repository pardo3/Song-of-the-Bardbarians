using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTriggerZone : MonoBehaviour
{
    private List<GameObject>    m_ListGameObjectsInside = new List<GameObject>();

    public  bool                m_bCanCollideWithPlayer;
    public  bool                m_bCanCollideWithEnemy;

    private bool                m_bIsCollidingWithPlayer;
    private bool                m_bIsCollidingWithEnemy;

    public  List<GameObject>    Get_ListAllGameObjectsInside()              {   return m_ListGameObjectsInside;         }
    public  GameObject          Get_ListGameObjectAtIndex(int index)        {   return m_ListGameObjectsInside[index];  }
    public  int                 GetListCount()                              {   return m_ListGameObjectsInside.Count;   } 

    public  bool                GetIsSomeoneInside()                        {   if( m_ListGameObjectsInside.Count > 0 ) { return true; } return false;      }
    public  bool                Get_ListContainsGameObject( GameObject go ) {   if(m_ListGameObjectsInside.Contains( go ) ) { return true;} return false;   }

    public  bool                GetIsPlayerInside()                         {   return m_bIsCollidingWithPlayer;        }
    public  bool                GetIsEnemyInside()                          {   return m_bIsCollidingWithEnemy;         }

    public void AddToList( GameObject goToAdd )                             
    {    
        //print(goToAdd.name + " was ADDED to list of " + gameObject.name);
        m_ListGameObjectsInside.Add(goToAdd);  
    }

    public void RemoveFromList( GameObject goToRemove )
    {
        //print(goToRemove.name + " was REMOVED from list of " + gameObject.name);
        m_ListGameObjectsInside.Remove( goToRemove );
    }

    public void TryRemoveFromList( GameObject goToRemove )   
    {
        if ( Get_ListContainsGameObject( goToRemove ) == true ) 
        { 
            RemoveFromList( goToRemove );
        }
    }
    
    public void Start()
    {
        if ( GetComponent<MeshRenderer>() )
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public virtual void Awake()
    {
        CTriggerZoneManager.List_AddTriggerZone( this );
    }

    public virtual void OnTriggerEnter( Collider other )
    {
        if ( m_bCanCollideWithPlayer == true && other.gameObject.tag == "Player" )
        {
            m_bIsCollidingWithPlayer = true;
            if ( Get_ListContainsGameObject( other.gameObject ) == false )
            {
                AddToList(other.gameObject);
            }
        }
        
        if ( m_bCanCollideWithEnemy == true && other.gameObject.tag == "Enemy" )
        {
            m_bIsCollidingWithEnemy = true;
            if ( Get_ListContainsGameObject( other.gameObject ) == false )
            {
                AddToList(other.gameObject);
            }
        }
    }
    
    public virtual void OnTriggerExit( Collider other )
    {
        if ( Get_ListContainsGameObject(other.gameObject) == true )
        {
            
            if ( other.gameObject.tag == "Player" )
            {
                m_bIsCollidingWithPlayer = false;
            }

            m_ListGameObjectsInside.Remove(other.gameObject);   

            if ( m_ListGameObjectsInside.Count == 0 && m_bIsCollidingWithEnemy == false )
            {
                m_bIsCollidingWithEnemy = false;
            }
        }
    }
}
