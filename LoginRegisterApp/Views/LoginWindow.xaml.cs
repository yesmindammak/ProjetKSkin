using System.Windows;
using Microsoft.Data.SqlClient;
using LoginRegisterApp.Helpers;
using LoginRegisterApp.Services;

namespace LoginRegisterApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();

                // Parameterized query - prevents SQL injection.
                const string query = @"
                    SELECT Password, StatutActivation, StatutValidation, Role
                    FROM Users
                    WHERE Username = @Username";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                using SqlDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    ShowError("No account found with this username.");
                    return;
                }

                string storedHash = reader.GetString(0);
                string statutActivation = reader.GetString(1);
                string statutValidation = reader.GetString(2);
                string role = reader.GetString(3);

                if (!PasswordHelper.VerifyPassword(password, storedHash))
                {
                    ShowError("Incorrect password.");
                    return;
                }

                // Desactive = account was turned off by the admin - blocks login entirely.
                // This is separate from "not yet validated", which still gets in (see below).
                if (statutActivation != "Actif")
                {
                    ShowError("Your account is deactivated. Contact an administrator.");
                    return;
                }

                bool estValide = statutValidation == "Valide";

                if (!estValide)
                {
                    // Not blocked - self-registered accounts get restricted access
                    // until the admin validates them, they don't get locked out entirely.
                    MessageBox.Show(
                        "Your account has not been validated yet. Please contact an administrator.\n\nYou can browse the catalogue, but purchase requests are disabled until your account is validated.",
                        "Account pending validation",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Housekeeping: regenerate passwords for anyone whose expiration date has
                // passed (5.2). Wrapped so a failure here never blocks this person's own login.
                try { PasswordExpirationService.RegenererMotsDePasseExpires(); }
                catch { /* best-effort - the next successful login will retry it */ }

                var mainWindow = new MainWindow(username, role, estValide);
                mainWindow.Show();
                this.Close();
            }
            catch (SqlException ex)
            {
                ShowError("Database error: " + ex.Message);
            }
        }

        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
