using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviourPunCallbacks
{   
    // Player Hands variables

    private float meleeTime = 0.7f;
    private float reloadingTime = 3f;

    public float range = 100f;
    public float fireRate = 8f;
    public GameObject bullettrail;
    public float impactForce = 20f;

    public int totalAmmo = 280;
    public int clipSize = 30;
    public int currentAmmo;
    public int health = 100;
    public int damage = 20;

    public float teste;

    public GameObject bulletObject;
    public AudioSource reloadSound;
    public AudioSource shootSound;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    private float nextTimeToFire = 0f;

    public bool noGuns = false;

    // Player Body variables

    public CharacterController controller;
    
    public Animator bodyAnimator;
    public Animator handAnimator;
    public Transform groundCheck;
    public LayerMask groundMask;
    
    public float speed = 7f;
    public float gravity = -19.62f;
    public float jumpHeight = 1.3f;
    public float groundDistance = 0.05f;

    public int killCounter = 0;
    public int deathCounter = 0;

    Vector3 velocity;

    bool isGrounded;
    bool waitingForSpawn;

    public bool jumpingAnim = false;
    public bool runningAnim = false;
    public bool idleAnim = true;
    public bool reloadingAnim = false;
    public bool shootingAnim = false;

    AudioSource aHeroHasFallen;
	CharacterController rb;
    Transform heaven;
    Camera fpsCam;
	PhotonView PV;
    Canvas canvas;
    Text bulletsText;
    Text lifeText;
    

	void Awake()
	{   
        fpsCam = GetComponentInChildren<Camera>();
		rb = GetComponent<CharacterController>();
		PV = GetComponent<PhotonView>();
        canvas = GetComponentInChildren<Canvas>();
	}


    void Start()
    {   
        
        aHeroHasFallen = GameObject.Find("aHeroHasFallen").GetComponent<AudioSource>();
        heaven = GameObject.Find("Heaven").GetComponent<Transform>();

        if(PV.IsMine)
		{   
            if(canvas){
                Text [] canvasItem = canvas.GetComponentsInChildren<Text>();

                for(int i = 0; i< canvasItem.Length; i++){
                    if(canvasItem[i].name == "Bullets")
                        bulletsText = canvasItem[i];
                    else if(canvasItem[i].name == "Life")
                        lifeText = canvasItem[i];
                }
            }
            waitingForSpawn = false;
            currentAmmo = clipSize;
            bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
            lifeText.text = health.ToString();
        }
        else
		{
			Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(GetComponentInChildren<Canvas>().gameObject);
            Destroy(fpsCam);
			Destroy(rb);
		}
    }

    // Update is called once per frame
    void Update()
    {   
        if(!PV.IsMine)
			return;

        if(waitingForSpawn)
            return;

        Debug.Log("Você matou: " + killCounter);
        Debug.Log("Você morreu: " + deathCounter);

        this.lifeText.text = health.ToString();

        checkHands();

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if(isGrounded && velocity.y <0)
        {   
            jumpingAnim = false;
            velocity.y = -2f;
        }

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpingAnim = true;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
            
        if(jumpingAnim){
            PV.RPC("OnAnimationChange",RpcTarget.Others,"Jump");
            bodyAnimator.Play("Jump");
        }else if(move.magnitude != 0f){  
            runningAnim = true;
            idleAnim = false;
            PV.RPC("OnAnimationChange",RpcTarget.Others,"Run");
            bodyAnimator.Play("Run");
        } else {
            idleAnim = true;
            runningAnim = false;
            PV.RPC("OnAnimationChange",RpcTarget.Others,"Idle");
            bodyAnimator.Play("Idle");
        }
        
    }

    [PunRPC]
    void OnAnimationChange(string anim)
    {
        bodyAnimator.Play(anim);
    }

    void checkHands()
    {   
        if(noGuns)
            return;

        if(reloadingAnim)
            return;

        if(Input.GetKeyDown(KeyCode.R)){
            if(totalAmmo > 0){
                if(clipSize != currentAmmo)
                    StartCoroutine(Reload());
            }
        }
        else if(Input.GetKeyDown(KeyCode.F)){
            StartCoroutine(Melee());
        } 
        else if(Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {   
            if(currentAmmo > 0){
                nextTimeToFire = Time.time + 1f/fireRate;
                Shoot();
            } else if(totalAmmo > 0){
                if(clipSize != currentAmmo)
                    StartCoroutine(Reload());
            } else{
                CheckAnimation();
            }
        } else{
            CheckAnimation();
        }
        

    }

    IEnumerator Reload()
    {   
        reloadingAnim = true;
        handAnimator.SetBool("Reloading", true);
        reloadSound.Play(0);
        yield return new WaitForSeconds(reloadingTime);

        handAnimator.SetBool("Reloading", false);

        yield return new WaitForSeconds(0.25f);

        int clip = totalAmmo - clipSize + currentAmmo;
        totalAmmo = clip;
        if(clip > clipSize)
            currentAmmo = clipSize;
        else 
            currentAmmo = System.Math.Abs(clip);
        
        reloadingAnim = false;

        bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
        
    }

    IEnumerator Melee()
    {   
        reloadingAnim = true;
        handAnimator.SetBool("Meleeing", true);
        yield return new WaitForSeconds(meleeTime);

        handAnimator.SetBool("Meleeing", false);

        yield return new WaitForSeconds(0.25f);

        reloadingAnim = false;
    }

    [PunRPC]
    public void ShootSound()
    {
        //shootSound.Play(0);
    }


    void Shoot()
    {   
        shootingAnim = true;
        muzzleFlash.Play();

        currentAmmo--;
        shootSound.Play(0);
        this.GetComponent<PhotonView>().RPC("ShootSound",RpcTarget.Others);

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f,0.5f,0f));
        RaycastHit hit;
    
        Vector3 targetPoint;
        if(Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);   

        Vector3 directionWithoutSpread = targetPoint - muzzleFlash.transform.position;

        GameObject bullets = Instantiate(bullettrail.gameObject, muzzleFlash.transform.position,Quaternion.identity);
        LineRenderer lineRenderer = bullets.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, muzzleFlash.transform.position);
        lineRenderer.SetPosition(1, targetPoint);

        /*
        object[] instanceData = new object[2];
        instanceData[0] = directionWithoutSpread.normalized;
        instanceData[1] = this.PV.InstantiationId;
        PhotonNetwork.Instantiate ("BulletTrail",muzzleFlash.transform.position,Quaternion.identity,0,instanceData);
        */
        shootingAnim = false;

        bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
    }

    void CheckAnimation()
    {   
        if(shootingAnim)
            return;
        
        if(jumpingAnim){
            handAnimator.Play("Idle");
        } else if(runningAnim){
            handAnimator.Play("Run");
        } else if(idleAnim){
            handAnimator.Play("Idle");
        }
    }

    IEnumerator ResetVariabels(PlayerMovement target) 
    {   
        target.jumpingAnim = false;
        target.runningAnim = false;
        target.idleAnim = true;
        target.velocity.y = -2f;
        target.totalAmmo = 280;
        target.clipSize = 30;
        target.currentAmmo = 30;
        target.enabled = false;
        yield return new WaitForSeconds(0.3f);

        float x = Random.Range(-(target.heaven.transform.position.x + 5f), (target.heaven.transform.position.x + 5f));
        float z = Random.Range(-(target.heaven.transform.position.z + 5f), target.heaven.transform.position.z + 5f);
        target.transform.position = new Vector3(x,target.heaven.transform.position.y + 4f,z);
        target.health = 100;
        //life.text = health.ToString();
        //target.bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
        yield return new WaitForSeconds(0.3f);
        target.enabled = true;
        target.waitingForSpawn = false;
        
    }

    void OnTriggerEnter(Collider other) 
    {   
        if(!PV.IsMine)
			return;

        if(other.tag == "Bullet")
        {   
            Bullet bulletHit = other.GetComponent<Bullet>();
            this.health = this.health - bulletHit.damage;
            bulletHit.HitPlayer();

            if(this.health <= 0){
                
                this.health = 0;

                PlayerMovement[] allPlayers = FindObjectsOfType<PlayerMovement>();

                foreach (PlayerMovement player in allPlayers)
                {
                    Debug.Log(player.PV.InstantiationId);
                    if(player.PV.InstantiationId == bulletHit.playerWhoShooted){
                        Kill(this,player);
                        break;
                    }
                }
                
            }
        } 

        if(other.tag == "Respawn")
        {   
            Kill(this,null);
        } else if(other.tag == "NoGuns")
        {
            noGuns = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(!PV.IsMine)
			return;

        if(other.tag == "NoGuns")
        {
            noGuns = false;
        } 
    }
    
    void Kill(PlayerMovement target, PlayerMovement enemy)
    {   
        if(target.waitingForSpawn == false)
        {   
            target.deathCounter++;
            PV.RPC("playGeralSound",RpcTarget.AllBuffered);
            if(enemy != null && (enemy.PV.InstantiationId != target.PV.InstantiationId)){
                enemy.killCounter ++;
            }
            target.waitingForSpawn = true;
            StartCoroutine(ResetVariabels(target));
        }
    }

    
    [PunRPC]
    public void playGeralSound()
    {
        aHeroHasFallen.Play(0);
    }

}
