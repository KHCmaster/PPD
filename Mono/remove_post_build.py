#!/usr/bin/python

import re

def remove_post_build(path):
  with open(path) as f:
    data = f.read()
    data = re.sub(r'<PostBuildEvent>.*</PostBuildEvent>', '<PostBuildEvent></PostBuildEvent>', data, flags = re.M | re.S)
  with open(path, 'w') as f:
    f.write(data)

remove_post_build('../Win/FlowScriptEngineBasic/FlowScriptEngineBasic.csproj')
remove_post_build('../Win/FlowScriptEngineBasicExtension/FlowScriptEngineBasicExtension.csproj')
remove_post_build('../Win/FlowScriptEngineConsole/FlowScriptEngineConsole.csproj')
remove_post_build('../Win/FlowScriptEngineData/FlowScriptEngineData.csproj')
