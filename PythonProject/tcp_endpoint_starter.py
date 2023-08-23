import socket

import rclpy
from rclpy.node import Node
from ros_tcp_endpoint import TcpServer
from std_msgs.msg import Float32MultiArray
from sensor_msgs.msg import Image
import cv2
from tools.cv2ROS import *


class CentralPublishers(Node):
    def __init__(self):
        super().__init__('centralPublisher')
        self.US_publisher=self.create_publisher(Image, 'US_images', 1)
        timer_period = 1/10.  # seconds
        self.timer = self.create_timer(timer_period, self.timer_callback)
        self.i = 0

    def timer_callback(self):
        cv2_image=cv2.imread(f'./data/{self.i%20}.png')
        msg = cv2_to_imgmsg(cv2_image)
        self.US_publisher.publish(msg)
        self.i += 1
        print(f"published {self.i}")


if __name__=='__main__':

    rclpy.init()
    tcp_server = TcpServer("UnityEndpoint", tcp_ip='0.0.0.0', tcp_port=10000)
    # tcp_server.subscribers_table
    tcp_server.start()


    tcp_server.setup_executor()

    tcp_server.destroy_nodes()
    rclpy.shutdown()
