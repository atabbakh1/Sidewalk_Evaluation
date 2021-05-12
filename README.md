# Sidewalk_Evaluation
##Evaluate sidewalk human traffic based on site factors

![Screenshot](https://github.com/atabbakh1/Sidewalk_Evaluation/blob/master/Sidewalk_Evaluation/Documentation/Capture.jpg)
This plugin is a WIP.
Evaluating the potential population of sidewalks in an urban contexts - such as NYC - involves numerous factor in which some of them can be unpredictable. This plugin provides a close estimation of population numbers - per sidewalk instance - based on certain requirements that may exist in the region of interest.


##How to use:
To use this plugin copy the *Sidewalk_Evaluation.gha* file from the *Testing* folder to your Grasshopper component folder.
<details>
  
  <summary>Components</summary>
  
  ## NYC Trees
  The Trees component takes care of parsing the CSV data provided. Users only need to specify the location of the file, and the corresponding data columns for X, Y, DBH, and     Borough. Trees will be loaded only for the specified NYC borough to reduce processing time needed.
  ![Screenshot](https://github.com/atabbakh1/Sidewalk_Evaluation/blob/master/Sidewalk_Evaluation/Documentation/Canvas at 11;48;17.png)
  
  ## Prepare Sidewalks 
  The Prep Sidewalks component will do the following:
- Check if curves are closed
- Check if curves are planar
- Exclude buildings outside the specified region
- Boolean union building footprints
- Boolean union nested sidewalk instances (this might need to be revised)

If users are confident their input sidewalk/building curves satisfy the above requirements, they can feel free to skip this step.
![Screenshot](https://github.com/atabbakh1/Sidewalk_Evaluation/blob/master/Sidewalk_Evaluation/Documentation/Canvas at 12;02;03.png)

  ## Evaluate
  The Evaluate component takes in all prepared parameters in addition to coloration, capacity utilization, and subway influence which are variable for users to quickly iterate through.
This component currently outputs three trees with equal branch count representing:
  - Sidewalks data tree
  - Building data tree (Items null if no buildings)
  - Counts data tree

Evaluate component will draw a preview of the sidewalk outlines in the Rhino viewport in addition to displaying the population values for each instance using user defined colors.
![Screenshot](https://github.com/atabbakh1/Sidewalk_Evaluation/blob/master/Sidewalk_Evaluation/Documentation/Canvas at 12;08;07.png)

</details>
