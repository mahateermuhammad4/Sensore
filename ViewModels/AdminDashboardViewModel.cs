using System.Collections.ObjectModel;
using System.Windows.Input;
using Sensore.Infrastructure;
using Sensore.Models;
using Sensore.Services;

namespace Sensore.ViewModels;

public class AdminDashboardViewModel : ViewModelBase
{
    private readonly MainViewModel _mainViewModel;
    private readonly AuthService _authService;
    private readonly PatientFrameQueryService _patientFrameQueryService;

    private string _newUserEmail = string.Empty;
    private string _newUserFullName = string.Empty;
    private string _newUserPassword = string.Empty;
    private UserRole _newUserRole = UserRole.Patient;
    private User? _selectedUser;
    private User? _selectedClinician;
    private User? _selectedPatient;
    private string _statusMessage = string.Empty;

    public AdminDashboardViewModel(
        MainViewModel mainViewModel,
        User currentUser,
        AuthService authService,
        PatientFrameQueryService patientFrameQueryService)
    {
        _mainViewModel = mainViewModel;
        _authService = authService;
        _patientFrameQueryService = patientFrameQueryService;
        CurrentUser = currentUser;

        CreateUserCommand = new RelayCommand(async _ => await CreateUserAsync());
        ToggleUserActiveCommand = new RelayCommand(async _ => await ToggleUserActiveAsync());
        AssignPatientCommand = new RelayCommand(async _ => await AssignPatientAsync());
        RefreshCommand = new RelayCommand(async _ => await LoadAsync());
        LogoutCommand = new RelayCommand(_ => _mainViewModel.NavigateToLogin());

        _ = LoadAsync();
    }

    public User CurrentUser { get; }

    public ObservableCollection<User> UserList { get; } = new();
    public ObservableCollection<User> Clinicians { get; } = new();
    public ObservableCollection<User> Patients { get; } = new();

    public IEnumerable<UserRole> Roles { get; } = Enum.GetValues<UserRole>();

    public string NewUserEmail
    {
        get => _newUserEmail;
        set
        {
            _newUserEmail = value;
            OnPropertyChanged();
        }
    }

    public string NewUserFullName
    {
        get => _newUserFullName;
        set
        {
            _newUserFullName = value;
            OnPropertyChanged();
        }
    }

    public string NewUserPassword
    {
        get => _newUserPassword;
        set
        {
            _newUserPassword = value;
            OnPropertyChanged();
        }
    }

    public UserRole NewUserRole
    {
        get => _newUserRole;
        set
        {
            _newUserRole = value;
            OnPropertyChanged();
        }
    }

    public User? SelectedUser
    {
        get => _selectedUser;
        set
        {
            _selectedUser = value;
            OnPropertyChanged();
        }
    }

    public User? SelectedClinician
    {
        get => _selectedClinician;
        set
        {
            _selectedClinician = value;
            OnPropertyChanged();
        }
    }

    public User? SelectedPatient
    {
        get => _selectedPatient;
        set
        {
            _selectedPatient = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand CreateUserCommand { get; }
    public ICommand ToggleUserActiveCommand { get; }
    public ICommand AssignPatientCommand { get; }
    public ICommand RefreshCommand { get; }

    public ICommand LogoutCommand { get; }

    private async Task LoadAsync()
    {
        var users = await _authService.GetAllUsersAsync();
        UserList.Clear();
        foreach (var user in users)
        {
            UserList.Add(user);
        }

        var clinicians = await _patientFrameQueryService.GetUsersByRoleAsync(UserRole.Clinician);
        Clinicians.Clear();
        foreach (var clinician in clinicians)
        {
            Clinicians.Add(clinician);
        }

        var patients = await _patientFrameQueryService.GetUsersByRoleAsync(UserRole.Patient);
        Patients.Clear();
        foreach (var patient in patients)
        {
            Patients.Add(patient);
        }
    }

    private async Task CreateUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUserEmail) || string.IsNullOrWhiteSpace(NewUserFullName) || string.IsNullOrWhiteSpace(NewUserPassword))
        {
            StatusMessage = "Fill email, full name, and password before creating a user.";
            return;
        }

        try
        {
            await _authService.CreateUserAsync(NewUserEmail, NewUserFullName, NewUserPassword, NewUserRole);
            StatusMessage = "User created.";
            NewUserEmail = string.Empty;
            NewUserFullName = string.Empty;
            NewUserPassword = string.Empty;
            NewUserRole = UserRole.Patient;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task ToggleUserActiveAsync()
    {
        if (SelectedUser == null)
        {
            StatusMessage = "Select a user first.";
            return;
        }

        await _authService.SetUserActiveAsync(SelectedUser.UserId, !SelectedUser.IsActive);
        StatusMessage = "User status updated.";
        await LoadAsync();
    }

    private async Task AssignPatientAsync()
    {
        if (SelectedClinician == null || SelectedPatient == null)
        {
            StatusMessage = "Select both a clinician and a patient.";
            return;
        }

        await _patientFrameQueryService.AssignPatientToClinicianAsync(SelectedClinician.UserId, SelectedPatient.UserId);
        StatusMessage = "Patient assigned to clinician.";
    }
}
