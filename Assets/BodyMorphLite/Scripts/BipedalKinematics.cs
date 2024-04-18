using System.Reflection;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

[RequireComponent(typeof(Animator))]
public class BipedalKinematics : MonoBehaviour
{
    Animator animator;
    float velocity;
    float weight;
    float lastHeight;
    Vector3 lastPosition;

    public bool leftEnabled, rightEnabled;
    bool[] footOnGround;

    [HideInInspector] public bool onGround;

    public float maxHeight = 0.5f;
    public float minHeight = 0.25f;
    public float radius = 0.05f;


    [Range(0.1f, 10f)] public float adaptSpeed = 1f;

    public LayerMask layerMask = 1;


    [HideInInspector] public float offset = 0f;

    Vector3[] ikPosition;
    Vector3[]  ikNormal;
    Quaternion[] ikRotation;
    Quaternion[] lastRotation;
    float[] lastFootHeight;

    Transform[] foot;

    void Start()
    {
        animator = GetComponent<Animator>();

        footOnGround = new bool[2];

        foot = new Transform[2];
        foot[0] = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        foot[1] = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

        ikPosition = new Vector3[2];
        ikNormal = new Vector3[2];
        ikRotation = new Quaternion[2];
        lastRotation = new Quaternion[2];
        lastFootHeight = new float[2];
    }


    private void FixedUpdate()
    {
        if (!animator) { return; }

        Vector3 targetVelocity = (lastPosition - transform.position) / Time.fixedDeltaTime;
        velocity = Mathf.Clamp(targetVelocity.magnitude, 1, targetVelocity.magnitude);
        lastPosition = transform.position;

   

        //On Ground
        onGround = footOnGround[1] || footOnGround[0];

        float targetWeight = onGround ? 1f : 0f;

        if (weight < targetWeight)
        {
            weight = Mathf.MoveTowards(weight, targetWeight, adaptSpeed * velocity * Time.fixedDeltaTime);
        }
        else
        {
            weight = Mathf.MoveTowards(weight, targetWeight, 10f * velocity * Time.fixedDeltaTime);
        }
    }
    void LateUpdate()
    {
        if (leftEnabled)
        {
            FootSolver(1, 0);
        }

        if (rightEnabled)
        {
            FootSolver(0, 1);
        }
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (!animator) { return; }

        if (leftEnabled && rightEnabled)
        {
            PelvisHeight();
        }
      
        if (leftEnabled)
        {
            FootMove(AvatarIKGoal.LeftFoot, 1);
        }
        if (rightEnabled)
        {
            FootMove(AvatarIKGoal.RightFoot, 0);
        }
    }

    private void PelvisHeight()
    {
        float leftOffset = ikPosition[1].y - transform.position.y;
        float rightOffset = ikPosition[0].y - transform.position.y;
        float hipsOffset = (leftOffset < rightOffset) ? leftOffset : rightOffset;

        Vector3 position = animator.bodyPosition;
        float height = hipsOffset * (0.75f * weight);
        lastHeight = Mathf.MoveTowards(lastHeight, height, Time.deltaTime);
        position.y += lastHeight + offset;

        animator.bodyPosition = position;
    }

    void FootMove(AvatarIKGoal foot, int index)
    {
        Vector3 targetPosition = animator.GetIKPosition(foot);
        Quaternion targetRotation = animator.GetIKRotation(foot);

        //Position
        targetPosition = transform.InverseTransformPoint(targetPosition);
        //ikPosition[index] *= transform.localScale.y;
        ikPosition[index] = transform.InverseTransformPoint(ikPosition[index]);
        lastFootHeight[index] = Mathf.MoveTowards(lastFootHeight[index], ikPosition[index].y, adaptSpeed * Time.deltaTime);
        targetPosition.y += lastFootHeight[index];

        targetPosition = transform.TransformPoint(targetPosition);


        targetPosition += ikNormal[index] * (offset);

        //Rotation
        Quaternion relative = Quaternion.Inverse(ikRotation[index] * targetRotation) * targetRotation;
        lastRotation[index] = Quaternion.RotateTowards(lastRotation[index], Quaternion.Inverse(relative), 90f * Time.deltaTime);

        targetRotation *= lastRotation[index];

        //Set IK
        animator.SetIKPosition(foot, targetPosition);
        animator.SetIKPositionWeight(foot, weight);
        animator.SetIKRotation(foot, targetRotation);
        animator.SetIKRotationWeight(foot, weight);
    }

    void FootSolver(int current, int other)
    {
        float steps = maxHeight;
        if (!footOnGround[other])
        {
            steps = minHeight;
        }

        Vector3 position = foot[current].position;
        position.y = transform.position.y + steps;

        float feetHeight = steps;

        if (Physics.SphereCast(position, radius, Vector3.down, out RaycastHit hit, steps * 2f, layerMask))
        {
            feetHeight = transform.position.y - hit.point.y;

            ikPosition[current] = hit.point;

            ikNormal[current] = hit.normal;

            Vector3 axis = Vector3.Cross(Vector3.up, hit.normal);
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            ikRotation[current] = Quaternion.AngleAxis(angle, axis);
        }

        footOnGround[current] = feetHeight < steps;

        if (!footOnGround[current])
        {
            ikPosition[current].y = transform.position.y - steps;

            ikRotation[current] = Quaternion.identity;
        }
    }
}