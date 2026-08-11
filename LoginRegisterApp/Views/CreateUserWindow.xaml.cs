using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LoginRegisterApp.Data;
using LoginRegisterApp.Helpers;

namespace LoginRegisterApp
{
    public partial class CreateUserWindow : Window
    {
        // Set to true right before closing when the account was actually created,
        // so MainWindow knows whether to reload the grid.
        public bool UserWasCreated { get; private set; }

        public CreateUserWindow()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            string name = NameBox.Text.Trim();
            string username = UsernameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string role = ((ComboBoxItem)RoleCombo.SelectedItem).Tag.ToString()!;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
            {
                ShowError("Merci de remplir tous les champs.");
                return;
            }

            try
            {
                if (UserRepository.ExistsByUsernameEmailOrPhone(username, email, phone))
                {
                    ShowError("Ce nom d'utilisateur, cet email ou ce téléphone est déjà utilisé.");
                    return;
                }

                // Admin-created accounts always get a system-generated password (5.1) -
                // there's no "choose your own" option here, unlike self-registration.
                string generatedPassword = PasswordHelper.GenerateRandomPassword();
                string hashed = PasswordHelper.HashPassword(generatedPassword);

                UserRepository.Create(username, name, email, phone, role, hashed);

                MessageBox.Show(
                    $"Compte créé avec succès.\n\nMot de passe généré : {generatedPassword}\n\nCommuniquez-le à l'utilisateur - il ne sera plus affiché après cette fenêtre.",
                    "Utilisateur créé", MessageBoxButton.OK, MessageBoxImage.Information);

                UserWasCreated = true;
                Close();
            }
            catch (System.Exception ex)
            {
                ShowError("Erreur lors de la création : " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
