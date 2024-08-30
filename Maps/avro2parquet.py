import avro.schema
import io
from avro.io import DatumReader, BinaryDecoder
import os
import pyarrow as pa
import geopandas
import geoarrow.pyarrow as ga
from geoarrow.pyarrow import io as ga_io
import geoarrow.pandas as _
import numpy as np
import shapely
import json

schemaPath = r"C:\temp\logs\AvroOneOf.avsc"
filePath = r"C:\temp\logs\2024-07-19_000.log"

schema = avro.schema.parse(open(schemaPath).read())
reader = DatumReader(schema)
fileSize = os.path.getsize(filePath)

listChannels = []
listContent = []
listGeoJson = []
listShapely = []

decoder = BinaryDecoder(io.BufferedReader(open(filePath, "rb")))
while decoder._reader.tell() < fileSize:
    content = reader.read(decoder)
    # geojson is not preserved
    listContent.append(json.dumps(content))
    
    # isolate and store geojson separately for type detection (and preservation)
    chanReq = content["ChannelRequest"]
    listChannels.append(chanReq["Id"])
    geojson = chanReq["Channel"]["Communications"][0]["CommunicationIdentity"]["GeographicLocation"]["GeoJsonLocation"]["Features"]
    listGeoJson.append(geojson[0])
    listShapely.append(shapely.from_geojson(geojson[0]))


print(decoder._reader.tell())

# Geopandas compatible with QGIS
d = {'channel': listChannels, 'content': listContent, 'geoJson': listGeoJson, 'geoParquet': listShapely}
gdf = geopandas.GeoDataFrame(d, crs="EPSG:4326", geometry="geoParquet")
gdf.to_parquet(r"C:\temp\Geopandas.parquet")