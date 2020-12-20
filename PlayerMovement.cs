using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback, IOnEventCallback
{   
    List<AudioSource> generalAudios = new List<AudioSource>();

    private float meleeTime = 0.7f;
    private float reloadingTime = 1.5f;

    private PlayerSounds playerSound;

    private float sprint_Volume = 0.6f;
    private float crouch_Volume = 0.1f;
    private float walk_Volume_Min = 0.2f, walk_Volume_Max = 0.4f;

    private float walk_Step_Distance = 0.42f;
    private float sprint_Step_Distance = 0.38f;
    private float crouch_Step_Distance = 0.54f;

    [HideInInspector] public float walkMagnitude;

    [HideInInspector] public float range;
    [HideInInspector] public float fireRate;
    [HideInInspector] public int maxAmmo;
    [HideInInspector] public int totalAmmo;
    [HideInInspector] public int clipSize;
    [HideInInspector] public int damage;
    [HideInInspector] public int currentAmmo;

    ChangeDroppedGun nearDroppedWeapon;

    public int health = 100;

    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    private float nextTimeToFire = 0f;

    public bool noGuns = false;
    byte actualWeapon;
    public int gunIndex = 0;
    
    public Animator bodyAnimator;
    public WeaponStats primaryGun;
    public WeaponStats secondaryGun;
    public WeaponStats terciaryGun;
    public List<Animator> handAnimator = new List<Animator>();
    public List<GameObject> handWeapons = new List<GameObject>();

    public Transform groundCheck;
    public LayerMask groundMask;
    
    float speed = 5.5f;
    public float gravity = -19.62f;
    public float jumpHeight = 1.3f;
    public float groundDistance = 0.05f;

    public int killStreak = 0;
    public int killCounter = 0;
    public int deathCounter = 0;

    IEnumerator actualRoutine; // create an IEnumerator object

    Vector3 velocity;

    public bool isGrounded;
    bool isAiming = false;
    bool isReloading = false;
    public bool waitingForSpawn;

    public bool isCrounching = false;
    public bool jumpingAnim = false;
    public bool runningAnim = false;
    public bool sprintingAnim = false;
    public bool idleAnim = true;
    public bool shootingAnim = false;
    
    GhostPosition ghostPosition;
    HeadPosition headPosition;
	public CharacterController controller;
    public CanvasGroup aimPoint;
    Transform heaven;
    Vector3 move;
    public Camera fpsCam;
    public Canvas canvas;
    HitMarker hitMarker;
    public MouseLook mouseLook;
    DI_System damageIndicator;

    public PhotonView PV;
    public Text bulletsText;
    public Text lifeText;
    public Text sensibilidadeText;
    public Text changeWeaponText;
    public TMP_Text streakText;

    public string Nickname;


    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {   
        object[] instantiationData = info.photonView.InstantiationData;

        Nickname = (string) instantiationData[0];
    }

	void Awake()
	{   
        headPosition = GetComponentInChildren<HeadPosition>();
        GameManager.players.Add(this);
        ghostPosition = GetComponentInChildren<GhostPosition>();
        mouseLook = GetComponentInChildren<MouseLook>();
        fpsCam = GetComponentInChildren<Camera>();
		controller = GetComponent<CharacterController>();
		PV = GetComponent<PhotonView>();
        canvas = GetComponentInChildren<Canvas>();
        playerSound = GetComponent<PlayerSounds>();
	}


    void Start()
    {   
        walkMagnitude = 0f;
        playerSound.volume_Min = walk_Volume_Min;
        playerSound.volume_Max = walk_Volume_Max;
        playerSound.step_Distance = walk_Step_Distance;

        AudioSource auxAudio = GameObject.Find("aHeroHasFallen").GetComponent<AudioSource>();
        if(auxAudio != null)
            generalAudios.Add(auxAudio);
        auxAudio = GameObject.Find("ameacaSound").GetComponent<AudioSource>();
        if(auxAudio != null)
            generalAudios.Add(auxAudio);
        auxAudio = GameObject.Find("gireiSound").GetComponent<AudioSource>();
        if(auxAudio != null)
            generalAudios.Add(auxAudio);
        heaven = GameObject.Find("Heaven").GetComponent<Transform>();
        GameManager.updateRequest.Add(true);

        //PV.RPC("updatePlayer",RpcTarget.AllBuffered);
        //updateRanking = GameObject.Find("GeneralCanvas").GetComponentInChildren<UpdateRanking>();
        //updateRanking.UpdatePlayers(); 

        if(PV.IsMine)
		{   
            Animator [] Animators = GetComponentsInChildren<Animator>();
            
            foreach (Animator item in Animators)
            {
                if(item.tag == "Weapon"){
                    handAnimator.Add(item);
                    handWeapons.Add(item.gameObject);

                    WeaponStats weasponStats = item.gameObject.GetComponent<WeaponStats>();
                    terciaryGun = weasponStats;
                    //else if(weasponStats.gunIndex == 1)
                    //    secondaryGun = weasponStats;
                    //else if(weasponStats.gunIndex == 2)
                    //    primaryGun = weasponStats;
                }
            }

            ChangeGuns(terciaryGun);
            actualWeapon = 3;

            if(canvas){
                Text [] canvasItem = canvas.GetComponentsInChildren<Text>();

                for(int i = 0; i< canvasItem.Length; i++){
                    if(canvasItem[i].name == "Bullets")
                        bulletsText = canvasItem[i];
                    else if(canvasItem[i].name == "Life")
                        lifeText = canvasItem[i];
                    else if(canvasItem[i].name == "Sensibilidade")
                        sensibilidadeText = canvasItem[i];
                    else if(canvasItem[i].name == "GetWeapon")
                        changeWeaponText = canvasItem[i];
                }
                //aimPoint = canvas.GetComponentInChildren<CanvasGroup>();
                hitMarker = canvas.GetComponentInChildren<HitMarker>();
                streakText = canvas.GetComponentInChildren<TMP_Text>();
                damageIndicator = canvas.GetComponent<DI_System>();
                //aimPoint.alpha = 1f;
            }
            sensibilidadeText.text = "sens: " + mouseLook.mouseSensitivity.ToString();
            sensibilidadeText.color = new Color(sensibilidadeText.color.r, sensibilidadeText.color.g, sensibilidadeText.color.b, 0f);
            streakText.text =  "0 Kill Streak";
            streakText.color = new Color(streakText.color.r, streakText.color.g, streakText.color.b, 0f);
            waitingForSpawn = false;
            isAiming = false;
            currentAmmo = clipSize;
            if(actualWeapon == 3)
                bulletsText.text = "";
            else
                bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
            lifeText.text = health.ToString();
            headPosition.Invisible();
            ghostPosition.Invisible();
        }
        else
		{
			Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(GetComponentInChildren<Canvas>().gameObject);
            Destroy(mouseLook);
            Destroy(fpsCam);
			Destroy(controller);
            Destroy(canvas);
		}
        //GameManager.updateRequest.Add(true);
    }

    // Update is called once per frame
    void Update()
    {   
        
        if(!PV.IsMine)
			return;

        if(waitingForSpawn)
            return;


        Debug.Log(gunIndex);

        if(nearDroppedWeapon != null){
            float distance = Vector3.Distance(this.transform.position, nearDroppedWeapon.transform.position);
            if(distance < 1.4){
                if((primaryGun != null && nearDroppedWeapon.currentGunIndex == primaryGun.gunIndex)||(secondaryGun != null && nearDroppedWeapon.currentGunIndex == secondaryGun.gunIndex)){
                    if(nearDroppedWeapon.currentGunIndex == primaryGun.gunIndex){
                        if(primaryGun.totalAmmo < primaryGun.maxAmmo){
                            primaryGun.totalAmmo = primaryGun.totalAmmo + primaryGun.maxAmmo/2 > primaryGun.maxAmmo ? primaryGun.maxAmmo : primaryGun.totalAmmo + primaryGun.maxAmmo/2;
                            //ChangeGuns(primaryGun);
                            PV.RPC("DestroyObject",RpcTarget.All,nearDroppedWeapon.gameObject.GetInstanceID());
                            playerSound.PlaySound(3,0.4f,1);
                            bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
                        }
                    } else {
                        if(secondaryGun.totalAmmo < secondaryGun.maxAmmo){
                            secondaryGun.totalAmmo = secondaryGun.totalAmmo + secondaryGun.maxAmmo/2 > secondaryGun.maxAmmo ? secondaryGun.maxAmmo : secondaryGun.totalAmmo + secondaryGun.maxAmmo/2;
                            //ChangeGuns(secondaryGun);
                            PV.RPC("DestroyObject",RpcTarget.All,nearDroppedWeapon.gameObject.GetInstanceID());
                            playerSound.PlaySound(3,0.4f,1);
                            bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
                        }
                    }
                } else {
                    changeWeaponText.text = "Pressione E para pegar " + nearDroppedWeapon.name.ToString();
                    if(Input.GetKeyDown(KeyCode.E)){
                        if(primaryGun == null){
                            primaryGun = nearDroppedWeapon.ChangeWeapons(primaryGun);
                            actualWeapon = 1;
                            ChangeGuns(primaryGun);
                        }else if(secondaryGun == null){
                            secondaryGun = nearDroppedWeapon.ChangeWeapons(secondaryGun);
                            actualWeapon = 2;
                            ChangeGuns(secondaryGun);
                        } else {
                            if(actualWeapon == 1 || actualWeapon == 3){
                                primaryGun = nearDroppedWeapon.ChangeWeapons(primaryGun);
                                actualWeapon = 1;
                                ChangeGuns(primaryGun);
                            } else {
                                secondaryGun = nearDroppedWeapon.ChangeWeapons(secondaryGun);
                                actualWeapon = 2;
                                ChangeGuns(secondaryGun);
                            }
                        }
                        bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
                        changeWeaponText.text = "";
                    }
                }
            } else {
                changeWeaponText.text = "";
                nearDroppedWeapon = null;
            }
        }

        if(Input.GetKeyDown(KeyCode.Alpha1)){
            if(actualWeapon == 1 || primaryGun == null) return;
            actualWeapon = 1;
            ChangeGuns(primaryGun);
            bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
            return;
        } else if(Input.GetKeyDown(KeyCode.Alpha2)){
            if(actualWeapon == 2 || secondaryGun == null) return;
            actualWeapon = 2;
            ChangeGuns(secondaryGun);
            bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
            return;
        } else if(Input.GetKeyDown(KeyCode.Alpha3)){
            if(actualWeapon == 3) return;
            actualWeapon = 3;
            ChangeGuns(terciaryGun);
            bulletsText.text = "";
            return;
        } 

        this.lifeText.text = health.ToString();

        CheckSpeed();
        CheckHands();

        float targetHeight;
        if(Input.GetKey(KeyCode.LeftControl) && isGrounded)
        {
            isCrounching = true;
            bodyAnimator.SetBool("Crouch", true);
            targetHeight = 1.0f;
        } else {
            isCrounching = false;
            bodyAnimator.SetBool("Crouch", false);
            targetHeight = 1.9f;
        }
        
        if(Input.GetKey(KeyCode.W) && isGrounded)
        {
            handAnimator[gunIndex].SetBool("W_pressed", true);
        } else {
            handAnimator[gunIndex].SetBool("W_pressed", false);
        }

        fpsCam.transform.position = Vector3.Lerp(fpsCam.transform.position, new Vector3(fpsCam.transform.position.x,controller.transform.position.y + targetHeight/2 -0.1f,fpsCam.transform.position.z), 7.5f * Time.deltaTime);

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        walkMagnitude = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude / Mathf.Sqrt(2.0f); 

        handAnimator[gunIndex].SetFloat("Walk_magnitude", walkMagnitude);

        bodyAnimator.SetFloat("Walk_magnitude", walkMagnitude);

        if(isGrounded && velocity.y <0)
        {   
            bodyAnimator.SetBool("Jump", false);
            jumpingAnim = false;
            velocity.y = -2f;
        }

        move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        CheckSensitivy();

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpingAnim = true;
            bodyAnimator.SetBool("Jump", true);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
    }

    [PunRPC]
    void OnAnimationChange(string anim)
    {
        bodyAnimator.Play(anim);
    }

    void CheckSensitivy()
    {
        if(Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Plus))
        {   
            sensibilidadeText.text = "sens: " + mouseLook.mouseSensitivity.ToString("#.##");
            sensibilidadeText.color = new Color(sensibilidadeText.color.r, sensibilidadeText.color.g, sensibilidadeText.color.b, 100f);
        } else if(Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus))
        {   
            sensibilidadeText.text = "sens: " + mouseLook.mouseSensitivity.ToString("#.##");
            sensibilidadeText.color = new Color(sensibilidadeText.color.r, sensibilidadeText.color.g, sensibilidadeText.color.b, 100f);
        }

        if((Input.GetKeyUp(KeyCode.KeypadPlus) || Input.GetKeyUp(KeyCode.Plus)) || (Input.GetKeyUp(KeyCode.KeypadMinus) || Input.GetKeyUp(KeyCode.Minus)))
        {   
            actualRoutine = ExitSensibilidade();
            StopCoroutine(actualRoutine);
            StartCoroutine(actualRoutine);
        }
        
    }   
    
    IEnumerator ExitSensibilidade()
    {   
        yield return new WaitForSeconds(3f);
        sensibilidadeText.color = new Color(sensibilidadeText.color.r, sensibilidadeText.color.g, sensibilidadeText.color.b, 0f);
    } 

    void ChangeGuns(WeaponStats weapon)
    {   
        if(weapon == null)
            return;

        if(actualRoutine != null){

            if(actualRoutine.ToString().Contains("Reload")){
                this.handAnimator[gunIndex].SetInteger("Reload", 0);
                this.StopCoroutine(actualRoutine);
                isReloading = false;
            }
            
        }
        gunIndex = weapon.gunIndex;
        for (int i = 0; i < handWeapons.Count; i++)
        {   
            if(i == gunIndex){
                handWeapons[i].SetActive(true); 
            }else
                handWeapons[i].SetActive(false);
        }
        maxAmmo = weapon.maxAmmo;
        currentAmmo = weapon.currentAmmo;
        totalAmmo = weapon.totalAmmo;
        clipSize = weapon.clipSize;
        damage = weapon.damage;
        fireRate = weapon.fireRate;
        range = weapon.range;
        reloadingTime = weapon.reloadingTime;
    }

    void CheckSpeed()
    {   
        if(isCrounching){
            playerSound.step_Distance = crouch_Step_Distance;
            playerSound.volume_Min = crouch_Volume;
            playerSound.volume_Max = crouch_Volume;
            this.speed = actualWeapon == 3 ? 4f : 3.5f;
        }else if(isAiming){
            playerSound.step_Distance = crouch_Step_Distance;
            playerSound.volume_Min = crouch_Volume;
            playerSound.volume_Max = crouch_Volume;
            this.speed = 4f;
        }else if(CurrentAnimation() == "Run"){
            playerSound.step_Distance = sprint_Step_Distance;
            playerSound.volume_Min = sprint_Volume;
            playerSound.volume_Max = sprint_Volume;
            this.speed = actualWeapon == 3 ? 10f : 8f;
            
        }else{
            playerSound.step_Distance = walk_Step_Distance;
            playerSound.volume_Min = walk_Volume_Min;
            playerSound.volume_Max = walk_Volume_Max;
            this.speed = actualWeapon == 3 ? 6f : 5f;
        }
    }

    void CheckHands()
    {   
        //if(CurrentAnimation() == "Idle" || CurrentAnimation() == "Move" || CurrentAnimation() == "Fire" || CurrentAnimation() == "Run")
            //aimPoint.alpha = 1f;
        //else
            //aimPoint.alpha = 0f;

        if(Input.GetKeyDown(KeyCode.R)){
            if(noGuns || CurrentAnimation() == "Reload" || actualWeapon == 3)
                return;
            if(totalAmmo > 0){
                if(clipSize != currentAmmo && isReloading == false){
                    isReloading = true;
                    actualRoutine = Reload();
                    StartCoroutine(actualRoutine);
                }
            }
        } 
        else {

            if(isReloading || CurrentAnimation() == "Reload")
                return;

            if(Input.GetButtonDown("Fire2")){
                if(actualWeapon == 3)
                    return;
                isAiming = !isAiming;
                handAnimator[gunIndex].SetBool("Sight", isAiming);
            }
            
        
            if(Input.GetButton("Fire1"))
            {   
                if(noGuns || actualWeapon == 3)
                    return;
                if(currentAmmo > 0){
                    if(CurrentAnimation() == "Run")
                        return;
                    if(Time.time >= nextTimeToFire){
                        nextTimeToFire = Time.time + fireRate;
                        handAnimator[gunIndex].SetInteger("Fire", 1);
                        Shoot();
                        return;
                    }
                }else{
                    handAnimator[gunIndex].SetInteger("Fire", 0);
                    if(totalAmmo > 0){
                        if(clipSize != currentAmmo){
                            isReloading = true;
                            actualRoutine = Reload();
                            StartCoroutine(actualRoutine);
                        }
                    }
                }  

            }else{
                handAnimator[gunIndex].SetInteger("Fire", 0);
                
                if(isAiming || isCrounching || CurrentAnimation() == "ZoomFire" || CurrentAnimation() == "Fire"){
                    return;
                }
                if(Input.GetButton("Fire3") && isGrounded){
                    handAnimator[gunIndex].SetBool("Run", true);
                    bodyAnimator.SetBool("Run", true);
                } else {
                    handAnimator[gunIndex].SetBool("Run", false);
                    bodyAnimator.SetBool("Run", false);
                }
            }
        }
        
        /*if(Input.GetKeyDown(KeyCode.F)){
            if(noGuns || isAiming)
                return;
            MeleeAttack();
            StartCoroutine(Melee());
        }else */

        
    }

    string CurrentAnimation()
    {   
        AnimatorClipInfo[] m_CurrentClipInfo = handAnimator[gunIndex].GetCurrentAnimatorClipInfo(0);
        if(m_CurrentClipInfo.Length >0)
            return m_CurrentClipInfo[0].clip.name;
        else
            return ""; 
    }
    /*
    void MeleeAttack()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, 2f))
        {   
            int amount = 0;
            if(hit.transform.tag == "PlayerHead")
                amount = 100;
            else if(hit.transform.tag == "PlayerTorso")
                amount = 100;
            else if(hit.transform.tag == "PlayerLegs")
                amount = 100;
            else if(hit.transform.tag == "PlayerFeet")
                amount = 100;
            if(amount != 0 ){
                if(hit.transform.gameObject){
                    PlayerMovement target = hit.transform.gameObject.GetComponentInParent<PlayerMovement>();
                    if(target.health > 0)
                        hitMarker.BodyHit();
                    object[] instanceData = new object[3];
                    instanceData[0] = this.PV.InstantiationId;
                    instanceData[1] = target.PV.InstantiationId;
                    instanceData[2] = amount;
                    
                    PV.RPC("TakeDamage",RpcTarget.All,instanceData);
                }
            }
        }
    }
    */

    IEnumerator Reload()
    {   
        isAiming = false;
        handAnimator[gunIndex].SetBool("Sight", false);
        /*if(currentAmmo <= 0)
            handAnimator[gunIndex].SetInteger("Reload", 0);
        else
            handAnimator[gunIndex].SetInteger("Reload", 1);*/
        handAnimator[gunIndex].SetInteger("Reload", 1);
            
        object[] instanceData = new object[3];
        instanceData[0] = this.PV.InstantiationId;
        instanceData[1] = 6;

        PhotonNetwork.Instantiate("Sounds",this.transform.position, Quaternion.identity,0,instanceData);

        yield return new WaitForSeconds(reloadingTime);
        handAnimator[gunIndex].SetInteger("Reload", 0);
       // handAnimator[gunIndex].SetBool("Sight", false);
        int clip = totalAmmo;
        totalAmmo = totalAmmo - clipSize + currentAmmo;

        if(totalAmmo <= 0){
            currentAmmo = clip;
            totalAmmo = 0;
        }else
            currentAmmo = clipSize;

        if(actualWeapon == 1 && primaryGun != null){
            primaryGun.currentAmmo = currentAmmo;
            primaryGun.totalAmmo = totalAmmo;
        } else if(actualWeapon == 2 && secondaryGun != null){
            secondaryGun.currentAmmo = currentAmmo;
            secondaryGun.totalAmmo = totalAmmo;
        }
            
        bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
        isReloading = false;
    }

    IEnumerator Melee()
    {   
        
        //handAnimator[gunIndex].SetBool("Melee", true);

        object[] instanceData = new object[3];
        instanceData[0] = this.PV.InstantiationId;
        instanceData[1] = 2;
        PhotonNetwork.Instantiate("Sounds",this.transform.position, Quaternion.identity,0,instanceData);
        yield return new WaitForSeconds(meleeTime);

        //handAnimator[gunIndex].SetBool("Melee", false);

        yield return new WaitForSeconds(0.25f);

        
    }

    void CreateDamageIndicator(int id, Transform position)
    {      

        if(this.PV.InstantiationId != id)
            return;

        if(!PV.IsMine)
			return;

        //damageIndicator.CreateIndicator(position);
    }

    void Shoot()
    {  
        shootingAnim = true;
        if(!isAiming && CurrentAnimation() == "Fire")
            muzzleFlash.Play();

        currentAmmo--;
        if(actualWeapon == 1){
            if(primaryGun != null)
                primaryGun.currentAmmo = currentAmmo;
        }else{
            if(secondaryGun != null)
                secondaryGun.currentAmmo = currentAmmo;
        }

        object[] instanceData = new object[3];
        instanceData[0] = this.PV.InstantiationId;
        instanceData[1] = gunIndex;

        PhotonNetwork.Instantiate("Sounds",this.transform.position, Quaternion.identity,0,instanceData);
        RaycastHit hit;


        Vector3 targetPosition;
        //Debug.Log(CurrentAnimation());
        if(CurrentAnimation() == "ZoomIdle" || CurrentAnimation() == "ZoomFire")
            targetPosition = fpsCam.transform.forward;
        else{
            float magnitude;
            magnitude = walkMagnitude > 0.1 ? 0.25f : 0.1f;
            targetPosition = new Vector3(fpsCam.transform.forward.x + Random.Range(-magnitude, magnitude),fpsCam.transform.forward.y + Random.Range(-magnitude, magnitude), fpsCam.transform.forward.z + Random.Range(-magnitude, magnitude));
        }
        if (Physics.Raycast(fpsCam.transform.position, targetPosition, out hit, range))
        {   
            int amount = 0;

            if(hit.transform.tag == "PlayerHead")
                amount = (int)((damage - (hit.distance)/10) * 1.5);
            else if(hit.transform.tag == "PlayerTorso")
                amount = (int)((damage - (hit.distance)/10) * 1.25);
            else if(hit.transform.tag == "PlayerLegs" || hit.transform.tag == "PlayerFeet")
                amount = (int)((damage - (hit.distance)/10));
            else if(hit.transform.tag == "Enemy")
                hitMarker.BodyHit();

            Debug.Log(amount);

            if(amount != 0 ){
                if(hit.transform.gameObject){
                    PlayerMovement target = hit.transform.gameObject.GetComponentInParent<PlayerMovement>();
                    if(target.health > 0){
                        if(amount == 50)
                            hitMarker.HeadshotHit();
                        else
                            hitMarker.BodyHit();
                    }
                    
                    instanceData[1] = target.PV.InstantiationId;
                    instanceData[2] = amount;
                    PV.RPC("TakeDamage",RpcTarget.All,instanceData);
                }
            } else {
                PhotonNetwork.Instantiate("HitParticles",hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        shootingAnim = false;

        bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
    }

    IEnumerator Respawn(PlayerMovement target) 
    {   
        target.killStreak = 0;
        target.jumpingAnim = false;
        target.runningAnim = false;
        target.idleAnim = true;
        target.velocity.y = -2f;
        ChangeGuns(target.terciaryGun);
        target.actualWeapon = 1;
        target.enabled = false;
        target.health = 100;
        yield return new WaitForSeconds(0.3f);
        object[] instanceData = new object[1];
        if(target.primaryGun != null)
            instanceData[0] = target.primaryGun.gunIndex;
        else 
            instanceData[0] = (int)Random.Range(0, 6);
        PhotonNetwork.Instantiate("DroppedGun",target.transform.position, Quaternion.identity,0,instanceData);
        float x = Random.Range((target.heaven.transform.position.x - 5f), (target.heaven.transform.position.x + 5f));
        float z = Random.Range((target.heaven.transform.position.z - 5f), target.heaven.transform.position.z + 5f);
        target.transform.position = new Vector3(x,target.heaven.transform.position.y + 4f,z);
        yield return new WaitForSeconds(0.3f);
        target.primaryGun = null;
        target.secondaryGun = null;
        target.health = 100;
        target.enabled = true;
        target.waitingForSpawn = false;
        
    }

    void OnTriggerStay(Collider other) 
    {
        if(!PV.IsMine)
			return;
        
        if(this.health <=0)
            return;
        
        Debug.Log(other.name);
        if(other.tag == "DroppedWeapon")
        {   
            nearDroppedWeapon = other.gameObject.GetComponentInParent<ChangeDroppedGun>();
        }
    }

    void OnTriggerEnter(Collider other) 
    {   
        if(!PV.IsMine)
			return;
        
        if(this.health <=0)
            return;

        if(other.tag == "Respawn")
        {  
            object[] instanceData = new object[3];
            instanceData[0] = this.PV.InstantiationId;
            instanceData[1] = this.PV.InstantiationId;
            instanceData[2] = 100;
            PV.RPC("TakeDamage",RpcTarget.All,instanceData);
        } else if(other.tag == "NoGuns")
        {
            noGuns = true;
        }
    }

    [PunRPC]
    public void DestroyObject(int instanceID) //, PlayerMovement playerWhoShooted
    {   
        GameObject [] objects = GameObject.FindGameObjectsWithTag("DropGameObject");
        foreach (GameObject go in objects)
        {
            if(go.GetInstanceID() == instanceID){
                Destroy(go);
                break;
            }
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
            if(player.PV.InstantiationId == (int) instantiationData[0])
                whoCausedDamage = player;
            if(player.PV.InstantiationId == (int) instantiationData[1])
                whoReceivedDamage = player;
                
        }
        if(whoReceivedDamage)
        {   
            //Debug.Log(whoReceivedDamage.PV.InstantiationId.ToString() + " " + PV.InstantiationId.ToString());
            if(whoReceivedDamage.PV.InstantiationId != whoCausedDamage.PV.InstantiationId)
                whoReceivedDamage.CreateDamageIndicator(whoReceivedDamage.PV.InstantiationId, whoCausedDamage.transform);
            //whoReceivedDamage.StopCoroutine(RestoreLife());
            whoReceivedDamage.health = whoReceivedDamage.health - (int)instantiationData[2];

            if(whoReceivedDamage.health <= 0){
            
                whoReceivedDamage.health = 0;
                
                //GameManager.UpdatePlayer(whoReceivedDamage);
                //GameManager.UpdatePlayer(whoCausedDamage);
                //PV.RPC("updatePlayer",RpcTarget.AllBuffered);

                //GameManager.updateRequest.Add(true);

                //PhotonNetwork.Instantiate("AkDroped",whoReceivedDamage.transform.position, Quaternion.identity);

                Kill(whoReceivedDamage,whoCausedDamage);
            }   
        }
        
    }

    void OnTriggerExit(Collider other)
    {
        if(!PV.IsMine)
			return;
        
        if(other.tag == "DroppedWeapon")
        {   
            changeWeaponText.text = "";
        }

        if(other.tag == "NoGuns")
        {
            noGuns = false;
        } 
    }
    
    void Kill(PlayerMovement target, PlayerMovement enemy)
    {   
        if(target != null && target.waitingForSpawn == false)
        {   
            target.deathCounter++;
            

            if(enemy != null && (enemy.PV.InstantiationId != target.PV.InstantiationId)){
                enemy.killCounter ++;
                enemy.killStreak ++;
                CheckKillStreak(enemy);
            } else {
                object[] instanceData = new object[2];
                instanceData[0] = 0;
                instanceData[1] = true;
                PV.RPC("playGeneralSound",RpcTarget.All,instanceData);
            }
            /*object[] instanceData = new object[2];
            instanceData[0] = target.primaryGun.gunIndex;
            PhotonNetwork.Instantiate("DroppedGun",target.transform.position, Quaternion.identity,0,instanceData);
*/

            target.waitingForSpawn = true;
            //updateRanking.UpdatePlayers();
            StartCoroutine(Respawn(target));
            //updateRanking.UpdatePlayers();
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
            object[] instanceData = new object[2];
            instanceData[0] = 2;
            instanceData[1] = false;
            PV.RPC("playGeneralSound",RpcTarget.All,instanceData);
        } else if(player.killStreak == 5){
            if(this.PV.InstantiationId == player.PV.InstantiationId)
                StartCoroutine(exibeKillStreak(player,"5"));
            object[] instanceData = new object[2];
            instanceData[0] = 1;
            instanceData[1] = false;
            PV.RPC("playGeneralSound",RpcTarget.All,instanceData);
        }else{
            /*
            object[] instanceData = new object[2];
            instanceData[0] = 2;
            instanceData[1] = true;
            PV.RPC("playGeneralSound",RpcTarget.All,instanceData);
            */
        }
        /*
        else if(player.killStreak == 5){
            StartCoroutine(exibeKillStreak(player,"5"));
        PV.RPC("playGireiSound",RpcTarget.All);
        }*/
        
    }

    [PunRPC]
    public void updatePlayer()
    {   
        GameManager.UpdatePlayer(this);
    }
    
    [PunRPC]
    public void playGeneralSound(object[] instantiationData)
    {   
        if((bool)instantiationData[1]){
            for (int i = 0; i < generalAudios.Count; i++)
            {
                if(generalAudios[i].isPlaying)
                    return;
            }
        }
        if(!generalAudios[(int)instantiationData[0]].isPlaying)
            generalAudios[(int)instantiationData[0]].Play(0);
    }

    public void Reset()
    {   
        waitingForSpawn = true;
        totalAmmo = 280;
        clipSize = 30;
        currentAmmo = clipSize;
        health = 100;
        speed = 5.5f;
        killStreak = 0;
        killCounter = 0;
        deathCounter = 0;
        isAiming = false;
        isReloading = false;
        isCrounching = false;
        jumpingAnim = false;
        runningAnim = false;
        sprintingAnim = false;
        idleAnim = true;
        shootingAnim = false;
        if(PV.IsMine){
            //aimPoint.alpha = 1f;
            canvas.gameObject.SetActive(true);
        }
        
        StartCoroutine(Respawn(this));
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code == GameManager.restartGameEventCode)
        {
            this.Reset();
            GameManager.updateRequest.Add(true); 
        }
    }

}