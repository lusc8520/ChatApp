namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public class ViewModelLocator
{
    public static MainViewModel MainViewModel { get; } = new();
    public static LoginViewModel LoginViewModel { get; } = new();
    public static RegisterViewModel RegisterViewModel { get; } = new();
}