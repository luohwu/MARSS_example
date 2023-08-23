using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

/// <summary>
///
/// </summary>
public class RosByteArrayPublisher : MonoBehaviour
{
    public ROSConnection ros;
    public string topicName = "audio";

    // Used to determine how much time has elapsed since the last message was published

    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<RosMessageTypes.Std.ByteMultiArrayMsg>(topicName);
    }

    private void Update()
    {   
    }
}