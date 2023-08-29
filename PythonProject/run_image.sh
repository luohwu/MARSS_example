#!/bin/bash

docker run -it -p 10000:10000 --gpus all --ipc=host --ulimit memlock=-1 --ulimit stack=67108864 local:latest
