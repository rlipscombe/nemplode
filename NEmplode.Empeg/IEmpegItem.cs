namespace NEmplode.Empeg
{
    public interface IEmpegItem
    {
        int Id { get; }
        bool IsPlaylist { get; }
        int Length { get; }
        string Title { get; }
    }
}