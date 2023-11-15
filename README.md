# Collection access roslyn analyzer
My pet project used for studying Roslyn compilator API for static code analysis.

Current version is rather strict and raises appropriate warning when two conditions are met: 
1. There is element named "Params" defined as expression-bodied get
i.e.
```cs
public IDictionary<string, string> Params => new Dictionary<string, string()
{
  {"0", string.Empty}
};
```

2. The file contains element modification via element access
```cs
Params["0"] = "XYZ"; //Warning should be raised here
```


Roslyn analyser template source: https://github.com/ryzngard/VSToolBoxAnalyzer
