using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using System;
public class top : MonoBehaviour
{
    // Gameobject関連
    [SerializeField] GameObject TrackedHMD;                     // HMDのトラッキングを反映させているGameObject
    [SerializeField] GameObject Camera;

    // ベクトル関連
    Vector3 HMDFirstPosition = new Vector3 (0.0f, 2.0f, 0.0f);  // HMDの初期位置(高い位置に設定)
    Vector3 HMDVirtualPosition;                                 // Redirected JampingさせるGameObject
    Quaternion HMDRotation;                                     // HMDの回転
    Vector3 waistPosition;
    Vector3 leftfootPosition;
    Vector3 rightfootPosition;
    private SteamVR_Action_Pose waist = SteamVR_Actions.default_Pose;
    private SteamVR_Action_Pose leftfoot = SteamVR_Actions.default_Pose;
    private SteamVR_Action_Pose rightfoot = SteamVR_Actions.default_Pose;
    bool readywaist = false;
    bool readyfoot = false; 
    bool jumpstart = false;
    float waistF = 0;
    float LfootF = 0;
    float RfootF = 0;

    float deltapos = 0;
    float apex = 0;

    [SerializeField] float totaltime;                           // 滞空時間


    // 変数関連
    [SerializeField] float threshold;                           // Redirected Jampingを発動させる閾値
    [SerializeField] float rate;                                // 倍率
    float difference;                                           // Jampの高さ
    float Jumptime;
    double max = 0.1;


    //アニメーションカーブ関連
    public AnimationCurve keyframe = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

    // Start is called before the first frame update
    void Start()
    {
        if (!TrackedHMD)
        {
            TrackedHMD = GameObject.Find("RealHMD");
        }
        if (!Camera)
        {
            Camera = GameObject.Find("VRCamera");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            // 旧式
            // HMDFirstPosition = InputTracking.GetLocalPosition(XRNode.Head);
            // print("1, (" + HMDFirstPosition.x + "," + HMDFirstPosition.y + "," + HMDFirstPosition.z + ")");
            // 推奨版に書き換え
            InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.devicePosition, out HMDFirstPosition);
            print("InputDevices.GetDeviceAtXRNode, \t(" + HMDFirstPosition.x.ToString("f8") + "," + HMDFirstPosition.y.ToString("f8") + "," + HMDFirstPosition.z.ToString("f8") + ")");
            HMDFirstPosition = TrackedHMD.transform.position;
            print("SteamVR_Tracked_Object, \t\t(" + HMDFirstPosition.x.ToString("f8") + "," + HMDFirstPosition.y.ToString("f8") + "," + HMDFirstPosition.z.ToString("f8") + ")");
            waistPosition = waist.GetLocalPosition(SteamVR_Input_Sources.Waist);
            waistF = waistPosition.y;
            Debug.Log(waistF);
            leftfootPosition = leftfoot.GetLocalPosition(SteamVR_Input_Sources.LeftFoot);
            LfootF = leftfootPosition.y;
            Debug.Log(LfootF);
            rightfootPosition = rightfoot.GetLocalPosition(SteamVR_Input_Sources.RightFoot);
            RfootF = rightfootPosition.y;
            Debug.Log(RfootF);
        }
    }

    void FixedUpdate() {
        {
            // Redirected Jamping処理
            //トラッカーの座標取得
            waistPosition = waist.GetLocalPosition(SteamVR_Input_Sources.Waist);
            //Debug.Log("腰" + waistPosition.y);
            leftfootPosition = leftfoot.GetLocalPosition(SteamVR_Input_Sources.LeftFoot);
            //Debug.Log("左足" + leftfootPosition.y);
            rightfootPosition = rightfoot.GetLocalPosition(SteamVR_Input_Sources.RightFoot);
            //Debug.Log("右足" + rightfootPosition.y);
            
            // 現在の取得HMDを取得
            // InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.devicePosition, out HMDVirtualPosition);
            HMDVirtualPosition = TrackedHMD.transform.position;
            // 計算処理
            difference = HMDVirtualPosition.y - HMDFirstPosition.y;

            if(max < difference)
            {
                max = difference;
            }

            if(waistF -waistPosition.y > 0.01)
            {
                readywaist = true;
                // Debug.Log("a");
            }

            if(leftfootPosition.y -LfootF > 0.01 && rightfootPosition.y - RfootF > 0.01)
            {
                readyfoot = true;
                // Debug.Log("b");
                //readywaist == true && readyfoot ==true
            }

            if (readywaist == true && readyfoot ==true)
            {
                // Debug.Log("c");
                if(difference > 0.02)
                {
                    if(jumpstart == false)
                    {
                        jumpstart = true;
                        // Debug.Log("d");

                    }
                    Jumptime += Time.deltaTime; 

                    deltapos = (keyframe.Evaluate(Jumptime / totaltime) + keyframe.Evaluate(1 - Jumptime / totaltime)) / 2;
                    // apex = keyframe.Evaluate(Jumptime / totaltime);

                    // if(apex < 0.25f)
                    // {
                    //     deltapos = keyframe.Evaluate(Jumptime / totaltime);
                    // }
                    // if(apex < 0.25f)
                    // {
                    //     deltapos = keyframe.Evaluate(0.5f - Jumptime / totaltime);
                    // }

                    Debug.Log(deltapos);

                    this.transform.position = new Vector3 (HMDVirtualPosition.x, HMDFirstPosition.y + deltapos * rate, HMDVirtualPosition.z);
                    //this.transform.position = new Vector3 (HMDVirtualPosition.x, HMDFirstPosition.y + difference*rate, HMDVirtualPosition.z);

                    //Debug.Log("高さ" + this.transform.position.y);
                }
                else
                {
                    this.transform.position = HMDVirtualPosition;
                    // if(0.22 < max && max < 0.28)
                    // {
                    //     Debug.Log("高さ" + max);
                    // }
                    // max = 0;
                }

                if(jumpstart == true && difference < 0.01)
                {
                    readywaist = false;
                    readyfoot = false;
                    jumpstart = false;
                    Jumptime = 0;
                    // Debug.Log("e");
                    // Debug.Log("高さ" + difference);
                }
            }
            else
            {
                this.transform.position = HMDVirtualPosition;
                if(0.22 < max && max < 0.28)
                {
                    Debug.Log("高さ" + max);
                    max = 0;
                }
                // Debug.Log("f");
            }

            // ついでにメインカメラに回転を追加
            // InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.deviceRotation, out HMDRotation);
            HMDRotation = TrackedHMD.transform.rotation;
            Camera.transform.rotation = HMDRotation;
        }
    }
}