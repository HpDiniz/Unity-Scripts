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
    private float nextTimeToFire = 0f;
    private float nextTimeToScream = 0f;
    private float nextTimeToChangeGuns = 0f;
    private float lastTimeIShooted = 0f;

    private PlayerSounds playerSound;

    private float walk_Step_Distance = 0.42f;
    private float sprint_Step_Distance = 0.38f;
    private float crouch_Step_Distance = 0.54f;

    string winner = "";
    private float vRecoil = 0.4f;
    private float hRecoil = 0.4f;

    [HideInInspector] public bool resetGame = false;
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

    IEnumerator sensibilityRoutine;
    IEnumerator messageRoutine;
    IEnumerator actualRoutine;
    IEnumerator resetRoutine;
    Vector3 velocity;

    bool isAiming = false;
    bool isReloading = false;
    bool isChangingGuns = false;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool waitingForSpawn;

    [HideInInspector] public bool isCrounching = false;
    [HideInInspector] public bool jumpingAnim = false;
    [HideInInspector] public bool runningAnim = false;
    [HideInInspector] public bool sprintingAnim = false;
    [HideInInspector] public bool idleAnim = true;
    [HideInInspector] public bool shootingAnim = false;

    GameObject terrain;
    Terrain worldTerrain;
    
    Vector3 move;
    Transform heaven;
    HitMarker hitMarker;
    GameObject sniperScope;
    GameObject weaponCamera;
    DI_System damageIndicator;
    HeadPosition headPosition;
    PlayerMovement playerWhoKilledMe;

    [HideInInspector] public Camera miniMapCam;
    [HideInInspector] public Camera fpsCam;
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public RectTransform playerIcon;
    [HideInInspector] public MouseLook mouseLook;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public CanvasGroup aimPoint;

    [HideInInspector] public PhotonView PV;
    [HideInInspector] public Text bulletsText;
    [HideInInspector] public Text lifeText;
    [HideInInspector] public Text sensibilidadeText;
    [HideInInspector] public Text changeWeaponText;
    [HideInInspector] public TMP_Text messageText;
    [HideInInspector] public TMP_Text rankingText;

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
		controller = GetComponent<CharacterController>();
		PV = GetComponent<PhotonView>();
        
        playerSound = GetComponent<PlayerSounds>();

        Canvas [] canvasGroup = GetComponentsInChildren<Canvas>();
        foreach (Canvas item in canvasGroup)
        {
            if(item.gameObject.name == "UI"){
                canvas = item;
            } else if(item.gameObject.name == "OnlyMap"){
                playerIcon = item.GetComponentInChildren<RectTransform>();
            }
        }

        Camera [] Cameras = GetComponentsInChildren<Camera>();
        foreach (Camera item in Cameras)
        {
            if(item.gameObject.name == "Main Camera"){
                fpsCam = item;
            } else if(item.gameObject.name == "ScopeCamera"){
                weaponCamera = item.gameObject;
            } else if(item.gameObject.name == "Minimap"){
                miniMapCam = item;
            }
        }
	}


    void Start()
    {   
        walkMagnitude = 0f;
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
        
        auxAudio = GameObject.Find("nukeSound").GetComponent<AudioSource>();
        if(auxAudio != null)
            generalAudios.Add(auxAudio);

        heaven = GameObject.Find("Heaven").GetComponent<Transform>();
        terrain = GameObject.Find("Terrain");
		worldTerrain = terrain.GetComponent<Terrain>();

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
                }
            }

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
                    else if(canvasItem[i].name == "Ranking")
                        changeWeaponText = canvasItem[i];
                }

                TMP_Text [] canvasTMP = canvas.GetComponentsInChildren<TMP_Text>();

                for(int i = 0; i< canvasTMP.Length; i++){
                    if(canvasTMP[i].name == "Message")
                        messageText = canvasTMP[i];
                    else if(canvasTMP[i].name == "Ranking")
                        rankingText = canvasTMP[i];
                }

                Image [] canvasImages = canvas.GetComponentsInChildren<Image>();

                for(int i = 0; i< canvasImages.Length; i++){
                    if(canvasImages[i].name == "SniperScope"){
                        sniperScope = canvasImages[i].gameObject;
                        sniperScope.SetActive(false);
                    }
                }

                //aimPoint = canvas.GetComponentInChildren<CanvasGroup>();
                hitMarker = canvas.GetComponentInChildren<HitMarker>();
                damageIndicator = canvas.GetComponent<DI_System>();
                //aimPoint.alpha = 1f;
            }

            ChangeRoutine(ChangeGuns(terciaryGun));

            actualWeapon = 3;

            sensibilidadeText.text = "sens: " + mouseLook.mouseSensitivity.ToString();
            sensibilidadeText.color = new Color(sensibilidadeText.color.r, sensibilidadeText.color.g, sensibilidadeText.color.b, 0f);
            messageText.text =  "0 Kill Streak";
            messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 0f);
            waitingForSpawn = false;
            isAiming = false;
            currentAmmo = clipSize;
            if(actualWeapon == 3)
                bulletsText.text = "";
            else
                bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();

            rankingText.text = this.Nickname + " " + this.killCounter.ToString() + "/" + this.deathCounter.ToString();
            lifeText.text = health.ToString();
            headPosition.Invisible();
            
            playerIcon.localScale = new Vector3(1f,1f,1f);
            Debug.Log(playerIcon.position);
        }
        else{
            Destroy(miniMapCam);
			Destroy(fpsCam.gameObject);
            Destroy(canvas.gameObject);
            Destroy(mouseLook);
            Destroy(fpsCam);
			Destroy(controller);
            
            playerIcon.localScale = new Vector3(1f,1f,1f);
		}

        /*
		{
			
            //Destroy(weaponCamera);
            Destroy(mouseLook);
            Destroy(fpsCam);
			Destroy(controller);
            Destroy(canvas);
            
        }
        */

        resetGame = false;
        ChangeGhostGun();
        PV.RPC("CallMethodForAllPlayers",RpcTarget.All,0,"");
    }

    // Update is called once per frame
    void Update()
    {   
        if(!PV.IsMine)
			return;

        if(resetGame){
            if(resetRoutine == null){
                resetRoutine = ResetGame(winner);
                StartCoroutine(resetRoutine);
            }
            
            return;
        }

        if(waitingForSpawn)
            return;
        
        if(killCounter >= 20){
            waitingForSpawn = true;
            PV.RPC("CallMethodForAllPlayers",RpcTarget.All,1,this.Nickname);
        }

        if(this.health < 1){
            this.health = 0;
            bulletsText.text = "";
            this.waitingForSpawn = true;

            object[] additionalData = new object[2];
            /*
            additionalData[0] = this.PV.InstantiationId;
            PV.RPC("UpdateDeaths",RpcTarget.AllBufferedViaServer,additionalData);
            PV.RPC("CheckWinner",RpcTarget.AllBufferedViaServer);
            */
            additionalData[0] = 0;
            additionalData[1] = true;
            PV.RPC("playGeneralSound",RpcTarget.All,additionalData);
            StartCoroutine(Respawn());
            return;
        }

        if(gunIndex == 0)
            bulletsText.text = "";

        if(nearDroppedWeapon != null){
            
            float distance = Vector3.Distance(this.transform.position, nearTransformDroppedWeapon.position);
            if(distance < 3){
                if((primaryGun != null && nearDroppedWeapon.currentGunIndex == primaryGun.gunIndex)||(secondaryGun != null && nearDroppedWeapon.currentGunIndex == secondaryGun.gunIndex)){
                    if(nearDroppedWeapon.currentGunIndex == primaryGun.gunIndex){
                        if(primaryGun.totalAmmo < primaryGun.maxAmmo){
                            primaryGun.totalAmmo = primaryGun.totalAmmo + primaryGun.maxAmmo/2 > primaryGun.maxAmmo ? primaryGun.maxAmmo : primaryGun.totalAmmo + primaryGun.maxAmmo/2;
                            if(gunIndex == primaryGun.gunIndex)
                                UpdateAmmo(primaryGun);
                            PV.RPC("DestroyObject",RpcTarget.All,nearDroppedWeapon.PV.InstantiationId);
                            playerSound.PlayOfflineSound(0,0.4f,0);
                            bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
                        }
                    } else {
                        if(secondaryGun.totalAmmo < secondaryGun.maxAmmo){
                            secondaryGun.totalAmmo = secondaryGun.totalAmmo + secondaryGun.maxAmmo/2 > secondaryGun.maxAmmo ? secondaryGun.maxAmmo : secondaryGun.totalAmmo + secondaryGun.maxAmmo/2;
                            if(gunIndex == secondaryGun.gunIndex)
                                UpdateAmmo(secondaryGun);
                            PV.RPC("DestroyObject",RpcTarget.All,nearDroppedWeapon.PV.InstantiationId);
                            playerSound.PlayOfflineSound(0,0.4f,0);
                            bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
                        }
                    }
                } else {
                    changeWeaponText.text = "Pressione E para pegar " + nearDroppedWeapon.name.ToString();
                    if(Input.GetKeyDown(KeyCode.E) && CurrentAnimation() != "Select"){
                        if(primaryGun == null){
                            primaryGun = nearDroppedWeapon.ChangeWeapons(0);
                            PV.RPC("DestroyObject",RpcTarget.All,nearDroppedWeapon.PV.InstantiationId);
                            actualWeapon = 1;
                            ChangeRoutine(ChangeGuns(primaryGun));
                        }else if(secondaryGun == null){
                            secondaryGun = nearDroppedWeapon.ChangeWeapons(0);
                            PV.RPC("DestroyObject",RpcTarget.All,nearDroppedWeapon.PV.InstantiationId);
                            actualWeapon = 2;
                            ChangeRoutine(ChangeGuns(secondaryGun));
                        } else {
                            if(actualWeapon == 1 || actualWeapon == 3){
                                primaryGun = nearDroppedWeapon.ChangeWeapons(primaryGun.gunIndex);
                                actualWeapon = 1;
                                ChangeRoutine(ChangeGuns(primaryGun));
                            } else {
                                secondaryGun = nearDroppedWeapon.ChangeWeapons(secondaryGun.gunIndex);
                                actualWeapon = 2;
                                ChangeRoutine(ChangeGuns(secondaryGun));
                            }
                        }
                        //bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
                        changeWeaponText.text = "";
                    }
                }
            } else {
                changeWeaponText.text = "";
                nearDroppedWeapon = null;
            }
        }
       
        if(Input.GetKeyDown(KeyCode.Alpha1)){
            if(Time.time >= nextTimeToChangeGuns){
                if(actualWeapon == 1 || primaryGun == null) return;
                actualWeapon = 1;
                ChangeRoutine(ChangeGuns(primaryGun));
                return;
            }
        } else if(Input.GetKeyDown(KeyCode.Alpha2)){
            if(Time.time >= nextTimeToChangeGuns){
                if(actualWeapon == 2 || secondaryGun == null) return;
                actualWeapon = 2;
                ChangeRoutine(ChangeGuns(secondaryGun));
                return;
            }
        } else if(Input.GetKeyDown(KeyCode.Alpha3)){
            if(actualWeapon == 3) return;
            actualWeapon = 3;
            ChangeRoutine(ChangeGuns(terciaryGun));
            bulletsText.text = "";
            return;
        } 


        if(Input.GetAxis("Mouse ScrollWheel") != 0){

            if(Time.time >= nextTimeToChangeGuns){
                
                if(secondaryGun != null && secondaryGun != null){
                    
                    int targetGun = 0;

                    if(actualWeapon == 2){
                        actualWeapon = 1;
                        targetGun = primaryGun.gunIndex;
                        ChangeRoutine(ChangeGuns(primaryGun));
                    } else {
                        actualWeapon = 2;
                        targetGun = secondaryGun.gunIndex;
                        ChangeRoutine(ChangeGuns(secondaryGun));
                    }
                
                    if(targetGun == 4 || targetGun == 5)
                        nextTimeToChangeGuns = Time.time + 0.8f;
                    else if(targetGun == 2)
                        nextTimeToChangeGuns = Time.time + 0.4f;
                    else  
                        nextTimeToChangeGuns = Time.time + 0.1f;

                }
            }
            
        }

        this.lifeText.text = health.ToString();

        CheckSpeed();
        CheckHands();
        CheckScreams();

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
            /*
            if(jumpingAnim){
                object[] instanceData = new object[2];
                instanceData[0] = this.PV.InstantiationId;
                instanceData[1] = 25;

                PhotonNetwork.Instantiate("Sounds",this.transform.position, Quaternion.identity,0,instanceData);
            }
            */
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
        }
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
            if(sensibilityRoutine != null)
                StopCoroutine(sensibilityRoutine);
            sensibilityRoutine = ExitSensibilidade();
            StartCoroutine(sensibilityRoutine);
        }
        
    }   
    
    IEnumerator ExitSensibilidade()
    {   
        yield return new WaitForSeconds(3f);
        sensibilidadeText.color = new Color(sensibilidadeText.color.r, sensibilidadeText.color.g, sensibilidadeText.color.b, 0f);
    } 

    //É AQUI QUE A TROCA DE ARMAS OCORRE
    IEnumerator ChangeGuns(WeaponStats weapon)
    {   
        if(weapon != null){

            isChangingGuns = true;

            nextTimeToFire = Time.time + (weapon.fireRate/3);

            if(primaryGun != null || secondaryGun != null){

                bool wasAiming = ((CurrentAnimation() == "ZoomIdle") || (isAiming == true));

                this.handAnimator[gunIndex].SetInteger("Reload", 0);
                this.handAnimator[gunIndex].SetBool("Sight", false);

                isAiming = false;
                isReloading = false;
                sniperScope.SetActive(false);
                weaponCamera.SetActive(true);
                fpsCam.fieldOfView = 60f;
                
                if(gunIndex > 0)
                    this.handAnimator[gunIndex].SetTrigger("TakeOut");

                if((gunIndex == 4 || (gunIndex == 5 && wasAiming))){
                    yield return new WaitForSeconds(0.6f);
                }else{
                    yield return new WaitForSeconds(0.1f);
                }

            }

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

            RuntimeAnimatorController ac = handAnimator[gunIndex].runtimeAnimatorController;
            for(int i = 0; i<ac.animationClips.Length; i++)
            {
                if(ac.animationClips[i].name == "Reload")
                {
                    reloadingTime = ac.animationClips[i].length;
                }
            }

            UpdateAmmo(weapon);
            damage = weapon.damage;
            fireRate = weapon.fireRate;
            range = weapon.range;
            ChangeGhostGun();

            if(bulletsText){
                bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
            }

            isChangingGuns = false;
        }
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
            playerSound.walkingStatus = 2;

            this.speed = actualWeapon == 3 ? 4f : 3.5f;
        }else if(isAiming){
            
            playerSound.step_Distance = crouch_Step_Distance;
            playerSound.walkingStatus = 2;

            this.speed = actualWeapon == 3 ? 4f : 3.5f;
        }else if(CurrentAnimation() == "Run"){
            
            playerSound.step_Distance = sprint_Step_Distance;
            playerSound.walkingStatus = 1;

            this.speed = actualWeapon == 3 ? 10f : 8f;
        }else{
            
            playerSound.step_Distance = walk_Step_Distance;
            playerSound.walkingStatus = 0;

            this.speed = actualWeapon == 3 ? 6f : 5f;
        }
    }

    void RemoveScope() 
    {   
        //isAiming = false;
        fpsCam.fieldOfView = 60f;
        sniperScope.SetActive(false);
        weaponCamera.SetActive(true);
        
        handAnimator[gunIndex].SetBool("Sight", false);

    }

    void ChangeRoutine(IEnumerator routine)
    {
        if(actualRoutine != null)
            StopCoroutine(actualRoutine);
        actualRoutine = routine;
        StartCoroutine(actualRoutine);
    }

    IEnumerator OnScoped(bool isAiming) 
    {   
        if(isAiming){
            yield return new WaitForSeconds(.3f);
            if(gunIndex == 5)
                fpsCam.fieldOfView = 15f;
            else if(gunIndex == 4)
                fpsCam.fieldOfView = 40f;
        } else {
            fpsCam.fieldOfView = 60f;
        }

        sniperScope.SetActive(isAiming);
        weaponCamera.SetActive(!isAiming);

    }

    void CheckHands()
    {   
        if(isChangingGuns)
            return;

        if(gunIndex == 5 && Time.time < nextTimeToFire){
            //RemoveScope();
            return;
        }

        if(Input.GetKeyDown(KeyCode.R)){
            if(noGuns || CurrentAnimation() == "Reload" || CurrentAnimation() == "Select" || actualWeapon == 3)
                return;
            if(totalAmmo > 0){
                if(clipSize != currentAmmo && isReloading == false){
                    isReloading = true;
                    ChangeRoutine(Reload());
                }
            }
        } 
        else {

            if(isReloading || CurrentAnimation() == "Reload")
                return;

            if(Input.GetButtonDown("Fire2")){
                if(gunIndex == 0)
                    return;

                isAiming = !isAiming;
                handAnimator[gunIndex].SetBool("Sight", isAiming);

                if(gunIndex == 4 || gunIndex == 5){
                    ChangeRoutine(OnScoped(isAiming));
                }
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

                        if(gunIndex != 5)
                            handAnimator[gunIndex].SetInteger("Fire", 1);
                        else {
                            handAnimator[gunIndex].SetTrigger("Fire");
                            if(actualRoutine != null)
                                StopCoroutine(actualRoutine);
                            RemoveScope();
                        }
                        Shoot();
                        return;
                    } else {
                        mouseLook.AddRecoil(0,0);
                    }
                }else{
                    mouseLook.AddRecoil(0,0);
                    if(gunIndex != 5)
                        handAnimator[gunIndex].SetInteger("Fire", 0);
                    if(totalAmmo > 0){
                        if(clipSize != currentAmmo){
                            if(Time.time >= nextTimeToFire){
                                isReloading = true;
                                ChangeRoutine(Reload());
                            }
                        }
                    }
                }  
            }else{
                mouseLook.AddRecoil(0,0);
                if(gunIndex != 5)
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
        mouseLook.AddRecoil(0,0);
        isAiming = false;
        handAnimator[gunIndex].SetBool("Sight", false);
        /*if(currentAmmo <= 0)
            handAnimator[gunIndex].SetInteger("Reload", 0);
        else
            handAnimator[gunIndex].SetInteger("Reload", 1);*/
        handAnimator[gunIndex].SetInteger("Reload", 1);

        sniperScope.SetActive(false);
        weaponCamera.SetActive(true);
        fpsCam.fieldOfView = 60f;
            
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

        damageIndicator.CreateIndicator(position);
    }

    void Shoot()
    {  
        //this.playerIcon.localScale = new Vector3(0f,0f,0f);
        int killCounterBefore = this.killCounter;
        shootingAnim = true;
        if(CurrentAnimation() == "Fire")
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
        
        if(CurrentAnimation() == "ZoomIdle" || CurrentAnimation() == "ZoomFire" || (isAiming && gunIndex == 5)){
            targetPosition = fpsCam.transform.forward;
        }else{
            float magnitude;
            magnitude = walkMagnitude > 0.1 ? 0.125f : 0.06f;
            targetPosition = new Vector3(fpsCam.transform.forward.x + Random.Range(-magnitude, magnitude),fpsCam.transform.forward.y + Random.Range(-magnitude, magnitude), fpsCam.transform.forward.z + Random.Range(-magnitude, magnitude));
        }
        if (Physics.Raycast(fpsCam.transform.position, targetPosition, out hit, range))
        {   
            int amount = 0;

            if(hit.transform.tag == "PlayerHead")
                amount = gunIndex == 5 ? (int)(damage * 2) : (int)((damage - (hit.distance)/50) * 2);
            else if(hit.transform.tag == "PlayerTorso")
                amount = gunIndex == 5 ? (int)(damage * 1.25) : (int)((damage - (hit.distance)/50) * 1.25);
            else if(hit.transform.tag == "PlayerLegs" || hit.transform.tag == "PlayerFeet")
                amount = gunIndex == 5 ? (int)(damage) : (int)((damage - (hit.distance)/50));
            else if(hit.transform.tag == "Enemy"){
                MonsterStats stats = hit.transform.gameObject.GetComponent<MonsterStats>();
                stats.hits++;
                hitMarker.BodyHit();
            }
        
            if(amount != 0 ){
                if(hit.transform.gameObject){
                    PlayerMovement target = hit.transform.gameObject.GetComponentInParent<PlayerMovement>();

                    if(target != null && target.health > 0){
                        if(hit.transform.tag == "PlayerHead")
                            hitMarker.HeadshotHit();
                        else
                            hitMarker.BodyHit();
                        
                        instanceData[1] = target.PV.InstantiationId;
                        instanceData[2] = amount;
                        PV.RPC("TakeDamage",RpcTarget.All,instanceData);
                    }
                }
            } else {
                PhotonNetwork.Instantiate("HitParticles",hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        
        shootingAnim = false;

        if(gunIndex == 5){
            RemoveScope();
            isAiming = false;
            mouseLook.AddRecoil(0,0);
        } else {
            
            float lastTime = lastTimeIShooted + fireRate + (fireRate/3);

            if(Time.time < lastTime){
                float h = Random.Range(-hRecoil,hRecoil);
                float v = Random.Range(-vRecoil,vRecoil);
                mouseLook.AddRecoil(v,hRecoil);
            } else {
                mouseLook.AddRecoil(0,0);
            }
        }

        if(this.killCounter != killCounterBefore)
            CheckKillStreak();

        lastTimeIShooted = Time.time;

        bulletsText.text = currentAmmo.ToString() + "/" + totalAmmo.ToString();
    }

    IEnumerator ResetGame(string winner){

        handAnimator[gunIndex].SetFloat("Walk_magnitude", 0f);
        handAnimator[gunIndex].SetBool("W_pressed", false);
        handAnimator[gunIndex].SetInteger("Reload", 0);
        handAnimator[gunIndex].SetBool("Sight", false);

        if(gunIndex != 5)
            handAnimator[gunIndex].SetInteger("Fire", 0);

        bodyAnimator.SetFloat("Walk_magnitude", 0f);
        bodyAnimator.SetBool("Crouch", false);
        bodyAnimator.SetBool("Jump", false);
        bodyAnimator.SetBool("Run", false);

        if(messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = ShowMessage(winner + " é o vencedor!",8f);
        StartCoroutine(messageRoutine);

        yield return new WaitForSeconds(8f);

        StartCoroutine(RestartGame());
    
    }

    public void ResetScore()
    {
        PlayerMovement [] players =  FindObjectsOfType<PlayerMovement>();

        for (int i=0; i<players.Length; i++)
        {   
            players[i].killStreak = 0;
            players[i].killCounter = 0;
            players[i].deathCounter = 0;
        }
    }

    public void UpdateScore()
    {
        PlayerMovement [] players =  FindObjectsOfType<PlayerMovement>();
        PlayerMovement [] playersOrder = players;

        for (int j=1; j<playersOrder.Length; j++) {
            for (int i=j; i>0 && playersOrder[i].killCounter > playersOrder[i-1].killCounter; i--) {
                PlayerMovement temporary;
                temporary = playersOrder [i];
                playersOrder [i] = playersOrder [i - 1];
                playersOrder [i - 1] = temporary;
            }
        }

        string texto = "";

        for (int i = 0; i < playersOrder.Length; i++)
        {
            if(!texto.Contains(players[i].Nickname))
                texto = texto + players[i].Nickname + " " + players[i].killCounter + "\n";//"/" + players[i].deathCounter + "\n";
        }
        
        if(rankingText)
            rankingText.text = texto;
    }

    [PunRPC]
    public void CallMethodForAllPlayers(int method, string winner)
    {      
        PlayerMovement [] players =  FindObjectsOfType<PlayerMovement>();

        for (int i=0; i<players.Length; i++)
        {   
            if(method == 0)
                players[i].UpdateScore();
            else if(method == 1){
                players[i].resetGame = true;
                players[i].winner = winner;  
            } else if(method == 2){
                players[i].ResetScore();
            }
        }

    }

    IEnumerator Respawn() 
    {   

        if(playerWhoKilledMe != null && playerWhoKilledMe.PV.InstantiationId != this.PV.InstantiationId){
            messageRoutine = ShowMessage(playerWhoKilledMe.Nickname + " meteu bala em você",4f);
        }else {
            messageRoutine = ShowMessage("Você se matou KKKK",4f);
        }

        StartCoroutine(messageRoutine);
            
        this.deathCounter++;
        this.killStreak = 0;

        /*
        if(rankingText)
            rankingText.text = this.Nickname + " " + this.killCounter + "/" + this.deathCounter + "\n";
        */

        object[] instanceData = new object[3];

        //PV.RPC("UpdateKD",RpcTarget.Others,this.PV.InstantiationId);
        
        instanceData[0] = gunIndex;
        yield return new WaitForSeconds(0.1f);
        this.jumpingAnim = false;
        this.runningAnim = false;
        this.isAiming = false;
        this.isCrounching = false;
        this.isReloading = false;
        this.idleAnim = true;
        this.velocity.y = -2f;

        ChangeRoutine(ChangeGuns(this.terciaryGun));

        this.actualWeapon = 3;
        
        yield return new WaitForSeconds(0.1f);
        if((int)instanceData[0] != terciaryGun.gunIndex)
            PhotonNetwork.Instantiate("DroppedGun",this.transform.position, Quaternion.identity,0,instanceData);

        bool setRandomPosition = false;


        float terrainLeft = worldTerrain.transform.position.x;
		float terrainBottom = worldTerrain.transform.position.z;
		float terrainWidth = worldTerrain.terrainData.size.x;
		float terrainLength = worldTerrain.terrainData.size.z;
		float terrainRight = terrainLeft + terrainWidth;
		float terrainTop = terrainBottom + terrainLength;
		float terrainHeight = 0f;
		RaycastHit hit;
		float randomPositionX, randomPositionY, randomPositionZ;
		Vector3 randomPosition = Vector3.zero;

        while(!setRandomPosition){

            randomPositionX = Random.Range(terrainLeft, terrainRight);
            randomPositionZ = Random.Range(terrainBottom, terrainTop);

            if(Physics.Raycast(new Vector3(randomPositionX, 9999f, randomPositionZ), Vector3.down, out hit, Mathf.Infinity, terrain.layer)){
                terrainHeight = hit.point.y;
            }

            randomPositionY = terrainHeight + 45f;
            randomPosition = new Vector3(randomPositionX, randomPositionY, randomPositionZ);

            PlayerMovement [] players =  FindObjectsOfType<PlayerMovement>();

            float nearDistance = 5000f;
        
            foreach (PlayerMovement player in players)
            {
                if(player.PV.InstantiationId != this.PV.InstantiationId){
                    float playerDistance = Vector3.Distance (player.transform.position, randomPosition);
                    if(playerDistance < nearDistance)
                        nearDistance = playerDistance;
                }
            }

            if(nearDistance >=100)
                setRandomPosition = true;

        }

        this.transform.position = randomPosition;

        /*
        float x = Random.Range((this.heaven.transform.position.x - 5f), (this.heaven.transform.position.x + 5f));
        float z = Random.Range((this.heaven.transform.position.z - 5f), this.heaven.transform.position.z + 5f);
        this.transform.position = new Vector3(x,this.heaven.transform.position.y + 4f,z);
        */
        ChangeRoutine(ChangeGuns(this.terciaryGun));
        yield return new WaitForSeconds(0.4f);
        
        this.primaryGun = null;
        this.secondaryGun = null;
        this.health = 100;
        
        this.waitingForSpawn = false;

        instanceData[0] = this.PV.InstantiationId;
        instanceData[1] = this.health;
        PV.RPC("UpdateLife",RpcTarget.Others,instanceData);
        
    }

    void OnTriggerStay(Collider other) 
    {
        if(!PV.IsMine)
			return;
        
        if(this.health <=0)
            return;

        if(other.tag == "DroppedWeapon")
        {
            //Physics.IgnoreCollision( other.gameObject.GetComponent<Collider>(), GetComponent<Collider>());
            nearDroppedWeapon = other.gameObject.GetComponentInParent<ChangeDroppedGun>();
            nearTransformDroppedWeapon = other.gameObject.transform;

        } else if(other.tag == "DropGameObject")
        {   
            //Physics.IgnoreCollision( other.gameObject.GetComponent<Collider>(), GetComponent<Collider>());
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

        if(other.tag == "Bomb")
        {  

        } else if(other.tag == "Respawn")
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
    public void DestroyObject(int instanceID)
    {   
        
        GameObject [] allDroppedGuns = GameObject.FindGameObjectsWithTag("DropGameObject");

        for (int i = 0; i < allDroppedGuns.Length; i++)
        {   
            ChangeDroppedGun dropped = allDroppedGuns[i].GetComponent<ChangeDroppedGun>();
            if(dropped.PV.InstantiationId == instanceID){
                dropped.DisableGun();
                break;
            }
        }
        
    }
    
    [PunRPC]
    public void TakeDamage(object[] instantiationData)
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
            if(whoReceivedDamage.PV.InstantiationId != whoCausedDamage.PV.InstantiationId)
                whoReceivedDamage.CreateDamageIndicator(whoReceivedDamage.PV.InstantiationId, whoCausedDamage.transform);

            whoReceivedDamage.health = whoReceivedDamage.health - (int)instantiationData[2];

            if(whoReceivedDamage.health <= 0){
                
                whoReceivedDamage.health = 0;

                if(whoReceivedDamage.PV.InstantiationId != whoCausedDamage.PV.InstantiationId){
                    if(this.PV.InstantiationId == whoCausedDamage.PV.InstantiationId){
                        this.killCounter++;
                        this.killStreak++;

                        PV.RPC("CallMethodForAllPlayers",RpcTarget.All,0,"");
                    }
                }
                
                whoReceivedDamage.playerWhoKilledMe = whoCausedDamage;

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
    public void UpdateDeaths(object[] instantiationData)
    {   
        PlayerMovement[] players = GetComponents<PlayerMovement>();

        for (int i = 0; i < players.Length; i++)
        {
            if(players[i].PV.InstantiationId == (int)instantiationData[0]){
                players[i].deathCounter++;
                break;
            } 
        }
    }

    [PunRPC]
    public void UpdateLife(object[] instantiationData)
    {   
        PlayerMovement[] players = GetComponents<PlayerMovement>();

        for (int i = 0; i < players.Length; i++)
        {
            if(players[i].PV.InstantiationId == (int)instantiationData[0]){
                players[i].health = (int)instantiationData[1];
                break;
            } 
        }
    }

    IEnumerator ShowMessage(string message, float timing) 
    {   
        if(messageText){
            this.messageText.text =  message;
            this.messageText.color = new Color(this.messageText.color.r, this.messageText.color.g, this.messageText.color.b, 100f);
            yield return new WaitForSeconds(timing);
            this.messageText.color = new Color(this.messageText.color.r, this.messageText.color.g, this.messageText.color.b, 0f);
        }
        
    }

    void CheckScreams()
    {   
        int willScream = -1;

        if(Input.GetKeyDown(KeyCode.Alpha4))
            willScream = 26;
        else if(Input.GetKeyDown(KeyCode.Alpha5))
            willScream = 27;
        else if(Input.GetKeyDown(KeyCode.Alpha6))
            willScream = 28;

        if(willScream > 0){
            if(Time.time >= nextTimeToScream){
                if(willScream > 27)
                    nextTimeToScream = Time.time + 7.0f;
                else
                    nextTimeToScream = Time.time + 3.0f;
    
                object[] instanceData = new object[2];
                instanceData[0] = this.PV.InstantiationId;
                instanceData[1] = willScream;

                PhotonNetwork.Instantiate("Sounds",this.transform.position, Quaternion.identity,0,instanceData);
            }
        }
    }

    void CheckKillStreak()
    {   
        if(this.killStreak % 5 == 0){
            messageRoutine = ShowMessage(this.killStreak.ToString() + " Kill Streak",4f);
            StartCoroutine(messageRoutine);

            if(this.killStreak < 16){
                object[] instanceData = new object[2];
                instanceData[0] = this.killStreak/5;
                instanceData[1] = false;
                PV.RPC("playGeneralSound",RpcTarget.All,instanceData);
            }
        }
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

    
    IEnumerator RestartGame()
    {   
        this.waitingForSpawn = true;
        this.meleeTime = 0.7f;
        this.reloadingTime = 1.5f;

        this.walk_Step_Distance = 0.42f;
        this.sprint_Step_Distance = 0.38f;
        this.crouch_Step_Distance = 0.54f;

        this.health = 100;

        this.nextTimeToFire = 0f;
        this.gunIndex = 0;

        this.killStreak = 0;
        this.killCounter = 0;
        this.deathCounter = 0;

        yield return new WaitForSeconds(0.1f);

        this.jumpingAnim = false;
        this.runningAnim = false;
        this.isAiming = false;
        this.isCrounching = false;
        this.isReloading = false;
        this.idleAnim = true;

        ChangeRoutine(ChangeGuns(this.terciaryGun));

        this.actualWeapon = 3;
        
        yield return new WaitForSeconds(0.1f);

        float x = Random.Range((this.heaven.transform.position.x - 5f), (this.heaven.transform.position.x + 5f));
        float z = Random.Range((this.heaven.transform.position.z - 5f), this.heaven.transform.position.z + 5f);
        this.transform.position = new Vector3(x,this.heaven.transform.position.y + 4f,z);
        yield return new WaitForSeconds(0.4f);

        this.primaryGun = null;
        this.secondaryGun = null;
        this.velocity.y = -2f;
        this.health = 100;
        this.speed = 5.5f;

        object[] instanceData = new object[3];
        instanceData[0] = this.PV.InstantiationId;
        instanceData[1] = this.health;

        PV.RPC("UpdateLife",RpcTarget.Others,instanceData);
        
        yield return new WaitForSeconds(0.1f);

        this.waitingForSpawn = false;
        this.resetGame = false;
        this.resetRoutine = null;

        PV.RPC("CallMethodForAllPlayers",RpcTarget.All,2,"");
        PV.RPC("CallMethodForAllPlayers",RpcTarget.All,0,"");
    }

}