using UnityEngine;


[RequireComponent(typeof(Animator))]
public class BipedalKinematics : MonoBehaviour
{
    Animator animator;

    private float weight;
    private float lastHeight;

    private bool[] ikEnabled;
    private bool[] footOnGround;

    private bool onGround;

    public bool OnGround { get => onGround;}

    [Header("Settings")]
    //Height Offset Range
    public float maxHeight = 0.5f;
    public float minHeight = 0.25f;

    //Foot Radius
    public float radius = 0.05f;


    [Range(0.1f, 10f)] public float offsetSpeed = 1.0f;
    [Range(0.1f, 10f)] public float fallModifier = 2.0f;
    [Range(0.1f, 10f)] public float adaptSpeed = 2.0f;
    [Range(0f, 360f)] public float adaptRotation = 180f;

    public LayerMask layerMask = 1;


    private float offset = 0f;

    public float Offset { set => offset = value; }

   

    private Vector3[] ikPosition;
    private Vector3[]  ikNormal;
    private Quaternion[] ikRotation;
    private Quaternion[] lastRotation;
    private float[] lastFootHeight;

    private Transform[] foot;

    private bool initialized;

    private void Awake()
    {
        if (!initialized)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        animator = GetComponent<Animator>();

        footOnGround = new bool[2];

        foot = new Transform[2];
        foot[0] = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        foot[1] = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

        ikEnabled = new bool[2];
        ikPosition = new Vector3[2];
        ikNormal = new Vector3[2];
        ikRotation = new Quaternion[2];
        lastRotation = new Quaternion[2];
        lastFootHeight = new float[2];

        initialized = true;

        Enable();
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

        //Right Foot Solver
        if (ikEnabled[0])
        {
            FootIKTarget(0, 1);
        }
        //Left Foot Solver
        if (ikEnabled[1])
        {
            FootIKTarget(1, 0);
        }

    }
    private void OnAnimatorIK(int layerIndex)
    {
        //Body Offset
        if (ikEnabled[0] && ikEnabled[1])
        {
            HipsOffset();
        }

        //Feet Offsets
        if (ikEnabled[0])
        {
            FootIKMove(AvatarIKGoal.RightFoot, 0);
        }

        if (ikEnabled[1])
        {
            FootIKMove(AvatarIKGoal.LeftFoot, 1);
        }

     
    }

    //IK Solvers
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

    /// <summary>
    /// Enables IK: null = Both, 0 = Right, 1 = Left.
    /// </summary>
    /// <param name="target">
    /// Target IK to enable.
    /// </param>
    public void Enable(int? target = null)
    {
        if (target != null)
        {
            ikEnabled[(int)target] = true;
        }
        else
        {
            ikEnabled[0] = true;
            ikEnabled[1] = true;
        }
    }

    /// <summary>
    /// Disables IK: null = Both, 0 = Right, 1 = Left.
    /// </summary>
    /// <param name="target">Target IK to enable.</param>
    public void Disable(int? target = null)
    {
        if(target != null)
        {
            ikEnabled[(int)target] = false;
        }
        else
        {
            ikEnabled[0] = false;
            ikEnabled[1] = false;
        }
    }

  
}