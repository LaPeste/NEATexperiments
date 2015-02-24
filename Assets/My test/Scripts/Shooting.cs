using UnityEngine;
using System.Collections;

public class Shooting : MonoBehaviour {

    public GameObject Bullet;
    public float offsett = 2;
    public float bulletSpeed = 1;
    public float waitForBullet = 3;
    private float timer = 0;
	// Use this for initialization
	void Start () {
        //StartCoroutine(Shoot());
	}
	
	// Update is called once per frame
	void Update ()
    {
        timer += Time.deltaTime;
        if(timer > waitForBullet)
        {
            timer = 0;
            Shoot();
        }
    
	}

    private void Shoot()
    {
        Vector3 bulletPos = transform.position - new Vector3 (Vector3.forward.x, Vector3.forward.y, Vector3.forward.z + offsett);
        GameObject bullet = Instantiate(Bullet, bulletPos, Bullet.transform.rotation) as GameObject;
        bullet.rigidbody.AddForce(transform.forward * bulletSpeed);
    }
}
