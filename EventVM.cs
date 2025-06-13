using FluentValidation;

namespace MyAyur.MeetEventApi
{
    public class EventVM
    {
        public DateTime StartTime { get; set; }
        public required string StartTimeZone { get; set; }
        public DateTime EndTime { get; set; }        
        public required string EndTimeZone { get; set; }
        public required IList<string> Attendees { get; set; }
    }
}
