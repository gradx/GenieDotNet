docker run -d -v "aero:/opt/aerospike/etc/"  ^ --name aerospike -p 3000-3002:3000-3002 aerospike:ce-7.1.0.6 --config-file /opt/aerospike/etc/aerospike.conf
