using Pet_Shop_Project.Models;
using Pet_Shop_Project.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Pet_Shop_Project.Views
{



    public partial class AdminReviewPage : Page, INotifyPropertyChanged
    {
        private readonly ReviewService _reviewService = new ReviewService();
        private readonly UserService _userService = new UserService();
        private ObservableCollection<ReviewDisplayItem> _allReviews;
        private ObservableCollection<ReviewDisplayItem> _filteredReviews;
        private int _totalReviews;

        public AdminReviewPage()
        {
            InitializeComponent();
            FilteredReviews = new ObservableCollection<ReviewDisplayItem>();
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

        public ObservableCollection<ReviewDisplayItem> AllReviews
        {
            get => _allReviews;
            set
            {
                _allReviews = value;
                OnPropertyChanged(nameof(AllReviews));
            }
        }

        public ObservableCollection<ReviewDisplayItem> FilteredReviews
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

        // Tai review va map ten san pham/nguoi dung de hien thi
        private async Task LoadReviewsAsync()
        {
            try
            {
                var reviews = await _reviewService.GetAllReviewsAsync();
                var productsTask = _reviewService.GetAllProductsForSelectionAsync();
                var usersTask = _userService.GetAllUsersAsync();

                await Task.WhenAll(productsTask, usersTask);

                var productLookup = BuildProductLookup(productsTask.Result);
                var userLookup = BuildUserLookup(usersTask.Result);

                var reviewItems = (reviews ?? new List<Review>())
                    .OrderByDescending(r => r.ReviewDate)
                    .Select(r => new ReviewDisplayItem(
                        r,
                        GetLookupValue(productLookup, r.ProductId),
                        GetLookupValue(userLookup, r.UserId)))
                    .ToList();

                AllReviews = new ObservableCollection<ReviewDisplayItem>(reviewItems);
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

        // Loc theo so sao va tu khoa tim kiem
        private void ApplyFilter()
        {
            if (AllReviews == null) return;

            var result = AllReviews.AsEnumerable();


            int? starFilter = GetSelectedStarFilter();
            if (starFilter.HasValue)
            {
                result = result.Where(r => r.Rating == starFilter.Value);
            }


            string searchText = SearchBox?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string searchTextLower = searchText.ToLower();
                result = result.Where(r =>
                    (r.ProductId?.ToLower().Contains(searchTextLower) ?? false) ||
                    (r.ProductName?.ToLower().Contains(searchTextLower) ?? false) ||
                    (r.UserId?.ToLower().Contains(searchTextLower) ?? false) ||
                    (r.UserName?.ToLower().Contains(searchTextLower) ?? false) ||
                    (r.Comment?.ToLower().Contains(searchTextLower) ?? false)
                );
            }


            FilteredReviews.Clear();
            foreach (var item in result)
            {
                FilteredReviews.Add(item);
            }

            TotalReviews = FilteredReviews.Count;


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

        // Xac nhan va xoa review da chon
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var review = button?.Tag as ReviewDisplayItem;

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

        // Tao lookup ProductId -> ProductName de hien thi nhanh
        private static Dictionary<string, string> BuildProductLookup(List<Product> products)
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (products == null) return lookup;

            foreach (var product in products)
            {
                if (product == null || string.IsNullOrWhiteSpace(product.ProductId)) continue;
                if (!lookup.ContainsKey(product.ProductId))
                {
                    lookup[product.ProductId] = product.Name ?? string.Empty;
                }
            }

            return lookup;
        }

        // Tao lookup UserId -> UserName de hien thi nhanh
        private static Dictionary<string, string> BuildUserLookup(ObservableCollection<User> users)
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (users == null) return lookup;

            foreach (var user in users)
            {
                if (user == null || string.IsNullOrWhiteSpace(user.UserId)) continue;
                if (!lookup.ContainsKey(user.UserId))
                {
                    lookup[user.UserId] = user.FullName ?? string.Empty;
                }
            }

            return lookup;
        }

        private static string GetLookupValue(Dictionary<string, string> lookup, string key)
        {
            if (lookup == null || string.IsNullOrWhiteSpace(key)) return string.Empty;
            return lookup.TryGetValue(key, out var value) ? value ?? string.Empty : string.Empty;
        }
    }

    public class ReviewDisplayItem
    {
        public ReviewDisplayItem(Review review, string productName, string userName)
        {
            if (review == null) throw new ArgumentNullException(nameof(review));

            ReviewId = review.ReviewId;
            ProductId = review.ProductId;
            UserId = review.UserId;
            Rating = review.Rating;
            Comment = review.Comment;
            ReviewDate = review.ReviewDate;
            ProductName = productName ?? string.Empty;
            UserName = userName ?? string.Empty;
        }

        public string ReviewId { get; }
        public string ProductId { get; }
        public string ProductName { get; }
        public string UserId { get; }
        public string UserName { get; }
        public int Rating { get; }
        public string Comment { get; }
        public DateTime ReviewDate { get; }
        public IEnumerable<int> RatingStars => Enumerable.Range(1, Math.Max(0, Rating));
    }
}
