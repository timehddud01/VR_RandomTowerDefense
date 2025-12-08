using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    public Transform bulletImpact;
    ParticleSystem bulletEffect;
    AudioSource bulletAudio;
    public Transform crosshair;

    // Start is called before the first frame update
    void Start()
    {
        bulletEffect = bulletImpact.GetComponent<ParticleSystem>();
        bulletAudio = bulletImpact.GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        ARAVRInput.DrawCrosshair(crosshair);
        if (ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger)){
            Ray ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
            
            bulletAudio.Stop();
            bulletAudio.Play();

            RaycastHit hitinfo;
            int playerLayer = 1<<LayerMask.NameToLayer("Player");
            int TowerLayer = 1<<LayerMask.NameToLayer("Tower");
            int layerMask = playerLayer | TowerLayer;

            if(Physics.Raycast(ray,out hitinfo, 200, ~layerMask)){
                //총알 파편 처리

                bulletEffect.Stop();
                bulletEffect.Play();
                bulletImpact.position = hitinfo.point;
                bulletImpact.forward = hitinfo.normal;

                if(hitinfo.transform.name.Contains("Drone")){
                    DroneAI drone = hitinfo.transform.GetComponent<DroneAI>();
                    if(drone){
                        drone.onDamagedProcess();
                    }
                }
                
            }


        }
    }
}
