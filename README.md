# MGS .geoms Parser

A naïve parser of MGS:TPP .geoms files that can extract the geometry contained within to an .obj file. There are additional data structures in the file that are currently unknown and hence ignored by this parser.

## Usage

`GeomsParser.exe \path\to\file.geoms`

If the file is parsed successfully, a file called `geoms.obj` will be created in the current working directory. This can be opened in any 3d modelling software for inspection.

## Observations

It seems the `_common_paths_fox2_0_geoprim_win.geoms` files contain the most geometry. It seems as though they contain geometry that defines an outer boundary of a level, along with bounding boxes for objects within the level that possibly define whether they are climbable/interactable.

For example, here is a screenshot of the resulting .obj file from `cuba_common_path_fox2_0_geoprim_win.geoms`:

![cuba .obj screenshot](cuba_screenshot.png)

## Releases
Releases are automatically built and uploaded by GitHub actions when a tag is pushed to the repository. You can download the latest build [here](https://github.com/oldbanana12/GeomsParser/releases/latest/download/GeomsParserX64.ZIP)