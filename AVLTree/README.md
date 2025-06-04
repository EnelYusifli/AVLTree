Dev doc////

Project: AVL Tree Visualizer
Language: C#
Frameworks: GTK# and Cairo

Classes and Responsibilities

TreeNode
This class represents a single node in the AVL tree. It holds the integer value of the node, its height for balancing purposes, left and right child pointers, and X, Y coordinates used to visually position the node in the drawing area. The height is automatically updated during insertions and deletions. The coordinates are only used for display.

AVLTree
This class contains all the AVL tree logic including insertion, deletion, rotation, balancing, height calculation, and layout of node positions for drawing.

Insert(int value):
Adds a new integer value to the tree. If the value already exists, it is ignored (no duplicates are allowed). The tree is recursively traversed to find the correct insertion point. After insertion, the heights of affected nodes are updated and the balance factor is checked. If the balance factor becomes +2 or -2, the tree is automatically rebalanced using left or right rotations.

Delete(int value):
Removes a node with the specified value. If the value does not exist, nothing happens. After deleting, the heights are updated and the tree is checked for imbalance, which is corrected with rotations as needed. 

RotateLeft(TreeNode node) and RotateRight(TreeNode node):
These methods perform a single rotation to fix a balance violation. They are called automatically during insert and delete operations if an imbalance is detected.

GetHeight(TreeNode node) and GetBalance(TreeNode node):
Helper methods used to calculate the height of a node and its balance factor (difference in height between left and right subtrees).

SetPositions():
This method updates the X and Y positions of every node to ensure the tree is displayed in a clear and readable way. It uses an in-order traversal to spread the nodes evenly along the horizontal axis and assigns vertical spacing based on depth.

AVLTreeApp
This class handles all GUI-related operations using GTK# and drawing with Cairo. It sets up the main window, drawing area, text entry, and buttons. It also handles mouse input and tree drawing.

OnDraw():
Draws the AVL tree visually. It is called automatically when the window needs to refresh (after an insertion or deletion). It uses the positions set by SetPositions() to determine where to draw each node and its connections.

OnMouseClick():
Handles mouse click events. If a double-click is detected on a node, that node is deleted from the tree. The display is refreshed afterward.

OnMouseMove():
Tracks mouse movements over the drawing area. When the mouse is hovering over a node, its appearance changes. 


User doc////

What it does:
This app shows how avl trees work. You can add and delete numbers. Tree stays balanced.

How to use:
1. Type number in box
2. Click "insert"
3. Tree draws it
4. Move mouse over a node
5. Double click orange circle = delete node
