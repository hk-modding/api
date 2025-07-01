# /// script
# requires-python = ">=3.10"
# dependencies = [
#     "pandas",
#     "plotly",
# ]
# ///

from datetime import datetime
import json
import pandas as pd
import plotly.express as px
import sys
import tempfile

color_map = {
    "LoadScene": "#636EFA",
    "LoadAsset": "#636EFA",
    "GetPreloadObjects": "#Ef553b",
    "CleanPreload": "#00CC96",
    "LoadMods": "#AB63FA",
}

temp_dir = tempfile.gettempdir()

with open(f"{temp_dir}/loadTimings.json", "r") as f:
    data = json.load(f)

if len(data) == 0:
    sys.exit(1)
    
for value in data:
    value["Diff"] = round(value["End"] - value["Start"], 6)
    value["Start"] = datetime.fromtimestamp(value["Start"])
    value["End"] = datetime.fromtimestamp(value["End"])
    name = value["Name"]
    value["Color"] = int(name) % 2 if name.isdigit() else name
df = pd.DataFrame(data)

fig = px.timeline(df, x_start="Start", x_end="End", y="Context", color="Color", hover_data=["Context", "Name", "Diff"],
                  color_discrete_map=color_map)
fig.update_layout(
    xaxis=dict(title="Timestamp", tickformat="%S.%L"),
    yaxis=dict(title="", autorange="reversed", showticklabels=False),
)
fig.for_each_trace(lambda trace: trace.update(text=None) if trace.name in ["1","2","3","LoadScene"] else None)

fig.update_layout(dragmode="pan")
fig.show(config={"scrollZoom": True})