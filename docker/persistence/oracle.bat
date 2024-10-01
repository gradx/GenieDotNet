docker volume create oracle-storage

docker run --name oracle ^
-p 1521:1521 ^
-e ORACLE_PWD=Test123 ^
-v oracle-storage:/opt/oracle/oradata ^
container-registry.oracle.com/database/free:latest