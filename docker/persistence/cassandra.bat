docker run -p 7000:7000 -p 9042:9042  -e CASSANDRA_USER=admin -e CASSANDRA_PASSWORD=admin --name some-cassandra -d cassandra:latest
