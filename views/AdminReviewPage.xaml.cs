using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Pet_Shop_Project.Views
{
    /// <summary>
    /// Trang quản lý đánh giá cho Admin
    /// </summary>
    public partial class AdminReviewPage : Page, INotifyPropertyChanged
    {
        private readonly ReviewService _reviewService = new ReviewService();
        private ObservableCollection<Review> _allReviews;
        private ObservableCollection<Review> _filteredReviews;
        private int _totalReviews;

        public AdminReviewPage()
        {
            InitializeComponent();
            FilteredReviews = new ObservableCollection<Review>();
            DataContext = this;
            Loaded += AdminReviewPage_Loaded;
        }

        public int TotalReviews
        {
            get => _totalReviews;
            set
            {
                _totalReviews = value;
                OnPropertyChanged(nameof(TotalReviews));
            }
        }

        public ObservableCollection<Review> AllReviews
        {
            get => _allReviews;
            set
            {
                _allReviews = value;
                OnPropertyChanged(nameof(AllReviews));
            }
        }

        public ObservableCollection<Review> FilteredReviews
        {
            get => _filteredReviews;
            set
            {
                _filteredReviews = value;
                OnPropertyChanged(nameof(FilteredReviews));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void AdminReviewPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadReviewsAsync();
        }

        private async Task LoadReviewsAsync()
        {
            try
            {
                var reviews = await _reviewService.GetAllReviewsAsync();
                AllReviews = new ObservableCollection<Review>(reviews.OrderByDescending(r => r.ReviewDate));
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải đánh giá: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbStarFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private int? GetSelectedStarFilter()
        {
            if (CmbStarFilter == null) return null;

            switch (CmbStarFilter.SelectedIndex)
            {
                case 1: return 5;
                case 2: return 4;
                case 3: return 3;
                case 4: return 2;
                case 5: return 1;
                default: return null;
            }
        }

        private void ApplyFilter()
        {
            if (AllReviews == null) return;

            var result = AllReviews.AsEnumerable();

            // Lọc theo số sao
            int? starFilter = GetSelectedStarFilter();
            if (starFilter.HasValue)
            {
                result = result.Where(r => r.Rating == starFilter.Value);
            }

            // Lọc theo search text
            string searchText = SearchBox?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                result = result.Where(r =>
                    (r.ProductId?.ToLower().Contains(searchText.ToLower()) ?? false) ||
                    (r.UserId?.ToLower().Contains(searchText.ToLower()) ?? false) ||
                    (r.Comment?.ToLower().Contains(searchText.ToLower()) ?? false)
                );
            }

            // Cập nhật ObservableCollection
            FilteredReviews.Clear();
            foreach (var item in result)
            {
                FilteredReviews.Add(item);
            }

            TotalReviews = FilteredReviews.Count;

            // Cập nhật UI
            EmptyState.Visibility = FilteredReviews.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void BtnAddVirtualReview_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AdminAddReviewDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                await LoadReviewsAsync();
                MessageBox.Show("Thêm đánh giá ảo thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var review = button?.Tag as Review;

            if (review == null) return;

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa đánh giá này?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = await _reviewService.DeleteReviewAsync(review.ReviewId);

                if (success)
                {
                    AllReviews.Remove(review);
                    ApplyFilter();
                    MessageBox.Show("Xóa thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
