using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum AttackType
{
    light,
    heavy
}

public class AttackScript : MonoBehaviour
{
    public KeyCode lightAttack;
    public KeyCode heavyAttack;
    [Space(20)]
    public LayerMask objectsToHit;
    public float distance;
    public float height;
    public float sphereSize = 1f;
    [Space(20)]
    //public float atkSpeed;
    public float minAnimLength = 0f;
    [Space]
    [SerializeField]
    private float atkProgress = 0;
    public int keyLimit = 0;
    private float atkDamage;
    [Space]
    public float inputWindow = 1f;


    public List<AttackType> keyPress = new List<AttackType>();

    [Space]
    public List<ComboPress> comboPress = new List<ComboPress>();

    public static bool inCombat = false;

   [System.Serializable]
    public class ComboPress
    {
        public float damageAtk = 10;
        [Space]
        /*public float newAtkSpeed = 0.2f;*/
        public float minAnimLength = 0.2f;
        public string comboAnimName;
        public AttackType[] sequence;
    }

    public Animator anim;

    private Vector3 rot;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        keyPress.Clear();

        for (int i = 0; i < comboPress.Count; i++)
        {
            if(keyLimit <= comboPress[i].sequence.Length)
            {
                keyLimit = comboPress[i].sequence.Length;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        rot = Quaternion.Euler(0.0f, CharaControllerMovement._targetRotation, 0.0f) * new Vector3(Vector3.forward.x * distance, height, Vector3.forward.z * distance);

        //increment atkprog while less than inputwindow end, else clear buffer
        if (atkProgress < minAnimLength + inputWindow)
        {
            atkProgress += Time.deltaTime;
        }
        else
            ClearBuffer();

        //only read input after min anim length has elapsed
        if (atkProgress > minAnimLength) {
            if (Input.GetKeyDown(lightAttack))
            {
                ComboCheck(AttackType.light);
                atkProgress = 0;
            }
            else if (Input.GetKeyDown(heavyAttack))
            {
                ComboCheck(AttackType.heavy);
                atkProgress = 0;
            }
        }

    }

    bool ComboCheck(AttackType attackType)
    {
        keyPress.Add(attackType);

        inCombat = true;

        //flag for checking if sequences match
        bool matches;
        for (int i = 0; i < comboPress.Count; i++)
        {
            matches = true;
            //check for matching length
            if (keyPress.Count == comboPress[i].sequence.Length)
            {
                //start checking the sequence elements one by one
                for (int j = 0; j < comboPress[i].sequence.Length && matches; j++)
                {
                    //if not matching, flag
                    if (keyPress[j] != comboPress[i].sequence[j])
                    {
                        matches = false;
                    }
                }
                //if all match, heccin go
                if (matches)
                {
                    //Debug.Log("This combo is use");
                    Debug.Log(comboPress[i].comboAnimName);
                    atkDamage = comboPress[i].damageAtk;
                    anim.SetTrigger(comboPress[i].comboAnimName);

                    //atkSpeed = comboPress[i].newAtkSpeed;
                    minAnimLength = comboPress[i].minAnimLength;
                    return true;
                }
            }
        }

        return false;
    }

    public void Attack()
    {
        //Creates a collision sphere
        Collider[] col = Physics.OverlapSphere(transform.position + rot, sphereSize, objectsToHit);

        //Finds all colliding object
        foreach (Collider collision in col)
        {
            Debug.Log(collision.gameObject.name);

            if(collision.gameObject.GetComponent<Enemy>())
            {
                collision.gameObject.GetComponent<Enemy>().Hit(atkDamage);
            }
        }
    }
    public void ClearBuffer()
    {
        keyPress.Clear();
    }

    public void IsNotInCombat()
    {
        inCombat = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + rot, sphereSize);
    }
}
