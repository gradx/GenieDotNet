docker volume create db2-storage

docker run -itd --name mydb2 --privileged=true -p 50000:50000 -e LICENSE=accept -e DB2INST1_PASSWORD=genie_in_a_bottle -e DBNAME=testdb -v db2-storage:/database ibmcom/db2