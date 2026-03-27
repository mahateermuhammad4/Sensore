using System.Windows.Input;
using Sensore.Infrastructure;
using Sensore.Services;

namespace Sensore.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly MainViewModel _mainViewModel;
    private readonly AuthService _authService;

    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(MainViewModel mainViewModel, AuthService authService, string? initialError = null)
    {
        _mainViewModel = mainViewModel;
        _authService = authService;
        _errorMessage = initialError ?? string.Empty;
        LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsBusy);
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoginCommand { get; }

    private async Task LoginAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            IsBusy = true;

            var user = await _authService.AuthenticateAsync(Email, Password);
            if (user == null)
            {
                ErrorMessage = "Invalid credentials or inactive account.";
                return;
            }

            _mainViewModel.NavigateToDashboard(user);
        }
        catch
        {
            ErrorMessage = "Unable to sign in right now.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
