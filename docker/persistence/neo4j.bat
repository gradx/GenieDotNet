docker run ^
    --restart always ^
    --publish=7474:7474 --publish=7687:7687 ^
    neo4j:5.23.0