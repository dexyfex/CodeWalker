CodeWalker by dexyfex
----------------------

This program is for viewing the contents of GTAV RPF archives.


Requirements:
--------------
- PC version of GTAV
- DirectX 11 and Shader Model 4.0 capable graphics
- Windows 7 and above, x64 processor
- 4GB RAM (8+ recommended)
- .NET framework 4.5 or newer (download from Microsoft..?)



Using the app:
---------------
On first startup, the app will prompt to browse for the GTAV game folder. If you have the Steam version installed
in the default location (C:\Program Files (x86)\Steam\SteamApps\common\Grand Theft Auto V), then this step will
be skipped automatically.

The World View will load by default. It will take a while to load.
Use the WASD keys to move the camera.
Drag the left mouse button to rotate the view.
Use the mouse wheel to zoom in/out, and change the movement speed. (Zoom in = slower motion)
XBox controller input is also supported.
The Toolbox can be shown by clicking the "<<" button in the top right-hand corner of the screen.
T opens the toolbar.

First-person mode can be activated with the P key, or by pressing the Start button on the XBox controller.
While in first-person mode, the left mouse button (or right trigger) will fire an egg.

Entities can be selected (with the right mouse button) by enabling the option on the Selection tab in the 
toolbox. The details of the selected entity, its archetype, and its drawable can be explored in the relevant
sub-tabs. (This option can also be activated with the arrow button on the toolbar)

When an entity is selected, E will switch to edit mode (or alternatively, edit mode can be activated by 
switching the Widget mode to anything other than Default). When in edit mode, Q will exit edit mode, W toggles 
the  position widget, E toggles rotation, and R toggles scale. Also when in edit mode, movement is still WSAD, 
but only while you're holding the left mouse button down, and not interacting with the widget.

Ctrl-Z and Ctrl-Y will Undo and Redo entity transformation (position/rotation/scale) actions.

The Project Window allows a CodeWalker project to be created (.cwproj), and files added to it. Editing 
entities while the Project Window is open will add the entity's .ymap to the current project. Ymap files can then 
be saved to disk, for use in a map mod. New ymap files can also be created, and entities can be added and removed.
Also supported for editing are .ynd files (traffic paths), trains .dat files (train tracks), and scenarios (.ymt).
(A full tutorial on making map mods is out of the scope of this readme.)

A full explanation of all the tools in this application is still on the to-do list!
The user is currently left to explore the options at their own peril.
Note some options may cause CodeWalker to crash, or otherwise stop working properly. Restart the program if this
happens!
Also note that this program is a constant work in progress, so bugs and crashes are to be expected.
Some parts of the world do not yet render correctly, but expect updates in the future to fix these issues.


Menu mode:
----------
The app can also be started with a main menu instead of loading the world view. This can be useful for situations
where the world view is not needed, and the world loading can be avoided.
To activate the menu mode, run CodeWalker with the 'menu' command line argument, e.g:
CodeWalker.exe menu


Explorer mode:
--------------
The app can be started with the 'explorer' command line argument. This displays an interface much like OpenIV,
with a Windows-Explorer style interface for browsing the game's .rpf archives. Double-click on files to open them.
Viewers for most file types are available, but hex view will be shown as a fallback.
To activate the explorer mode, run the command:
CodeWalker.exe explorer
Alternatively, run the CodeWalker Explorer batch file in the program's directory.


Main Toolbar:
-------------
The main toolbar is used to access most of the editing features in CodeWalker. Shortcuts for new, open and
create files are provided. The selection mode can be changed with the "pointer" button. Move, rotate and
scale buttons provide access to the different editing widget modes.
Other shortcuts on the toolbar include buttons to open the Selection Info window, and the Project window.
See the tooltips on the toolbar items for hints.


Project Window:
---------------
The project window is the starting point for editing files in CodeWalker. Project files can be created,
and files can be added to them. It is recommended to create and save a project file before adding files
to be edited and saved.
The tree view displays the files in the current project, and their contents.


YMAP editing:
-------------
New YMAP files can be created via the project window, and existing ymap files can be edited.
To edit an existing single player YMAP, first change codewalker DLC level to patchday2ng, and enable DLC.
Open the toolbar, and enable Entity selection mode. Enable the Move widget with the toolbar Move button.
Open the project window with the toolbar button. Changes made while the project window is open are
automatically added to the project.
Select an entity to edit by right clicking when the entity is moused over, and its bounding box shown in
white. Move, rotate and/or scale the selected entity with the widget. When the first change is made, the
entity's YMAP will be added to the current project. If no project is open, a new one will be created.
The edited YMAP file can be saved to the drive using the File menu in the project window.
After saving the file, it needs to be added into the mods folder. Using OpenIV, find the existing YMAP
file using the search function (note: the correct path for the edited YMAP can be found in the selection
info window in CodeWalker, when an entity is selected, look for YMap>RpfFileEntry in the selection info
property grid). Replace the edited YMAP into a copy of the correct archive in the /mods folder.
Newly created YMAPs can be added to DLC archives in the same manner.


Traffic Paths (YND) editing:
----------------------------
[TODO - write this!]


Train Tracks editing:
---------------------
[TODO - write this!]


Scenario Regions (YMT) editing:
-------------------------------
[TODO: write this!]
See https://youtu.be/U0nrVL44Fb4 - Scenario Editing Tutorial



Regarding game files: (FYI)
----------------------------

The PC GTAV world is stored in the RPF archives in many different file formats. As expected, some formats are
used for storing rendering-related content, for example the textures and 3d models, while other formats are used
for storing game and engine related data.

The main formats when it comes to rendering GTAV content are:

.ytd - Texture Dictionary - Stores texture data in a DirectX format convenient for loading to the GPU. 
.ydr - Drawable - Contains a single asset's 3d model. Can contain a Texture Dictionary, and up to 4 LODs of a model.
.ydd - Drawable Dictionary - A collection of Drawables packed into a single file.
.yft - Fragment - Contains a Drawable, along with other metadata for example physics data.


The content Assets are pieced together to create the GTAV world via MapTypes (Archetypes) and MapData
(Entity placements). At a high level, Archeypes define objects that are placeable, and Entities define
where those objects are placed to make up the world. The collision mesh data for the world is stored in Bounds 
files.
The formats for these are:

.ytyp - MapTypes - Contains a group of MapTypes (Archetypes), each defining an object that could be placed.
.ymap - MapData - Contains placements of Archetypes, each defining an Entity in the world.
.ybn - Bounds - Contains collision mesh / bounding data for pieces of the world.


The EntityData contained within the MapData (.ymap) files forms the LOD hierarchy. This heierarchy is arranged
such that the lowest detail version of the world, at the root of the hierarchy, is represented by a small number
of large models that can all be rendered simultaneously to draw the world at a great distance. The next branch
in the hierarchy splits each of these large models into a group of smaller objects, each represented in a higher
detail than the previous level. This pattern is continued for up to 6 levels of detail. When rendering the world,
the correct level of detail for each branch in the hierarchy needs to be determined, as obviously the highest
detail objects cannot all be rendered at once due to limited computing resources.

In CodeWalker, This is done by recursing the LOD tree from the roots, checking how far away from the camera the 
node's Entity is. If it is below a certain value, then the current level is used, otherwise it moves to the
next higher level, depending on the LOD distance setting.
(In the Ymap view, the highest LOD, ORPHANHD, is not rendered by default. The ORPHANHD entities can often be
manually renderd by specifying the correct _strm_ ymap file for the area in question in the ymap text box. The 
_strm ymap name can often be found by mouse-selecting a high detail object in the area and noting what ymap the
Entity is contained in, in the selection details panel.)

