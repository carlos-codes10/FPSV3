using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGun : Gun
{

    [SerializeField] GameObject particleEffect;
    public override bool AttemptFire()
    {
        if (!base.AttemptFire())
            return false;

        var b = Instantiate(bulletPrefab, gunBarrelEnd.transform.position, gunBarrelEnd.rotation);
        b.GetComponent<Projectile>().Initialize(100, 30, 3, 150, Explosion); // version without special effect

        anim.SetTrigger("shoot");
        elapsed = 0;
        ammo -= 1;

        return true;
    }


    void Explosion(HitData data)
    {
        Vector3 impactLocation = data.location;

        Instantiate(particleEffect, impactLocation, Quaternion.identity);
    }

}
