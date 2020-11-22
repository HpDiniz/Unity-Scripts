using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitMarker : MonoBehaviour
{   
    Image [] childrens;
    public Color principalColor;
    public Color fadedColor;
    public AudioSource hitSound;
    //public float fadeTime = 2f;
    // Start is called before the first frame update
    void Start()
    {
        childrens = this.GetComponentsInChildren<Image>();
        foreach (Image image in childrens)
        {
            image.color = fadedColor;
        } 

    }

    public void HeadshotHit()
    {   
        StopAllCoroutines();
        StartCoroutine(ShowHitMarkerHS());
    }

    public void BodyHit()
    {   
        StopAllCoroutines();
        StartCoroutine(ShowHitMarker());
    }

    IEnumerator ShowHitMarker()
    {
        hitSound.Play(0);
        foreach (Image image in childrens)
        {   
            if(image.name != "Headshot")
                image.color = principalColor;
        } 
        yield return new WaitForSeconds(0.25f);
        FadeOut();
    }

    void FadeOut()
    {  
        foreach (Image image in childrens)
        {   
            if(image.name != "Headshot")
                image.color = fadedColor;//Color.Lerp(fadedColor, principalColor, fadeTime * Time.deltaTime);
        } 
    }

    IEnumerator ShowHitMarkerHS()
    {
        hitSound.Play(0);
        foreach (Image image in childrens)
        {   
            image.color = principalColor;
        } 
        yield return new WaitForSeconds(0.25f);
        FadeOutHS();
    }

    void FadeOutHS()
    {  
        foreach (Image image in childrens)
        {   
            image.color = fadedColor;//Color.Lerp(fadedColor, principalColor, fadeTime * Time.deltaTime);
        } 
    }
}
