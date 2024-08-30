import geopandas
import pandas

# https://public.opendatasoft.com/explore/?sort=modified&refine.keyword=usa

# name, iso3, continent, region, 
world = geopandas.read_parquet(r"world-administrative-boundaries.parquet")
world = world.drop('geo_point_2d', axis=1)

world.to_parquet(r"world-administrative-boundaries-cleansed.parquet")

# ste_code, ste_name
state = geopandas.read_parquet(r"georef-united-states-of-america-state.parquet")
state = state.drop('geo_point_2d', axis=1)
#print(state.columns.values.tolist())
state.to_parquet(r"georef-united-states-of-america-state-cleansed.parquet")

# (ste_code, ste_name) coty_code, coty_name, coty_name_long
county = geopandas.read_parquet(r"georef-united-states-of-america-county.parquet")
county = county.drop('geo_point_2d', axis=1)
print(county.columns.values.tolist())
county.to_parquet(r"georef-united-states-of-america-county-cleansed.parquet")

# (ste_code, ste_name, coty_code, coty_name) zcta5_code, zcta5_name
zcta = geopandas.read_parquet(r"georef-united-states-of-america-zcta5.parquet")
zcta = zcta.drop('geo_point_2d', axis=1)
zcta.to_parquet(r"georef-united-states-of-america-zcta5-cleansed.parquet")

# (ste_code, ste_name, coty_code, coty_name) pla_code, pla_name, pla_name_long
place = geopandas.read_parquet(r"georef-united-states-of-america-place.parquet")
place = place.drop('geo_point_2d', axis=1)
place.to_parquet(r"georef-united-states-of-america-place-cleansed.parquet")

schools = pandas.read_parquet(r"schools.parquet")
schools = schools.drop('bbox', axis=1)
print(schools.columns.values.tolist())