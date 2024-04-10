using System.Windows.Forms;
using System;
using LiveCharts;
using LiveCharts.Wpf;
using ZedGraph;
using System.Diagnostics;
using System.ComponentModel;

namespace Lab5
{
    public partial class MainForm : Form
    {
        private BindingSource bindingSource;
        public CityList cityList;

        private ZedGraphControl zedGraphControl1;
        private PictureBox pictureBox1;
        private DataGridView dataGridView1;
        private PropertyGrid propertyGrid1;
        private BindingNavigator bindingNavigator1;

        public MainForm()
        {
            InitializeComponent();
            InitializeData();
            InitializeUI();
        }

        public void InitializeData()
        {
            cityList = new CityList(new List<City>
            {
                new City("Нью-Йорк", 8398748, Country.USA, "./emblems/new_york.png"),
                new City("Москва", 12615882, Country.Russia, "./emblems/moscow.png"),
                new City("Пекин", 21707000, Country.China, "./emblems/beijing.png"),
                new City("Лондон", 8908081, Country.UK, "./emblems/london.png"),
                new City("Дели", 18980000, Country.India, "./emblems/delhi.png"),
                new City("Сан-Паулу", 12252023, Country.Brazil, "./emblems/sao_paulo.png"),
                new City("Сеул", 9720846, Country.SouthKorea, "./emblems/seoul.png"),
                new City("Лос-Анджелес", 3987488, Country.USA, "./emblems/los_angeles.png")
            });
        }

        private void InitializeUI()
        {
            bindingSource = new BindingSource();
            bindingSource.AllowNew = true;
            bindingSource.DataSource = cityList.Cities;
            bindingSource.ListChanged += BindingSource_ListChanged;
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            Controls.Add(tableLayoutPanel);

            TableLayoutPanel additionalTableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill
            };
            additionalTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            additionalTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            additionalTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tableLayoutPanel.Controls.Add(additionalTableLayoutPanel, 0, 0);

            pictureBox1 = new PictureBox
            {
                Dock = DockStyle.Fill
            };
            bindingSource.CurrentChanged += (sender, e) =>
            {
                if (bindingSource.Current != null)
                {
                    City selectedCity = (City)bindingSource.Current;
                    if (!string.IsNullOrEmpty(selectedCity.EmblemPath) && File.Exists(selectedCity.EmblemPath))
                    {
                        pictureBox1.Image = Image.FromFile(selectedCity.EmblemPath);
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                }
            };
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.DoubleClick += (sender, e) =>
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Image Files (*.jpg, *.png, *.bmp)|*.jpg;*.png;*.bmp";
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string imagePath = openFileDialog.FileName;
                        pictureBox1.Image = Image.FromFile(imagePath);
                        if (bindingSource.Current != null)
                        {
                            City selectedCity = (City)bindingSource.Current;
                            selectedCity.EmblemPath = imagePath;
                            pictureBox1.Image = Image.FromFile(imagePath);
                        }
                    }
                }
            };
            tableLayoutPanel.Controls.Add(pictureBox1, 1, 0);

            propertyGrid1 = new PropertyGrid
            {
                Dock = DockStyle.Fill,
            };
            propertyGrid1.DataBindings.Add("SelectedObject", bindingSource, "");
            tableLayoutPanel.Controls.Add(propertyGrid1, 1, 1);

            zedGraphControl1 = new ZedGraphControl
            {
                Dock = DockStyle.Fill,
            };
            tableLayoutPanel.Controls.Add(zedGraphControl1, 0, 1);
            RefreshGraph();

            dataGridView1 = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = bindingSource
            };
            additionalTableLayoutPanel.Controls.Add(dataGridView1, 0, 1);

            bindingNavigator1 = new BindingNavigator
            {
                Dock = DockStyle.Top,
                BindingSource = bindingSource
            }; 
            bindingNavigator1.AddStandardItems();
            additionalTableLayoutPanel.Controls.Add(bindingNavigator1, 0, 0);

            ToolStripButton btnSave = new ToolStripButton();
            btnSave.Text = "Сохранить";
            btnSave.Click += BtnSave_Click;

            ToolStripButton btnLoad = new ToolStripButton();
            btnLoad.Text = "Загрузить";
            btnLoad.Click += BtnLoad_Click;

            bindingNavigator1.Items.Add(btnSave);
            bindingNavigator1.Items.Add(btnLoad);

            ToolStripTextBox filterTextBox = new ToolStripTextBox();
            filterTextBox.Name = "filterTextBox";
            filterTextBox.ToolTipText = "Фильтр по населению";

            bindingNavigator1.Items.Add(new ToolStripLabel("Фильтр по населению:"));
            bindingNavigator1.Items.Add(filterTextBox);

            filterTextBox.KeyPress += (sender, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            };

            filterTextBox.TextChanged += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(filterTextBox.Text))
                {
                    int populationFilter;
                    if (int.TryParse(filterTextBox.Text, out populationFilter))
                    {
                        bindingSource.DataSource = cityList.Cities.Where(city => city.Population >= populationFilter).ToList();
                    }
                }
            };
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON files (*.json)|*.json|XML files (*.xml)|*.xml";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    string extension = Path.GetExtension(filePath);

                    if (extension.ToLower() == ".json")
                    {
                        cityList.SaveToJson(cityList.Cities, filePath);
                    }
                    else if (extension.ToLower() == ".xml")
                    {
                        cityList.SaveToXml(cityList.Cities, filePath);
                    }
                }
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON files (*.json)|*.json|XML files (*.xml)|*.xml";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    string extension = Path.GetExtension(filePath);

                    if (extension.ToLower() == ".json")
                    {
                        cityList.Cities = cityList.LoadFromJson<City>(filePath);
                    }
                    else if (extension.ToLower() == ".xml")
                    {
                        cityList.Cities = cityList.LoadFromXml<City>(filePath);
                    }

                    bindingSource.DataSource = cityList.Cities;
                }
            }

        }

        private void BindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            RefreshGraph();
        }

        private void RefreshGraph()
        {
            var groupedCities = ((List<City>)bindingSource.DataSource).GroupBy(c => c.Country)
                                    .Select(g => new { SettlementType = g.Key, AveragePopulation = g.Average(c => c.Population) });

            GraphPane pane = zedGraphControl1.GraphPane;
            pane.Title.Text = "Среднее значение населения по типу поселения";
            pane.XAxis.Title.Text = "Тип поселения";
            pane.YAxis.Title.Text = "Среднее население";

            pane.CurveList.Clear();

            int itemscount = groupedCities.Count();
            List<string> names = new List<string>();

            List<double> values = new List<double>();

            foreach (var cityGroup in groupedCities)
            {
                names.Add(cityGroup.SettlementType.ToString());
                values.Add(cityGroup.AveragePopulation);
            }

            BarItem curve = pane.AddBar("Гистограмма", null, values.ToArray(), Color.Blue);
            pane.XAxis.Type = AxisType.Text;
            pane.XAxis.Scale.TextLabels = names.ToArray();

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }
    }
}
