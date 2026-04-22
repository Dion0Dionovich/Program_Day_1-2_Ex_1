using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace Program
{

    public partial class MainWindow : Window
    {
        private List<Product> allProducts = new();
        private ObservableCollection<Product> displayProducts = new();


        public MainWindow()
        {
            InitializeComponent();
            LoadProducts();

            DataGrid_ProductTable.ItemsSource = displayProducts;
            DataGrid_ProductTable.SelectionMode = DataGridSelectionMode.Extended;
            DataGrid_ProductTable.PreviewMouseLeftButtonDown += DataGrid_PreviewMouseLeftButtonDown;

            var categories = allProducts.Select(p => p.Category).Distinct().ToList();
            ComboBox_Category.Items.Clear();
            ComboBox_Category.Items.Add("Не выбрано");
            ComboBox_Category.Items.Add("Все категории");
            foreach (var cat in categories) ComboBox_Category.Items.Add(cat);
            ComboBox_Category.SelectedIndex = 0;

            ComboBox_PriceRange.Items.Clear();
            ComboBox_PriceRange.Items.Add("Не выбрано");
            ComboBox_PriceRange.Items.Add("Все цены");
            ComboBox_PriceRange.Items.Add("до 100 руб.");
            ComboBox_PriceRange.Items.Add("100-500 руб.");
            ComboBox_PriceRange.Items.Add("500-1000 руб.");
            ComboBox_PriceRange.Items.Add("от 1000 руб.");
            ComboBox_PriceRange.SelectedIndex = 0;

            ComboBox_DiscountPercentage.Items.Add("Не выбрано");
            for (int i = 5; i <= 50; i += 5) ComboBox_DiscountPercentage.Items.Add($"{i}%");
            ComboBox_DiscountPercentage.SelectedIndex = 0;
            ComboBox_DiscountPercentage.IsEnabled = false;

            ComboBox_Category.SelectionChanged += FilterProducts;
            ComboBox_PriceRange.SelectionChanged += FilterProducts;
            CheckBox_discount.Checked += (s, e) => ComboBox_DiscountPercentage.IsEnabled = true;
            CheckBox_discount.Unchecked += (s, e) => ComboBox_DiscountPercentage.IsEnabled = false;
            Butt_result.Click += CalculateTotal;
            TextBox_count.KeyDown += TextBox_count_KeyDown;

            FilterProducts(null, null);
        }


        private void LoadProducts()
        {
            var products = new[]
            {
                new Product { Name = "Молоко 1л", Price = 89, Category = "Молочные" },
                new Product { Name = "Кефир 1л", Price = 75, Category = "Молочные" },
                new Product { Name = "Сметана 200г", Price = 65, Category = "Молочные" },
                new Product { Name = "Творог 200г", Price = 120, Category = "Молочные" },
                new Product { Name = "Йогурт", Price = 55, Category = "Молочные" },
                new Product { Name = "Хлеб белый", Price = 45, Category = "Хлебобулочные" },
                new Product { Name = "Хлеб черный", Price = 50, Category = "Хлебобулочные" },
                new Product { Name = "Батон", Price = 40, Category = "Хлебобулочные" },
                new Product { Name = "Булочка", Price = 25, Category = "Хлебобулочные" },
                new Product { Name = "Печенье", Price = 80, Category = "Хлебобулочные" },
                new Product { Name = "Вода 0.5л", Price = 35, Category = "Напитки" },
                new Product { Name = "Сок 1л", Price = 120, Category = "Напитки" },
                new Product { Name = "Кола 0.5л", Price = 90, Category = "Напитки" },
                new Product { Name = "Чай", Price = 150, Category = "Напитки" },
                new Product { Name = "Кофе", Price = 350, Category = "Напитки" }
            };
            allProducts.AddRange(products);
        }


        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as DependencyObject;
            while (hit != null && hit is not DataGridRow) hit = VisualTreeHelper.GetParent(hit);

            if (hit is DataGridRow row && Keyboard.Modifiers != ModifierKeys.Control && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                DataGrid_ProductTable.SelectedItems.Clear();
                DataGrid_ProductTable.SelectedItems.Add(row.Item);
                e.Handled = true;
            }
        }


        private void FilterProducts(object sender, SelectionChangedEventArgs e)
        {
            displayProducts.Clear();

            string selectedCategory = ComboBox_Category.SelectedItem?.ToString() ?? "Не выбрано";
            string selectedPriceRange = ComboBox_PriceRange.SelectedItem?.ToString() ?? "Не выбрано";

            if (selectedCategory == "Не выбрано" || selectedPriceRange == "Не выбрано") return;

            var filtered = allProducts.AsEnumerable();

            if (selectedCategory != "Все категории")
                filtered = filtered.Where(p => p.Category == selectedCategory);

            if (selectedPriceRange != "Все цены")
            {
                if (selectedPriceRange == "до 100 руб.")
                    filtered = filtered.Where(p => p.Price <= 100);
                else if (selectedPriceRange == "100-500 руб.")
                    filtered = filtered.Where(p => p.Price >= 100 && p.Price <= 500);
                else if (selectedPriceRange == "500-1000 руб.")
                    filtered = filtered.Where(p => p.Price >= 500 && p.Price <= 1000);
                else if (selectedPriceRange == "от 1000 руб.")
                    filtered = filtered.Where(p => p.Price >= 1000);
            }

            foreach (var product in filtered) displayProducts.Add(product);
        }


        private void CalculateTotal(object sender, RoutedEventArgs e)
        {
            double total = 0;
            foreach (Product item in DataGrid_ProductTable.SelectedItems)
                total += item.Price * item.Quantity;

            if (CheckBox_discount.IsChecked == true && ComboBox_DiscountPercentage.SelectedIndex > 0)
            {
                string discountText = ComboBox_DiscountPercentage.SelectedItem.ToString().Replace("%", "");
                if (int.TryParse(discountText, out int percent))
                {
                    total -= total * percent / 100;
                    TextBlock_result.Text = $"Итого к оплате: {total:F2} руб. (скидка {percent}%)";
                    return;
                }
            }

            TextBlock_result.Text = $"Итого к оплате: {total:F2} руб.";
        }


        private void TextBox_count_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            if (!int.TryParse(TextBox_count.Text, out int quantity) || quantity <= 0)
            {
                TextBox_count.Text = "";
                return;
            }

            int appliedCount = 0;
            foreach (Product item in DataGrid_ProductTable.SelectedItems)
            {
                item.Quantity = quantity;
                appliedCount++;
            }

            MessageBox.Show(appliedCount > 0
                ? $"Количество '{quantity}' установлено для {appliedCount} товаров"
                : "Не выбрано ни одного товара", "Информация");

            TextBox_count.Text = "";
        }


        public class Product : INotifyPropertyChanged
        {
            public string Name { get; set; }
            public double Price { get; set; }
            public string Category { get; set; }

            private int quantity;
            public int Quantity
            {
                get => quantity;
                set { quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(Total)); }
            }

            public double Total => Price * Quantity;

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
