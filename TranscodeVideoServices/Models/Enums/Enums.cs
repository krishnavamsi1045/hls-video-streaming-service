namespace TranscodeVideoServices.Models.Enums
{
    public enum JobStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
    }
    public enum VideoStatus
    {
        Processing,
        Uploaded,
        Ready,
        Failed
    }
}
