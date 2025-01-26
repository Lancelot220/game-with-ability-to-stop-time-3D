using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Shooting : MonoBehaviour
{
    [Header("Options")]
    public int attackPower = 3;
    public int maxAmmo = 30; //M16
    public int ammo;
    [SerializeField]
     private bool AddBulletSpread = true;
    [SerializeField]
    private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Others")]
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private Transform BulletSpawnPoint;
    [SerializeField]
    private ParticleSystem ImpactParticleSystem;
    [SerializeField]
    private TrailRenderer BulletTrail;
    [SerializeField]
    private float ShootDelay;
    [SerializeField]
    private LayerMask Mask;
    [SerializeField]
    private LayerMask withoutHitEffect;
    //private Animator Animator;
    private PlayerStats player;

    private void Awake()
    {
        //Animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        ammo = maxAmmo;
    }

    public void Shoot()
    {
        if(ammo > 0)
        {
            //Animator.SetBool("isShooting", true);
            ShootingSystem.Play();
            Vector3 direction = GetDirection();

            if (Physics.Raycast(BulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, Mask))
            {
                TrailRenderer trail = Instantiate(BulletTrail,BulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, hit));
            }

            ammo--;
        }
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;

        if (AddBulletSpread)
        {
            direction += new Vector3(
                Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
            );

            direction.Normalize();
        }

        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit Hit)
    {
        float time = 0;
        Vector3 startPosition = Trail.transform.position;

        while (time < 1)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
            time += Time.deltaTime / Trail.time;

            yield return null;
        }
        //Animator.SetBool("isShooting", false);
        Trail.transform.position = Hit.point;
        if (Hit.transform.gameObject.layer != withoutHitEffect) Instantiate(ImpactParticleSystem, Hit.point, Quaternion.LookRotation(Hit.normal));

        if(Hit.transform.gameObject.layer == 3)
        {
            if (player.health > 0)
            {
                player.health -=attackPower;
                Debug.LogWarning("Your health left:" + player.health);

                StartCoroutine(Rumble.RumblePulse(0.25f, 1f, 0.5f));
            }
        }

        Destroy(Trail.gameObject, Trail.time);
    }
}
