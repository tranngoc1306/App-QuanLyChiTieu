using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace ExpenseTracker.Services
{
    public static class LocalizationService
    {
        public static string CurrentLanguage
        {
            get => Preferences.Get("AppLanguage", "vi");
            set => Preferences.Set("AppLanguage", value);
        }

        private static Dictionary<string, Dictionary<string, string>> _translations = new()
        {
            ["vi"] = new Dictionary<string, string>
            {
                // Common
                ["OK"] = "OK",
                ["Cancel"] = "Hủy",
                ["Save"] = "Lưu",
                ["Delete"] = "Xóa",
                ["Edit"] = "Chỉnh sửa",
                ["Add"] = "Thêm",
                ["Back"] = "Quay lại",
                ["Confirm"] = "Xác nhận",
                ["Success"] = "Thành công",
                ["Error"] = "Lỗi",

                // Settings
                ["Settings"] = "Cài đặt",
                ["Account"] = "TÀI KHOẢN",
                ["ChangePassword"] = "Đổi mật khẩu",
                ["Language"] = "Ngôn ngữ",
                ["Currency"] = "Tiền tệ",
                ["Data"] = "DỮ LIỆU",
                ["ExportData"] = "Xuất dữ liệu",
                ["ManageCategories"] = "Quản lý danh mục",
                ["About"] = "VỀ ỨNG DỤNG",
                ["Version"] = "Phiên bản",
                ["AboutApp"] = "Giới thiệu",
                ["Logout"] = "Đăng xuất",
                ["DeleteAccount"] = "Xóa tài khoản",
                ["EditProfile"] = "Chỉnh sửa hồ sơ",

                // Edit Profile
                ["DisplayName"] = "Tên hiển thị",
                ["ChooseAvatar"] = "Chọn avatar",
                ["EnterName"] = "Vui lòng nhập tên",
                ["UpdateSuccess"] = "Cập nhật thành công",
                ["UpdateFailed"] = "Cập nhật thất bại",

                // Change Password
                ["CurrentPassword"] = "Mật khẩu hiện tại",
                ["NewPassword"] = "Mật khẩu mới",
                ["ConfirmPassword"] = "Xác nhận mật khẩu mới",
                ["EnterCurrentPassword"] = "Vui lòng nhập mật khẩu hiện tại",
                ["EnterNewPassword"] = "Vui lòng nhập mật khẩu mới",
                ["PasswordMinLength"] = "Mật khẩu mới phải có ít nhất 6 ký tự",
                ["PasswordNotMatch"] = "Mật khẩu xác nhận không khớp",
                ["PasswordSameAsCurrent"] = "Mật khẩu mới phải khác mật khẩu hiện tại",
                ["PasswordIncorrect"] = "Mật khẩu hiện tại không đúng",
                ["PasswordChangeSuccess"] = "Đã đổi mật khẩu thành công",

                // Logout
                ["LogoutConfirm"] = "Bạn có chắc muốn đăng xuất?",

                // Delete Account
                ["DeleteAccountConfirm"] = "Bạn có chắc muốn xóa tài khoản?\n\nTất cả dữ liệu của bạn sẽ bị xóa vĩnh viễn và không thể khôi phục.",
                ["DeleteAccountFinal"] = "XÓA TÀI KHOẢN",
                ["DeleteAccountSuccess"] = "Tài khoản đã được xóa",
                ["DeleteAccountFailed"] = "Không thể xóa tài khoản",

                // Export Data
                ["ExportSuccess"] = "Xuất dữ liệu thành công",
                ["ExportFailed"] = "Xuất dữ liệu thất bại",
                ["NoDataToExport"] = "Không có dữ liệu để xuất",
                ["ExportMessage"] = "Dữ liệu đã được lưu vào thư mục Downloads",

                // Manage Categories
                ["ManageCategoriesTitle"] = "Quản lý danh mục",
                ["ExpenseCategories"] = "Danh mục chi tiêu",
                ["IncomeCategories"] = "Danh mục thu nhập",
                ["AddCategory"] = "Thêm danh mục",
                ["EditCategory"] = "Chỉnh sửa danh mục",
                ["DeleteCategory"] = "Xóa danh mục",
                ["CategoryName"] = "Tên danh mục",
                ["CategoryIcon"] = "Icon",
                ["CategoryColor"] = "Màu sắc",
                ["SelectIcon"] = "Chọn icon",
                ["SelectColor"] = "Chọn màu",
                ["DeleteCategoryConfirm"] = "Bạn có chắc muốn xóa danh mục này?",
                ["CategoryNameRequired"] = "Vui lòng nhập tên danh mục",
                ["CategorySaveSuccess"] = "Lưu danh mục thành công",
                ["CategoryDeleteSuccess"] = "Xóa danh mục thành công"
            },
            ["en"] = new Dictionary<string, string>
            {
                // Common
                ["OK"] = "OK",
                ["Cancel"] = "Cancel",
                ["Save"] = "Save",
                ["Delete"] = "Delete",
                ["Edit"] = "Edit",
                ["Add"] = "Add",
                ["Back"] = "Back",
                ["Confirm"] = "Confirm",
                ["Success"] = "Success",
                ["Error"] = "Error",

                // Settings
                ["Settings"] = "Settings",
                ["Account"] = "ACCOUNT",
                ["ChangePassword"] = "Change Password",
                ["Language"] = "Language",
                ["Currency"] = "Currency",
                ["Data"] = "DATA",
                ["ExportData"] = "Export Data",
                ["ManageCategories"] = "Manage Categories",
                ["About"] = "ABOUT",
                ["Version"] = "Version",
                ["AboutApp"] = "About",
                ["Logout"] = "Logout",
                ["DeleteAccount"] = "Delete Account",
                ["EditProfile"] = "Edit Profile",

                // Edit Profile
                ["DisplayName"] = "Display Name",
                ["ChooseAvatar"] = "Choose Avatar",
                ["EnterName"] = "Please enter name",
                ["UpdateSuccess"] = "Update successful",
                ["UpdateFailed"] = "Update failed",

                // Change Password
                ["CurrentPassword"] = "Current Password",
                ["NewPassword"] = "New Password",
                ["ConfirmPassword"] = "Confirm New Password",
                ["EnterCurrentPassword"] = "Please enter current password",
                ["EnterNewPassword"] = "Please enter new password",
                ["PasswordMinLength"] = "New password must be at least 6 characters",
                ["PasswordNotMatch"] = "Confirm password does not match",
                ["PasswordSameAsCurrent"] = "New password must be different from current password",
                ["PasswordIncorrect"] = "Current password is incorrect",
                ["PasswordChangeSuccess"] = "Password changed successfully",

                // Logout
                ["LogoutConfirm"] = "Are you sure you want to logout?",

                // Delete Account
                ["DeleteAccountConfirm"] = "Are you sure you want to delete your account?\n\nAll your data will be permanently deleted and cannot be recovered.",
                ["DeleteAccountFinal"] = "DELETE ACCOUNT",
                ["DeleteAccountSuccess"] = "Account deleted",
                ["DeleteAccountFailed"] = "Cannot delete account",

                // Export Data
                ["ExportSuccess"] = "Data exported successfully",
                ["ExportFailed"] = "Export data failed",
                ["NoDataToExport"] = "No data to export",
                ["ExportMessage"] = "Data saved to Downloads folder",

                // Manage Categories
                ["ManageCategoriesTitle"] = "Manage Categories",
                ["ExpenseCategories"] = "Expense Categories",
                ["IncomeCategories"] = "Income Categories",
                ["AddCategory"] = "Add Category",
                ["EditCategory"] = "Edit Category",
                ["DeleteCategory"] = "Delete Category",
                ["CategoryName"] = "Category Name",
                ["CategoryIcon"] = "Icon",
                ["CategoryColor"] = "Color",
                ["SelectIcon"] = "Select Icon",
                ["SelectColor"] = "Select Color",
                ["DeleteCategoryConfirm"] = "Are you sure you want to delete this category?",
                ["CategoryNameRequired"] = "Please enter category name",
                ["CategorySaveSuccess"] = "Category saved successfully",
                ["CategoryDeleteSuccess"] = "Category deleted successfully"
            }
        };

        public static string GetString(string key)
        {
            var lang = CurrentLanguage;
            if (_translations.ContainsKey(lang) && _translations[lang].ContainsKey(key))
            {
                return _translations[lang][key];
            }
            return key;
        }

        public static void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;
        }
    }
}