User Documentation//
Usage
Run the app. Type a number in the box. Click Insert to add it or Delete to remove it. After that, use Prev and Next to step through what happened. The canvas shows the tree with circles (nodes) and lines (edges), and a message tells you what’s going on. Close the window to quit.

Developer Documentation//
The code has two main parts: AVLTree for the tree logic and AVLTreeApp for the UI and drawing. It uses TreeNode for nodes and TreeOperationStep to save snapshots of the tree for each step.

TreeNode
This is a node in the tree. It holds a value, left and right children, a height for balancing, and X, Y for where to draw it. The Clone function copies the node and its children for caching.

AVLTree
This class runs the show for the tree. It keeps the root node and a list of steps to track changes.

GetHeight: Grabs a node’s height. Used to check balance and update heights.
GetBalance: Figures out if a node’s leaning too far left or right (left height - right height). If it’s > 1 or < -1, we need to rotate.
UpdateHeight: Sets a node’s height based on its children’s heights. Keeps the tree balanced.
Record: Saves a copy of the tree and a message for each step.
InsertWithSteps: Adds a number to the tree and saves steps. Clears old steps, calls Insert, and logs the final step.
DeleteWithSteps: Removes a number and saves steps. Clears old steps, calls Insert, and logs the final step.
Insert: The recursive insert function. Adds a node, updates heights, and rotates if the tree’s off balance (like LL, RR, LR, RL cases). Logs each step.
Delete: Removes a node recursively. Handles cases like no children, one child, or two children. Updates heights, rotates if needed, and logs steps.
RotateRight & RotateLeft: Fix imbalances by rearranging nodes. Updates heights and the root if needed, then logs the step.

AVLTreeApp
This is the GTK window with the UI and drawing.

AVLTreeApp: Sets up the window, text box, buttons (Insert, Delete, Prev, Next), and canvas. Ties buttons to actions.
OnInsertClicked & OnDeleteClicked: Read the text box, check if it’s a number, then call InsertWithSteps or DeleteWithSteps.
StartStepNavigation: Kicks off step-by-step viewing after an insert/delete. Resets the step index, positions nodes, and redraws.
ChangeStep: Moves to the next or previous step. Updates the node positions, message, and canvas.
UpdatePositions & SetPositions: Set where nodes go on the canvas so the tree looks nice.
OnDraw, DrawEdges, DrawNodes: Draw the tree with Cairo. OnDraw clears the canvas and calls the others. DrawEdges makes lines between nodes, and DrawNodes draws circles with numbers.
