using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviourPunCallbacks
{   
    private float meleeTime = 0.7f;
    private float reloadingTime = 3f;

    public float range = 100f;
    public float fireRate = 8f;
    public float impactForce = 20f;

    public int totalAmmo = 280;
    public int clipSize = 30;
    public int currentAmmo;
    public int health = 100;
    public int damage = 20;

    public GameObject bulletObject;
    public AudioSource reloadSound;
    public AudioSource shootSound;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    private float nextTimeToFire = 0f;

    public bool noGuns = false;

    public CharacterController controller;
    
    public Animator bodyAnimator;
    public Animator handAnimator;
    public Transform groundCheck;
    public LayerMask groundMask;
    
    public float speed = 7f;
    public float gravity = -19.62f;
    public float jumpHeight = 1.3f;
    public float groundDistance = 0.05f;

    public int killStreak = 0;
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

    UpdateRanking updateRanking;
    AudioSource aHeroHasFallen;
    AudioSource gireiSound;
    AudioSource ameacaSound;
	CharacterController rb;
    Transform heaven;
    Camera fpsCam;
    Canvas canvas;
    HitMarker hitMarker;
    MouseLook mouseLook;

    public PhotonView PV;
    public Text bulletsText;
    public Text lifeText;
    public Text sensibilidadeText;
    public TMP_Text streakText;

	void Awake()
	{   
        mouseLook = GetComponentInChildren<MouseLook>();
        fpsCam = GetComponentInChildren<Camera>();
		rb = GetComponent<CharacterController>();
		PV = GetComponent<PhotonView>();
        canvas = GetComponentInChildren<Canvas>();
	}


    void Start()
    {   
        
        aHeroHasFallen = GameObject.Find("aHeroHasFallen").GetComponent<AudioSource>();
        ameacaSound = GameObject.Find("ameacaSound").GetComponent<AudioSource>();
        gireiSound = GameObject.Find("gireiSound").GetComponent<AudioSource>();
        heaven = GameObject.Find("Heaven").GetComponent<Transform>();
        updateRanking = GameObject.Find("GeneralCanvas").GetComponentInChildren<UpdateRanking>();
        updateRanking.UpdatePlayers();
        
        if(PV.IsMine)
		{   
            if(canvas){
                Text [] canvasItem = canvas.GetComponentsInChildren<Text>();

                for(int i = 0; i< canvasItem.Length; i++){
                    if(canvasItem[i].name == "Bullets")
                        bulletsText = canvasItem[i];
                    else if(canvasItem[i].name == "Life")
                        lifeText = canvasItem[i];
                    else if(canvasItem[i].name == "Sensibilidade")
                        sensibilidadeText = canvasItem[i];
                }
                hitMarker = canvas.GetComponentInChildren<HitMarker>();
                streakText = canvas.GetComponentInChildren<TMP_Text>();
                
            }

            sensibilidadeText.text = "sens: " + mouseLook.mouseSensitivity.ToString();
            sensibilidadeText.color = new Color(sensibilidadeText.color.r, sensibilidadeText.color.g, sensibilidadeText.color.b, 0f);
            streakText.text =  "0 Kill Streak";
            streakText.color = new Color(streakText.color.r, streakText.color.g, streakText.color.b, 0f);
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

        checkSensitivy();

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

    void checkSensitivy()
    {
        if(Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Plus))
        {   
            StopCoroutine(ExibeSensibilidade());
            StartCoroutine(ExibeSensibilidade());
        } else if(Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus))
        {   
            StopCoroutine(ExibeSensibilidade());
            StartCoroutine(ExibeSensibilidade());
        }
        
    }   

    IEnumerator ExibeSensibilidade()
    {   
        sensibilidadeText.text = "sens: " + mouseLook.mouseSensitivity.ToString("#.##");
        sensibilidadeText.color = new Color(sensibilidadeText.color.r, sensibilidadeText.color.g, sensibilidadeText.color.b, 100f);
        yield return new WaitForSeconds(3f);
        sensibilidadeText.color = new Color(sensibilidadeText.color.r, sensibilidadeText.color.g, sensibilidadeText.color.b, 0f);
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

        RaycastHit hit;

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {   
            int amount = 0;
            
            if(hit.transform.tag == "PlayerHead")
                amount = 40;
            else if(hit.transform.tag == "PlayerTorso")
                amount = 20;
            else if(hit.transform.tag == "PlayerLegs")
                amount = 15;
            else if(hit.transform.tag == "PlayerFeet")
                amount = 10;

            Debug.Log(hit.transform.tag);
            if(amount != 0 ){
                if(hit.transform.gameObject){
                    PlayerMovement target = hit.transform.gameObject.GetComponentInParent<PlayerMovement>();
                    if(target.health > 0)
                        hitMarker.Hitted();

                    object[] instanceData = new object[3];
                    instanceData[0] = amount;
                    instanceData[1] = target.PV.InstantiationId;
                    instanceData[2] = this.PV.InstantiationId;
                    
                    PV.RPC("TakeDamage",RpcTarget.AllBuffered,instanceData);
                }
            } else {
                PhotonNetwork.Instantiate("HitParticles",hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

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
            //StartCoroutine(RestoreLife());
            handAnimator.Play("Idle");
        }
    }
    /*
    public IEnumerator RestoreLife()
    {
        yield return new WaitForSeconds(5f);
        if(this.health + 10 > 100){
            this.health = 100;
            StopCoroutine(RestoreLife());
        }else
            this.health = this.health + 10;
        yield return new WaitForSeconds(0.5f);
        if(this.health + 10 > 100){
            this.health = 100;
            StopCoroutine(RestoreLife());
        }else
            this.health = this.health + 10;
        yield return new WaitForSeconds(0.5f);
        if(this.health + 10 > 100){
            this.health = 100;
            StopCoroutine(RestoreLife());
        }else
            this.health = this.health + 15;
        yield return new WaitForSeconds(0.5f);
        if(this.health + 15 > 100){
            this.health = 100;
            StopCoroutine(RestoreLife());
        }else
            this.health = this.health + 15;
        yield return new WaitForSeconds(0.5f);
        if(this.health + 15 > 100){
            this.health = 100;
            StopCoroutine(RestoreLife());
        }else
            this.health = this.health + 15;
        yield return new WaitForSeconds(0.5f);
        if(this.health + 15 > 100){
            this.health = 100;
            StopCoroutine(RestoreLife());
        }else
            this.health = this.health + 15;
        yield return new WaitForSeconds(0.5f);
        if(this.health + 20 > 100){
            this.health = 100;
            StopCoroutine(RestoreLife());
        }else
            this.health = this.health + 20;
    } */

    IEnumerator ResetVariabels(PlayerMovement target) 
    {   
        target.killStreak = 0;
        target.jumpingAnim = false;
        target.runningAnim = false;
        target.idleAnim = true;
        target.velocity.y = -2f;
        target.totalAmmo = 280;
        target.clipSize = 30;
        target.currentAmmo = 30;
        target.enabled = false;
        target.health = 100;
        yield return new WaitForSeconds(0.3f);
        float x = Random.Range(-(target.heaven.transform.position.x + 5f), (target.heaven.transform.position.x + 5f));
        float z = Random.Range(-(target.heaven.transform.position.z + 5f), target.heaven.transform.position.z + 5f);
        target.transform.position = new Vector3(x,target.heaven.transform.position.y + 4f,z);
        yield return new WaitForSeconds(0.3f);
        target.health = 100;
        target.enabled = true;
        target.waitingForSpawn = false;
        
    }

    void OnTriggerEnter(Collider other) 
    {   
        if(!PV.IsMine)
			return;
        
        if(this.health <=0)
            return;
        /*
        if(other.tag == "Gun")
        {   
            if(this.totalAmmo + 180 > 280)
                this.totalAmmo = 280;
            else
                this.totalAmmo = this.totalAmmo + 180;

            Destroy(other);
            Debug.Log("Eh gun");
        }*/

        if(other.tag == "Respawn")
        {  
            object[] instanceData = new object[3];
            instanceData[0] = 100;
            instanceData[1] = this.PV.InstantiationId;
            instanceData[2] = this.PV.InstantiationId;
            PV.RPC("TakeDamage",RpcTarget.AllBuffered,instanceData);
        } else if(other.tag == "NoGuns")
        {
            noGuns = true;
        }
    }
    
    [PunRPC]
    public void TakeDamage(object[] instantiationData) //, PlayerMovement playerWhoShooted
    {   

        PlayerMovement whoReceivedDamage = null;
        PlayerMovement whoCausedDamage = null;

        PlayerMovement [] players =  FindObjectsOfType<PlayerMovement>();
        
        foreach (PlayerMovement player in players)
        {
            if(player.PV.InstantiationId == (int) instantiationData[1])
                whoReceivedDamage = player;
            if(player.PV.InstantiationId == (int) instantiationData[2])
                whoCausedDamage = player;
                
        }
        if(whoReceivedDamage)
        {   
            //whoReceivedDamage.StopCoroutine(RestoreLife());
            whoReceivedDamage.health = whoReceivedDamage.health - (int)instantiationData[0];

            if(whoReceivedDamage.health <= 0){
            
                whoReceivedDamage.health = 0;

                //PhotonNetwork.Instantiate("AkDroped",whoReceivedDamage.transform.position, Quaternion.identity);

                Kill(whoReceivedDamage,whoCausedDamage);
            }   
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
            
            if(enemy != null && (enemy.PV.InstantiationId != target.PV.InstantiationId)){
                enemy.killCounter ++;
                enemy.killStreak ++;
                CheckKillStreak(enemy);
            } else {
                PV.RPC("playDeathSound",RpcTarget.AllBuffered);
            }
            target.waitingForSpawn = true;
            updateRanking.UpdatePlayers();
            StartCoroutine(ResetVariabels(target));
            updateRanking.UpdatePlayers();
        }
    }

    IEnumerator exibeKillStreak(PlayerMovement player, string kills) 
    {   
        player.streakText.text =  kills + " Kill Streak";
        player.streakText.color = new Color(player.streakText.color.r, player.streakText.color.g, player.streakText.color.b, 100f);
        yield return new WaitForSeconds(4f);
        player.streakText.color = new Color(player.streakText.color.r, player.streakText.color.g, player.streakText.color.b, 0f);
        
    }

    void CheckKillStreak(PlayerMovement player)
    {   
        if(player.killStreak == 10){
            if(this.PV.InstantiationId == player.PV.InstantiationId)
                StartCoroutine(exibeKillStreak(player,"10"));
            PV.RPC("playGireiSound",RpcTarget.AllBuffered);
        } else if(player.killStreak == 5){
            if(this.PV.InstantiationId == player.PV.InstantiationId)
                StartCoroutine(exibeKillStreak(player,"5"));
            PV.RPC("playAmeacaSound",RpcTarget.AllBuffered);
        }else
            PV.RPC("playDeathSound",RpcTarget.AllBuffered);
        /*
        else if(player.killStreak == 5){
            StartCoroutine(exibeKillStreak(player,"5"));
            PV.RPC("playGireiSound",RpcTarget.AllBuffered);
        }*/
        
    }
    [PunRPC]
    public void playDeathSound()
    {   
        if(!gireiSound.isPlaying)
            aHeroHasFallen.Play(0);
    }

    [PunRPC]
    public void playAmeacaSound()
    {   
        if(!ameacaSound.isPlaying && !gireiSound.isPlaying)
            ameacaSound.Play(0);
    }

    [PunRPC]
    public void playGireiSound()
    {
        if(!gireiSound.isPlaying)
            gireiSound.Play(0);
    }

}
