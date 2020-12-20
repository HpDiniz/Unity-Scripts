using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DI_System : MonoBehaviour
{
    /*
    [Header("References")]
    [SerializeField] private DamageIndicator indicatorPrefab = null;
    [SerializeField] private RectTransform holder = null;
    [SerializeField] private new Camera camera = null;
    [SerializeField] private Transform player = null;

    private Dictionary<Transform, DamageIndicator> Indicators = new Dictionary<Transform, DamageIndicator>();

    public Action<Transform> CreateIndicator = delegate {};

    private void OnEnable()
    {
        CreateIndicator += Create;
    }
    private void OnDisable()
    {
        CreateIndicator -= Create;
    }
    void Create(Transform target)
    {
        if(Indicators.ContainsKey(target))
        {
            Indicators[target].Restart();
            return;
        }
        DamageIndicator newIndicator = Instantiate(indicatorPrefab, holder);
        newIndicator.Register(target,player,new Action( () => { Indicators.Remove(target); }));

        Indicators.Add(target, newIndicator);
    }
    */
}
