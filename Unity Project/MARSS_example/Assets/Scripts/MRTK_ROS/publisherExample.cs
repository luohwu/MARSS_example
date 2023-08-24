using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class publisherExample : MonoBehaviour
{
    // Start is called before the first frame update
    ROS2.IPublisher<std_msgs.msg.String> publisher;
    void Start()
    {
        publisher = ROS2Listener.instance.node.CreatePublisher<std_msgs.msg.String>("Unity");
        InvokeRepeating("publish", 0, 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void publish()
    {
        std_msgs.msg.String msg = new std_msgs.msg.String();
        msg.Data = "msg from Unity";
        publisher.Publish(msg);
    }
}
