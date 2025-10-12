using System;
using Microsoft.Extensions.DependencyInjection;
using SDV.Application.Interfaces;
using SDV.Application.Services;
using SDV.Domain.Entities.Agendas;
using SDV.Domain.Entities.Calendars;
using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Messages;
using SDV.Domain.Entities.Planners;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Specification;
using SDV.Domain.Specification.Context;
using SDV.Domain.Specification.Interfaces;

namespace SDV.Application.Resolver;

public static class ResolverIoC
{
    public static IServiceCollection AddApplications(this IServiceCollection services)
    {
        //Add others dependencies
        services.AddTransient<IClientApplication, ClientApplication>();
        services.AddTransient<IAgendaApplication, AgendaApplication>();
        services.AddTransient<IPlannerApplication, PlannerApplication>();
        services.AddTransient<ICalendarApplication, CalendarApplication>();
        services.AddTransient<IMessageApplication, MessageApplication>();
        services.AddTransient<IPlanApplication, PlanApplication>();
        services.AddTransient<IPaymentApplication, PaymentApplication>();
        services.AddTransient<IOrderApplication, OrderApplication>();



        //Add specifications rules
        services.AddTransient<IValidationSpecification<Client>, ClientValidationSpecification>();
        services.AddTransient<SpecificationContext<Client>>();

        services.AddTransient<IValidationSpecification<Agenda>, AgendaValidationSpecification>();
        services.AddTransient<SpecificationContext<Agenda>>();

        services.AddTransient<IValidationSpecification<Planner>, PlannerValidationSpecification>();
        services.AddTransient<SpecificationContext<Planner>>();

        services.AddTransient<IValidationSpecification<Calendar>, CalendarValidationSpecification>();
        services.AddTransient<SpecificationContext<Calendar>>();

        services.AddTransient<IValidationSpecification<Message>, MessageValidationSpecification>();
        services.AddTransient<SpecificationContext<Message>>();

        services.AddTransient<IValidationSpecification<Plan>, PlanValidationSpecification>();
        services.AddTransient<SpecificationContext<Plan>>();

        return services;
    }
}
