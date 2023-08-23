using RosMessageTypes.Sensor;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UI;

public class RosConnectionExample : MonoBehaviour
{

    // WebCamTexture tex;
    Texture2D texRos;
    public RawImage display;
    ROSConnection rosNode;
    string webcamiagetopic = "US_images";

    void Start()
    {
        // start the ROS connection
        rosNode = ROSConnection.GetOrCreateInstance();

        rosNode.Subscribe<ImageMsg>(webcamiagetopic, displayImage);
        rosNode.RegisterPublisher<RosMessageTypes.Std.ByteMultiArrayMsg>("audio");


    }



    public void displayImage(ImageMsg img)
    {

        texRos = new Texture2D((int)img.width, (int)img.height, TextureFormat.RGB24, false); // , TextureFormat.RGB24
        BgrToRgb(img.data);
        texRos.LoadRawTextureData(img.data);

        texRos.Apply();
        display.texture = texRos;
    }

    public void BgrToRgb(byte[] data)
    {
        for (int i = 0; i < data.Length; i += 3)
        {
            byte dummy = data[i];
            data[i] = data[i + 2];
            data[i + 2] = dummy;
        }
    }

    public void publishBytes(byte[] data)
    {
        sbyte[] sdata = (sbyte[])(Array)data;
        RosMessageTypes.Std.ByteMultiArrayMsg msg=new RosMessageTypes.Std.ByteMultiArrayMsg(new RosMessageTypes.Std.MultiArrayLayoutMsg(),sdata);
        rosNode.Publish("audio", msg);
        Debug.Log($"publish a bytearray of size: {data.Length}");
    }
}
