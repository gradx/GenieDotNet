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
import pandas
from shapely.geometry import box
import shapely.geometry
import duckdb

buildings = r"overture\buildings_usa.parquet"
schools = r"overture\schools.parquet"

duckdb.sql(f'''
        COPY (
                SELECT 
                    *, 
                    --REPLACE (ST_GeomFromWKB(geometry) as geometry), 
                    --CAST(categories.main AS VARCHAR) as main_category,
                    --CAST(categories.alternate AS VARCHAR) as alternate_category 
                    FROM read_parquet('{buildings}', filename=true, hive_partitioning=1)
                 WHERE class ILIKE '%school%' OR subtype ILIKE '%education%' OR subtype ILIKE '%education%'
        ) TO '{schools}' (FORMAT 'parquet', CODEC 'zstd')
''')

schoolsPQ = pandas.read_parquet(schools)
print(schoolsPQ.columns.values.tolist())

listFixed = []

def fix_row(row):
    listFixed.append(shapely.from_wkb(row['geometry']))
    #listFixed.append(b.oriented_envelope)
    #listFixed.append(json.dumps(b))


schoolsPQ.apply(fix_row, axis=1)

print(schoolsPQ.iloc[0])

# ['id', 'geometry', 'bbox', 'version', 'update_time', 'sources', 'names', 'categories', 'confidence', 'websites', 'socials', 'emails', 'phones', 'brand', 'addresses', 'filename', 'theme', 'type', 'main_category', 'alternate_category']

#listShapely.append(shapely.from_geojson(geojson[0]))
#d = {'channel': listChannels, 'content': listContent, 'geoJson': listGeoJson, 'geoParquet': listShapely}
gdf = geopandas.GeoDataFrame({'geometry': listFixed}, crs="EPSG:4326", geometry="geometry")
gdf.to_parquet(r"C:\temp\overture\school2.parquet")