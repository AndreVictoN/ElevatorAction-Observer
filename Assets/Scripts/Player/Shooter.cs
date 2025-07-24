using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject projectile;
    private GameObject _instance = null;
    [SerializeField]private float _timeToShoot = 1;
    private bool _firstShoot = true;

    void Awake()
    {
        _firstShoot = true;
    }
    
    void Update()
    {
        if(this.gameObject.transform.parent.name == "Player")
        {
            if(!GameManager.Instance.IsGamePaused() && Input.GetKeyDown(KeyCode.Z) && !PlayerController.Instance.IsInDoor() && _timeToShoot == 0)
            {
                _instance = Instantiate(projectile, transform.position, Quaternion.identity);
                GameManager.Instance.PlayAudio(3);
                _timeToShoot = 0.5f;
            }

            if(PlayerController.Instance.IsCrouching())
            {
                this.transform.localPosition = new Vector2(this.transform.localPosition.x, -0.33f);
            }else{
                this.transform.localPosition = new Vector2(this.transform.localPosition.x, 0.09f);
            }
        }
        
        if(transform.parent != null && transform.parent.GetComponent<Enemy>() != null){
            if(!GameManager.Instance.IsGamePaused() && _timeToShoot == 0 && transform.parent.GetComponent<Enemy>().IsSeeingPlayer())
            {
                if(!_firstShoot)
                {
                    _instance = Instantiate(projectile, transform.position, Quaternion.identity);
                    GameManager.Instance.PlayAudio(4);
                }else{
                    _firstShoot = false;
                }
                _timeToShoot = 1f;
            }
        }

        if(_instance != null) _instance.transform.SetParent(this.gameObject.transform);
        _instance = null;

        if(_timeToShoot > 0) _timeToShoot -= Time.deltaTime;
        else if(_timeToShoot < 0) _timeToShoot = 0;
    }
}
