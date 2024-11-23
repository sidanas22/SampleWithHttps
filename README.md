```Dockerfile
docker compose --env-file ./docker.env.dev -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.dev.debug.yml -p sampleproject --ansi never up --build --remove-orphans -d
```

```Dockerfile
docker compose -p sampleproject down
```