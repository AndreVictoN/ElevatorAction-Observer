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
        int floor = 0;
        //Debug.Log(evt.ToString());
        if (evt != EventsEnum.PLAYER_IN_ELEVATOR && evt != EventsEnum.PLAYER_NOT_IN_ELEVATOR)
        {
            if (this.gameObject.CompareTag(evt.ToString()))
            {
                this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            }
            else
            {
                StartCoroutine(ActivateColision());
            }
        }
        else if (evt == EventsEnum.PLAYER_IN_ELEVATOR)
        {
            this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
        else if (evt == EventsEnum.PLAYER_NOT_IN_ELEVATOR)
        {
            foreach (EventsEnum enumEvent in EventsEnum.GetValues(typeof(EventsEnum)))
            {
                floor++;
                if(this == null || gameObject == null) { break; }
                if (this.gameObject.CompareTag(enumEvent.ToString())) { break; }
            }
            if (this == null || gameObject == null) return;
            if (PlayerController.NetInstance.GetCurrentFloor() != floor) this.gameObject.GetComponent<BoxCollider2D>().enabled = true;
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
    }

    private void ManageElevatorColliders()
    {
        List<Collider2D> colliders = new();

        if(_elevatorsToIgnore.Count > 0)
        {
            foreach(ElevatorManager elevator in _elevatorsToIgnore)
            {
                colliders.AddRange(elevator.GetComponents<BoxCollider2D>());
            }
        }

        foreach(BoxCollider2D collider in colliders)
        {
            Physics2D.IgnoreCollision(collider, this.gameObject.GetComponent<BoxCollider2D>(), true);
        }
    }
}
