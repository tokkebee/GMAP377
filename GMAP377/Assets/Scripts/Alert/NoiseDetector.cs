using System;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

public class NoiseDetector : MonoBehaviour
{
    //Alert Message
    [SerializeField] protected float noiseThreshold, noiseMaximum, noiseBase;
    private AlertBroadcastManager _alert;
    //Event if creature is Alerted
    public delegate void AlertedHandler(Vector3 position);
    [CanBeNull] public event AlertedHandler Alerted;

    protected void Awake() => _alert = FindAnyObjectByType<AlertBroadcastManager>();
    protected void OnEnable() => _alert.AlertEvent += OnAlert;
    protected void OnDisable() => _alert.AlertEvent -= OnAlert;

    protected virtual void OnAlert(Vector3 pos, float volume)
    {
        float distance = Vector3.Distance(transform.position, pos);
        float detectionValue = noiseMaximum * math.pow(noiseBase, distance);
        if(detectionValue<noiseThreshold) Alerted?.Invoke(pos);
    }
}
