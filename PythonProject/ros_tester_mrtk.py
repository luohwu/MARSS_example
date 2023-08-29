import socket

import rclpy
import std_msgs.msg
from rclpy.node import Node

from std_msgs.msg import Float32MultiArray
from sensor_msgs.msg import Image
import cv2
from tools.cv2ROS import *

import threading

class ExampleSubscriber(Node):

    def __init__(self):
        super().__init__('minimal_subscriber')
        self.subscription = self.create_subscription(
            std_msgs.msg.ByteMultiArray,
            'audio',
            self.listener_callback,
            10)
        self.subscription  # prevent unused variable warning

    def listener_callback(self, msg):
        with open("./test.wav",mode="bw") as f:
            f.write(bytearray(np.array(msg.data)))
            print(f"writed the coming data to .wav file")

class ExamplePublisher(Node):
    def __init__(self):
        super().__init__('centralPublisher')
        self.US_publisher=self.create_publisher(Image, 'US_images', 0)
        timer_period = 1/5.  # seconds
        self.timer = self.create_timer(timer_period, self.timer_callback)
        self.i = 0

    def timer_callback(self):
        cv2_image=cv2.imread(f'./data/{self.i%20}.png')
        msg = cv2_to_imgmsg(cv2_image)
        self.US_publisher.publish(msg)
        self.i += 1
        # print(f"published {self.i}")







if __name__=='__main__':

    rclpy.init()

    minimal_subscriber = ExampleSubscriber()

    rclpy.spin(minimal_subscriber)

    # Destroy the node explicitly
    # (optional - otherwise it will be done automatically
    # when the garbage collector destroys the node object)
    minimal_subscriber.destroy_node()
    rclpy.shutdown()

    # centralPublishers = CentralPublishers()
    # rclpy.spin(centralPublishers)
    # rclpy.shutdown()


