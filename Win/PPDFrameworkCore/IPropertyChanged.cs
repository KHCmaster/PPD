namespace PPDFrameworkCore
{
    public delegate void PropertyChangedEventHandler(string propertyName);
    public interface IPropertyChanged
    {
        event PropertyChangedEventHandler PropertyChanged;
    }
}
