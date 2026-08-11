using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoginRegisterApp.Models
{
    public class UserRow : INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Role { get; set; } = "";

        private string _statutActivation = "Actif";
        public string StatutActivation
        {
            get => _statutActivation;
            set { _statutActivation = value; OnPropertyChanged(); }
        }

        private string _statutValidation = "NonValide";
        public string StatutValidation
        {
            get => _statutValidation;
            set { _statutValidation = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
