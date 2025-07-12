docker pull redis

docker run --name my-redis -p 6379:6379 -d redis

If you want to persist data:
docker run --name my-redis -p 6379:6379 -v redis-data:/data -d redis

name my-redis: Names the container.

-p 6379:6379: Maps Redis default port.

-d: Runs in detached mode.
