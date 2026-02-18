# Matlab fixture authoring template

Copy this template when adding a new Matlab-exported fixture:

```json
{
  "name": "matlab_case_name",
  "input": {
    "vertices": [
      [0.0, 0.0, 0.0]
    ],
    "faces": [
      [0, 1, 2]
    ]
  },
  "expected": {
    "tetraCount": 1,
    "tetraVolume": 0.0,
    "volumeTolerance": 1e-8,
    "exactTetrahedra": [
      [0, 1, 2, 3]
    ]
  },
  "options": {
    "checkInput": true,
    "autoResolveIntersections": true,
    "failOnSelfIntersections": true,
    "verbose": false,
    "planeDistanceTolerance": 1e-10,
    "epsilon": 1e-8
  }
}
```

For non-deterministic triangulation outcomes, omit `tetraCount` and `exactTetrahedra` and keep only volume assertions.

`checkSelfIntersections` is still accepted as a legacy shorthand and maps to both intersection flags when explicit values are omitted.
