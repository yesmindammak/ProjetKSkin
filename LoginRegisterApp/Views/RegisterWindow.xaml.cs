using System.Linq;
using System.Windows;
using Microsoft.Data.SqlClient;
using LoginRegisterApp.Helpers;
using LoginRegisterApp.Services;

namespace LoginRegisterApp
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            string name = NameBox.Text.Trim();
            string username = UsernameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
            {
                ShowError("Please fill in all fields.");
                return;
            }

            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();

                // Check uniqueness before inserting, so we can give a clear error message.
                const string checkQuery = @"
                    SELECT COUNT(*) FROM Users
                    WHERE Username = @Username OR Email = @Email OR PhoneNumber = @Phone";

                using (var checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@Username", username);
                    checkCommand.Parameters.AddWithValue("@Email", email);
                    checkCommand.Parameters.AddWithValue("@Phone", phone);

                    int existingCount = (int)checkCommand.ExecuteScalar();
                    if (existingCount > 0)
                    {
                        ShowError("Username, email or phone number is already in use.");
                        return;
                    }
                }

                // PasswordBox exposes .Password, not .Text - Text doesn't exist on PasswordBox.
                string typedPassword = PasswordBox.Password.Trim();

                bool userTypedOwnPassword = !string.IsNullOrWhiteSpace(typedPassword);

                if (userTypedOwnPassword && !IsPasswordStrongEnough(typedPassword, out string passwordError))
                {
                    ShowError(passwordError);
                    return;
                }

                // If the user typed a password, use it. Otherwise auto-generate one.
                string finalPassword = userTypedOwnPassword
                    ? typedPassword
                    : PasswordHelper.GenerateRandomPassword();

                string hashedPassword = PasswordHelper.HashPassword(finalPassword);

                // GeneratedPassword column only records the password when it was auto-generated
                // (so you can see who's still on a system-issued password vs a self-chosen one).
                // It's hashed the same way as Password - we never store any password as plain text.
                // If the user chose their own, we leave this column NULL.
                string? hashedGeneratedPassword = userTypedOwnPassword
                    ? null
                    : PasswordHelper.HashPassword(finalPassword);

                // Self-registration: Actif (the account works) but NonValide (restricted access
                // until an admin validates it) - see cahier des charges 5.1. This is different
                // from an admin-created account, which would be Actif + Valide directly.
                const string insertQuery = @"
                    INSERT INTO Users
                        (Username, Name, Password, PhoneNumber, GeneratedPassword, Email, Role, StatutActivation, StatutValidation)
                    VALUES
                        (@Username, @Name, @Password, @Phone, @GeneratedPassword, @Email, 'Client', 'Actif', 'NonValide')";

                using var insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@Username", username);
                insertCommand.Parameters.AddWithValue("@Name", name);
                insertCommand.Parameters.AddWithValue("@Password", hashedPassword);
                insertCommand.Parameters.AddWithValue("@Phone", phone);
                insertCommand.Parameters.AddWithValue("@GeneratedPassword",
                    (object?)hashedGeneratedPassword ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Email", email);

                insertCommand.ExecuteNonQuery();

                // Self-registration: admins need to know a new account is waiting (5.1/5.3).
                NotificationService.NotifierNouveauCompteEnAttente(username);

                string confirmationMessage = userTypedOwnPassword
                    ? "Account created! Your account is pending validation by an administrator. You can log in with the password you chose."
                    : $"Account created!\n\nYour password is: {finalPassword}\n\nPlease save it, you'll need it to log in. Your account is pending validation by an administrator.";

                MessageBox.Show(confirmationMessage, "Registration successful",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
            catch (SqlException ex)
            {
                ShowError("Database error: " + ex.Message);
            }
        }

        private void GoToLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        // Minimum 6 characters, at least one uppercase letter.
        // Add more rules here later (digit, special char, etc.) if you want it stricter.
        private bool IsPasswordStrongEnough(string password, out string error)
        {
            if (password.Length < 6)
            {
                error = "Your password must be at least 6 characters long.";
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                error = "Your password must contain at least one uppercase letter.";
                return false;
            }

            error = "";
            return true;
        }
    }
}
