docker volume create roach-single

docker run -d ^
  --env COCKROACH_DATABASE=genie ^
  --env COCKROACH_USER=admin ^
  --env COCKROACH_PASSWORD=password ^
  --name=roach-single ^
  --hostname=roach-single ^
  -p 26257:26257 ^
  -p 8080:8080 ^
  -v "roach-single:/cockroach/cockroach-data"  ^
  cockroachdb/cockroach:v24.2.1 start-single-node ^
  --insecure ^
  --http-addr=roach-single:8080