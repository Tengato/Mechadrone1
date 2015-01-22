This repository contains a WIP of my attempt to build a 3D game engine in C#
for the XNA 4.0 platform. My goal is to explore game engine problems and add
support for features common to a first- or third–person 3D ‘outdoor’ game,
eventually making small games using the engine. Since it’s built on the XNA
platform, I start out with a lot of data types created for me and a simplified
interface to the graphics library. I also get a framework for the asset
conditioning pipeline and runtime content resource manager.

Some notable files to look out for:
•	\Mechadrone1\Mechadrone1\Rendering\SceneNodes\ImplicitBoundingBoxNode.cs
•	\Mechadrone1\Mechadrone1\Gameplay\ActorManager.cs
•	\Mechadrone1\Mechadrone1\ActorComponents\BipedControllerComponent.cs
•	\Mechadrone1\Mechadrone1Content\shaders\NormSpecSkinPhong.fx

Some of the main features I’ve developed:
•	Flexible level content loader (\Manifracture). Using a custom XML
asset type describing a level, the engine will instantiate a game object model
consisting of componentized Actor objects.
•	A basic rendering engine (\Mechadrone1\Mechadrone1\Rendering).  Uses a
scene graph to cull objects outside of the camera frustum.  Binds object
material data to shader parameters. Also uses a simple implementation of shadow
maps.
•	An assortment of common shaders (\Mechadrone1\Mechadrone1Content\shaders).
The one used for a typical character uses skinning, normal mapping, and an
irradiance map. The terrain shader blends multiple diffuse and normal textures,
and receives shadows.
•	Custom content importer for model assets (\SkelematorPipeline). The
default XNA behavior didn’t include all the material and animation data I needed
in my model asset files, so now they are accompanied by an XML file with the
extra info and the SkelematorPipeline classes merge the data into the runtime
types.
•	Custom heightmap processor (\Mechadrone1\SkelematorPipeline\Terrain).
I added a subdivision surface step to smooth the heightmap data since an 8 bpp
heightmap can be blocky.

Some other parts that I did not create or deserve credit were:
•	The background art for some of the screens was created by rich4rt and
chamoth143 and used without permission from deviantart.com. (This art would not
go into a final product that would be distributed.)
•	The BEPU 3D physics library (http://bepuphysics.codeplex.com/)
•	Designs for numerous systems were adapted from Microsoft's XNA sample projects
(http://xbox.create.msdn.com/en-US/education/catalog)
•	Soldier model was purchased from http://www.dexsoft-games.com/

