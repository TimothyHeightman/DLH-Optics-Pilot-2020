// BACK-END File Structure

The BACKEND folder will be where we keep working ideas and development for the back end

Important to use the following guidelines so 
that we avoid merge conflicts during development

1. Within the BACKEND folder - create your OWN folder for each individual task.
	Example: BACKEND/LaserSource/
			 BACKEND/DiffractionGrating/
			 BACKEND/RayPropagation/
			 ...

2. Within this folder. Create any subfolders you may need to organise that task.
	Example: BACKEND/DiffractionGrating/Meshes
			 BACKEND/DiffractionGrating/Materials
			 BACKEND/DiffractionGrating/Scenes
			 BACKEND/DiffractionGrating/Scripts
			 ...

3. When you are staging changes to the repository, only include the folders you
   have created/changed - and exclude any other unnecessary unity packages / metadata

If we follow these guidelines, then every time you commit changes
on the GIT repo - those changes will all be self-contained and will
not clash with other peoples work.