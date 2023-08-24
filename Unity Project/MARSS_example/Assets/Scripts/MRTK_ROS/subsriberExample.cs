using ROS2;
using sensor_msgs.msg;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class subsriberExample : MonoBehaviour
{
    // Start is called before the first frame update

    private ROS2Listener rosNode;
    private ISubscription<std_msgs.msg.String> _sub;
    void Start()
    {
        rosNode = ROS2Listener.instance;
        rosNode.node.CreateSubscription<std_msgs.msg.String>("test", msg => { Debug.Log(msg.Data); });


    }

    // Update is called once per frame
    void Update()
    {

    }

}