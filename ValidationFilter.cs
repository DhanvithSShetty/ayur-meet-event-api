﻿using FluentValidation;
using System.Net;

namespace MyAyur.MeetEventApi
{
    public class ValidationFilter<T> : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            T? argToValidate = context.GetArgument<T>(0);
            IValidator<T>? validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

            if (validator is not null)
            {
                var validationResult = await validator.ValidateAsync(argToValidate!);
                if (!validationResult.IsValid)
                {
                    return Results.Json(validationResult.ToDictionary(),
                        statusCode: (int)HttpStatusCode.UnprocessableEntity);
                }
            }

            // Otherwise invoke the next filter in the pipeline
            return await next(context);
        }
    }
}
