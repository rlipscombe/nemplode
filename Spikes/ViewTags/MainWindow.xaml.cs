using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ViewTags
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<TagLib.Id3v2.Tag> _dataSource =
            new ObservableCollection<TagLib.Id3v2.Tag>();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Columns = new ObservableCollection<DataGridColumn>();
            dataGrid.ItemsSource = _dataSource;

            OpenCommand = new RelayCommand(Open);
        }

        private void Open()
        {
            AddFile(@"D:\Temp\Homogenic\10 - All Is Full Of Love.mp3");
        }

        public ObservableCollection<DataGridColumn> Columns { get; private set; }

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                AddFile(file);
            }
        }

        private void AddFile(string file)
        {
            var f = TagLib.File.Create(file);
            var tag = (TagLib.Id3v2.Tag) f.GetTag(TagLib.TagTypes.Id3v2);
            _dataSource.Add(tag);

            Columns.Merge(tag.GetFrames().Select(FrameColumn.Create));
        }

        public ICommand OpenCommand { get; private set; }
    }
}