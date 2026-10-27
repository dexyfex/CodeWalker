# Game files, as described by the original CodeWalker

This is the "Regarding game files" section of the README of CodeWalker by dexyfex
(the C# tool this project is a port of), kept here because it explains the
formats the Rust crates read. Text is the original author's.

The PC GTAV world is stored in the `RPF` archives in many different file formats. As expected, some formats are used for storing rendering-related content, for example the textures and 3d models, while other formats are used for storing game and engine related data.

The main formats when it comes to rendering GTAV content are:
`.ytd` - Texture Dictionary - Stores texture data in a DirectX format convenient for loading to the GPU. 
`.ydr` - Drawable - Contains a single asset's 3d model. Can contain a Texture Dictionary, and up to 4 LODs of a model.
`.ydd` - Drawable Dictionary - A collection of Drawables packed into a single file.
`.yft` - Fragment - Contains a Drawable, along with other metadata for example physics data.

The content Assets are pieced together to create the GTAV world via MapTypes (Archetypes) and MapData (Entity placements). At a high level, Archetypes define objects that are placeable, and Entities define where those objects are placed to make up the world. The collision mesh data for the world is stored in Bounds files.

### The formats for these are:
`.ytyp` - MapTypes - Contains a group of MapTypes (Archetypes), each defining an object that could be placed.
`.ymap` - MapData - Contains placements of Archetypes, each defining an Entity in the world.
`.ybn` - Bounds - Contains collision mesh / bounding data for pieces of the world.

The EntityData contained within the MapData (`.ymap`) files forms the LOD hierarchy. This hierarchy is arranged such that the lowest detail version of the world, at the root of the hierarchy, is represented by a small number of large models that can all be rendered simultaneously to draw the world at a great distance. The next branch in the hierarchy splits each of these large models into a group of smaller objects, each represented in a higher detail than the previous level. This pattern is continued for up to 6 levels of detail. When rendering the world, the correct level of detail for each branch in the hierarchy needs to be determined, as obviously the highest detail objects cannot all be rendered at once due to limited computing resources.

In CodeWalker, This is done by recursing the LOD tree from the roots, checking how far away from the camera the node's Entity is. If it is below a certain value, then the current level is used, otherwise it moves to the next higher level, depending on the LOD distance setting. (In the Ymap view, the highest LOD, ORPHANHD, is not rendered by default. The ORPHANHD entities can often be manually rendered by specifying the correct `strm ymap` file for the area in question in the `ymap` text box. The `strm ymap` name can often be found by mouse-selecting a high detail object in the area and noting what `ymap` the entity is contained in, in the selection details panel.)