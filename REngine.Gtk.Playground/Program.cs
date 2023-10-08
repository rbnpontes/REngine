using Gtk;

Application.Init();

Application app = new Application("rengine.gtk.playground", GLib.ApplicationFlags.None);
app.Register(GLib.Cancellable.Current);

Window window = new Window(WindowType.Toplevel);
window.WindowPosition = WindowPosition.Center;
window.Title = "[REngine] Gtk Playground";
window.Resize(800, 500);

TreeView samplesList = new TreeView();
samplesList.Expand = true;

var textRenderer = new CellRendererText();
samplesList.AppendColumn(
	new TreeViewColumn("Sample Name", textRenderer, "text", 0, null)
);

ListStore samplesListModel = new ListStore(new Type[] { typeof(string) });
for(int i =0; i < 10; ++i)
{
	samplesListModel.SetValue(
		samplesListModel.Append(),
		0, $"Hello World ({i})"
	);
}

samplesList.Model = samplesListModel;
samplesList.Selection.Changed += (s, e) =>
{
	samplesList.Selection.GetSelected(out ITreeModel model, out TreeIter iter);
	Console.WriteLine("Selected: " + samplesListModel.GetValue(iter, 0).ToString());
};


Button button0 = new Button();
Button button1 = new Button();
Viewport viewport = new Viewport();

button0.Label = "Load Selected";

samplesList.Expand = viewport.Expand = true;

Box vbox = new(Orientation.Vertical, 5);
vbox.Add(samplesList);
vbox.Add(button0);
vbox.Expand = false;
vbox.WidthRequest = 200;


Box hbox = new Box(Orientation.Horizontal, 5);
hbox.Add(vbox);
hbox.Add(viewport);

var window2 = new Window(WindowType.Toplevel);
window2.Resize(300, 300);
window2.Add(new Button
{
	Label = "Hello World"
});


window.Add(hbox);
window.ShowAll();

viewport.Add(window2);
window2.ShowAll();

Application.Run();