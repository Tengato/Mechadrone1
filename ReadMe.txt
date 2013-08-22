This repository contains a WIP of my attempt to build a 3D game engine in C# for the XNA 4.0 platform.  My goal is to explore game engine problems and add support for features common to a first- or third–person 3D ‘outdoor’ game, eventually making small games using the engine.  Since it’s built on the XNA platform, I start out with a lot of data types created for me and a simplified interface to the graphics library.  I also got a free framework for the asset conditioning pipeline and runtime content resource manager.

You can run the engine demo on Windows by running \Drop\setup.exe

Some important files to look out for:
•	\Mechadrone1\Mechadrone1\Rendering\SceneManager.cs
•	\Mechadrone1\Mechadrone1\Gameplay\Game1Manager.cs
•	\Mechadrone1\Mechadrone1\Gameplay\GameObject.cs
•	\Mechadrone1\Mechadrone1Content\shaders\NormSpecSkinPhong.fx

Here are some of the main features I’ve developed:
•	Flexible level content loader (\Manifracture and ).  Using a custom XML asset type describing a level, the engine will instantiate a game object model consisting of classes that inherits from GameObject and implement any kind of gameplay behavior.
•	A basic rendering engine (\Mechadrone1\Mechadrone1\Rendering).  No fancy culling algorithms yet, but it does bind object material data to shader parameters.  I also have a simple implementation of shadow maps.
•	An assortment of common shaders (\Mechadrone1\Mechadrone1Content\shaders). The one I use for the soldier uses skinning, normal mapping, and Phong model with 3 directional lights.  There is a variant that adds an environment map.  The terrain shader blends multiple diffuse and normal textures, and receives shadows.
•	Custom content importer for model assets (\SkelematorPipeline). The default XNA behavior didn’t include all the material and animation data I needed in my model asset files, so now they are accompanied by an XML file with the extra info and the SkelematorPipeline classes merge the data into the runtime types.
•	Custom heightmap processor (\Mechadrone1\SkelematorPipeline\Terrain).  I added a Gaussian blur step to make the vertices smoother since an 8 bpp heightmap can be blocky.

Some other parts that I did not create were:
•	The BEPU 3D physics library (http://bepuphysics.codeplex.com/)
•	camera movement is from the XNA ShipGame sample (http://xbox.create.msdn.com/en-US/education/catalog/starterkit/shipgame)
•	screen transition system is from the XNA Game State Management sample (http://xbox.create.msdn.com/en-US/education/catalog/sample/game_state_management)
•	soldier model was purchased from http://www.dexsoft-games.com/

