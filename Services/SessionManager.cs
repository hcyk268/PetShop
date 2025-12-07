using Pet_Shop_Project.Models;

namespace Pet_Shop_Project.Services
{
    public static class SessionManager
    {
        private static User _currentUser;

        public static User CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        public static bool IsLoggedIn
        {
            get { return _currentUser != null; }
        }

        public static void Logout()
        {
            _currentUser = null;
        }

        public static void UpdateUserInfo(User updatedUser)
        {
            if (_currentUser != null && _currentUser.UserId == updatedUser.UserId)
            {
                _currentUser = updatedUser;
            }
        }
    }
}