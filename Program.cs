using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using MyAyur.MeetEventApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IValidator<EventVM>, EventValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/create-event", async (EventVM meetEvent) =>
{
    List<EventAttendee> attendees = new();
    string? clientId = Environment.GetEnvironmentVariable("ClientId"),
            clientSecret = Environment.GetEnvironmentVariable("ClientSecret"),
            refreshToken = Environment.GetEnvironmentVariable("RefreshToken"),
            eventSummary = Environment.GetEnvironmentVariable("EventSummary"),
            eventDescription = Environment.GetEnvironmentVariable("EventDescription"),
            calendarId = Environment.GetEnvironmentVariable("CalendarId");

    var initializer = new GoogleAuthorizationCodeFlow.Initializer
    {
        ClientSecrets = new ClientSecrets
        {
            ClientId = clientId,
            ClientSecret = clientSecret
        }
    };

    GoogleAuthorizationCodeFlow flow = new(initializer);

    TokenResponse token = new()
    {
        RefreshToken = refreshToken
    };

    UserCredential credentials = new(flow, "user", token);

    CalendarService calendarService = new(new BaseClientService.Initializer
    {
        HttpClientInitializer = credentials
    });

    foreach (string attendee in meetEvent.Attendees)
    {
        attendees.Add(new EventAttendee() { Email = attendee });
    }

    Event calendarEvent = new()
    {
        Summary = eventSummary,
        Location = "",
        Description = eventDescription,
        Attendees = attendees,
        ColorId = "10",
        Start = new EventDateTime
        {
            DateTimeDateTimeOffset = meetEvent.StartTime, // Set the start date and time of the event
            TimeZone = meetEvent.StartTimeZone // Set the desired time zone
        },
        End = new EventDateTime
        {
            DateTimeDateTimeOffset = meetEvent.EndTime, // Set the end date and time of the event
            TimeZone = meetEvent.EndTimeZone // Set the desired time zone
        },
        Reminders = new Event.RemindersData()
        {
            UseDefault = false,
            Overrides = new EventReminder[]
            {
                 new() { Method = "email", Minutes = 24 * 60 }
            }
        },
        ConferenceData = new()
        {
            CreateRequest = new CreateConferenceRequest()
            {
                RequestId = DateTime.UtcNow.ToString("yyyyMMddHHmmss")
            }
        }
    };
    EventsResource.InsertRequest request = calendarService.Events.Insert(calendarEvent, calendarId);
    request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.All;
    request.SendNotifications = true;
    request.ConferenceDataVersion = 1;
    Event eventResult = await request.ExecuteAsync();

    return eventResult;
})
.WithName("CreateGoogleEvent")
.AddEndpointFilter<ValidationFilter<EventVM>>()
.WithOpenApi();

app.Run();