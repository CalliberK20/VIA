using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class LineOfSightChecker : MonoBehaviour
{
    public float FieldOfView;
    //public LayerMask LineOfSightLayers;
    public List<string> LineOfSightTags;

    public delegate void GainSightEvent(Transform target);
    public GainSightEvent OnGainSight;
    public delegate void LoseSightEvent(Transform target);
    public LoseSightEvent OnLoseSight;

    [HideInInspector]
    public SphereCollider SphereCollider;
    private Coroutine _checkForLineOfSight;

    private void Awake()
    {
        SphereCollider = GetComponent<SphereCollider>();
    }

    //something enters, start checking line of sight if matches LoS tags
    private void OnTriggerEnter(Collider other)
    {
        foreach (string tag in LineOfSightTags)
        {
            if (!other.CompareTag(tag))
                continue;

            Debug.Log($"Collider {other} in range!");

            //very first target getting aggro will stay as aggro target unless it leaves
            if (!CheckLineOfSight(other.transform))
            {
                
                _checkForLineOfSight = StartCoroutine(CheckLineOfSightInterval(other.transform));
            }
            break;
        }
    }

    //something leaves, stop checking line of sight if matches LoS tags
    private void OnTriggerExit(Collider other)
    {
        foreach (string tag in LineOfSightTags)
        {
            if (!other.CompareTag(tag))
                continue;

            Debug.Log($"Collider {other} exited range!");

            OnLoseSight?.Invoke(other.transform);
            if (_checkForLineOfSight != null)
                StopCoroutine(_checkForLineOfSight);
            break;
        }
    }

    private bool CheckLineOfSight(Transform target)
    {
        OnGainSight?.Invoke(target);
        /*Vector3 direction = (target.position - transform.position).normalized;
        if (Vector3.Dot(transform.forward, direction) >= Mathf.Cos(FieldOfView))
        {
            OnGainSight?.Invoke(target);
        }*/
        return false;
    }

    //update interval for checking line of sight
    private IEnumerator CheckLineOfSightInterval(Transform target)
    {
        WaitForSeconds wait = new(0.1f);

        while (!CheckLineOfSight(target))
        {
            //LoS might stop being checked as soon as target acquired?
            yield return wait;
        }
    }
}