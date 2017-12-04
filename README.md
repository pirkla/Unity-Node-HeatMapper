# Unity-Node-HeatMapper
Node based heat mapping for unity

# Info
This is the second version of the Heat Mapper I've made.  This one uses a Dictionary to look up the location of the nearest node from a given Vector2.  The old one used spherecasters and that was just plain dumb.  I've used this with 20,000 nodes in a simple demo and had a frame rate of about 100 fps according to the graphics stats.  I made it so that the AI could find the location of concentrations of other units, and the graphic portion for testing of the system. 

# Features
-Multiple layers on the heat map to accomodate different teams or objects.

-Generates image of the heat map from the nodes.

-Function to find the heat of an area around each node.  Only partially implemented, not optimized.

# Setup
Create an empty GameObject and add the HeatMap script.  Choose your desired interval(distance between nodes) width and height.  If you want to see an image of the heatmap create a canvas with a raw image and drop it into the Hea Map Raw Image field.  
Hit the CreateGrid button and wait for the grid to generate.  To view the grid after generation press the Show Boxes button.

Add the HeatMapAffector script to any GameObject you want to affect the heat map.  
