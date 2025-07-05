using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InvisibleWalls : MonoBehaviour, IObserver
{
    [SerializeField] private List<ElevatorManager> _elevatorsToIgnore = new();

    #region Observer
    public void OnNotify(EventsEnum evt)
    {
        Debug.Log(evt.ToString());
        if(this.gameObject.CompareTag(evt.ToString()))
        {
            this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }else{
            StartCoroutine(ActivateColision());
        }
    }

    public IEnumerator ActivateColision()
    {
        yield return new WaitForSeconds(2f);
        this.gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }
    #endregion

    void Awake()
    {
        ManageElevatorColliders();

        if(this.gameObject.CompareTag("FIRST_FLOOR"))
        {
            this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void ManageElevatorColliders()
    {
        List<Collider2D> colliders = new();

        foreach(ElevatorManager elevator in _elevatorsToIgnore)
        {
            colliders.AddRange(elevator.GetComponents<BoxCollider2D>());
        }

        foreach(BoxCollider2D collider in colliders)
        {
            Physics2D.IgnoreCollision(collider, this.gameObject.GetComponent<BoxCollider2D>(), true);
        }
    }
}
