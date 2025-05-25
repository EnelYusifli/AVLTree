dev doc

project: avl tree visualizer using gtk (c#)
language: c# with gtk and cairo

classes:
TreeNode: holds each node's value and position (x, y)
AVLTree: handles insert, delete, rotations, height
AVLTreeApp: makes the app window, buttons, drawing area

how it works:
insert(value): adds number to tree, balances it
delete(value): removes number, balances tree
rotateLeft / rotateRight: keep tree balanced
SetPositions(): update x, y of each node to draw nicely
OnDraw(): paints tree on screen
OnMouseClick(): delete node by double-click
OnMouseMove(): shows node info when mouse is over

user doc

what it does:
this app shows how avl trees work. you can add and delete numbers. tree stays balanced.

how to use:
1. type number in box
2. click "insert"
3. tree draws it
4. move mouse over a node
5. double click orange circle = delete node

rules:
only numbers (integers)
no duplicate numbers
