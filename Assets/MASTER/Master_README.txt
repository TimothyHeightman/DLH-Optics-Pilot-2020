// MASTER File Structure

The MASTER folder will be where we combine our work to develop working demo scenes

Important to use the following guidelines so 
that we avoid merge conflicts during development

1. Within the MASTER folder - create your OWN folder for each individual task.
	Example: MASTER/CheckpointA/
			 MASTER/CheckpointB/
			 MASTER/CheckpointC/
			 ...

2. Within this folder. Create any subfolders you may need to organise that task.
	Example: MASTER/CheckpointA/Scenes
			 MASTER/CheckpointA/Prefabs
			 MASTER/CheckpointA/Materials
			 ...

3. When you are staging changes to the repository, only include the folders you
   have created/changed - and exclude any other unnecessary unity packages / metadata

If we follow these guidelines, then every time you stage changes
on the GIT repo - those changes will all be self-contained and will
not clash with other peoples work.