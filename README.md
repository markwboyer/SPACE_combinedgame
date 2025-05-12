# SPACE_CombinedGame

SPACE (Supervisory Piloting of Autonomously Crewed Explorers) Research Game\
\
This game is developed by Mark Boyer at CU-Boulder as part of his PhD research into Human-Autonomy Teaming.  This is for research purposes only.\
\
The object of the game is for the "MOUSE" (Mars Observation Unmanned Science Explorer) rover to visit as many scientific sites as possible to maximize Autonomy Task (AT) Score.\
\
AT Score is calculated as ATScore = 100 x NumberGoalsVisited - 2 x TimeDriving - 5 x TimeInHazard. \  
\
To maximize score, players need to balance time vs. time in hazards to visit as many scientific sites as possible.\
\
There are 3 "AI Agents" that do the path planning.  One agent ("default") plans an optimal route using A*.  The other are suboptimal routes that a variations of hazard score weighting. The "zero cost" agent
acts as if the hazards have zero cost, so this agent calculates the fastest route and does not avoid any hazards.  The "high cost" agent acts as if the hazards have a very high cost (500,000 vs. default=5),
so it generates routes that avoids hazards at all costs, even going far out of its way.  In this way, the user is presented with 3 "AI Agents" to decide for route planning purposes.

# **Scenes**

## StartMenu
Updates player info (UserID, Trial Number), configuration (see below), and whether to save results to GitHub or not.  Should be 1 UserID per person.\
  Configs:  A - Manual Route only\
            B - No explanation condition\
            C - Global goal explanation only\
            D - Contrastive explanation only\
            E - Deductive explanation only\
            F - Global + contrastive\
            G - Global + deductive\
            H - Deductive + contrastive\
            I - All explanations available\
  Press start to transition to GameScene and begin gameplay.\
  \
GameScene - Contains all elements of the game play, including post-round surveys.\
\
**Main Elements of Game**\
The three primary scripts that control the game are: RoverDriving, AstartPathfinder, and BuildMap.

## **BuildMap**
This script is responsible for generating the elements of the map and game (boulders, hazards, goals, grid).\
\
The grid defines how each object located within the map, with min/max X and Y being defined by trial and error.  Those can be adjusted if playing on a different sized screen.  The cellsize determines how big each cell 
is for the AStar search (smaller cells = finer grid = more points to search over).  Cell size of 1.0 is what I've used (wouldn't change).  The grid converts the world position into a Node at each grid location, which
allows for storing information used to calculate the paths (Boulder? , 2D worldPosition, gridX, gridY, hazardWeight at that grid point).\
\
The boulders, hazards, and goals are each spawned on their own method.  The boulders have a certain buffer around the edges to prevent going outside the map.  The hazards are also distributed randomly within confines of the
map by adding a percentage buffer (80% in x, 60% in y).  Adjust within method if wanting to change.  Hazards also have a scaling size factor that is also stored in a vector called hazardScaleArray.  The scaling factor can 
be changed in the Unity editor (currently min = 20, max = 60).  The goals are spawned by spreading them evenly in the X and Y directions to ensure there are goals spread apart.  If there are 4 goals, then each will be spread
randomly within their own quartile in both x and y.  If a goal is too close to a boulder, it'll be moved until it's no longer a conflict with the boulder.  Initially, I have it generating 4 goals, then subsequently 2 but may
change that to 3 if runtime permits.\
\
A couple other random helping methods as well that should be self-explanatory.

## **RoverDriving**
This is the main script that runs most of the game.  Most of the parameters to modify are at the top of the Unity editor (hazard costs, speed, AT factors, SAGAT start times).  See file path for saving.\
\
The overall methodology of the path creation is as follows:  The "paths" are List<Node>, where each entry is a Node which contains the world position.  The path usually contains 100-400 nodes, which the rover uses to move from one
node to the next in a direct line.  The cost is calculated separately using calculations in AStar.  The paths should avoid all boulders from the path planning process, but not necessarily hazards.\
\
Initialization of all the buttons in start and precalculating all the routes and costs between the goals (in AStar).  More on that in AStar.\
\
Configure game: This method sets which buttons and displays are active depending on which condition is selected in Start.\
\
**TogglePause**: This controls whether the rover is trying to move.  The reasons the rover CAN'T move: isPaused condition is met (button is clicked to paused condition), there is a warning to one of the warnings in the warning panel, or
there is no route confirmed.  This should prevent from getting into a situation where the rover is trying to drive without a route selected.  There is also an associated square that turns red when the rover isn't moving.  Also, if 
battery goes <10% the rover stops.  If the rover can drive and the button is pushed (PUSH TO GO!), it'll start the movement coroutine MoveAlongPath which moves the rover along the path.  There's a separate method with a 0.2 second
delay to prevent accidentally double clicking.\
\
ToggleContrastViews: Might delete, but this is to be able to toggle the contrastive view on and off.  Doesn't work well.\
\
**GenerateAltRoutesOnClick2**: This is the primary method that starts the route creation process.  Overall, it gets the rovers position, generates paths from the current position to each goal, adds those to the dictionaries, then computes
the lowest cost route using a brute force search through all possible combinations of routes (ex: start->2->3->1->4, start->3->2->4->1, etc) and picks the lowest cost.  The only factor that changes the cost is the hazard cost, which changes
how directly the rover goes between goals and how much it avoids hazards.  In the case of "No Explanations" (Condition B), it'll pick a random number 0 (low cost), 1 (default), 2 (high) for which route to display first, and then if they want a different route it'll
run the "route slot machine" for 4 seconds, then randomly pick another one randomly to display.  For generating the paths, it uses a coroutine since it generally takes some time to calculate the routes.\
\
CheckAndDisplayRoutes: This helper method makes sure that each of the routes has been fully calculated before displaying all 3 routes (if contrastive view enabled) and associated buttons.\
\
AddBestPossible: These methods calculate the best possible score to reference for the player performance.  This is done by calculating the optimal path (default hazard costs), then calculating the battery it would take to traverse that path, then subtracting
that battery from the "bestPossibleBattery".  This would be if the best route was chosen with zero down time and zero stops.  The player's performance relative to this bar is how ATScore is actually calculated as a fraction (ie ATScore = 0.87).  Above .90 is
about the best I can do when purely focused on the AT task only.  **NEEDS MORE REFINEMENT ON BATTERY USAGE**\
\
ConfirmRoute: This is a check step with associated button to prevent rover from getting into a situation without a path.  Makes some buttons disappear before can click the "PUSH TO GO!" button to start it driving.\
\
SetRouteXXXXCost: This is the step to set the actual path for the rover to the one clicked by respective button.  Hides other routes and displays only chosen route, but doesn't clear other routes.\
\
OnRouteChosen: Helper method to log when route is chosen and confirmed.\
\
ClearPathsOnClick: This does more than just clear the paths when the button is clicked.  This method is referenced a few other times to clear the actual paths (not just the linerenderer visual paths) and reset the buttons.\
\
DisplayAltRoute: This method is for generating the visual paths in colors corresponding to each hazard cost.  Added a slight x offset (1.5f) so that when multiple paths are displayed together you can see them. Also made slightly transparent.\
\
DisplayPath: This just displays the path once chosen, along with the stats.  Could be combined with DisplayAltRoute....\
\
**MoveAlongPath**: This is the most important method for actually moving the rover along the chosen route.  The main While loop uses the variable isMoving (set in TogglePause) and targetIndex (how many nodes along the chosen path).  The location
of the rover is taken from the RigidBody2D and then it's moved directly towards the next node in the list using the targetIndex.  Once the rigidbody gets within a certain distance of a goal (10.0f since sometimes the path isn't perfect), it counts
as "visiting" the goal and logs it in the goalList.IsVisited=true and loggedGoals lists.  **This could definitely be improved and reduced**. A couple other checks along the way for conditions that would cause the rover to stop (driveWarning or SAGAT).
Once the rover visits all the goals (ie goalCount == totalGoals), then it'll pause the rover, clean up all the routes, spawn new goals, precompute the paths between the new goals, and break the coroutine.\
\
FindPathToAllGoalsCoroutine: This method takes in current position and a hazard cost, checks which goals haven't been visited yet, calculates the starter route (ie from currentPosition to each goal), then computes the optimal path (ex: start->2->3->1->4, start->3->2->4->1, etc).  This is referenced several times since this is the overall method for searching through the possible combinations to find the optimal path (that method is FindOptimalPath).\
\
SetManualRoute: Method for setting the manual route.  See ManualRoute class for separate details on how it's created.  Simple: capture user clicks in order, then finds shortest path between points that avoids the boulders. This also captures the best
possible route for performance comparison.\
\
runSAGAT: **Possibly move to own class since it's a lot of text**.  This captures the necessary info to run the SAGAT questions mid-game.  3 levels of questions (level 1: perception, level 2: comprehension, level 3: prediction).  This will randomly pick one question
from each bank of (L1, L2, L3) of possible questions to display, and display those during the popup.\
\
Several methods for filling in the answers to SAGAT questions.  1st answer is always the correct answer, other 3 are randomised.  For questions that are categorical (true/false or warning/cautions), 1 correct and 3 incorrect answers easy to make.
For numerical questions, overall method is to set a min and max value, then divide into quartiles and pick a random incorrect answer in the quartiles that the correct answer is not in.  For 0-100 questions, this is easy (0-24, 25-49, 50-74, 75-100), but
still need to fine tune how big to make the upper/lower bounds for other questions and decide "how close is close enough" for incorrect SAGAT answers to make it a good test of comprehension.\
\
EndGameAndExportLogs: Method to export CSV of the various logs (clickLog=primary game play, performance, clicks, Warnings/Cautions; SAGAT = SAGAT questions; SAGATTrust = 2 trust questions during SAGAT; PostTrial = 12 question post-trial questionnaire including
TLX (6), trust (3), explainability (3)).  Also does the OptimalPerformance calcs: when EndGame is clicked, if there is "optimalBattery" remaining (ie the least possible battery used for the given goals), it'll generate more goals and then simulate more routes
to those goals until the "optimalBattery" is depleted.  Ex: real game user visited 8 goals before battery expired (10% remaining), but optimalBattery=32% will generate 2 more goals, calculate score and battery usage (ex: 12%), add the score to bestPossibleScore 
and subtract 32-12=20% remaining, then repeat until optimal battery <10% for end of route.  Not perfect but generates score higher than any person can achieve so will give a score 0-1.  Primary storage is to local drive for development but then will also 
have an option to upload to GitHub for backup storage.  **TO DO: DEVELOP WAY TO UPLOAD TO BIOASTRO MONGODB DATABASE INSTEAD**\
\
## **AstarPathfinder**
This class is the place where the pathfinding actually is done.  Overall, I'm transitioning to using Dictionaries for the various paths, which make it a lot simpler to access and store.  There are two primary types of dictionaries used (with 3 versions each
for the various hazard costs).  One Dict is for storing the List<Node> between goal X to goal Y (Dictionary<(X, Y), List<Node>>), which will have the full path between X and Y (usually a couple hundred Nodes).  The other is a dictionary of the cost to go
between X and Y (Dict<(X, Y), float>).  For all of these dictionaries, I use the goal numbers for X and Y (starting at 0), and then entries that are from current location to goal X are (-1, X), which captures the starter paths and costs.\
\
PrecomputePathsBetweenGoals: This method finds the paths and costs between all possible combinations of goals when new goals are spawned.  This is run during start and right after finding the last displayed goal since it's relatively computationally intensive.
Calculate both the forward and reverse paths (sometimes not the same).  Do this for low cost (0) and high hazard costs as well for the 3 agents.\
\
PreComputeFinalPaths: Same as above but only does the default cost since this is only used for calculating the idealscore post-round.\
\
UpdateAltRoutesandPaths3: This method finds the path and cost from currentPosition to each goal and fills in the dictionaries accordingly.\
\
UpdateAltRoutesAndPaths4: Same as 3, but only for the default cost to save time post-round calculating ideal score.\
\
FindOptimalPath4: Takes in the hazard cost and unvisited goals list and generates a List<Node> of the lowest cost path using FindLowestCostPath methods.  This method takes the higher-level best path (ex: start->2->3->1->4) and then splices together the
actual paths into a single path (List<Node>).  FindOptPostRoundPath does same just for default cost only.\
\
FindLowestCostPath_XXX: These methods use the routeCost dictionaries to run through all the permutations of routes (ex: start->2->3->1->4) to find the lowest cost one.\
\
**FindPath2**: This is the workhorse of the AStar pathplanner.  Input a start and end point and hazard cost, returns a path (List<Node>).  Basically, have an open and closed list, and while there are still unvisited nodes (openList>0), it will keep exploring to find
a path with a lower cost.  Checks if there's a lower overall total cost for this node (Fcost) by adding the hCost (heuristic cost from current node to goal) and gCost (cost from start to node). It checks each neighbor and selects the lowest cost until goal node is found, and then retraces the lowest cost.  Also includes increasing the gCost for nodes that are in hazard, which is why hazard cost has to be very high (500,000) to make a meaningful difference in route plan.  Also track the closest node to the goal in case 
it can't find a path to exactly the goal but can only find something close.\
\
FindValidEndPoint (Same for startpoint): This will move the end point of the path around in case the goal is too close to a boulder and it can't get to exactly the goal.\
\
RetracePath: Retraces path from the endpoint backwards to start using the parent construct (lowest cost point before).\
\
FindPathCost: This calculates the path cost for Astar, but this isn't the same as the ATScore.  This is the total AStar path cost (usually 1e6 to 1e7).\
\
FindPredictedHazardTime: Calculates expected time in hazard.  Uses factor of 0.12f*RoverDriving.speed for calculating. **Need to remove 0.12f in code to just make it simpler**.\
\
FindPredictedRouteScore: This is actual prediction of ATScore using number of nodes and time in hazard.  Needs to be simplified and refined.\

## TextboxManager
This is the managing script for the warning and caution panel, which mimics an aircraft WCA panel.  There are 15 textboxes the are there to simulate faults in the rover and test user situation awareness.  The first 11 are the "drive warnings" and if any one of them turns into a warning the rover will stop until the user clicks the box to "reset the system".  The last 4 are "science warnings" and if one of those is red the rover won't be able to do science when it reaches a goal and therefore receive zero reward when it crosses that goal.  The user can't reset cautions, only warnings.  The pk table is the probability threshold for going from one condition to another. The time intervals are how often the script will run to check to activate or deactivate a caution panel (InvokeRepeating methods).  Need some more user testing to calibrate both.  Set all of these in the inspector in Unity.\
\
There are two important items changed when a textbox is activated.  The textboxes[i] array is the array that contains all the actual textboxes (0-14) and controls what color textboxes[i] is.  The textboxesStatus[i] array keeps track of which condition each textbox is in (0=no issue, 1=caution, 2=warning).  This is used for verifying and tracking which overall issues (drive or science) should be present at any given moment.\

## Miscellaneous
Deductive Explanation displays: Shows route stats that include ATScore prediction, route time prediction, hazard time prediction, battery usage prediction in text.\
\
Global explanation: displays simple text description of overall "training objective" of "AI agent". Either "Minimize time" "Balance time vs. hazards" or "Avoid hazards" or "Manual Route".\
\
Battery slider: Slider that has baseline drain rate and moving drain rate.  Will stop the rover from moving at 10% and turn red. **NEED TO IMPROVE BATTERY PREDICTION CALCS**\
\
Score displays (2): One for actual AT score accumulated throughout the round, one for predicted score of just this route.  Should match what's in the Deductive Score box.\
\
PostTrialSurveys: 12 questions with sliders for administering post-round questions.  Questions 1-6 are NASA TLX, 7-9 are explainability questions, 10-12 are trust questions.\

























