using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class Demonstration : MonoBehaviour
{
    public Animator animator;

    [Range(0f, 5f)] public float speed;
    [Range(0f, 6f)] public float movement;

    private float fallTime;
    private float jumpTime;

    public bool jump;

    public GameObject[] objectPool;
    public float dropTime;
    int objectOrder;
    Vector3 targetPosition;
    // Start is called before the first frame update
    void Start()
    {
        objectPool = new GameObject[] {
            GameObject.CreatePrimitive(PrimitiveType.Cube),
            GameObject.CreatePrimitive(PrimitiveType.Cube),
            GameObject.CreatePrimitive(PrimitiveType.Sphere)
        };

        foreach (GameObject obj in objectPool)
        {

            Vector3 position = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.0f, -0.4f), 4.5f);
            obj.transform.position = position;
            obj.gameObject.SetActive(false);
        }

        dropTime = 0f;
    }
    // Update is called once per frame
    void Update()
    {
        if(movement > 0f)
        {
            dropTime -= Time.deltaTime;

            if(dropTime <= 0f)
            {
                objectPool[objectOrder].SetActive(true);

                if (objectOrder != objectPool.Length - 1)
                {
                    Vector3 rotation = new Vector3(Random.Range(-15f, 15f), 0f, 0f);
                    objectPool[objectOrder].transform.eulerAngles = rotation;

                    Vector3 scale = new Vector3(1f, 1f, Random.Range(0.5f, 2f));
                    objectPool[objectOrder].transform.localScale = scale;
                }

                objectOrder = objectOrder >= objectPool.Length - 1  ? 0 : objectOrder + 1;
                dropTime = Random.Range(1f, 1.25f) * (7f - movement);
            }

            foreach (GameObject obj in objectPool)
            {

                if (obj.gameObject.activeSelf)
                {
                    Vector3 position = obj.transform.position - Vector3.forward * speed * movement * 0.5f * Time.deltaTime;


                    if (position.z < -4.5f)
                    {
                        position = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.0f, -0.4f), 4.5f);
                        

                        obj.SetActive(false);
                    }

                    obj.transform.position = position;
                }
            }

            Vector3 rayPosition = animator.transform.position + Vector3.up * 0.45f * animator.transform.localScale.y;

            if (Physics.Raycast(rayPosition, Vector3.forward, out RaycastHit hit, 2f))
            {
                if (hit.distance < 1f)
                {
                    if (hit.transform.localEulerAngles.x >= 0f)
                    {
                        jump = true;
                    }
                }
            }
            Debug.DrawRay(rayPosition, Vector3.forward);

            if (Physics.Raycast(rayPosition, Vector3.down, out RaycastHit hit2, 1f))
            {
                targetPosition = hit2.point;
            }
            else
            {
                targetPosition = Vector3.zero;
            }

            animator.transform.position = Vector3.MoveTowards(animator.transform.position, targetPosition, 1f * Time.fixedDeltaTime);

        }

       

        CharacterMovement();

    }

 


    void CharacterMovement()
    {
        animator.SetFloat("MotionSpeed", speed);
        animator.SetFloat("Speed", movement);

        JumpAndGravity();
     
    }

    private void JumpAndGravity()
    {
        if (!jump)
        {
            fallTime = 0.15f;

            animator.SetBool("Jump", false);
            animator.SetBool("Fall", false);
            
            if (jump && jumpTime <= 0.0f)
            {
           
                animator.SetBool("Jump", true);
                
            }

            if (jumpTime >= 0.0f)
            {
                jumpTime -= Time.deltaTime;
            }
        }
        else
        {
            jumpTime = 0.5f;

            if (fallTime >= 0.0f)
            {
                fallTime -= Time.deltaTime;
            }
            else
            {
                animator.SetBool("Fall", true);
                jump = false;
            }

         
        }

    }
}
