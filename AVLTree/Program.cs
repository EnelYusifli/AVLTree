using System;
using Gtk;
using Cairo;
using Gdk;
using System.Collections.Generic;

// Represents one node in the AVL tree
public class TreeNode
{
	public int Value; // number in node
	public TreeNode? Left; // left child
	public TreeNode? Right; // right child
	public int Height; // height of this node (used in AVL balance)

	// position on screen
	public double X, Y;

	public TreeNode(int value)
	{
		Value = value;
		Height = 1; // start height is 1
	}

	// copy whole subtree
	public TreeNode Clone()
	{
		var n = new TreeNode(Value) { Height = Height };
		if (Left != null) n.Left = Left.Clone(); // copy left
		if (Right != null) n.Right = Right.Clone(); // copy right
		return n;
	}
}

// 1 step of insert/delete for visuals
public class TreeOperationStep
{
	public TreeNode? SnapshotRoot; // copy of tree
	public string Description; // what happened
	public TreeOperationStep(TreeNode? root, string desc)
	{
		SnapshotRoot = root;
		Description = desc;
	}
}

// Main AVL Tree class
public class AVLTree
{
	private TreeNode? root = null; // tree start

	public readonly List<TreeOperationStep> steps = new(); // list of all steps
	public TreeNode? Root => root; // get tree root

	#region Helpers

	private int GetHeight(TreeNode? n) => n?.Height ?? 0; // height or 0
	private int GetBalance(TreeNode? n) => n == null ? 0 : GetHeight(n.Left) - GetHeight(n.Right); // balance = left - right

	private void UpdateHeight(TreeNode n) => n.Height = Math.Max(GetHeight(n.Left), GetHeight(n.Right)) + 1; // update height

	private void Record(string msg)
	{
		// save snapshot + message
		steps.Add(new TreeOperationStep(root?.Clone(), msg));
	}
	#endregion

	#region Public Methods

	// insert + record steps
	public void InsertWithSteps(int value)
	{
		steps.Clear(); // remove old steps
		root = Insert(root, value);
		Record($"Finished insertion of {value}");
	}

	// delete + record steps
	public void DeleteWithSteps(int value)
	{
		steps.Clear(); // remove old steps
		root = Delete(root, value);
		Record($"Finished deletion of {value}");
	}
	#endregion

	#region Insert/Delete Logic

	private TreeNode Insert(TreeNode? node, int value)
	{
		if (node == null)
		{
			var created = new TreeNode(value);
			root ??= created; // if tree was empty
			Record($"Create node {value}");
			return created;
		}

		if (value < node.Value)
			node.Left = Insert(node.Left, value); // go left
		else if (value > node.Value)
			node.Right = Insert(node.Right, value); // go right
		else
			return node; // same number, do nothing

		UpdateHeight(node); // recalculate height
		int balance = GetBalance(node); // get balance

		// 4 cases of unbalanced
		if (balance > 1 && value < node.Left!.Value)
		{
			Record($"Right rotate on {node.Value} (LL)");
			return RotateRight(node);
		}
		if (balance < -1 && value > node.Right!.Value)
		{
			Record($"Left rotate on {node.Value} (RR)");
			return RotateLeft(node);
		}
		if (balance > 1 && value > node.Left!.Value)
		{
			Record($"Left rotate on {node.Left!.Value} (LR – first)");
			node.Left = RotateLeft(node.Left);
			Record($"Right rotate on {node.Value} (LR – second)");
			return RotateRight(node);
		}
		if (balance < -1 && value < node.Right!.Value)
		{
			Record($"Right rotate on {node.Right!.Value} (RL – first)");
			node.Right = RotateRight(node.Right);
			Record($"Left rotate on {node.Value} (RL – second)");
			return RotateLeft(node);
		}

		return node;
	}

	private TreeNode? Delete(TreeNode? node, int value)
	{
		if (node == null) return null;

		if (value < node.Value)
			node.Left = Delete(node.Left, value); // go left
		else if (value > node.Value)
			node.Right = Delete(node.Right, value); // go right
		else
		{
			// found node to delete
			if (node.Left == null || node.Right == null)
			{
				Record($"Remove node {node.Value}");
				node = node.Left ?? node.Right;
			}
			else
			{
				TreeNode successor = node.Right;
				while (successor.Left != null) successor = successor.Left;
				Record($"Replace {node.Value} with {successor.Value}");
				node.Value = successor.Value;
				node.Right = Delete(node.Right, successor.Value);
			}
		}

		if (node == null) return null;

		UpdateHeight(node); // update height
		int balance = GetBalance(node); // check balance

		// 4 rotation cases
		if (balance > 1 && GetBalance(node.Left) >= 0)
		{
			Record($"Right rotate on {node.Value} (LL)");
			return RotateRight(node);
		}
		if (balance > 1 && GetBalance(node.Left) < 0)
		{
			Record($"Left rotate on {node.Left!.Value} (LR - first)");
			node.Left = RotateLeft(node.Left);
			Record($"Right rotate on {node.Value} (LR – second)");
			return RotateRight(node);
		}
		if (balance < -1 && GetBalance(node.Right) <= 0)
		{
			Record($"Left rotate on {node.Value} (RR)");
			return RotateLeft(node);
		}
		if (balance < -1 && GetBalance(node.Right) > 0)
		{
			Record($"Right rotate on {node.Right!.Value} (RL – first)");
			node.Right = RotateRight(node.Right);
			Record($"Left rotate on {node.Value} (RL – second)");
			return RotateLeft(node);
		}

		return node;
	}

	private TreeNode RotateRight(TreeNode y)
	{
		TreeNode x = y.Left!;
		TreeNode? T2 = x.Right;

		x.Right = y;
		y.Left = T2;

		UpdateHeight(y);
		UpdateHeight(x);

		if (root == y) root = x;
		Record($"After right rotate – new root {x.Value}");
		return x;
	}

	private TreeNode RotateLeft(TreeNode x)
	{
		TreeNode y = x.Right!;
		TreeNode? T2 = y.Left;

		y.Left = x;
		x.Right = T2;

		UpdateHeight(x);
		UpdateHeight(y);

		if (root == x) root = y;
		Record($"After left rotate – new root {y.Value}");
		return y;
	}
	#endregion
}


//window app
public class AVLTreeApp : Gtk.Window
{
	Entry input;
	Button insertButton;
	Button deleteButton;
	Button prevStepButton;
	Button nextStepButton;
	Label message;
	DrawingArea drawing;

	AVLTree tree = new();
	int stepIndex = 0;

	// visual consts
	const double Radius = 20;
	const double YGap = 60;

	public AVLTreeApp() : base("AVL Tree – step by step")
	{
		SetDefaultSize(800, 600);
		SetPosition(WindowPosition.Center);
		DeleteEvent += (o, e) => Application.Quit();

		// build UI
		VBox layout = new VBox(false, 5);
		HBox inputBox = new HBox(false, 5);
		input = new Entry();
		insertButton = new Button("Insert");
		deleteButton = new Button("Delete");
		prevStepButton = new Button("Prev");
		nextStepButton = new Button("Next");
		message = new Label("Enter value, press Insert or Delete, then navigate steps.");

		insertButton.Clicked += OnInsertClicked;
		deleteButton.Clicked += OnDeleteClicked;
		prevStepButton.Clicked += (s, e) => ChangeStep(-1);
		nextStepButton.Clicked += (s, e) => ChangeStep(1);

		inputBox.PackStart(input, false, false, 0);
		inputBox.PackStart(insertButton, false, false, 0);
		inputBox.PackStart(deleteButton, false, false, 0);
		inputBox.PackStart(prevStepButton, false, false, 0);
		inputBox.PackStart(nextStepButton, false, false, 0);

		drawing = new DrawingArea();
		drawing.Drawn += OnDraw;

		layout.PackStart(inputBox, false, false, 0);
		layout.PackStart(message, false, false, 0);
		layout.PackStart(drawing, true, true, 0);
		Add(layout);
		ShowAll();
	}

	#region Button handlers
	void OnInsertClicked(object? sender, EventArgs e)
	{
		if (int.TryParse(input.Text, out int val))
		{
			tree.InsertWithSteps(val);
			StartStepNavigation();
		}
		else message.Text = "Invalid integer.";
	}

	void OnDeleteClicked(object? sender, EventArgs e)
	{
		if (int.TryParse(input.Text, out int val))
		{
			tree.DeleteWithSteps(val);
			StartStepNavigation();
		}
		else message.Text = "Invalid integer.";
	}
	#endregion

	void StartStepNavigation()
	{
		stepIndex = 0;
		if (tree.steps.Count == 0)
		{
			message.Text = "No steps recorded. Tree unchanged.";
		}
		else
		{
			UpdatePositions(tree.steps[stepIndex].SnapshotRoot);
			message.Text = tree.steps[stepIndex].Description + $"  (step {stepIndex + 1}/{tree.steps.Count})";
		}
		drawing.QueueDraw();
	}

	void ChangeStep(int delta)
	{
		if (tree.steps.Count == 0) return;
		stepIndex = Math.Clamp(stepIndex + delta, 0, tree.steps.Count - 1);
		UpdatePositions(tree.steps[stepIndex].SnapshotRoot);
		message.Text = tree.steps[stepIndex].Description + $"  (step {stepIndex + 1}/{tree.steps.Count})";
		drawing.QueueDraw();
	}

	#region Layout logic
	void UpdatePositions(TreeNode? root)
	{
		if (root == null) return;
		SetPositions(root, 400, 40, 200);
	}

	void SetPositions(TreeNode? node, double x, double y, double offset)
	{
		if (node == null) return;
		node.X = x; node.Y = y;
		SetPositions(node.Left, x - offset, y + YGap, offset / 2);
		SetPositions(node.Right, x + offset, y + YGap, offset / 2);
	}
	#endregion

	#region Drawing
	void OnDraw(object sender, DrawnArgs args)
	{
		var cr = args.Cr;
		cr.SetSourceRGB(1, 1, 1);
		cr.Paint();
		if (tree.steps.Count == 0) return;
		var rootSnapshot = tree.steps[stepIndex].SnapshotRoot;
		cr.SetSourceRGB(0, 0, 0);
		cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
		cr.SetFontSize(12);
		DrawEdges(cr, rootSnapshot);
		DrawNodes(cr, rootSnapshot);
	}

	void DrawEdges(Context cr, TreeNode? n)
	{
		if (n == null) return;
		if (n.Left != null)
		{
			cr.MoveTo(n.X, n.Y);
			cr.LineTo(n.Left.X, n.Left.Y);
			cr.Stroke();
			DrawEdges(cr, n.Left);
		}
		if (n.Right != null)
		{
			cr.MoveTo(n.X, n.Y);
			cr.LineTo(n.Right.X, n.Right.Y);
			cr.Stroke();
			DrawEdges(cr, n.Right);
		}
	}

	void DrawNodes(Context cr, TreeNode? n)
	{
		if (n == null) return;
		cr.SetSourceRGB(0.3, 0.6, 1);
		cr.Arc(n.X, n.Y, Radius, 0, 2 * Math.PI);
		cr.FillPreserve();
		cr.SetSourceRGB(0, 0, 0);
		cr.Stroke();

		var text = n.Value.ToString();
		var te = cr.TextExtents(text);
		cr.MoveTo(n.X - te.Width / 2, n.Y + te.Height / 2);
		cr.ShowText(text);
		cr.NewPath();

		DrawNodes(cr, n.Left);
		DrawNodes(cr, n.Right);
	}
	#endregion

	public static void Main()
	{
		Application.Init();
		new AVLTreeApp();
		Application.Run();
	}
}
//check