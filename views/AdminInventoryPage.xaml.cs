using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pet_Shop_Project.Views
{
    public partial class AdminInventory : Page
    {
        private ObservableCollection<InventoryItem> allItems;
        private ObservableCollection<InventoryItem> filteredItems;
        private ObservableCollection<InventoryItem> currentPageItems;

        private int currentPage = 1;
        private const int itemsPerPage = 10;
        private int totalPages = 1;

        public AdminInventory()
        {
            InitializeComponent();
            Loaded += AdminInventory_Loaded;
        }

        #region Load Data

        private async void AdminInventory_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInventoryDataAsync();
        }

        private async Task LoadInventoryDataAsync()
        {
            try
            {
                var items = await InventoryService.GetAllInventoryAsync();
                allItems = new ObservableCollection<InventoryItem>(items);
                filteredItems = new ObservableCollection<InventoryItem>(allItems);

                UpdatePagination();
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Filtering

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void StockFilter_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void CateFilter_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (allItems == null) return;

            var filtered = allItems.AsEnumerable();

            // Text search
            string searchText = SearchBox?.Text?.Trim().ToLower() ?? "";

            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(i =>
                    i.ProductCode.ToLower().Contains(searchText) ||
                    i.ProductName.ToLower().Contains(searchText));
            }

            // Stock level filter
            if (RbInStock?.IsChecked == true)
            {
                filtered = filtered.Where(i => i.StockQuantity > 0);
            }
            else if (RbOutOfStock?.IsChecked == true)
            {
                filtered = filtered.Where(i => i.StockQuantity == 0);
            }

            // Category filter
            string selectedCategory = null;
            if (AllCategory?.IsChecked == true) selectedCategory = null;
            else if (FoodCategory?.IsChecked == true) selectedCategory = "Thức ăn";
            else if (ToyCategory?.IsChecked == true) selectedCategory = "Đồ chơi";
            else if (DeviceCategory?.IsChecked == true) selectedCategory = "Thiết bị";
            else if (ToolCategory?.IsChecked == true) selectedCategory = "Dụng cụ";

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                filtered = filtered.Where(i =>
                    string.Equals(i.Category, selectedCategory, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(i.Product?.Category, selectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            filteredItems = new ObservableCollection<InventoryItem>(filtered);
            currentPage = 1;
            UpdatePagination();
            UpdateSummary();
        }

        #endregion

        #region Pagination

        private void UpdatePagination()
        {
            if (filteredItems == null || filteredItems.Count == 0)
            {
                InventoryDataGrid.ItemsSource = null;
                PageInfoText.Text = "0";
                ResultInfoText.Text = "Không có kết quả";
                totalPages = 0;
                return;
            }

            totalPages = (int)Math.Ceiling((double)filteredItems.Count / itemsPerPage);

            if (currentPage > totalPages)
                currentPage = totalPages;
            if (currentPage < 1)
                currentPage = 1;

            int skip = (currentPage - 1) * itemsPerPage;
            currentPageItems = new ObservableCollection<InventoryItem>(
                filteredItems.Skip(skip).Take(itemsPerPage)
            );

            InventoryDataGrid.ItemsSource = currentPageItems;

            PageInfoText.Text = currentPage.ToString();

            int start = skip + 1;
            int end = Math.Min(skip + itemsPerPage, filteredItems.Count);
            ResultInfoText.Text = $"Hiển thị {start} - {end} của {filteredItems.Count} kết quả";

            // Enable/disable pagination buttons
            BtnFirstPage.IsEnabled = currentPage > 1;
            BtnPrevPage.IsEnabled = currentPage > 1;
            BtnNextPage.IsEnabled = currentPage < totalPages;
            BtnLastPage.IsEnabled = currentPage < totalPages;
        }

        private void BtnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 1;
            UpdatePagination();
        }

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdatePagination();
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                UpdatePagination();
            }
        }

        private void BtnLastPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = totalPages;
            UpdatePagination();
        }

        #endregion

        #region CRUD Operations

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ShowInventoryEditDialog(null);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as InventoryItem;

            if (item == null) return;

            ShowInventoryEditDialog(item);
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as InventoryItem;

            if (item == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa sản phẩm '{item.ProductName}' khỏi kho?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (await InventoryService.DeleteInventoryItemAsync(item.ProductId))
                    {
                        MessageBox.Show("Xóa thành công!",
                            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadInventoryDataAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa: {ex.Message}",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (filteredItems == null) return;

            var toDelete = filteredItems.Where(i => i.IsSelected).ToList();
            if (toDelete.Count == 0)
            {
                MessageBox.Show("Chưa chọn mặt hàng nào.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Xóa {toDelete.Count} mặt hàng đã chọn?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            foreach (var item in toDelete)
            {
                await InventoryService.DeleteInventoryItemAsync(item.ProductId);
            }

            await LoadInventoryDataAsync();
        }

        private void InventoryDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (InventoryDataGrid.SelectedItem is InventoryItem item)
            {
                ShowInventoryEditDialog(item);
            }
        }

        #endregion

        #region Overlay Dialog Management

        private void ShowInventoryEditDialog(InventoryItem item)
        {
            var dialogPage = new AdminInventoryEditDialog(item);

            // Subscribe to the dialog's close event
            dialogPage.DialogClosed += async (success) =>
            {
                HideOverlay();
                if (success)
                {
                    await LoadInventoryDataAsync();
                }
            };

            OverlayFrame.Content = dialogPage;
            OverlayContainer.Visibility = Visibility.Visible;
        }

        private void HideOverlay()
        {
            OverlayFrame.Content = null;
            OverlayContainer.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Helper Methods

        private void UpdateSummary()
        {
            if (filteredItems != null)
            {
                TotalItemsText.Text = filteredItems.Count.ToString();
            }
            else
            {
                TotalItemsText.Text = "0";
            }
        }

        private void SelectAllCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            bool isChecked = SelectAllCheckBox.IsChecked == true;

            if (currentPageItems != null)
            {
                foreach (var item in currentPageItems)
                {
                    item.IsSelected = isChecked;
                }
            }
        }

        private void RowCheckChanged(object sender, RoutedEventArgs e)
        {
            if (currentPageItems == null || !currentPageItems.Any()) return;
            SelectAllCheckBox.Checked -= SelectAllCheckBox_Changed;
            SelectAllCheckBox.Unchecked -= SelectAllCheckBox_Changed;
            SelectAllCheckBox.IsChecked = currentPageItems.All(i => i.IsSelected);
            SelectAllCheckBox.Checked += SelectAllCheckBox_Changed;
            SelectAllCheckBox.Unchecked += SelectAllCheckBox_Changed;
        }

        #endregion
    }
}