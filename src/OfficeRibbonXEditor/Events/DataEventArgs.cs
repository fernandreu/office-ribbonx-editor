namespace OfficeRibbonXEditor.Events;

/// <inheritdoc />
/// <summary>
/// An EventArgs with a Data property of any type
/// </summary>
/// <typeparam name="T">The type of the Data property</typeparam>
public class DataEventArgs<T> : EventArgs where T : class
{
    public DataEventArgs()
    {
    }

    public DataEventArgs(T? data)
    {
        this.Data = data;
    }

    /// <summary>
    /// Gets or sets the data passed by the event
    /// </summary>
    public T? Data { get; set; }
}