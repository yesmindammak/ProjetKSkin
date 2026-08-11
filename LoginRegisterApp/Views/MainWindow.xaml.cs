using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using LoginRegisterApp.Data;
using LoginRegisterApp.Models;
using LoginRegisterApp.Services;

namespace LoginRegisterApp
{
    public class ProduitCardItem
    {
        public Produit Produit { get; set; } = new Produit();
        public bool CanCreateDemande { get; set; }

        public int ProduitId { get => Produit.ProduitId; set => Produit.ProduitId = value; }
        public string Nom { get => Produit.Nom; set => Produit.Nom = value; }
        public decimal Prix { get => Produit.Prix; set => Produit.Prix = value; }
        public int Stock { get => Produit.Stock; set => Produit.Stock = value; }
        public string? ImageUrl { get => Produit.ImageUrl; set => Produit.ImageUrl = value; }
        public string? Description { get => Produit.Description; set => Produit.Description = value; }
        public string? Ingredients { get => Produit.Ingredients; set => Produit.Ingredients = value; }
        public string? Type { get => Produit.Type; set => Produit.Type = value; }
        public string? Couleur { get => Produit.Couleur; set => Produit.Couleur = value; }
        public string? Marque { get => Produit.Marque; set => Produit.Marque = value; }
    }

    public partial class MainWindow : Window
    {
        private readonly string _username;
        private readonly string _role;
        private readonly bool _estValide;
        private readonly int _currentUserId;

        private List<Produit> _allProducts = new List<Produit>();
        private string _activeCategoryFilter = "";
        private string _activeSearchText = "";
        private Produit? _selectedProductForModal;

        public MainWindow(string username, string role, bool estValide)
        {
            InitializeComponent();

            _username = username;
            _role = role;
            _estValide = estValide;
            _currentUserId = UserRepository.GetUserId(_username);

            WelcomeText.Text = $"Bienvenue, {_username} 👋";
            ConfigureForRole();
            ConfigureForValidation();

            LoadDashboardStats();
        }

        private void ConfigureForRole()
        {
            bool isAdmin = _role == "Admin";

            RoleBadgeText.Text = _role switch
            {
                "Admin" => "Administrateur",
                "SuperviseurAchat" => "Superviseur d'achat",
                _ => "Utilisateur Commercial",
            };

            RoleBadge.Background = _role switch
            {
                "Admin" => (Brush)FindResource("AccentGoldBrush"),
                "SuperviseurAchat" => (Brush)FindResource("DangerBrush"),
                _ => (Brush)FindResource("PrimaryBrush"),
            };

            NavUsers.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

           
          
            
            NavDemandes.Visibility = isAdmin ? Visibility.Collapsed : Visibility.Visible;

            TopNewDemandeBtn.Visibility = isAdmin ? Visibility.Collapsed : Visibility.Visible;
            ModalOrderButton.Visibility = isAdmin ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ConfigureForValidation()
        {
            bool canCreateNewDemande = !_role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && _estValide;
            TopNewDemandeBtn.Visibility = canCreateNewDemande ? Visibility.Visible : Visibility.Collapsed;
            ModalOrderButton.Visibility = canCreateNewDemande ? Visibility.Visible : Visibility.Collapsed;
            NavDemandes.Visibility = canCreateNewDemande ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadDashboardStats()
        {
            try
            {
                ProductSyncService.Synchroniser();
                _allProducts = ProduitRepository.GetAll();

                StatProduitsCount.Text = $"{_allProducts.Count} produits";

                var demandes = (_role == "Admin" || _role == "SuperviseurAchat")
                    ? DemandeAchatRepository.GetAllDisplay()
                    : DemandeAchatRepository.GetByUserDisplay(_currentUserId);
                StatDemandesCount.Text = $"{demandes.Count} demande(s)";

                var contacts = ContactRepository.GetByUser(_currentUserId);
                StatContactsCount.Text = $"{contacts.Count} contact(s)";

                var notifications = NotificationRepository.GetForUserDisplay(_currentUserId);
                if (notifications.Count > 0)
                {
                    DashboardNotifText.Text = $"{notifications[0].Objet} : {notifications[0].Contenu}";
                }

                LoadMarquesShowcase();
                ApplyProductFilters();
            }
            catch (Exception ex)
            {
                DashboardNotifText.Text = "Erreur de chargement : " + ex.Message;
            }
        }

        private void LoadMarquesShowcase()
        {
            // Strictly DB-only brands!
            var marquesFromDb = ProduitRepository.GetDistinctMarques();

            if (marquesFromDb != null && marquesFromDb.Count > 0)
            {
                NoMarqueWarningText.Visibility = Visibility.Collapsed;
                MarquesItemsControl.Visibility = Visibility.Visible;
                MarquesItemsControl.ItemsSource = marquesFromDb;
            }
            else
            {
                // No hardcoded brands generated by AI - purely wait for database rows inserted via SSMS or sync!
                NoMarqueWarningText.Visibility = Visibility.Visible;
                MarquesItemsControl.Visibility = Visibility.Collapsed;
                MarquesItemsControl.ItemsSource = null;
            }
        }

        private void ApplyProductFilters()
        {
            var filtered = _allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_activeCategoryFilter))
            {
                filtered = filtered.Where(p =>
                    (p.Nom != null && p.Nom.Contains(_activeCategoryFilter, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Type != null && p.Type.Contains(_activeCategoryFilter, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Description != null && p.Description.Contains(_activeCategoryFilter, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(_activeSearchText))
            {
                filtered = filtered.Where(p =>
                    (p.Nom != null && p.Nom.Contains(_activeSearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Marque != null && p.Marque.Contains(_activeSearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Ingredients != null && p.Ingredients.Contains(_activeSearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Description != null && p.Description.Contains(_activeSearchText, StringComparison.OrdinalIgnoreCase)));
            }

            bool canCreate = _role != "Admin";
            var cardItems = filtered.Select(p => new ProduitCardItem
            {
                Produit = p,
                CanCreateDemande = canCreate
            }).ToList();

            ProduitsItemsControl.ItemsSource = cardItems;
        }

        private void BrandCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.Tag != null)
            {
                string brandName = elem.Tag.ToString() ?? "";
                _activeCategoryFilter = "";
                _activeSearchText = brandName;
                SearchBox.Text = brandName;
                CatalogueTitleText.Text = $"Produits de la marque : {brandName}";

                // Switch to Catalogue view!
                SwitchToNav(NavCatalogue, CataloguePanel);
                ApplyProductFilters();
            }
        }

        private void CategoryFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                _activeCategoryFilter = btn.Tag?.ToString() ?? "";

                foreach (var child in new[] { CatAllBtn, CatCleanserBtn, CatSerumBtn, CatMoisturizerBtn, CatSunBtn })
                {
                    if (child != null)
                        child.Foreground = (Brush)FindResource(child == btn ? "PrimaryDarkBrush" : "TextDarkBrush");
                }

                CatalogueTitleText.Text = string.IsNullOrEmpty(_activeCategoryFilter)
                    ? "Catalogue complet des produits"
                    : $"Produits : {_activeCategoryFilter}";

                // Switch to Catalogue view!
                SwitchToNav(NavCatalogue, CataloguePanel);
                ApplyProductFilters();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _activeSearchText = SearchBox.Text.Trim();
            if (!string.IsNullOrEmpty(_activeSearchText) && CataloguePanel.Visibility != Visibility.Visible)
            {
                SwitchToNav(NavCatalogue, CataloguePanel);
            }
            ApplyProductFilters();
        }

        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            _activeCategoryFilter = "";
            _activeSearchText = "";
            SearchBox.Text = "";
            SwitchToNav(NavDashboard, DashboardPanel);
            LoadDashboardStats();
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            var clicked = (ToggleButton)sender;

            if (clicked == NavDashboard)
            {
                SwitchToNav(NavDashboard, DashboardPanel);
                LoadDashboardStats();
            }
            else if (clicked == NavCatalogue)
            {
                SwitchToNav(NavCatalogue, CataloguePanel);
                LoadProduits();
            }
            else if (clicked == NavDemandes)
            {
                if (_role == "Admin") return; // Admin has no access
                SwitchToNav(NavDemandes, DemandesPanel);
                LoadDemandes();
            }
            else if (clicked == NavNotifications)
            {
                SwitchToNav(NavNotifications, NotificationsPanel);
                LoadNotifications();
            }
            else if (clicked == NavUsers)
            {
                SwitchToNav(NavUsers, UsersPanel);
                LoadUsers();
                LoadExpirationSetting();
            }
        }

        private void SwitchToNav(ToggleButton targetButton, FrameworkElement? targetPanel)
        {
            foreach (var toggle in new[] { NavDashboard, NavCatalogue, NavDemandes, NavNotifications, NavUsers })
            {
                if (toggle != null)
                    toggle.IsChecked = toggle == targetButton;
            }

            DashboardPanel.Visibility = Visibility.Collapsed;
            CataloguePanel.Visibility = Visibility.Collapsed;
            DemandesPanel.Visibility = Visibility.Collapsed;
            NotificationsPanel.Visibility = Visibility.Collapsed;
            UsersPanel.Visibility = Visibility.Collapsed;

            if (targetPanel != null)
            {
                targetPanel.Visibility = Visibility.Visible;
            }
        }

        // ==================== CATALOGUE & PRODUCT DETAILS ====================

        private void LoadProduits()
        {
            try
            {
                _allProducts = ProduitRepository.GetAll();
                ApplyProductFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de charger le catalogue : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        private void ProductCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elem)
            {
                if (elem.DataContext is ProduitCardItem cardItem)
                    OpenProductModal(cardItem.Produit);
                else if (elem.DataContext is Produit p)
                    OpenProductModal(p);
            }
        }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int produitId)
            {
                var produit = ProduitRepository.GetById(produitId);
                if (produit != null)
                    OpenProductModal(produit);
            }
        }

        private void QuickOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Admin") return;

            if (sender is Button btn && btn.Tag is int produitId)
            {
                var produit = ProduitRepository.GetById(produitId);
                if (produit != null)
                    OpenNewDemandeModal(produit);
            }
        }

        private void OpenProductModal(Produit produit)
        {
            _selectedProductForModal = produit;
            DetailProductName.Text = produit.Nom;
            DetailProductPrice.Text = $"{produit.Prix:0.00} DT";
            DetailProductStock.Text = produit.Stock.ToString();
            DetailProductDescription.Text = string.IsNullOrWhiteSpace(produit.Description) ? "Aucune description." : produit.Description;
            DetailProductIngredients.Text = string.IsNullOrWhiteSpace(produit.Ingredients) ? "Non spécifié." : produit.Ingredients;

            if (!string.IsNullOrWhiteSpace(produit.ImageUrl))
            {
                try
                {
                    DetailProductImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(produit.ImageUrl, UriKind.RelativeOrAbsolute));
                }
                catch
                {
                    DetailProductImage.Source = null;
                }
            }
            else
            {
                DetailProductImage.Source = null;
            }

            // Load Disponibilité par Point de Vente
            try
            {
                var pdvList = PointDeVenteRepository.GetDisponibilitesForProduit(produit.ProduitId);
                PointDeVenteItemsControl.ItemsSource = pdvList;
            }
            catch
            {
                PointDeVenteItemsControl.ItemsSource = null;
            }

            ProductDetailModal.Visibility = Visibility.Visible;
        }

        private void CloseProductDetail_Click(object sender, RoutedEventArgs e)
        {
            ProductDetailModal.Visibility = Visibility.Collapsed;
        }

        private void OrderThisProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Admin") return;
            ProductDetailModal.Visibility = Visibility.Collapsed;
            OpenNewDemandeModal(_selectedProductForModal);
        }

        // ==================== DEMANDES D'ACHAT ====================

        private void LoadDemandes()
        {
            try
            {
                var list = (_role == "SuperviseurAchat" || _role == "Admin")
                    ? DemandeAchatRepository.GetAllDisplay()
                    : DemandeAchatRepository.GetAllDisplayForUserAndPortal(_currentUserId);

                DemandesGrid.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des demandes : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewDemandeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Admin" || !_estValide) return;
            OpenNewDemandeModal(null);
        }

        private void OpenNewDemandeModal(Produit? preselectedProduct)
        {
            if (_role == "Admin" || !_estValide) return;

            DemandeErrorText.Visibility = Visibility.Collapsed;
            ClientInfoBadgeText.Text = "Entrez un téléphone ou un nom pour rechercher un contact existant.";
            ClientInfoBadgeText.Foreground = (Brush)FindResource("TextMutedBrush");

            ClientPhoneBox.Clear();
            ClientNomBox.Clear();
            ClientPrenomBox.Clear();
            ClientEmailBox.Clear();
            ClientVilleBox.Clear();
            ClientAdresseBox.Clear();
            ClientGouvernoratCombo.SelectedIndex = 0;
            DemandeQuantiteBox.Text = "1";

            var produits = ProduitRepository.GetAll();
            DemandeProduitCombo.ItemsSource = produits;

            var pointsDeVente = PointDeVenteRepository.GetAllPointsDeVente();
            DemandePdvCombo.ItemsSource = pointsDeVente;
            if (pointsDeVente.Count > 0) DemandePdvCombo.SelectedIndex = 0;

            if (preselectedProduct != null)
                DemandeProduitCombo.SelectedValue = preselectedProduct.ProduitId;
            else if (produits.Count > 0)
                DemandeProduitCombo.SelectedIndex = 0;

            NewDemandeModal.Visibility = Visibility.Visible;
        }

        private void CloseNewDemandeModal_Click(object sender, RoutedEventArgs e)
        {
            NewDemandeModal.Visibility = Visibility.Collapsed;
        }

        private void ClientContactInput_Changed(object sender, RoutedEventArgs e)
        {
            string phone = ClientPhoneBox.Text.Trim();
            string nom = ClientNomBox.Text.Trim();
            string prenom = ClientPrenomBox.Text.Trim();

            Contact? existing = null;

            if (!string.IsNullOrWhiteSpace(phone))
            {
                existing = ContactRepository.FindByPhone(phone);
            }

            if (existing == null && !string.IsNullOrWhiteSpace(nom) && !string.IsNullOrWhiteSpace(prenom))
            {
                existing = ContactRepository.FindByName(nom, prenom);
            }

            if (existing != null)
            {
                if (string.IsNullOrWhiteSpace(ClientNomBox.Text)) ClientNomBox.Text = existing.Nom;
                if (string.IsNullOrWhiteSpace(ClientPrenomBox.Text)) ClientPrenomBox.Text = existing.Prenom;
                if (string.IsNullOrWhiteSpace(ClientPhoneBox.Text)) ClientPhoneBox.Text = existing.Telephone;
                if (string.IsNullOrWhiteSpace(ClientEmailBox.Text)) ClientEmailBox.Text = existing.Email ?? "";
                if (string.IsNullOrWhiteSpace(ClientVilleBox.Text)) ClientVilleBox.Text = existing.Ville ?? "";
                if (string.IsNullOrWhiteSpace(ClientAdresseBox.Text)) ClientAdresseBox.Text = existing.Adresse ?? "";

                if (!string.IsNullOrWhiteSpace(existing.Gouvernorat))
                {
                    foreach (ComboBoxItem item in ClientGouvernoratCombo.Items)
                    {
                        if (item.Content?.ToString()?.Equals(existing.Gouvernorat, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            ClientGouvernoratCombo.SelectedItem = item;
                            break;
                        }
                    }
                }

                ClientInfoBadgeText.Text = $"✓ Client existant sélectionné dans la base de données : {existing.Nom} {existing.Prenom} (#{existing.ContactId})";
                ClientInfoBadgeText.Foreground = (Brush)FindResource("PrimaryDarkBrush");
            }
        }

        private void DemandeLivraisonCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemandeLivraisonCombo == null || DemandePdvCombo == null || DemandePaiementCombo == null) return;

            string mode = (DemandeLivraisonCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            if (mode.Contains("Express", StringComparison.OrdinalIgnoreCase))
            {
                foreach (PointDeVente pdv in DemandePdvCombo.Items)
                {
                    if (pdv.Nom.Contains("En Ligne", StringComparison.OrdinalIgnoreCase))
                    {
                        DemandePdvCombo.SelectedItem = pdv;
                        break;
                    }
                }
                foreach (ComboBoxItem item in DemandePaiementCombo.Items)
                {
                    if (item.Content?.ToString()?.Contains("Carte", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        DemandePaiementCombo.SelectedItem = item;
                        break;
                    }
                }
            }
            else if (mode.Contains("Domicile", StringComparison.OrdinalIgnoreCase))
            {
                AutoSelectBestStoreForLocation();
            }
        }

        private void ClientLocation_Changed(object sender, EventArgs e)
        {
            if (DemandeLivraisonCombo == null || DemandePdvCombo == null) return;
            string mode = (DemandeLivraisonCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            if (mode.Contains("Domicile", StringComparison.OrdinalIgnoreCase))
            {
                AutoSelectBestStoreForLocation();
            }
        }

        private void AutoSelectBestStoreForLocation()
        {
            if (DemandePdvCombo == null || DemandeProduitCombo == null) return;

            string gouvernorat = (ClientGouvernoratCombo?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string ville = ClientVilleBox?.Text?.Trim() ?? "";
            int produitId = (DemandeProduitCombo.SelectedValue is int pId) ? pId : 0;

            int bestPdvId = PointDeVenteRepository.ResolveBestPointDeVente(produitId, "Livraison à Domicile", gouvernorat, ville, "", null);

            foreach (PointDeVente pdv in DemandePdvCombo.Items)
            {
                if (pdv.PointDeVenteId == bestPdvId)
                {
                    DemandePdvCombo.SelectedItem = pdv;
                    break;
                }
            }
        }

        private void SubmitDemande_Click(object sender, RoutedEventArgs e)
        {
            if (_role == "Admin") return;
            DemandeErrorText.Visibility = Visibility.Collapsed;

            if (DemandeProduitCombo.SelectedValue == null)
            {
                ShowDemandeError("Veuillez sélectionner un produit.");
                return;
            }

            int produitId = (int)DemandeProduitCombo.SelectedValue;
            var produit = ProduitRepository.GetById(produitId);
            if (produit == null)
            {
                ShowDemandeError("Produit introuvable.");
                return;
            }

            if (!int.TryParse(DemandeQuantiteBox.Text.Trim(), out int quantite) || quantite <= 0)
            {
                ShowDemandeError("Veuillez saisir une quantité valide (> 0).");
                return;
            }

            if (produit.Stock < quantite)
            {
                ShowDemandeError($"Stock insuffisant ! Stock disponible actuel : {produit.Stock} unité(s).");
                return;
            }

            string phone = ClientPhoneBox.Text.Trim();
            string nom = ClientNomBox.Text.Trim();
            string prenom = ClientPrenomBox.Text.Trim();
            string email = ClientEmailBox.Text.Trim();
            string gouvernorat = (ClientGouvernoratCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tunis";
            string ville = ClientVilleBox.Text.Trim();
            string adresse = ClientAdresseBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(prenom))
            {
                ShowDemandeError("Veuillez remplir le nom, le prénom et le téléphone du client.");
                return;
            }

            try
            {
                var contact = ContactRepository.FindByPhone(phone) ?? ContactRepository.FindByName(nom, prenom);
                int contactId;
                if (contact != null)
                {
                    contactId = contact.ContactId;
                }
                else
                {
                    contactId = ContactRepository.Create(new Contact
                    {
                        Nom = nom,
                        Prenom = prenom,
                        Telephone = phone,
                        Email = string.IsNullOrWhiteSpace(email) ? null : email,
                        Gouvernorat = gouvernorat,
                        Ville = string.IsNullOrWhiteSpace(ville) ? null : ville,
                        Adresse = string.IsNullOrWhiteSpace(adresse) ? null : adresse,
                        CreePar = _currentUserId
                    });
                }

                string modeLivraison = (DemandeLivraisonCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Livraison à Domicile";
                string modePaiement = (DemandePaiementCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Paiement à la Livraison";
                int? selectedPdvId = DemandePdvCombo.SelectedValue as int?;

                int targetPdvId = PointDeVenteRepository.ResolveBestPointDeVente(produitId, modeLivraison, gouvernorat, ville, adresse, selectedPdvId);

                var demande = new DemandeAchat
                {
                    UtilisateurId = _currentUserId,
                    ContactId = contactId,
                    ProduitId = produitId,
                    PointDeVenteId = targetPdvId,
                    Quantite = quantite,
                    Origine = "Desktop",
                    ModeLivraison = modeLivraison,
                    ModePaiement = modePaiement
                };

                int demandeId = DemandeAchatRepository.Create(demande);
                demande.DemandeId = demandeId;

                NotificationService.NotifierDemandeCreee(demande, _username);

                NewDemandeModal.Visibility = Visibility.Collapsed;
                LoadDemandes();
                LoadDashboardStats();

                MessageBox.Show("Demande d'achat créée avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowDemandeError("Erreur lors de la création : " + ex.Message);
            }
        }

        private void ShowDemandeError(string msg)
        {
            DemandeErrorText.Text = msg;
            DemandeErrorText.Visibility = Visibility.Visible;
        }

        private void ValiderDemande_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int demandeId)
            {
                try
                {
                    var demande = DemandeAchatRepository.GetById(demandeId);
                    if (demande != null)
                    {
                        // 1. Atomic SQL Mutex Concurrency Protection: Try to deduct overall stock atomically in local DB
                        bool deducted = ProduitRepository.TryDeductStockAtomic(demande.ProduitId, demande.Quantite);
                        if (!deducted)
                        {
                            MessageBox.Show("Stock insuffisant ",
                                            "Avertissement Stock / Concurrence", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        int targetPdvId = demande.PointDeVenteId ?? PointDeVenteRepository.ResolveBestPointDeVente(demande.ProduitId, demande.ModeLivraison ?? "", null, null, null, null);

                        // 2. Deduct store level stock for assigned Point de Vente
                        PointDeVenteRepository.DeductStoreStockAtomic(demande.ProduitId, targetPdvId, demande.Quantite);

                        // 3. Synchronize store stock deduction with external DB & Recalculate overall product stock = SUM(Store Quantities)
                        var produit = ProduitRepository.GetById(demande.ProduitId);
                        string? pdvNom = PointDeVenteRepository.GetPointDeVenteNomById(targetPdvId);
                        if (produit != null && !string.IsNullOrEmpty(pdvNom))
                        {
                            PointDeVenteRepository.DeductExternalStoreStockAtomic(produit.ReferenceExterne, produit.Nom, pdvNom, demande.Quantite);
                            PointDeVenteRepository.RecalculateTotalStockForProduit(demande.ProduitId, produit.ReferenceExterne, produit.Nom);
                        }

                        DemandeAchatRepository.SetStatut(demandeId, "Validee");
                        LoadDemandes();
                        LoadDashboardStats();
                        MessageBox.Show("La demande d'achat a été validée !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefuserDemande_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int demandeId)
            {
                try
                {
                    DemandeAchatRepository.SetStatut(demandeId, "Refusee");
                    LoadDemandes();
                    LoadDashboardStats();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloturerDemande_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int demandeId)
            {
                try
                {
                    var demande = DemandeAchatRepository.GetById(demandeId);
                    if (demande != null)
                    {
                        DemandeAchatRepository.Cloturer(demandeId);
                        NotificationService.NotifierDemandeCloturee(demande, _username);
                        LoadDemandes();
                        LoadDashboardStats();
                        MessageBox.Show("La demande d'achat a été clôturée. Notifications envoyées.", "Clôture", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ==================== NOTIFICATIONS ====================

        private void LoadNotifications()
        {
            try
            {
                var notifs = NotificationRepository.GetForUserDisplay(_currentUserId);
                NotificationsGrid.ItemsSource = notifs;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des notifications : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MarkNotificationRead_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && int.TryParse(btn.Tag.ToString(), out int notifId))
            {
                try
                {
                    NotificationRepository.MarkAsRead(notifId);
                    LoadNotifications();
                    MessageBox.Show("La notification a été marquée comme lue avec succès !", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ==================== USERS & PASSWORD EXPIRATION CONFIG ====================

        private void NewUserButton_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateUserWindow { Owner = this };
            createWindow.ShowDialog();

            if (createWindow.UserWasCreated)
                LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                UsersGrid.ItemsSource = UserRepository.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de charger les utilisateurs : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadExpirationSetting()
        {
            try
            {
                int duree = ParametrageExpirationRepository.GetDureeValiditeJoursActuelle() ?? 90;
                CurrentExpirationDaysText.Text = $"{duree} jours";
                ExpirationDaysInput.Text = duree.ToString();
            }
            catch (Exception)
            {
                CurrentExpirationDaysText.Text = "90 jours";
            }
        }

        private void SaveExpirationDays_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ExpirationDaysInput.Text.Trim(), out int days) || days <= 0)
            {
                MessageBox.Show("Veuillez saisir un nombre de jours valide (> 0).", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ParametrageExpirationRepository.DefinirDureeExpiration(days, _currentUserId);
                LoadExpirationSetting();
                MessageBox.Show($"Durée de validité des mots de passe mise à jour : {days} jours.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement du paramétrage : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActivationToggle_Click(object sender, RoutedEventArgs e)
        {
            var toggle = (ToggleButton)sender;
            var row = (UserRow)toggle.DataContext;
            string newStatut = toggle.IsChecked == true ? "Actif" : "Desactive";

            try
            {
                UserRepository.SetStatutActivation(row.Username, newStatut);
                row.StatutActivation = newStatut;
            }
            catch (Exception ex)
            {
                toggle.IsChecked = !toggle.IsChecked;
                MessageBox.Show("Impossible de mettre à jour le statut : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValidationToggle_Click(object sender, RoutedEventArgs e)
        {
            var toggle = (ToggleButton)sender;
            var row = (UserRow)toggle.DataContext;
            string newStatut = toggle.IsChecked == true ? "Valide" : "NonValide";

            try
            {
                UserRepository.SetStatutValidation(row.Username, newStatut);
                row.StatutValidation = newStatut;

                if (newStatut == "Valide")
                    NotificationService.NotifierCompteValide(row.UserId);
            }
            catch (Exception ex)
            {
                toggle.IsChecked = !toggle.IsChecked;
                MessageBox.Show("Impossible de mettre à jour le statut : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
