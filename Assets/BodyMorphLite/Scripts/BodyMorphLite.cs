using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BodyMorphLite : MonoBehaviour
{
    Animator animator;
    bool initialized;

    Transform
        hips, spine, chest, upperChest, neck, head, 
        leftHand, rightHand, leftFoot, rightFoot,
        leftFinger0, leftFinger1, leftFinger2, leftFinger3, leftFinger4,
        rightFinger0, rightFinger1, rightFinger2, rightFinger3, rightFinger4,
        leftArm, rightArm, leftLowerArm, rightLowerArm, 
        leftLeg, rightLeg, leftLowerLeg, rightLowerLeg, 
        leftShoulder, rightShoulder;

    float ankleHeight;
    

    public SkinnedMeshRenderer avatarBody;

    public bool inverseKinematics;
    BipedalKinematics kinematics;

    [Header("Body")]
    [Range(0.5f, 1.5f)] public float heightInput = 1.0f;
    [Range(0.5f, 1.5f)] public float upperBodyInput = 1.0f;
    [Range(0.5f, 1.5f)] public float lowerBodyInput = 1.0f;

    [Header("Head")]
    [Range(0.5f, 1.5f)] public float headInput = 1.0f;
    [Range(0.5f, 1.5f)] public float neckInput = 1.0f;

    [Header("UpperBody")]
    [Range(0.5f, 1.5f)] public float waistInput = 1.0f;
    [Range(0.5f, 1.5f)] public float chestInput = 1.0f;
    [Range(0.5f, 1.5f)] public float spineInput = 1.0f;
    [Range(0.5f, 2.0f)] public float shouldersInput = 1.0f;

    [Header("Arms")]
    [Range(0.5f, 1.5f)] public float upperArmsInput = 1.0f;
    [Range(0.5f, 1.5f)] public float lowerArmsInput = 1.0f;
    [Range(0.5f, 1.5f)] public float handsInput = 1.0f;
    [Range(0.5f, 2.5f)] public float fingersInput = 1.0f;

    [Header("LowerBody")]
    [Range(0.8f, 1.2f)] public float legsInput = 1.0f;
    [Range(0.5f, 1.5f)] public float feetInput = 1.0f;
    [Range(0.01f, 0.5f)] public float feetRadius = 0.05f;

    [HideInInspector]public float offset = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        if (!initialized)
        {
            Initialize();
        }
    }

    void Initialize()
    {
        animator = GetComponent<Animator>();

        avatarBody = GetComponentInChildren<SkinnedMeshRenderer>();


        if (animator.isHuman) 
        {
            hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            upperChest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            rightLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            head = animator.GetBoneTransform(HumanBodyBones.Head);
            neck = animator.GetBoneTransform(HumanBodyBones.Neck);


            leftFinger0 = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
            leftFinger1 = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
            leftFinger2 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
            leftFinger3 = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
            leftFinger4 = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);

            rightFinger0 = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
            rightFinger1 = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
            rightFinger2 = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            rightFinger3 = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
            rightFinger4 = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);

            ankleHeight = rightFoot.position.y - transform.position.y;

            initialized = true;

      
        }
        else
        {
    
            initialized = false;
            Debug.LogError("Avatar is not Humanoid");
            return;
        }

        if(inverseKinematics)
        {
            kinematics = GetComponent<BipedalKinematics>();
            if (kinematics == null)
            {
                kinematics = gameObject.AddComponent<BipedalKinematics>();
                kinematics.leftEnabled = true;
                kinematics.rightEnabled = true;
            }
        }
   
    }


    //void OnValidate() { UnityEditor.EditorApplication.delayCall += Validate; }

    private void OnValidate()
    {

        if (!initialized || animator == null)
        {
            Initialize();
        }

        if(inverseKinematics  )
        {
            if (kinematics == null)
            {
                Initialize();
            }
        }
        else
        {
            if (kinematics != null)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(kinematics);
                };
            }
        }

        if(avatarBody == null)
        {
            Debug.LogError("Avatar Body is missing");
            return;
        }

        Scale();
    }

    public void Scale()
    {
#if UNITY_EDITOR
        if (hips == null)
        {
            Initialize();
        }
#endif
        //Calculating Scales
        float lowerBodyScale = lowerBodyInput;

        transform.localScale = (heightInput + lowerBodyScale - 1.0f) * Vector3.one;

        float spineScale = spineInput / lowerBodyScale;

        float upperLegScale = legsInput;

        float lowerLegScale = 1f / upperLegScale / upperLegScale / (feetInput / feetInput);

        float feetScale = 1f / lowerLegScale / legsInput * feetInput;

    
        float waistScale = waistInput / spineScale / lowerBodyScale;

        float upperBodyScale = (waistScale + upperBodyInput - 1f);

        float chestScale = chestInput / waistScale / spineInput;

        float neckScale = neckInput / chestInput;
        float headScale = headInput / neckInput;

        float shoulderScale = shouldersInput / chestInput;
        float upperArmScale = upperArmsInput / shouldersInput;
        float lowerArmScale = lowerArmsInput / upperArmsInput;

        float handScale = handsInput / lowerArmsInput;
        float fingerScale = fingersInput;


        //Applying Scales
        leftLeg.localScale = upperLegScale * Vector3.one;
        rightLeg.localScale = upperLegScale * Vector3.one;
        leftLowerLeg.localScale = lowerLegScale * Vector3.one;
        rightLowerLeg.localScale = lowerLegScale * Vector3.one;
        leftFoot.localScale = feetScale * Vector3.one;
        rightFoot.localScale = feetScale * Vector3.one;


        spine.localScale = spineScale * Vector3.one;

        chest.localScale = upperBodyScale * Vector3.one;

        upperChest.localScale = chestScale * Vector3.one;

        neck.localScale = neckScale * Vector3.one;
        head.localScale = headScale * Vector3.one;

        leftShoulder.localScale = shoulderScale * Vector3.one;
        rightShoulder.localScale = shoulderScale * Vector3.one;

        leftArm.localScale = upperArmScale * Vector3.one;
        rightArm.localScale = upperArmScale * Vector3.one;

        leftLowerArm.localScale = lowerArmScale * Vector3.one;
        rightLowerArm.localScale = lowerArmScale * Vector3.one;


        leftHand.localScale = handScale * Vector3.one;
        rightHand.localScale = handScale * Vector3.one;

        leftFinger0.localScale = fingerScale * Vector3.one;
        leftFinger1.localScale = fingerScale * Vector3.one;
        leftFinger2.localScale = fingerScale * Vector3.one;
        leftFinger3.localScale = fingerScale * Vector3.one;
        leftFinger4.localScale = fingerScale * Vector3.one;
        rightFinger0.localScale = fingerScale * Vector3.one;
        rightFinger1.localScale = fingerScale * Vector3.one;
        rightFinger2.localScale = fingerScale * Vector3.one;
        rightFinger3.localScale = fingerScale * Vector3.one;
        rightFinger4.localScale = fingerScale * Vector3.one;



        //Offset Output
        float legSlider, legOffset;

        if (legsInput > 1f)
        {
            legSlider = Mathf.InverseLerp(0f, 1.2f, legsInput);
            legOffset = legSlider * -0.01f;
        }
        else
        {
            legSlider = Mathf.InverseLerp(1f, 0.8f, legsInput);
            legOffset = legSlider * 0.03f;
        }

        float feetOffset = ((ankleHeight * feetScale) - ankleHeight);
        offset = (feetOffset + legOffset) * transform.localScale.y;

        if (inverseKinematics)
        {
            kinematics.offset = offset;
            kinematics.radius = (feetRadius * transform.localScale.y) * feetInput;
        }
    }

    public void Randomize()
    {
        heightInput = Random.Range(0.5f,1.5f);
        upperBodyInput = Random.Range(0.5f, 1.5f);
        lowerBodyInput = Random.Range(0.5f, 1.5f);

        headInput = Random.Range(0.5f,1.5f);
        neckInput = Random.Range(0.5f,1.5f);

        chestInput = Random.Range(0.5f,1.5f);
        shouldersInput = Random.Range(0.5f,1.5f);
        upperArmsInput = Random.Range(0.5f,1.5f);
        lowerArmsInput = Random.Range(0.5f,1.5f);
        handsInput = Random.Range(0.5f,1.5f);
        fingersInput = Random.Range(0.5f, 1.5f);

        spineInput = Random.Range(0.5f, 1.5f);
        legsInput = Random.Range(0.8f, 1.2f);
        feetInput = Random.Range(0.5f, 1.5f);

        Scale();
    }
 


}
