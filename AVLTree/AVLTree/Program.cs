using System;
using Gtk;
using Cairo;
using Gdk;

// this is one node in the tree
public class TreeNode
{
	public int Value; // number in the node
	public TreeNode? Left;
	public TreeNode? Right;
	public int Height;

	public double X, Y; // position on screen

	public TreeNode(int value)
	{
		Value = value;
		Height = 1;
	}
}

// this is the avl tree logic
public class AVLTree
{
	private TreeNode? root = null;

	public TreeNode? Root => root;

	public void Insert(int value)
	{
		root = Insert(root, value);
	}

	public void Delete(int value)
	{
		root = Delete(root, value);
	}

	public int GetHeight(TreeNode? node) => node?.Height ?? 0;

	public int GetBalance(TreeNode? node) =>
		node == null ? 0 : GetHeight(node.Left) - GetHeight(node.Right);

	private TreeNode Insert(TreeNode? node, int value)
	{
		if (node == null)
			return new TreeNode(value);

		if (value < node.Value)
			node.Left = Insert(node.Left, value);
		else if (value > node.Value)
			node.Right = Insert(node.Right, value);
		else
			return node; // no duplicate values

		node.Height = Math.Max(GetHeight(node.Left), GetHeight(node.Right)) + 1;

		int balance = GetBalance(node);

		if (balance > 1 && value < node.Left!.Value)
			return RotateRight(node);
		if (balance < -1 && value > node.Right!.Value)
			return RotateLeft(node);
		if (balance > 1 && value > node.Left!.Value)
		{
			node.Left = RotateLeft(node.Left);
			return RotateRight(node);
		}
		if (balance < -1 && value < node.Right!.Value)
		{
			node.Right = RotateRight(node.Right);
			return RotateLeft(node);
		}

		return node;
	}

	private TreeNode? Delete(TreeNode? node, int value)
	{
		if (node == null)
			return null;

		if (value < node.Value)
			node.Left = Delete(node.Left, value);
		else if (value > node.Value)
			node.Right = Delete(node.Right, value);
		else
		{
			if (node.Left == null) return node.Right;
			if (node.Right == null) return node.Left;

			TreeNode min = node.Right;
			while (min.Left != null)
				min = min.Left;

			node.Value = min.Value;
			node.Right = Delete(node.Right, min.Value);
		}

		node.Height = Math.Max(GetHeight(node.Left), GetHeight(node.Right)) + 1;

		int balance = GetBalance(node);

		if (balance > 1 && GetBalance(node.Left) >= 0)
			return RotateRight(node);
		if (balance > 1 && GetBalance(node.Left) < 0)
		{
			node.Left = RotateLeft(node.Left);
			return RotateRight(node);
		}
		if (balance < -1 && GetBalance(node.Right) <= 0)
			return RotateLeft(node);
		if (balance < -1 && GetBalance(node.Right) > 0)
		{
			node.Right = RotateRight(node.Right);
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

		y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;
		x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;

		return x;
	}

	private TreeNode RotateLeft(TreeNode x)
	{
		TreeNode y = x.Right!;
		TreeNode? T2 = y.Left;

		y.Left = x;
		x.Right = T2;

		x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;
		y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;

		return y;
	}
}

// this is the window app
public class AVLTreeApp : Gtk.Window
{
	Entry input;
	Button insertButton;
	DrawingArea drawing;
	Label message;

	AVLTree tree = new AVLTree();
	TreeNode? hoveredNode = null;

	const double Radius = 20;
	const double XGap = 50;
	const double YGap = 60;

	public AVLTreeApp() : base("AVL Tree")
	{
		SetDefaultSize(800, 600);
		SetPosition(Gtk.WindowPosition.Center);
		DeleteEvent += (o, e) => Application.Quit();

		VBox layout = new VBox(false, 5);
		HBox inputBox = new HBox(false, 5);

		input = new Entry();
		insertButton = new Button("Insert");
		insertButton.Clicked += OnInsertClicked;

		message = new Label("Enter a number and click Insert.");

		inputBox.PackStart(input, false, false, 0);
		inputBox.PackStart(insertButton, false, false, 0);

		drawing = new DrawingArea();
		drawing.AddEvents((int)(EventMask.ButtonPressMask | EventMask.PointerMotionMask));
		drawing.Drawn += OnDraw;
		drawing.MotionNotifyEvent += OnMouseMove;
		drawing.ButtonPressEvent += OnMouseClick;

		layout.PackStart(inputBox, false, false, 0);
		layout.PackStart(message, false, false, 0);
		layout.PackStart(drawing, true, true, 0);

		Add(layout);
		ShowAll();
	}

	void OnInsertClicked(object? sender, EventArgs e)
	{
		if (int.TryParse(input.Text, out int val))
		{
			tree.Insert(val);
			input.Text = "";
			message.Text = $"Inserted {val}";
			UpdatePositions();
			drawing.QueueDraw();
		}
		else
		{
			message.Text = "Please enter a valid number.";
		}
	}

	void UpdatePositions()
	{
		SetPositions(tree.Root, 400, 40, 200);
	}

	void SetPositions(TreeNode? node, double x, double y, double offset)
	{
		if (node == null) return;

		node.X = x;
		node.Y = y;

		SetPositions(node.Left, x - offset, y + YGap, offset / 2);
		SetPositions(node.Right, x + offset, y + YGap, offset / 2);
	}

	void OnDraw(object sender, DrawnArgs args)
	{
		Context cr = args.Cr;
		cr.SetSourceRGB(1, 1, 1);
		cr.Paint();

		cr.SetSourceRGB(0, 0, 0);
		cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
		cr.SetFontSize(12);

		DrawEdges(cr, tree.Root);
		DrawNodes(cr, tree.Root);
	}

	void DrawEdges(Context cr, TreeNode? node)
	{
		if (node == null) return;

		if (node.Left != null)
		{
			cr.MoveTo(node.X, node.Y);
			cr.LineTo(node.Left.X, node.Left.Y);
			cr.Stroke();
			DrawEdges(cr, node.Left);
		}

		if (node.Right != null)
		{
			cr.MoveTo(node.X, node.Y);
			cr.LineTo(node.Right.X, node.Right.Y);
			cr.Stroke();
			DrawEdges(cr, node.Right);
		}
	}

	void DrawNodes(Context cr, TreeNode? node)
	{
		if (node == null) return;

		bool isHovered = (node == hoveredNode);

		if (isHovered)
			cr.SetSourceRGB(1, 0.5, 0);
		else
			cr.SetSourceRGB(0.3, 0.6, 1);

		cr.Arc(node.X, node.Y, Radius, 0, 2 * Math.PI);
		cr.FillPreserve();
		cr.SetSourceRGB(0, 0, 0);
		cr.Stroke();

		string text = node.Value.ToString();
		TextExtents te = cr.TextExtents(text);
		cr.MoveTo(node.X - te.Width / 2, node.Y + te.Height / 2);
		cr.ShowText(text);
		cr.NewPath();

		DrawNodes(cr, node.Left);
		DrawNodes(cr, node.Right);
	}

	void OnMouseMove(object sender, MotionNotifyEventArgs args)
	{
		double mx = args.Event.X;
		double my = args.Event.Y;
		hoveredNode = null;

		FindHovered(tree.Root, mx, my);
		drawing.QueueDraw();
	}

	void FindHovered(TreeNode? node, double mx, double my)
	{
		if (node == null) return;

		double dx = mx - node.X;
		double dy = my - node.Y;

		if (Math.Sqrt(dx * dx + dy * dy) <= Radius)
		{
			hoveredNode = node;
			message.Text = $"Double click to delete {node.Value}";
		}

		FindHovered(node.Left, mx, my);
		FindHovered(node.Right, mx, my);
	}

	void OnMouseClick(object sender, ButtonPressEventArgs args)
	{
		if (args.Event.Type == EventType.TwoButtonPress && hoveredNode != null)
		{
			int val = hoveredNode.Value;
			tree.Delete(val);
			message.Text = $"Deleted {val}";
			UpdatePositions();
			drawing.QueueDraw();
		}
	}

	public static void Main()
	{
		Application.Init();
		new AVLTreeApp();
		Application.Run();
	}
}
