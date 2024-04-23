using System.Reflection;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

[RequireComponent(typeof(Animator))]
public class BipedalKinematics : MonoBehaviour
{
    Animator animator;

    private float weight;
    private float lastHeight;

    public bool leftEnabled, rightEnabled;
    private bool[] footOnGround;

    private bool onGround;

    public bool OnGround { get => onGround;}

    //Height Offset Range
    public float maxHeight = 0.5f;
    public float minHeight = 0.25f;

    //Foot Radius
    public float radius = 0.05f;

    [Header("Settings")]
    [Range(0.1f, 10f)] public float offsetSpeed = 0.75f;
    [Range(0.1f, 10f)] public float fallModifier = 1f;
    [Range(0.1f, 10f)] public float adaptSpeed = 1f;
    [Range(0f, 360f)] public float adaptRotation = 90f;


    public LayerMask layerMask = 1;


    private float offset = 0f;
    public float Offset { get => offset; set => offset = value; }

    private Vector3[] ikPosition;
    private Vector3[]  ikNormal;
    private Quaternion[] ikRotation;
    private Quaternion[] lastRotation;
    private float[] lastFootHeight;

    private Transform[] foot;

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
        //Check if one of the feet on Ground
        onGround = footOnGround[1] || footOnGround[0];

        float targetWeight = onGround ? 1.0f : 0.0f;
        weight = Mathf.MoveTowards(weight, targetWeight, adaptSpeed * Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        //Feet Solvers
        if (leftEnabled)
        {
            FootIKTarget(1, 0);
        }

        if (rightEnabled)
        {
            FootIKTarget(0, 1);
        }
    }
    private void OnAnimatorIK(int layerIndex)
    {
        //Body Offset
        if (leftEnabled && rightEnabled)
        {
            HipsOffset();
        }
      
        //Feet Offset
        if (leftEnabled)
        {
            FootIKMove(AvatarIKGoal.LeftFoot, 1);
        }

        if (rightEnabled)
        {
            FootIKMove(AvatarIKGoal.RightFoot, 0);
        }
    }

    //Body Offset
    private void HipsOffset()
    {
        float leftOffset = ikPosition[1].y - transform.position.y;
        float rightOffset = ikPosition[0].y - transform.position.y;
        float hipsOffset = (leftOffset < rightOffset) ? leftOffset : rightOffset;

        Vector3 position = animator.bodyPosition;

        //Ascending-Descending Velocity
        float offsetVelocity = lastHeight < hipsOffset ? offsetSpeed : offsetSpeed * fallModifier;

        lastHeight = Mathf.MoveTowards(lastHeight, hipsOffset, offsetVelocity * Time.deltaTime);
        position.y += lastHeight + offset;

        animator.bodyPosition = position;
    }

    //Position and Rotation of feet
    private void FootIKMove(AvatarIKGoal foot, int index)
    {
        Vector3 targetPosition = animator.GetIKPosition(foot);
        Quaternion targetRotation = animator.GetIKRotation(foot);

        //Position
        targetPosition = transform.InverseTransformPoint(targetPosition);
        ikPosition[index] = transform.InverseTransformPoint(ikPosition[index]);
        lastFootHeight[index] = Mathf.MoveTowards(lastFootHeight[index], ikPosition[index].y, adaptSpeed * Time.deltaTime);
        targetPosition.y += lastFootHeight[index];

        targetPosition = transform.TransformPoint(targetPosition);


        targetPosition += ikNormal[index] * (offset);

        //Rotation
        Quaternion relative = Quaternion.Inverse(ikRotation[index] * targetRotation) * targetRotation;
        lastRotation[index] = Quaternion.RotateTowards(lastRotation[index], Quaternion.Inverse(relative), adaptRotation * Time.deltaTime);

        targetRotation *= lastRotation[index];

        //Set IK Goals
        animator.SetIKPosition(foot, targetPosition);
        animator.SetIKPositionWeight(foot, weight);
        animator.SetIKRotation(foot, targetRotation);
        animator.SetIKRotationWeight(foot, weight);
    }


    private void FootIKTarget(int current, int other)
    {
        float height = maxHeight;
        if (!footOnGround[other])
        {
            height = minHeight;
        }

        Vector3 position = foot[current].position;
        position.y = transform.position.y + height;

        float feetHeight = height;


        //Check for collision
        if (Physics.SphereCast(position, radius, Vector3.down, out RaycastHit hit, height * 2.0f, layerMask))
        {
            feetHeight = transform.position.y - hit.point.y;

            ikPosition[current] = hit.point;

            ikNormal[current] = hit.normal;

            Vector3 axis = Vector3.Cross(Vector3.up, hit.normal);
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            ikRotation[current] = Quaternion.AngleAxis(angle, axis);
        }

        footOnGround[current] = feetHeight < height;

        //Align with animation while not on ground
        if (!footOnGround[current])
        {
            ikPosition[current].y = transform.position.y - height;
            ikRotation[current] = Quaternion.identity;
        }
    }
}