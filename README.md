# Off-Axis Camera, by Clément "CFaz" Fazilleau

# Overview

This project is a unity package for a simple and lightweight OffAxisCamera, based on [this excellent article](https://medium.com/try-creative-tech/off-axis-projection-in-unity-1572d826541e) from Michel de Brisis and the paper ["Generalized Perspective Projection"](http://160592857366.free.fr/joe/ebooks/ShareData/Generalized%20Perspective%20Projection.pdf) by Robert Kooima

### __Compatible with all render pipelines.__

# Features

This package allows for simple and fast creation of cameras with off-axis projection, to modify the camera perspective.

![image](https://user-images.githubusercontent.com/35767293/174667069-65a2ca11-3ae7-4d94-a31e-1ec47d7e4b1e.png)

You only have to add this component to your camera, then set a plane size and camera position.
All those properties are accessible at runtime, and the code is commented and accessible.

The custom tool also allows to edit the projection plane size and camera POV position.

# Installation

- In unity 2019+, go to Package Manager (Window/Package Manager).
- Click the "+" button, then choose "Add package from git URL..."
- Add "https://github.com/cfazilleau/OffAxisCamera" to the text field and press enter or click "Add".

# Properties

The accessible Properties of the OffAxisCamera component are the following:

```cs
/// Camera attached to this component, available after Awake().
public Camera Camera;

/// Size of the projection plane.
public Vector2 PlaneSize

/// Point of view plane rect in local coordinates, centered on the camera position.
/// Setting this will add the new rect center value to the camera position.
public Rect PlaneRect

/// Clamp near plane of the attached camera to the projection plane.
public bool UseProjectionAsNearPlane

/// Point Of View of the camera in world coordinates.
public Vector3 PointOfView

/// Point Of View of the camera in local coordinates.
public Vector3 PointOfViewLocal
```

----

![Unity_nINzpL8jpu](https://user-images.githubusercontent.com/35767293/174666001-e6cd3cba-750f-4715-be78-d9a717603b62.gif)
