using FluentValidation;

namespace MyAyur.MeetEventApi
{
    public class EventValidator : AbstractValidator<EventVM>
    {
        public EventValidator()
        {
            RuleFor(x => x.StartTime).NotEmpty();
            RuleFor(x => x.StartTimeZone).NotEmpty();
            RuleFor(x => x.EndTime).NotEmpty();
            RuleFor(x => x.EndTimeZone).NotEmpty();
            RuleFor(x => x.Attendees).NotEmpty();
        }
    }
}
