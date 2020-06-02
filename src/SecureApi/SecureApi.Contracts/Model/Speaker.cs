namespace SecureApi.Speaker.Model
{
    public class Speaker
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Level Level { get; set; }
    }

    public enum Level
    {
        Beginner,
        Moderate,
        Experienced,
        Keynote
    }
}
