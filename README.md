# 2d_terrain

A large-scale procedurally-generated side-view terrain mesh in Unity2D.

One of the primary goals of this project is to store a large number of voxels without using too much RAM. This is accomplished by storing the data in a quadtree structure, allowing large square sections of identical tiles to be stored with as much memory as a single tile, as can be seen in the wireframe:

![1](/Images/Unity_2019-09-14_00-50-09.png)

This allows for fairly large worlds to be generated and kept in memory with great performance:

![2](/Images/Unity_2019-09-14_05-17-06.png)

Below: the same world as above, zoomed into a specific area. Note that the left-hand view of both images is at the same zoom level. While the entire world shown above is still loaded, the below image demonstrates that the QuadTree structure the world is stored in makes it very easy and efficient to only create a mesh for those tiles that are within the camera's view.

![3](/Images/Unity_2019-09-14_05-16-15.png)
