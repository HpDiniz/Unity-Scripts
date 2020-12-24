using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
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
    Transform nearTransformDroppedWeapon;

    public int health = 100;

    [HideInInspector] public ParticleSystem muzzleFlash;
    [HideInInspector] public GameObject impactEffect;
    [HideInInspector] public int lastDamageUser = -1;

    private float nextTimeToFire = 0f;

    [HideInInspector] public bool noGuns = false;
    byte actualWeapon;
    [HideInInspector] public int gunIndex = 0;
    
    [HideInInspector] public Animator bodyAnimator;
    [HideInInspector] public WeaponStats primaryGun;
    [HideInInspector] public WeaponStats secondaryGun;
    [HideInInspector] public WeaponStats terciaryGun;
    [HideInInspector] public List<Animator> handAnimator = new List<Animator>();
    [HideInInspector] public List<GameObject> handWeapons = new List<GameObject>();
    GhostPosition[] ghostPosition;

    public Transform groundCheck;
    public LayerMask groundMask;
    
    float speed = 5.5f;
    public float gravity = -19.62f;
    public float jumpHeight = 1.3f;
    public float groundDistance = 0.05f;

    [HideInInspector] public int killStreak = 0;
    [HideInInspector] public int killCounter = 0;
    [HideInInspector] public int deathCounter = 0;

    IEnumerator actualRoutine; // create an IEnumerator object
    Vector3 velocity;

    bool isAiming = false;
    bool isReloading = false;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool waitingForSpawn;

    [HideInInspector] public bool isCrounching = false;
    [HideInInspector] public bool jumpingAnim = false;
    [HideInInspector] public bool runningAnim = false;
    [HideInInspector] public bool sprintingAnim = false;
    [HideInInspector] public bool idleAnim = true;
    [HideInInspector] public bool shootingAnim = false;
    
    Vector3 move;
    Transform heaven;
    HitMarker hitMarker;
    DI_System damageIndicator;
    HeadPosition headPosition;

    [HideInInspector] public Camera fpsCam;
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public MouseLook mouseLook;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public CanvasGroup aimPoint;

    [HideInInspector] public PhotonView PV;
    [HideInInspector] public Text bulletsText;
    [HideInInspector] public Text lifeText;
    [HideInInspector] public Text sensibilidadeText;
    [HideInInspector] public Text changeWeaponText;
    [HideInInspector] public TMP_Text streakText;

    [HideInInspector] public string Nickname;


    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {   
        object[] instantiationData = info.photonView.InstantiationData;

        Nickname = (string) instantiationData[0];
    }

	void Awake()
	{   
        headPosition = GetComponentInChildren<HeadPosition>();
        GameManager.players.Add(this);
        ghostPosition = GetComponentsInChildren<GhostPosition>();
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

        //GameManager.updateRequest.Add(true);

        //PV.RPC("updatePlayer",RpcTarget.AllBuffered);
        //updateRanking = GameObject.Find("GeneralCanvas").GetComponentInChildren<UpdateRanking>();
        //updateRanking.UpdatePlayers(); 

        if(PV.IsMine)
		{   
            Animator [] Animators = GetComponentsInChildren<Animator>();
            
            foreach (Animator item in Animators)
            {
                if(item.gameObject.layer == 3){
                    handAnimator.Add(item);
                    handWeapons.Add(item.gameObject);

                    WeaponStats weasponStats = item.gameObject.GetComponent<WeaponStats>();
                    if(weasponStats.gunIndex == 0)
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

            /*
            for (int i = 0; i < ghostPosition.Length; i++)
            {
                ghostPosition[i].Invisible();
            }
            */
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

        ChangeGhostGun();
    }

    // Update is called once per frame
    void Update()
    {   
        if(!PV.IsMine)
			return;
        if(waitingForSpawn)
            return;
        
        Debug.Log("LAST DAMAGE USER: " + lastDamageUser);
        Debug.Log("KILL STREAK:" + this.killStreak);
        Debug.Log("MORTES: " + this.deathCounter);

        if(this.health < 1){
            this.health = 0;
            bulletsText.text = "";
            this.waitingForSpawn = true;

            object[] additionalData = new object[2];
            additionalData[0] = this.PV.InstantiationId;
            additionalData[1] = lastDamageUser;
            
            PV.RPC("UpdateKills",RpcTarget.All,additionalData);

            additionalData[0] = 0;
            additionalData[1] = true;
            PV.RPC("playGeneralSound",RpcTarget.All,additionalData);
            StartCoroutine(Respawn());
            return;
        }

        if(nearDroppedWeapon != null){
            
            float distance = Vector3.Distance(this.transform.position, nearTransformDroppedWeapon.position);
            if(distance < 1.4){
                if((primaryGun != null && nearDroppedWeapon.currentGunIndex == primaryGun.gunIndex)||(secondaryGun != null && nearDroppedWeapon.currentGunIndex == secondaryGun.gunIndex)){
                    if(nearDroppedWeapon.currentGunIndex == primaryGun.gunIndex){
                        if(primaryGun.totalAmmo < primaryGun.maxAmmo){
                            primaryGun.totalAmmo = primaryGun.totalAmmo + primaryGun.maxAmmo/2 > primaryGun.maxAmmo ? primaryGun.maxAmmo : primaryGun.totalAmmo + primaryGun.maxAmmo/2;
                            if(gunIndex == primaryGun.gunIndex)
                                UpdateAmmo(primaryGun);
                            PV.RPC("DestroyObject",RpcTarget.All,nearDroppedWeapon.gameObject.GetInstanceID());
                            playerSound.PlaySound(3,0.4f,1);
                            bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
                        }
                    } else {
                        if(secondaryGun.totalAmmo < secondaryGun.maxAmmo){
                            secondaryGun.totalAmmo = secondaryGun.totalAmmo + secondaryGun.maxAmmo/2 > secondaryGun.maxAmmo ? secondaryGun.maxAmmo : secondaryGun.totalAmmo + secondaryGun.maxAmmo/2;
                            if(gunIndex == secondaryGun.gunIndex)
                                UpdateAmmo(secondaryGun);
                            PV.RPC("DestroyObject",RpcTarget.All,nearDroppedWeapon.gameObject.GetInstanceID());
                            playerSound.PlaySound(3,0.4f,1);
                            bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
                        }
                    }
                } else {
                    changeWeaponText.text = "Pressione E para pegar " + nearDroppedWeapon.name.ToString();
                    if(Input.GetKeyDown(KeyCode.E) && CurrentAnimation() != "Select"){
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

    void ChangeGhostGun()
    {   

        for (int i = 0; i < ghostPosition.Length; i++)
        {   
            if(ghostPosition[i].gunIndex == gunIndex && !PV.IsMine){
                ghostPosition[i].Visible();
            }else
                ghostPosition[i].Invisible();
        }/*
        if(PV.IsMine){
            for (int i = 0; i < ghostPosition.Length; i++)
            {
                ghostPosition[i].Invisible();
            }
        } else {
            for (int i = 0; i < ghostPosition.Length; i++)
            {
                if(ghostPosition[i].gunIndex == gunIndex)
                    ghostPosition[i].Visible();
                else
                    ghostPosition[i].Invisible();
            }
        }*/
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

        //

        object[] instanceData = new object[3];
        instanceData[0] = this.PV.InstantiationId;
        instanceData[1] = weapon.gunIndex;
        
        PV.RPC("UpdateGunIndex",RpcTarget.Others,instanceData);
        gunIndex = weapon.gunIndex;
        for (int i = 0; i < handWeapons.Count; i++)
        {   
            if(i == gunIndex){
                handWeapons[i].SetActive(true); 
            }else
                handWeapons[i].SetActive(false);
        }
        UpdateAmmo(weapon);
        damage = weapon.damage;
        fireRate = weapon.fireRate;
        range = weapon.range;
        reloadingTime = weapon.reloadingTime;
        ChangeGhostGun();
    }

    void UpdateAmmo(WeaponStats weapon)
    {
        maxAmmo = weapon.maxAmmo;
        currentAmmo = weapon.currentAmmo;
        totalAmmo = weapon.totalAmmo;
        clipSize = weapon.clipSize;
    }

    void CheckSpeed()
    {   
        if(!isGrounded)
            return;

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
            if(noGuns || CurrentAnimation() == "Reload" || CurrentAnimation() == "Select" || actualWeapon == 3)
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
        
    }

    string CurrentAnimation()
    {   
        if(handAnimator != null && handAnimator.Count > 0){
            AnimatorClipInfo[] m_CurrentClipInfo = handAnimator[gunIndex].GetCurrentAnimatorClipInfo(0);
            if(m_CurrentClipInfo != null && m_CurrentClipInfo.Length >0)
                return m_CurrentClipInfo[0].clip.name;
        }
        
        return "Nothing"; 
    }

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
        instanceData[1] = gunIndex+6;

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

            if(amount != 0 ){
                if(hit.transform.gameObject){
                    PlayerMovement target = hit.transform.gameObject.GetComponentInParent<PlayerMovement>();
                    if(target.health > 0){
                        if(hit.transform.tag == "PlayerHead")
                            hitMarker.HeadshotHit();
                        else
                            hitMarker.BodyHit();
                    }
                    
                    instanceData[1] = target.PV.InstantiationId;
                    instanceData[2] = amount;
                    PV.RPC("TakeDamage",RpcTarget.All,instanceData);
                    CheckKillStreak();
                }
            } else {
                PhotonNetwork.Instantiate("HitParticles",hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        shootingAnim = false;

        bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
    }

    IEnumerator Respawn() 
    {   
        yield return new WaitForSeconds(0.1f);
        this.killStreak = 0;
        this.lastDamageUser = -1;
        this.jumpingAnim = false;
        this.runningAnim = false;
        this.isAiming = false;
        this.isCrounching = false;
        this.isReloading = false;
        this.idleAnim = true;
        this.velocity.y = -2f;
        ChangeGuns(this.terciaryGun);
        this.actualWeapon = 1;
        this.enabled = false;
        this.health = 100;
        yield return new WaitForSeconds(0.1f);
        object[] instanceData = new object[1];
        instanceData[0] = gunIndex;
        PhotonNetwork.Instantiate("DroppedGun",this.transform.position, Quaternion.identity,0,instanceData);
        float x = Random.Range((this.heaven.transform.position.x - 5f), (this.heaven.transform.position.x + 5f));
        float z = Random.Range((this.heaven.transform.position.z - 5f), this.heaven.transform.position.z + 5f);
        this.transform.position = new Vector3(x,this.heaven.transform.position.y + 4f,z);
        yield return new WaitForSeconds(0.4f);
        this.primaryGun = null;
        this.secondaryGun = null;
        this.health = 100;
        this.enabled = true;
        this.waitingForSpawn = false;
        
    }

    void OnTriggerStay(Collider other) 
    {
        if(!PV.IsMine)
			return;
        
        if(this.health <=0)
            return;

        if(other.tag == "DroppedWeapon")
        {   
            nearDroppedWeapon = other.gameObject.GetComponentInParent<ChangeDroppedGun>();
            nearTransformDroppedWeapon = other.gameObject.transform;

        } else if(other.tag == "DropGameObject")
        {
            nearDroppedWeapon = other.gameObject.GetComponent<ChangeDroppedGun>();
            nearTransformDroppedWeapon = other.gameObject.transform;
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
                Debug.Log("DAMAGE: " + whoReceivedDamage.PV.InstantiationId + " - " + whoCausedDamage.PV.InstantiationId);
                whoReceivedDamage.lastDamageUser = whoCausedDamage.PV.InstantiationId;

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

    [PunRPC]
    public void UpdateKills(object[] instantiationData)
    {   
        PlayerMovement[] players = GetComponents<PlayerMovement>();

        for (int i = 0; i < players.Length; i++)
        {
            if(players[i].PV.InstantiationId == (int)instantiationData[0]){
                players[i].deathCounter++;
            } 
            if((int)instantiationData[0] == (int)instantiationData[1])
                break;
            if(players[i].PV.InstantiationId == (int)instantiationData[1]){
                Debug.Log(players[i].name);
                players[i].killCounter ++;
                players[i].killStreak ++;
            }
        }
    }

    IEnumerator exibeKillStreak(string kills) 
    {   
        this.streakText.text =  kills + " Kill Streak";
        this.streakText.color = new Color(this.streakText.color.r, this.streakText.color.g, this.streakText.color.b, 100f);
        yield return new WaitForSeconds(4f);
        this.streakText.color = new Color(this.streakText.color.r, this.streakText.color.g, this.streakText.color.b, 0f);
        
    }

    void CheckKillStreak()
    {   
        if(this.killStreak == 10){
            
            StartCoroutine(exibeKillStreak("10"));
            object[] instanceData = new object[2];
            instanceData[0] = 2;
            instanceData[1] = false;
            PV.RPC("playGeneralSound",RpcTarget.All,instanceData);
        } else if(this.killStreak == 5){

            StartCoroutine(exibeKillStreak("5"));
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
        else if(this.killStreak == 5){
            StartCoroutine(exibeKillStreak(this,"5"));
        PV.RPC("playGireiSound",RpcTarget.All);
        }*/
        
    }

    [PunRPC]
    public void UpdateGunIndex(object[] instantiationData)
    {   
        PlayerMovement[] players = GetComponents<PlayerMovement>();

        for (int i = 0; i < players.Length; i++)
        {
            if(players[i].PV.InstantiationId == (int)instantiationData[0]){
                players[i].gunIndex = (int)instantiationData[1];
                for (int j = 0; j < players[i].ghostPosition.Length; j++)
                {   
                    if(players[i].ghostPosition[j].gunIndex == gunIndex){
                        players[i].ghostPosition[j].Visible();
                    }else
                        players[i].ghostPosition[j].Invisible();
                }
                break;
            }
        }
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

}